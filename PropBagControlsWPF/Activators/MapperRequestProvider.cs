using System.Collections.Generic;
using System.Windows;
using System;
using System.Linq;

namespace DRM.PropBagControlsWPF
{
    public class MapperRequestProvider : IMapperRequestProvider
    {
        #region Private Fields and Properties

        private ResourceDictionary _resources;

        // TODO: Consider parsing the entire resource file on first access.
        //private Dictionary<string, MapperRequest> _mrFromOurResources;
        //private Dictionary<string, MapperRequest> MrFromOurResources
        //{
        //    get
        //    {
        //        if (_mrFromOurResources == null)
        //        {
        //            _mrFromOurResources = GetMapperRequests(_resources);
        //        }
        //        return _mrFromOurResources;
        //    }
        //}

        #endregion

        #region Constructor

        public MapperRequestProvider()
        {
            _resources = null;
        }

        public MapperRequestProvider(ResourceDictionary resources, string resourceKeySuffix)
        {
            _resources = resources;
            ResourceKeySuffix = resourceKeySuffix;
        }

        #endregion

        #region Public Properties

        public string ResourceKeySuffix { get; }
        public bool CanFindMapperRequestWithJustAKey => _resources != null;

        #endregion

        #region Public Methods

        public MapperRequestTemplate GetMapperRequest(string resourceKey)
        {
            if (!CanFindMapperRequestWithJustAKey) throw new InvalidOperationException($"This instance of {nameof(PropBagTemplateProvider)} was not provide a ResourceDictionary.");
            return GetMapperRequest(_resources, resourceKey);
        }

        public MapperRequestTemplate GetMapperRequest(ResourceDictionary resources, string resourceKey)
        {
            if (resources == null) throw new ArgumentNullException(nameof(resources));
            if (resourceKey == null) throw new ArgumentNullException(nameof(resourceKey));

            string cookedKey = GetResourceKeyWithSuffix(resourceKey);

            object resource;
            try
            {
                resource = resources[cookedKey];
            }
            catch (KeyNotFoundException knfe)
            {
                throw new KeyNotFoundException($"Could not find a MapperRequest with key = {cookedKey}.", knfe);
            }

            if (resource == null)
            {
                throw new InvalidOperationException($"Could not find a MapperRequest with key = {cookedKey}.");
            }

            if (TryParse(resource, cookedKey, out MapperRequestTemplate mapperRequest))
            {
                return mapperRequest;
            }
            else
            {
                throw new InvalidOperationException($"The MapperRequest with key = {cookedKey} could not be parsed.");
            }
        }

        public Dictionary<string, MapperRequestTemplate> GetMapperRequests(ResourceDictionary resources)
        {
            if (resources == null) throw new ArgumentNullException(nameof(resources));

            Dictionary<string, MapperRequestTemplate> result = new Dictionary<string, MapperRequestTemplate>();

            foreach (ResourceDictionary rd in resources.MergedDictionaries)
            {
                foreach (object objKey in rd.Keys)
                {
                    object rdEntry = rd[objKey];
                    string strKey = (string)objKey;

                    if (TryParse(rdEntry, strKey, out MapperRequestTemplate mr))
                    {
                        result.Add(strKey, mr);
                    }
                }
            }

            if (result.Count == 0)
            {
                result = null;
            }

            return result;
        }

        #endregion

        #region Private Methods

        private bool TryParse(object rdEntry, string resourceKey, out MapperRequestTemplate mr)
        {
            if (rdEntry is MapperRequestTemplate pbTemplateTest)
            {
                mr = pbTemplateTest;
                return true;
            }
            else
            {
                mr = null;
                return false;
            }
        }

        private string GetResourceKeyWithSuffix(string rawKey)
        {
            string result = ResourceKeySuffix != null ? $"{rawKey}_{ResourceKeySuffix}" : rawKey;
            return result;
        }

        #endregion
    }
}
