using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;
using DRM.TypeSafePropertyBag;
using System;
using System.ComponentModel;
using System.Windows;

namespace PropBagTestApp
{
    public class AutoMapperHelpers
    {
        public AutoMapperProvider InitializeAutoMappers(IPropModelProvider propModelProvider,
            IPropFactory defaultPropFactory)
        {
            IMapTypeDefinitionProvider mapTypeDefinitionProvider = new SimpleMapTypeDefinitionProvider();

            ICachePropBagMappers mappersCachingService = new SimplePropBagMapperCache();

            AutoMapperProvider autoMapperProvider = new AutoMapperProvider
                (
                propModelProvider: propModelProvider,
                mapTypeDefinitionProvider: mapTypeDefinitionProvider,
                mappersCachingService: mappersCachingService,
                defaultPropFactory: defaultPropFactory
                );

            return autoMapperProvider;
        }

        #region NOTES

        //-------------------------

        // Other Dependencies that could be managed, but not part of AutoMapper, per say.
        // 1. Type Converter Cache used by most, if not all IPropFactory.
        // 2. DoSetDelegate Cache -- basically baked into IPropBag -- not many different options
        // 3. PropCreation Delegate Cache --  only used by IPropFactory this is not critical now, but could become so 
        // 4. Event Listeners that could managed better if done as a central service -- used by PropBag and the Binding engine.

        //------------------------ 

        //******************

        // Services that we need to focus on now.
        // 1. ViewModel creation 
        // 2. PropFactory boot strapping
        // 3. Proxy Model creation (we should be able to use 95% of the same services as those provided for ViewModel
        //          creation, in fact, ProxModel creation is probably the driver and ViewModel can benefit from
        //          novel techniqes explored here.
        // 4. Creating a language for ViewModel configuration.
        // 5. Creating services that allow for data flow behavior to be declared and executed without having
        //          to write code.
        // 6. Creating ViewModel sinks for data coming from the View dynamically, ReactiveUI has a 
        //          a way of doing this from the ViewModel to the View, can we build a facility to allow the reverse?
        // 
        // 7. Allowing the View to affect the behavior of the ViewModel dynamically.
        // 8. Design-time support including AutoMapper mapping configuration and testing.
        //******************

        // +++++++++++++++++++

        // Other services that should be addressed
        // 1. Building TypeDescriptors / Type Descriptions / PropertyInfo / Custom MetaData for reflection.
        // 2. XML Serialization services for saving / hydrating IPropBag objects.
        // 
        // +++++++++++++++++++

        #endregion

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
        public static PropModelProvider PropModelProvider { get; }
        public static AutoMapperProvider AutoMapperProvider { get; }
        public static IPropFactory ThePropFactory { get; }

        static JustSayNo() 
        {
            ThePropFactory = new PropFactory
                (
                    returnDefaultForUndefined: false,
                    typeResolver: GetTypeFromName,
                    valueConverter: null
                );

            IPropBagTemplateProvider propBagTemplateProvider
                = new PropBagTemplateProvider(Application.Current.Resources);

            PropModelProvider = new PropModelProvider(propBagTemplateProvider);

            AutoMapperProvider = new AutoMapperHelpers().InitializeAutoMappers(PropModelProvider, ThePropFactory);
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
