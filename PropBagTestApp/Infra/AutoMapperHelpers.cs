using DRM.PropBag.AutoMapperSupport;
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
    using PropIdType = UInt32;
    using PropNameType = String;

    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;

    public class AutoMapperHelpers
    {
        public SimpleAutoMapperProvider InitializeAutoMappers(IPropModelProvider propModelProvider)
        {
            IPropBagMapperBuilderProvider propBagMapperBuilderProvider
                = new SimplePropBagMapperBuilderProvider
                (
                    wrapperTypeCreator: null,
                    viewModelActivator: null
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
        public static SimpleAutoMapperProvider AutoMapperProvider { get; }
        public static IPropFactory ThePropFactory { get; }

        public static PSAccessServiceProviderType PropStoreAccessServiceProvider { get; }

        static JustSayNo()
        {
            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider = new SimpleHandlerDispatchDelegateCacheProvider();

            IProvidePropStoreAccessService<PropIdType, PropNameType> result = 
                new SimplePropStoreAccessServiceProvider(MAX_NUMBER_OF_PROPERTIES, handlerDispatchDelegateCacheProvider);

            //IProvideDelegateCaches delegateCacheProvider = new SimpleDelegateCacheProvider();

            ThePropFactory = new WPFPropFactory
                (
                    propStoreAccessServiceProvider: PropStoreAccessServiceProvider,
                    //delegateCacheProvider: delegateCacheProvider,
                    typeResolver: GetTypeFromName,
                    valueConverter: null
                );

            IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources);

            IViewModelActivator vmActivator = new SimpleViewModelActivator();
            PropModelProvider = new PropModelProvider(propBagTemplateProvider, ThePropFactory, vmActivator);

            ViewModelHelper = new ViewModelHelper(PropModelProvider, vmActivator);

            AutoMapperProvider = new AutoMapperHelpers().InitializeAutoMappers(PropModelProvider);
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
