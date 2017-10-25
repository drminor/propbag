using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;
using DRM.PropBag.ViewModelBuilder;

using DRM.PropBag.ControlsWPF.WPFHelpers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using DRM.TypeSafePropertyBag;
using PropBagTestApp.ViewModels;

namespace PropBagTestApp
{
    // TODO: Convert this static class to an instance class that takes the 
    // the MappingStrategy, the ConfiguredMappers cache and the ModuleBuilderInfo settings in 
    // its constructor.
    // So that the source of these services can be determined at run-time so that we can test the units that
    // rely on this service in isolation.

    static class AutoMapperHelpers
    {
        public static T GetNewViewModel<T>(string instanceKey, IPropFactory propFactory) where T : IPropBag
        {
            PropBagTemplate pbt = PropBagTemplateResources.GetPropBagTemplate(instanceKey);

            return GetNewViewModel<T>(pbt, propFactory);
        }

        public static T GetNewViewModel<T>(PropBagTemplate pbt, IPropFactory propFactory) where T : IPropBag
        {
            PropModel pm = pbt.GetPropModel();

            T result = (T)Activator.CreateInstance(typeof(T), pm, propFactory);
            return result;
        }


        // TODO: Consider supporting finding the PBT requied for a mapper by class name.
        public static IPropBagMapper<TSource, TDestination> GetMapper<TSource, TDestination>(string instanceKey)
        {
            IPropBagMapper<TSource, TDestination> result = null;

            PropBagMappingStrategyEnum mappingStrategy = SettingsExtensions.MappingStrategy;
            ConfiguredMappers mappersCache = SettingsExtensions.ConfiguredMappers;
            IModuleBuilderInfo propBagProxyBuilder = SettingsExtensions.PropBagProxyBuilder;

            Type dtViewModelType = typeof(TDestination);

            if (PropBagTemplateResources.PropBagTemplates == null)
            {
                System.Diagnostics.Debug.WriteLine("PCVM could not get BoundPropBags from Appliation Resources.");
                return result;
            }

            PropBagTemplate bpTemplate = PropBagTemplateResources.GetPropBagTemplate(instanceKey);

            PropModel propModel = bpTemplate.GetPropModel();

            // Extra Members
            if (mappingStrategy == PropBagMappingStrategyEnum.ExtraMembers)
            {
                // TODO!: DO WE NEED THIS?
                TypeDescription typeDescription = propModel.BuildTypeDesc(dtViewModelType);

                if (typeDescription == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not build a TypeDescription from the PropMode with instance key = {instanceKey}.");
                    return result;
                }

                Type rtViewModelType = propBagProxyBuilder.BuildVmProxyClass(typeDescription);
                // END -- DO WE NEED THIS?

                //Type rtViewModelType = typeof(TDestination); //typeof(ReferenceBindViewModelPB);

                IPropBagMapperKey<TSource, TDestination> mapperRequest
                    = new PropBagMapperKey<TSource, TDestination>(propModel, rtViewModelType, mappingStrategy);
            }

            // Emit Proxy
            else if (mappingStrategy == PropBagMappingStrategyEnum.EmitProxy)
            {
                TypeDescription typeDescription = propModel.BuildTypeDesc(dtViewModelType);

                if (typeDescription == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not build a TypeDescription from the PropMode with instance key = {instanceKey}.");
                    return result;
                }

                Type rtViewModelType = propBagProxyBuilder.BuildVmProxyClass(typeDescription);

                IPropBagMapperKey<TSource, TDestination> mapperRequest
                    = new PropBagMapperKey<TSource, TDestination>(propModel, rtViewModelType, mappingStrategy);

                mappersCache.Register(mapperRequest);
                result = (IPropBagMapper<TSource, TDestination>)mappersCache.GetMapperToUse(mapperRequest);
            }

            // Emit Wrapper
            //else if(mappingStrategy == PropBagMappingStrategyEnum.EmitWrapper)
            //{

            //}
            else
            {
                throw new InvalidOperationException($"The mapping strategy: {mappingStrategy} is not recognized or is not supported.");
            }

            if (result == null)
            {
                System.Diagnostics.Debug.WriteLine($"Could not get an AutoMapper for <{typeof(TSource)},{typeof(TDestination)}>");
            }
            return result;
        }

    }
    
}
