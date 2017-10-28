using DRM.PropBag.ControlModel;
using DRM.PropBag.ViewModelBuilder;

using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public class AutoMapperProvider
    {
        PropBagMappingStrategyEnum _mappingStrategy { get; }

        IPropModelProvider _propModelProvider { get; }
        TypeDescriptionProvider _typeDescriptionProvider { get; }

        ConfiguredMappers _mappersCache { get; }
        IModuleBuilderInfo _propBagProxyBuilder { get; }

        #region Constructors

        private AutoMapperProvider() { } // Disallow the parameterless constructor.

        public AutoMapperProvider(PropBagMappingStrategyEnum mappingStrategy,
            IPropModelProvider propModelProvider,
            ConfiguredMappers mappersCache,
            IModuleBuilderInfo propBagProxyBuilder,
            TypeDescriptionProvider typeDescriptionProvider)
        {
            _mappingStrategy = mappingStrategy;
            _propModelProvider = propModelProvider ?? throw new ArgumentNullException(nameof(propModelProvider));
            _typeDescriptionProvider = typeDescriptionProvider ?? throw new ArgumentNullException(nameof(typeDescriptionProvider));
            _mappersCache = mappersCache ?? throw new ArgumentNullException(nameof(mappersCache));
            _propBagProxyBuilder = propBagProxyBuilder ?? throw new ArgumentNullException(nameof(propBagProxyBuilder));

            //if(!propModelProvider.CanFindPropBagTemplateWithJustKey)
            //{
            //    throw new ArgumentException("The PropModelProvider must have been created with a ResourceDictionary or other similar resource.");
            //}
        }

        #endregion

        // TODO: Consider supporting finding the PBT requied for a mapper by class name.
        public IPropBagMapper<TSource, TDestination> GetMapper<TSource, TDestination>(string instanceKey)
        {
            IPropBagMapper<TSource, TDestination> result = null;

            Type dtViewModelType = typeof(TDestination);

            PropModel propModel = _propModelProvider.GetPropModel(instanceKey);

            // Extra Members
            if (_mappingStrategy == PropBagMappingStrategyEnum.ExtraMembers)
            {
                // TODO!: DO WE NEED THIS?
                TypeDescription typeDescription = _typeDescriptionProvider.BuildTypeDesc(propModel,dtViewModelType);

                if (typeDescription == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not build a TypeDescription from the PropMode with instance key = {instanceKey}.");
                    return result;
                }

                Type rtViewModelType = _propBagProxyBuilder.BuildVmProxyClass(typeDescription);
                // END -- DO WE NEED THIS?

                //Type rtViewModelType = typeof(TDestination); //typeof(ReferenceBindViewModelPB);

                IPropBagMapperKey<TSource, TDestination> mapperRequest
                    = new PropBagMapperKey<TSource, TDestination>(propModel, rtViewModelType, _mappingStrategy);
            }

            // Emit Proxy
            else if (_mappingStrategy == PropBagMappingStrategyEnum.EmitProxy)
            {
                TypeDescription typeDescription = _typeDescriptionProvider.BuildTypeDesc(propModel, dtViewModelType);

                if (typeDescription == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not build a TypeDescription from the PropMode with instance key = {instanceKey}.");
                    return result;
                }

                Type rtViewModelType = _propBagProxyBuilder.BuildVmProxyClass(typeDescription);

                IPropBagMapperKey<TSource, TDestination> mapperRequest
                    = new PropBagMapperKey<TSource, TDestination>(propModel, rtViewModelType, _mappingStrategy);

                _mappersCache.Register(mapperRequest);
                result = (IPropBagMapper<TSource, TDestination>)_mappersCache.GetMapperToUse(mapperRequest);
            }

            // Emit Wrapper
            //else if(mappingStrategy == PropBagMappingStrategyEnum.EmitWrapper)
            //{

            //}
            else
            {
                throw new InvalidOperationException($"The mapping strategy: {_mappingStrategy} is not recognized or is not supported.");
            }

            if (result == null)
            {
                System.Diagnostics.Debug.WriteLine($"Could not get an AutoMapper for <{typeof(TSource)},{typeof(TDestination)}>");
            }
            return result;
        }

    }
    
}
