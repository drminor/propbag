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
    public class RemotePropModelProvider : IProvidePropModels
    {
        #region Private Fields

        private IParsePropBagTemplates _pbtParser;
        private ResourceDictionaryProvider _resourceDictionaryProvider;

        private IPropFactoryFactory _propFactoryFactory;

        public string MapperConfigPackageNameSuffix { get; }

        private Dictionary<string, IPropModel> _propModelCache;
        private Dictionary<string, IMapperRequest> _mapperRequestCache;

        #endregion

        #region Constructors

        public RemotePropModelProvider
            (
            ResourceDictionaryProvider resourceDictionaryProvider,
            IParsePropBagTemplates propBagTemplateParser,
            IPropFactoryFactory propFactoryFactory,
            string mapperConfigPackageNameSuffix
            )

        {
            _resourceDictionaryProvider = resourceDictionaryProvider;
            _pbtParser = propBagTemplateParser;
            _propFactoryFactory = propFactoryFactory;
            MapperConfigPackageNameSuffix = mapperConfigPackageNameSuffix;

            _propModelCache = new Dictionary<string, IPropModel>();
            _mapperRequestCache = new Dictionary<string, IMapperRequest>();

            
        }

        #endregion

        #region Bulk Parsing

        public IDictionary<string, IPropModel> LoadPropModels(string basePath, string[] filenames)
        {
            Dictionary<string, IPropModel> result = new Dictionary<string, IPropModel>();

            Thread thread = new Thread(() =>
            {
                ResourceDictionary rd = _resourceDictionaryProvider.Load(basePath, filenames);

                foreach(ResourceDictionary rdChild in rd.MergedDictionaries)
                {
                    foreach(DictionaryEntry kvp in rdChild)
                    {
                        if(kvp.Value is PropBagTemplate pbt)
                        {
                            IPropModel propModel = _pbtParser.ParsePropModel(pbt);

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
                            // Go ahead and fetch the PropModel from the key specified in the "template" request -- since the 
                            // the receiver of this PropBag.MapperRequest will probably not have access to a PropModel Provider.
                            //IPropModel propModel = GetPropModel(mr.DestinationPropModelKey);
                            //IMapperRequest mrCooked = new MapperRequest(mr.SourceType, propModel, mr.ConfigPackageName);

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

        public IPropModel GetPropModel(string resourceKey)
        {
            if(_propModelCache == null)
            {
                throw new InvalidOperationException("You must first call LoadPropModels.");
            }

            if(_propModelCache.TryGetValue(resourceKey, out IPropModel propModel))
            {
                return FixUpPropFactory(propModel, _propFactoryFactory);
            }
            else
            {
                throw new KeyNotFoundException("No PropModel with that resource key can be found.");
            }
        }

        IPropModel FixUpPropFactory(IPropModel propModel, IPropFactoryFactory propFactoryGenerator/*, IPropFactory fallBackPropFactory*/)
        {
            // Include a reference to this PropModelProvider
            propModel.PropModelProvider = this;

            //TOOD: We are working on moving all PropFactory creation logic
            //to the caller's level: PropModelProviders should not have truck with creating PropFactories.

            //// If the propModel does not specify a PropFactory, but it does specify a PropFactoryType,
            //// use the PropFactoryFactory given to us to create a PropFactory.
            //if (propModel.PropFactory == null)
            //{
            //    if (propModel.PropFactoryType != null)
            //    {
            //        IPropFactory generated = propFactoryGenerator.BuildPropFactory(propModel.PropFactoryType);
            //        propModel.PropFactory = generated;
            //    }
            //    else
            //    {
            //        // If no value was supplied for either the PropFactory or the PropFactoryType,
            //        // then use the default or 'fallback' propFactory.
            //        propModel.PropFactory = fallBackPropFactory;
            //    }
            //}

            if (propModel.PropFactory == null)
            {
                if(propModel.PropFactoryType != null)
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

            //if (propModel.PropFactory == null)
            //{
            //    propModel.PropFactory = fallBackPropFactory;
            //}

            // If the propModel does not supply a PropFactory, use the one assigned to us upon construction.
            return propModel;
        }
        #endregion

        #region AutoMapperRequest Lookup Support

        public bool CanFindMapperRequestWithJustKey => true;
        public bool HasMrLookupResources => true;

        public IMapperRequest GetMapperRequest(string resourceKey)
        {
            if (_mapperRequestCache == null)
            {
                throw new InvalidOperationException("You must first call LoadMapperRequests.");
            }

            string cookedKey = GetResourceKeyWithSuffix(resourceKey);


            if (_mapperRequestCache.TryGetValue(cookedKey, out IMapperRequest mrCooked))
            {
                return mrCooked;
            }
            else
            {
                throw new KeyNotFoundException($"No MapperRequest with resource key: {cookedKey} can be found.");
            }
        }

        private string GetResourceKeyWithSuffix(string rawKey)
        {
            string result = MapperConfigPackageNameSuffix != null ? $"{rawKey}_{MapperConfigPackageNameSuffix}" : rawKey;
            return result;
        }

        #endregion
    }
}
