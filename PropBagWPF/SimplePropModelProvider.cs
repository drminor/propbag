using DRM.PropBag;
using DRM.PropBag.ViewModelTools;
using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DRM.PropBagWPF
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public class SimplePropModelProvider : IProvidePropModels
    { 
        #region Private Fields

        private IPropBagTemplateProvider _propBagTemplateProvider;
        private IMapperRequestProvider _mapperRequestProvider;
        private IParsePropBagTemplates _pbtParser;
        private IPropFactoryFactory _propFactoryFactory;

        private Dictionary<string, PropModelType> _propModelCache;
        private Dictionary<string, IMapperRequest> _mapperRequestCache;

        #endregion

        #region Constructor

        public SimplePropModelProvider
            (
            IPropBagTemplateProvider propBagTemplateProvider,
            IMapperRequestProvider mapperRequestProvider,
            IParsePropBagTemplates propBagTemplateParser,
            IPropFactoryFactory propFactoryFactory,
            string resourceKeySuffix
            )
        {
            _propBagTemplateProvider = propBagTemplateProvider;
            _mapperRequestProvider = mapperRequestProvider;
            _pbtParser = propBagTemplateParser;
            _propFactoryFactory = propFactoryFactory ?? throw new ArgumentNullException(nameof(propFactoryFactory));
            ResourceKeySuffix = resourceKeySuffix;

            _propModelCache = new Dictionary<string, PropModelType>();
            _mapperRequestCache = new Dictionary<string, IMapperRequest>();

        }

        #endregion

        #region Public Properties

        public string ResourceKeySuffix { get; }

        #endregion

        #region PropBagTemplate Locator Support

        public bool CanFindPropBagTemplateWithJustKey => _propBagTemplateProvider?.CanFindPropBagTemplateWithJustAKey != false;
        public bool HasPbtLookupResources => _propBagTemplateProvider != null;

        // TODO: Add locks to this code.
        public PropModelType GetPropModel(string resourceKey)
        {
            if(_propModelCache.TryGetValue(resourceKey, out PropModelType value))
            {
                return value;
            }
            else
            {
                PropModelType result = FetchPropModel(resourceKey);
                _propModelCache.Add(resourceKey, result);
                return result;
            }
        }

        private PropModelType FetchPropModel(string resourceKey)
        {
            try
            {
                if (CanFindPropBagTemplateWithJustKey)
                {
                    PropBagTemplate pbt = _propBagTemplateProvider.GetPropBagTemplate(resourceKey);
                    PropModelType pm = _pbtParser.ParsePropModel(pbt);
                    FixUpPropFactory(pm, _propFactoryFactory);
                    return pm;
                }
                else if (HasPbtLookupResources)
                {
                    throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                        $"if this PropModelProvider was supplied with a PropBagTemplateProvider upon construction. " +
                        $"No class implementing: {nameof(IPropBagTemplateProvider)} was provided. " +
                        $"Please supply a PropBagTemplate object.");
                }
                else
                {
                    throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                        $"if this PropModelProvider was supplied with the necessary resources upon construction. " +
                        $"A {_propBagTemplateProvider.GetType()} was provided, but it does not have the necessary resources. " +
                        $"Please supply a ResourceDictionary and ResourceKey or a ProbBagTemplate object.");
                }
            }
            catch (System.Exception e)
            {
                throw new ApplicationException($"PropBagTemplate for ResourceKey = {resourceKey} was not found.", e);
            }
        }

        public PropModelType GetPropModel(ResourceDictionary rd, string resourceKey)
        {
            try
            {
                if (HasPbtLookupResources)
                {
                    PropBagTemplate pbt = _propBagTemplateProvider.GetPropBagTemplate(resourceKey);
                    PropModelType pm = _pbtParser.ParsePropModel(pbt);
                    FixUpPropFactory(pm, _propFactoryFactory);
                    return pm;
                }
                else
                {
                    throw new InvalidOperationException($"A call providing a ResourceDictionary and a ResouceKey can only be done, " +
                        $"if this PropModelProvider was supplied with a resource upon construction. " +
                        $"No class implementing: {nameof(IPropBagTemplateProvider)} was provided.");
                }
            }
            catch (System.Exception e)
            {
                throw new ApplicationException("Resource was not found.", e);
            }
        }

        private PropModelType FixUpPropFactory(PropModelType propModel, IPropFactoryFactory propFactoryGenerator)
        {
            //// Include a reference to this PropModelProvider
            //propModel.PropModelProvider = this;

            // If the propModel does not supply a PropFactory, create one using the PropFactoryType.
            if (propModel.PropFactory == null)
            {
                if (propModel.PropFactoryType != null)
                {
                    // If the propModel does not specify a PropFactory, but it does specify a PropFactoryType,
                    // use the PropFactoryFactory given to us to create a PropFactory.
                    IPropFactory generated = propFactoryGenerator.BuildPropFactory(propModel.PropFactoryType);
                    propModel.PropFactory = generated;

                    System.Diagnostics.Debug.WriteLine($"Just created a new PropFactory of type: {propModel.PropFactoryType} for the propModel with FullClassName: {propModel}.");
                }
                else
                {
                    throw new InvalidOperationException($"The PropModel with FullClassName: {propModel} does not have a value for PropFactory, nor does it have a value for PropFactoryType.");
                }
            }

            return propModel;
        }

        #endregion

        #region AutoMapperRequest Lookup Support

        public bool CanFindMapperRequestWithJustKey => _mapperRequestProvider?.CanFindMapperRequestWithJustAKey != false;
        public bool HasMrLookupResources => _mapperRequestProvider != null;

        public IMapperRequest GetMapperRequest(string resourceKey)
        {
            string cookedKey = GetResourceKeyWithSuffix(resourceKey);

            if(_mapperRequestCache.TryGetValue(cookedKey, out IMapperRequest value))
            {
                return value;
            }
            else
            {
                IMapperRequest result = FetchMapperRequest(cookedKey);
                _mapperRequestCache.Add(cookedKey, result);
                return result;
            }
        }

        private IMapperRequest FetchMapperRequest(string resourceKey)
        {
            if (CanFindMapperRequestWithJustKey)
            {
                try
                {
                    MapperRequestTemplate mr = _mapperRequestProvider.GetMapperRequest(resourceKey);
                    IMapperRequest mrCooked = new MapperRequest(mr.SourceType, mr.DestinationPropModelKey, mr.ConfigPackageName);
                    return mrCooked;
                }
                catch (System.Exception e)
                {
                    throw new ApplicationException($"MapperRequest for ResourceKey = {resourceKey} was not found.", e);
                }
            }
            else if (HasMrLookupResources)
            {
                throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                    $"if this PropModelProvider was supplied with a MapperRequestProvider upon construction. " +
                    $"No class implementing: {nameof(IMapperRequestProvider)} was provided. " +
                    $"Please supply a MapperRequest object.");
            }
            else
            {
                throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                    $"if this PropModelProvider was supplied with the necessary resources upon construction. " +
                    $"A {_mapperRequestProvider.GetType()} was provided, but it does not have the necessary resources. " +
                    $"Please supply a ResourceDictionary and ResourceKey or a MapperRequest object.");
            }
        }

        public IMapperRequest GetMapperRequest(ResourceDictionary rd, string resourceKey)
        {
            if (HasMrLookupResources)
            {
                try
                {
                    MapperRequestTemplate mr = _mapperRequestProvider.GetMapperRequest(rd, resourceKey);
                    IMapperRequest mapperRequest = new MapperRequest(mr.SourceType, mr.DestinationPropModelKey, mr.ConfigPackageName);
                    return mapperRequest;
                }
                catch (System.Exception e)
                {
                    throw new ApplicationException("Resource was not found.", e);
                }
            }
            else
            {
                throw new InvalidOperationException($"A call providing a ResourceDictionary and a ResouceKey can only be done, " +
                    $"if this PropModelProvider was supplied with a resource upon construction. " +
                    $"No class implementing: {nameof(IMapperRequestProvider)} was provided.");
            }
        }

        private string GetResourceKeyWithSuffix(string rawKey)
        {
            string result = ResourceKeySuffix != null ? $"{rawKey}_{ResourceKeySuffix}" : rawKey;
            return result;
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Only used for Diagnostics.
        /// </summary>
        public void ClearPropItemSets()
        {
            foreach (KeyValuePair<string, PropModelType> kvp in _propModelCache)
            {
                foreach (IPropModelItem pi in kvp.Value.GetPropItems())
                {
                    pi.PropTemplate = null;
                    pi.PropCreator = null;
                    pi.InitialValueCooked = null;
                }
            }
        }

        #endregion
    }
}
