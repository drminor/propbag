using DRM.PropBag;
using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace DRM.PropBagWPF
{
    using PropModelType = IPropModel<String>;

    public class RemotePropModelBuilder : IPropModelBuilder
    {
        #region Private Fields

        private IParsePropBagTemplates _pbtParser;
        private ResourceDictionaryProvider _resourceDictionaryProvider;

        private IPropFactoryFactory _propFactoryFactory;

        //public string MapperConfigPackageNameSuffix { get; }

        private Dictionary<string, PropModelType> _propModelCache;
        private Dictionary<string, IMapperRequest> _mapperRequestCache;

        #endregion

        #region Constructors

        public RemotePropModelBuilder
            (
            ResourceDictionaryProvider resourceDictionaryProvider,
            IParsePropBagTemplates propBagTemplateParser,
            IPropFactoryFactory propFactoryFactory
            //, string mapperConfigPackageNameSuffix
            )
        {
            _resourceDictionaryProvider = resourceDictionaryProvider;
            _pbtParser = propBagTemplateParser;
            _propFactoryFactory = propFactoryFactory;
            //MapperConfigPackageNameSuffix = mapperConfigPackageNameSuffix;

            _propModelCache = new Dictionary<string, PropModelType>();
            _mapperRequestCache = new Dictionary<string, IMapperRequest>();
        }

        #endregion

        #region Bulk Parsing

        public IDictionary<string, PropModelType> LoadPropModels(string basePath, string[] filenames)
        {
            Dictionary<string, PropModelType> result = new Dictionary<string, PropModelType>();

            Thread thread = new Thread(() =>
            {
                ResourceDictionary rd = _resourceDictionaryProvider.Load(basePath, filenames);

                foreach(ResourceDictionary rdChild in rd.MergedDictionaries)
                {
                    foreach(DictionaryEntry kvp in rdChild)
                    {
                        if(kvp.Value is PropBagTemplate pbt)
                        {
                            PropModelType propModel = _pbtParser.ParsePropModel(pbt);

                            result.Add((string) kvp.Key, propModel);
                            _propModelCache.Add((string)kvp.Key, propModel);
                        }
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);

            thread.Start();
            thread.Join();

            thread = null;

            _propModelCache = result;
            return result;
        }

        public IDictionary<string, IMapperRequest> LoadMapperRequests(string basePath, string[] filenames)
        {
            Dictionary<string, IMapperRequest> result = new Dictionary<string, IMapperRequest>();

            Thread thread = new Thread(() =>
            {
                ResourceDictionary rd = _resourceDictionaryProvider.Load(basePath, filenames);

                foreach (ResourceDictionary rdChild in rd.MergedDictionaries)
                {
                    foreach (DictionaryEntry kvp in rdChild)
                    {
                        if (kvp.Value is MapperRequestTemplate mrTemplate)
                        {
                            IMapperRequest mr = new MapperRequest(mrTemplate.SourceType, mrTemplate.DestinationPropModelKey, mrTemplate.ConfigPackageName);

                            result.Add((string)kvp.Key, mr);
                            _mapperRequestCache.Add((string)kvp.Key, mr);
                        }
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);

            thread.Start();
            thread.Join();

            thread = null;

            _mapperRequestCache = result;
            return result;
        }

        #endregion

        #region PropBagTemplate Locator Support

        public bool CanFindPropBagTemplateWithJustKey => true;
        public bool HasPbtLookupResources => true;

        public PropModelType GetPropModel(string resourceKey)
        {
            if(_propModelCache == null)
            {
                throw new InvalidOperationException("You must first call LoadPropModels.");
            }

            if(_propModelCache.TryGetValue(resourceKey, out PropModelType propModel))
            {
                return FixUpPropFactory(propModel, _propFactoryFactory);
            }
            else
            {
                throw new KeyNotFoundException("No PropModel with that resource key can be found.");
            }
        }

        private PropModelType FixUpPropFactory(PropModelType propModel, IPropFactoryFactory propFactoryGenerator)
        {
            //// Include a reference to this PropModelProvider
            //propModel.PropModelProvider = this;

            if (propModel.PropFactory == null)
            {
                if (propModel.PropFactoryType != null)
                {
                    // If the propModel does not specify a PropFactory, but it does specify a PropFactoryType,
                    // use the PropFactoryFactory given to us to create a PropFactory.
                    IPropFactory generated = propFactoryGenerator.BuildPropFactory(propModel.PropFactoryType);
                    propModel.PropFactory = generated;
                }
                else
                {
                    throw new InvalidOperationException("The PropModel does not have a value for PropFactory, nor does it have a value for PropFactoryType.");
                }
            }

            return propModel;
        }

        #endregion

        public IDictionary<string, string> GetTypeToKeyMap()
        {
            if (_propModelCache == null)
            {
                throw new InvalidOperationException("You must first call LoadPropModels.");
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

        #region AutoMapperRequest Lookup Support

        public bool CanFindMapperRequestWithJustKey => true;
        public bool HasMrLookupResources => true;

        public IMapperRequest GetMapperRequest(string resourceKey)
        {
            if (_mapperRequestCache == null)
            {
                throw new InvalidOperationException("You must first call LoadMapperRequests.");
            }

            //string cookedKey = GetResourceKeyWithSuffix(resourceKey);

            if (_mapperRequestCache.TryGetValue(resourceKey, out IMapperRequest mrCooked))
            {
                return mrCooked;
            }
            else
            {
                throw new KeyNotFoundException($"No MapperRequest with resource key: {resourceKey} can be found.");
            }
        }

        //private string GetResourceKeyWithSuffix(string rawKey)
        //{
        //    string result = MapperConfigPackageNameSuffix != null ? $"{rawKey}_{MapperConfigPackageNameSuffix}" : rawKey;
        //    return result;
        //}

        #endregion
    }
}
