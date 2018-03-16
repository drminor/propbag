using System.Collections.Generic;
using System.Windows;
using System;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBagControlsWPF
{
    public class PropBagTemplateProvider : IPropBagTemplateProvider
    {
        #region Private Fields and Properties

        private ResourceDictionary _resources;


        // TODO: Consider parsing the entire resource file on first access.
        //private Dictionary<string, PropBagTemplate> _pbtsFromOurResources;
        //private Dictionary<string, PropBagTemplate> PbtsFromOurResources
        //{
        //    get
        //    {
        //        if (_pbtsFromOurResources == null)
        //        {
        //            _pbtsFromOurResources = GetPropBagTemplates(_resources);
        //        }
        //        return _pbtsFromOurResources;
        //    }
        //}

        #endregion

        #region Constructor

        public PropBagTemplateProvider()
        {
            _resources = null;
        }

        public PropBagTemplateProvider(ResourceDictionary resources/*, string resourceKeySuffix*/)
        {
            _resources = resources;
            //ResourceKeySuffix = resourceKeySuffix;
        }

        #endregion

        #region Public Properties

        //public string ResourceKeySuffix { get; }
        public bool CanFindPropBagTemplateWithJustAKey => _resources != null;

        #endregion

        #region Public Methods

        public PropBagTemplate GetPropBagTemplate(string resourceKey)
        {
            if (!CanFindPropBagTemplateWithJustAKey) throw new InvalidOperationException($"This instance of {nameof(PropBagTemplateProvider)} was not provide a ResourceDictionary.");
            return GetPropBagTemplate(_resources, resourceKey);
        }

        public PropBagTemplate GetPropBagTemplate(ResourceDictionary resources, string resourceKey)
        {
            if (resources == null) throw new ArgumentNullException(nameof(resources));
            if (resourceKey == null) throw new ArgumentNullException(nameof(resourceKey));

            //string cookedKey = GetResourceKeyWithSuffix(resourceKey);

            object resource;
            try
            {
                resource = resources[resourceKey];
            }
            catch (KeyNotFoundException knfe)
            {
                throw new KeyNotFoundException($"Could not find a PropBag Template with key = {resourceKey}.", knfe);
            }

            if (resource == null)
            {
                throw new InvalidOperationException($"Could not find a PropBag Template with key = {resourceKey}.");
            }

            if (TryParse(resource, resourceKey, out PropBagTemplate pbTemplate))
            {
                return pbTemplate;
            }
            else
            {
                throw new InvalidOperationException($"The PropBag Template with key = {resourceKey} could not be parsed.");
            }
        }

        public IDictionary<string, IPropBagTemplate> GetPropBagTemplates()
        {
            IDictionary<string, IPropBagTemplate> result = GetPropBagTemplates(_resources);
            return result;
        }

        public IDictionary<string, IPropBagTemplate> GetPropBagTemplates(ResourceDictionary resources)
        {
            if (resources == null) throw new ArgumentNullException(nameof(resources));

            Dictionary<string, IPropBagTemplate> result = new Dictionary<string, IPropBagTemplate>();

            // TODO: build an enumerator that walks the tree of resource dictionaries.
            //ResourceDictionary resources = System.Windows.Application.Current.Resources;

            foreach (ResourceDictionary rd in resources.MergedDictionaries)
            {
                foreach (object objKey in rd.Keys)
                {
                    object rdEntry = rd[objKey];
                    string strKey = (string)objKey;

                    if (TryParse(rdEntry, strKey, out PropBagTemplate pbTemplate))
                    {
                        result.Add(strKey, pbTemplate);
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

        private bool TryParse(object rdEntry, string resourceKey, out PropBagTemplate pbTemplate)
        {
            if (rdEntry is PropBagTemplate pbTemplateTest)
            {
                if (resourceKey != pbTemplateTest.ClassName && resourceKey != pbTemplateTest.FullClassName)
                {
                    System.Diagnostics.Debug.WriteLine("PropBagTemplate Resource Warning: ResourceKey does not match the ClassName or FullClassName.");
                }
                pbTemplate = pbTemplateTest;
                return true;
            }
            else
            {
                pbTemplate = null;
                return false;
            }
        }

        //private string GetResourceKeyWithSuffix(string rawKey)
        //{
        //    string result = ResourceKeySuffix != null ? $"{rawKey}_{ResourceKeySuffix}" : rawKey;
        //    return result;
        //}

        #endregion
    }
}
