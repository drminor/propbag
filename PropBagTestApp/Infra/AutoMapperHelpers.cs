using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
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


        #region OLD CODE

        //public static T GetNewViewModel<T>(string instanceKey, IPropFactory propFactory) where T : IPropBag
        //{
        //    PropBagTemplate pbt = PropBagTemplateResources.GetPropBagTemplate(instanceKey);

        //    return GetNewViewModel<T>(pbt, propFactory);
        //}

        //public static T GetNewViewModel<T>(PropBagTemplate pbt, IPropFactory propFactory) where T : IPropBag
        //{
        //    PropModel pm = pbt.GetPropModel();

        //    T result = (T)Activator.CreateInstance(typeof(T), pm, propFactory);
        //    return result;
        //}


        //// TODO: Consider supporting finding the PBT requied for a mapper by class name.
        //public static IPropBagMapper<TSource, TDestination> GetMapper<TSource, TDestination>(string instanceKey)
        //{
        //    IPropBagMapper<TSource, TDestination> result = null;

        //    PropBagMappingStrategyEnum mappingStrategy = SettingsExtensions.MappingStrategy;
        //    ConfiguredMappers mappersCache = SettingsExtensions.ConfiguredMappers;
        //    IModuleBuilderInfo propBagProxyBuilder = SettingsExtensions.PropBagProxyBuilder;

        //    Type dtViewModelType = typeof(TDestination);

        //    if (PropBagTemplateResources.PropBagTemplates == null)
        //    {
        //        System.Diagnostics.Debug.WriteLine("PCVM could not get BoundPropBags from Appliation Resources.");
        //        return result;
        //    }

        //    PropBagTemplate bpTemplate = PropBagTemplateResources.GetPropBagTemplate(instanceKey);

        //    PropModel propModel = bpTemplate.GetPropModel();

        //    // Extra Members
        //    if (mappingStrategy == PropBagMappingStrategyEnum.ExtraMembers)
        //    {
        //        // TODO!: DO WE NEED THIS?
        //        TypeDescription typeDescription = propModel.BuildTypeDesc(dtViewModelType);

        //        if (typeDescription == null)
        //        {
        //            System.Diagnostics.Debug.WriteLine($"Could not build a TypeDescription from the PropMode with instance key = {instanceKey}.");
        //            return result;
        //        }

        //        Type rtViewModelType = propBagProxyBuilder.BuildVmProxyClass(typeDescription);
        //        // END -- DO WE NEED THIS?

        //        //Type rtViewModelType = typeof(TDestination); //typeof(ReferenceBindViewModelPB);

        //        IPropBagMapperKey<TSource, TDestination> mapperRequest
        //            = new PropBagMapperKey<TSource, TDestination>(propModel, rtViewModelType, mappingStrategy);
        //    }

        //    // Emit Proxy
        //    else if (mappingStrategy == PropBagMappingStrategyEnum.EmitProxy)
        //    {
        //        TypeDescription typeDescription = propModel.BuildTypeDesc(dtViewModelType);

        //        if (typeDescription == null)
        //        {
        //            System.Diagnostics.Debug.WriteLine($"Could not build a TypeDescription from the PropMode with instance key = {instanceKey}.");
        //            return result;
        //        }

        //        Type rtViewModelType = propBagProxyBuilder.BuildVmProxyClass(typeDescription);

        //        IPropBagMapperKey<TSource, TDestination> mapperRequest
        //            = new PropBagMapperKey<TSource, TDestination>(propModel, rtViewModelType, mappingStrategy);

        //        mappersCache.Register(mapperRequest);
        //        result = (IPropBagMapper<TSource, TDestination>)mappersCache.GetMapperToUse(mapperRequest);
        //    }

        //    // Emit Wrapper
        //    //else if(mappingStrategy == PropBagMappingStrategyEnum.EmitWrapper)
        //    //{

        //    //}
        //    else
        //    {
        //        throw new InvalidOperationException($"The mapping strategy: {mappingStrategy} is not recognized or is not supported.");
        //    }

        //    if (result == null)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Could not get an AutoMapper for <{typeof(TSource)},{typeof(TDestination)}>");
        //    }
        //    return result;
        //}

        #endregion
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
            IProvidePropStoreAccessService<PropIdType, PropNameType> result = new SimplePropStoreAccessServiceProvider(MAX_NUMBER_OF_PROPERTIES);

            ThePropFactory = new PropFactory
                (
                    propStoreAccessServiceProvider: PropStoreAccessServiceProvider,
                    typeResolver: GetTypeFromName,
                    valueConverter: null
                );

            IPropBagTemplateProvider propBagTemplateProvider
                = new PropBagTemplateProvider(Application.Current.Resources);

            PropModelProvider = new PropModelProvider(propBagTemplateProvider, ThePropFactory);

            ViewModelHelper = new ViewModelHelper(PropModelProvider);

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
