﻿using DRM.PropBag;
using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DRM.PropBagWPF
{
    using PropModelType = IPropModel<String>;

    public class SimplePropModelBuilder : IPropModelBuilder
    { 
        #region Private Fields

        private IPropBagTemplateBuilder _propBagTemplateBuilder;
        private IMapperRequestBuilder _mapperRequestBuilder;
        private IParsePropBagTemplates _pbtParser;
        private IPropFactoryFactory _propFactoryFactory;

        private Dictionary<string, PropModelType> _propModelCache;
        private Dictionary<string, IMapperRequest> _mapperRequestCache;

        #endregion

        #region Constructor

        public SimplePropModelBuilder
            (
            IPropBagTemplateBuilder propBagTemplateBuilder,
            IMapperRequestBuilder mapperRequestBuilder,
            IParsePropBagTemplates propBagTemplateParser,
            IPropFactoryFactory propFactoryFactory
            )
        {
            _propBagTemplateBuilder = propBagTemplateBuilder;
            _mapperRequestBuilder = mapperRequestBuilder;
            _pbtParser = propBagTemplateParser;
            _propFactoryFactory = propFactoryFactory ?? throw new ArgumentNullException(nameof(propFactoryFactory));

            _propModelCache = new Dictionary<string, PropModelType>();
            _mapperRequestCache = new Dictionary<string, IMapperRequest>();
        }

        #endregion

        #region PropBagTemplate Locator Support

        public bool CanFindPropBagTemplateWithJustKey => _propBagTemplateBuilder?.CanFindPropBagTemplateWithJustAKey != false;
        public bool HasPbtLookupResources => _propBagTemplateBuilder != null;

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
                    PropBagTemplate pbt = _propBagTemplateBuilder.GetPropBagTemplate(resourceKey);
                    PropModelType pm = _pbtParser.ParsePropModel(pbt);
                    FixUpPropFactory(pm, _propFactoryFactory);
                    return pm;
                }
                else if (HasPbtLookupResources)
                {
                    throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                        $"if this PropModelBuilder was supplied with a PropBagTemplateBuilder upon construction. " +
                        $"No class implementing: {nameof(IPropBagTemplateBuilder)} was provided. " +
                        $"Please supply a PropBagTemplate object.");
                }
                else
                {
                    throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                        $"if this PropModelBuilder was supplied with the necessary resources upon construction. " +
                        $"A {_propBagTemplateBuilder.GetType()} was provided, but it does not have the necessary resources. " +
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
                    PropBagTemplate pbt = _propBagTemplateBuilder.GetPropBagTemplate(resourceKey);
                    PropModelType pm = _pbtParser.ParsePropModel(pbt);
                    FixUpPropFactory(pm, _propFactoryFactory);
                    return pm;
                }
                else
                {
                    throw new InvalidOperationException($"A call providing a ResourceDictionary and a ResouceKey can only be done, " +
                        $"if this PropModelBuilder was supplied with a resource upon construction. " +
                        $"No class implementing: {nameof(IPropBagTemplateBuilder)} was provided.");
                }
            }
            catch (System.Exception e)
            {
                throw new ApplicationException("Resource was not found.", e);
            }
        }

        private PropModelType FixUpPropFactory(PropModelType propModel, IPropFactoryFactory propFactoryGenerator)
        {
            //// Include a reference to this PropModelBuilder
            //propModel.PropModelBuilder = this;

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

        public IDictionary<string, string> GetTypeToKeyMap()
        {
            if (_propModelCache.Count < 1)
            {
                int count = ParseAll(_propModelCache);
                System.Diagnostics.Debug.WriteLine($"GetTypeToKeyMap found {count} PropModels.");
            }

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (KeyValuePair<string, PropModelType> kvp in _propModelCache)
            {
                string fullClassName = kvp.Value.FullClassName;

                if (result.ContainsKey(fullClassName))
                {
                    throw new InvalidOperationException("Found duplicate class name.");
                }

                result.Add(fullClassName, kvp.Key);
            }

            return result;
        }

        private int ParseAll(Dictionary<string, PropModelType> propModelCache)
        {
            int result = 0;

            IDictionary<string, IPropBagTemplate> templates = _propBagTemplateBuilder.GetPropBagTemplates();

            foreach(KeyValuePair<string, IPropBagTemplate> kvp in templates)
            {
                PropModelType pm = _pbtParser.ParsePropModel(kvp.Value);
                FixUpPropFactory(pm, _propFactoryFactory);

                propModelCache.Add(kvp.Key, pm);
                result++;
            }

            return result;
        }

        #region AutoMapperRequest Lookup Support

        public bool CanFindMapperRequestWithJustKey => _mapperRequestBuilder?.CanFindMapperRequestWithJustAKey != false;
        public bool HasMrLookupResources => _mapperRequestBuilder != null;

        public IMapperRequest GetMapperRequest(string resourceKey)
        {
            //string cookedKey = GetResourceKeyWithSuffix(resourceKey);

            if(_mapperRequestCache.TryGetValue(resourceKey, out IMapperRequest value))
            {
                return value;
            }
            else
            {
                IMapperRequest result = FetchMapperRequest(resourceKey);
                _mapperRequestCache.Add(resourceKey, result);
                return result;
            }
        }

        private IMapperRequest FetchMapperRequest(string resourceKey)
        {
            if (CanFindMapperRequestWithJustKey)
            {
                try
                {
                    MapperRequestTemplate mr = _mapperRequestBuilder.GetMapperRequest(resourceKey);
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
                    $"if this PropModelBuilder was supplied with a MapperRequestBuilder upon construction. " +
                    $"No class implementing: {nameof(IMapperRequestBuilder)} was provided. " +
                    $"Please supply a MapperRequest object.");
            }
            else
            {
                throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                    $"if this PropModelBuilder was supplied with the necessary resources upon construction. " +
                    $"A {_mapperRequestBuilder.GetType()} was provided, but it does not have the necessary resources. " +
                    $"Please supply a ResourceDictionary and ResourceKey or a MapperRequest object.");
            }
        }

        public IMapperRequest GetMapperRequest(ResourceDictionary rd, string resourceKey)
        {
            if (HasMrLookupResources)
            {
                try
                {
                    MapperRequestTemplate mr = _mapperRequestBuilder.GetMapperRequest(rd, resourceKey);
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
                    $"if this PropModelBuilder was supplied with a resource upon construction. " +
                    $"No class implementing: {nameof(IMapperRequestBuilder)} was provided.");
            }
        }

        //private string GetResourceKeyWithSuffix(string rawKey)
        //{
        //    string result = ResourceKeySuffix != null ? $"{rawKey}_{ResourceKeySuffix}" : rawKey;
        //    return result;
        //}

        #endregion

        #region Diagnostics

        /// <summary>
        /// Only used for Diagnostics.
        /// </summary>
        public void ClearPropItemSets()
        {
            foreach (KeyValuePair<string, PropModelType> kvp in _propModelCache)
            {
                foreach (IPropItemModel pi in kvp.Value.GetPropItems())
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
