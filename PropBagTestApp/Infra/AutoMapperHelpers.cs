using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.Caches;
using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;
using DRM.PropBagWPF;
using DRM.TypeSafePropertyBag;
using DRM.ViewModelTools;
using System;
using System.ComponentModel;
using System.Windows;

namespace PropBagTestApp.Infra
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class AutoMapperHelpers
    {
        public IProvideAutoMappers InitializeAutoMappers(IPropModelProvider propModelProvider, PSAccessServiceCreatorInterface storeAccessCreator)
        {
            IPropBagMapperBuilderProvider propBagMapperBuilderProvider
                = new SimplePropBagMapperBuilderProvider
                (
                    wrapperTypeCreator: null,
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
    }

    // TODO: Fix this!!
    public static class JustSayNo // To using Static-based config providers.
    {
        // Maximum number of PropertyIds for any one given Object.
        private const int LOG_BASE2_MAX_PROPERTIES = 16;
        public static readonly int MAX_NUMBER_OF_PROPERTIES = (int)Math.Pow(2, LOG_BASE2_MAX_PROPERTIES); //65536;

        public static PropModelProvider PropModelProvider { get; }
        public static ViewModelHelper ViewModelHelper { get; }
        public static IProvideAutoMappers AutoMapperProvider { get; }
        public static IPropFactory ThePropFactory { get; }

        public static PSAccessServiceCreatorInterface PropStoreAccessServiceCreator { get; }

        static JustSayNo()
        {
            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();

            PSAccessServiceCreatorInterface result = 
                new SimplePropStoreServiceEP(MAX_NUMBER_OF_PROPERTIES, handlerDispatchDelegateCacheProvider);

            IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates));

            ThePropFactory = new WPFPropFactory
                (
                    typeResolver: GetTypeFromName,
                    valueConverter: null,
                    delegateCacheProvider: delegateCacheProvider
                );

            IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources);

            IViewModelActivator vmActivator = new SimpleViewModelActivator();
            PropModelProvider = new PropModelProvider(propBagTemplateProvider, ThePropFactory, vmActivator, PropStoreAccessServiceCreator);

            ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator, PropStoreAccessServiceCreator);

            AutoMapperProvider = new AutoMapperHelpers().InitializeAutoMappers(PropModelProvider, PropStoreAccessServiceCreator);
        }

        public static Type GetTypeFromName(string typeName)
        {
            Type result;
            try
            {
                result = Type.GetType(typeName);
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", e);
            }

            if (result == null)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.");
            }

            return result;
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
