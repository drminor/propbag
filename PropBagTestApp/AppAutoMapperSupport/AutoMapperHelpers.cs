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

namespace PropBagTestApp
{
    static class AutoMapperHelpers
    {
        public static T GetNewViewModel<T>(string instanceKey, IPropFactory propFactory) where T : IPropBagMin
        {
            PropBagTemplate pbt = PropBagTemplateResources.GetPropBagTemplate(instanceKey);

            return GetNewViewModel<T>(pbt, propFactory);
        }

        public static T GetNewViewModel<T>(PropBagTemplate pbt, IPropFactory propFactory) where T : IPropBagMin
        {
            PropModel pm = pbt.GetPropModel();

            T result = (T)Activator.CreateInstance(typeof(T), pm, propFactory);
            return result;
        }


        // TODO: Consider supporting finding the PBT requied for a mapper by class name.
        public static PropBagMapper<TSource, TDestination> GetMapper<TSource, TDestination>(string instanceKey)
        {
            PropBagMappingStrategyEnum mappingStrategy = SettingsExtensions.MappingStrategy;
            PropBagMapper<TSource, TDestination> result = null;

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

            TypeDescription typeDescription = propModel.BuildTypeDesc(dtViewModelType);

            if (typeDescription == null)
            {
                System.Diagnostics.Debug.WriteLine($"Could not build a TypeDescription from the PropMode with instance key = {instanceKey}.");
                return result;
            }

            Type rtViewModelType = propBagProxyBuilder.BuildVmProxyClass(typeDescription);

            PropBagMapperKey<TSource, TDestination> mapperRequest
                = new PropBagMapperKey<TSource, TDestination>
                    (propModel, rtViewModelType, mappingStrategy);

            mappersCache.Register(mapperRequest);

            result = (PropBagMapper<TSource, TDestination>)mappersCache.GetMapperToUse(mapperRequest);
            if(result == null)
            {
                System.Diagnostics.Debug.WriteLine($"Could not get an AutoMapper for <{typeof(TSource)},{typeof(TDestination)}>");
            }

            return result;
        }

    }
    
}
