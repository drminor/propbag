using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.Caches;
using DRM.PropBag.ControlModel;
using DRM.PropBagControlsWPF;

using DRM.PropBagWPF;

using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;
using System;
using System.ComponentModel;
using System.Windows;

namespace PropBagTestApp.Infra
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    // TODO: Fix this!!
    public static class JustSayNo // JustSayNo to using Static-based config providers.
    {
        public static ITypeDescBasedTConverterCache TypeDescBasedTConverterCache { get; }

        public static IPropFactory DefaultPropFactory { get; }
        public static IProvidePropModels PropModelProvider { get; }
        public static ViewModelHelper ViewModelHelper { get; }
        public static IProvideAutoMappers AutoMapperProvider { get; }

        public static string PackageConfigName { get; set; }

        static JustSayNo()
        {
            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(DRM.PropBag.PropBag), typeof(APFGenericMethodTemplates));

            TypeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
            IConvertValues pfvc = new PropFactoryValueConverter(TypeDescBasedTConverterCache);

            ResolveTypeDelegate typeResolver = null;

            DefaultPropFactory = new WPFPropFactory(delegateCacheProvider: delegateCacheProvider, valueConverter: pfvc, typeResolver: typeResolver);

            IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources);
            IViewModelActivator vmActivator = new SimpleViewModelActivator();
            PSAccessServiceCreatorInterface propStoreEntryPoint = GetPropStoreEntryPoint();

            PropModelProvider = new PropModelProvider(propBagTemplateProvider, DefaultPropFactory, vmActivator, propStoreEntryPoint);
            ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, propStoreEntryPoint);
            AutoMapperProvider = GetAutoMapperProvider(PropModelProvider, propStoreEntryPoint);
        }

        public static PSAccessServiceCreatorInterface GetPropStoreEntryPoint()
        {
            int MAX_NUMBER_OF_PROPERTIES = 65536;

            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();

            PSAccessServiceCreatorInterface result = new SimplePropStoreServiceEP(MAX_NUMBER_OF_PROPERTIES, handlerDispatchDelegateCacheProvider);

            return result;
        }

        private static IProvideAutoMappers GetAutoMapperProvider
            (
            IProvidePropModels propModelProvider,
            PSAccessServiceCreatorInterface storeAccessCreator
            )
        {
            IPropBagMapperBuilderProvider propBagMapperBuilderProvider = new SimplePropBagMapperBuilderProvider
                (
                wrapperTypesCreator: null,
                viewModelActivator: null,
                storeAccessCreator: storeAccessCreator
                );

            IMapTypeDefinitionProvider mapTypeDefinitionProvider = new SimpleMapTypeDefinitionProvider();

            ICachePropBagMappers mappersCachingService = new SimplePropBagMapperCache();

            SimpleAutoMapperProvider autoMapperProvider = new SimpleAutoMapperProvider
                (
                mapTypeDefinitionProvider: mapTypeDefinitionProvider,
                mappersCachingService: mappersCachingService,
                mapperBuilderProvider: propBagMapperBuilderProvider,
                propModelProvider: propModelProvider
                );

            return autoMapperProvider;
        }

        #region InDesign Support

        public static bool InDesignMode() => _isInDesignMode.HasValue && _isInDesignMode == true;

        public static bool? _isInDesignMode;

        public static bool IsInDesignModeStatic
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool)DependencyPropertyDescriptor
                                        .FromProperty(prop, typeof(FrameworkElement))
                                        .Metadata.DefaultValue;
                }

                return _isInDesignMode.Value;
            }
        }

        #endregion
    }
}
