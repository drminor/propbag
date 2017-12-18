using System.Collections.Generic;
using System.Windows;

namespace DRM.PropBag.ControlsWPF
{
    public class PropBagTemplateProvider : IPropBagTemplateProvider
    {
        #region Private Fields and Properties

        private ResourceDictionary _resources;

        Dictionary<string, PropBagTemplate> _pbtsFromOurResources;
        Dictionary<string, PropBagTemplate> PbtsFromOurResources
        {
            get
            {
                if (_pbtsFromOurResources == null)
                {
                    _pbtsFromOurResources = GetPropBagTemplates(_resources);
                }
                return _pbtsFromOurResources;
            }
        }

        Dictionary<string, MapperRequest> _mrFromOurResources;
        Dictionary<string, MapperRequest> MrFromOurResources
        {
            get
            {
                if (_mrFromOurResources == null)
                {
                    _mrFromOurResources = GetMapperRequests(_resources);
                }
                return _mrFromOurResources;
            }
        }

        #endregion

        #region Public Properties

        public bool CanFindPropBagTemplateWithJustKey => _resources != null;

        #endregion

        #region Constructor

        public PropBagTemplateProvider()
        {
            _resources = null;
        }

        public PropBagTemplateProvider(ResourceDictionary resources)
        {
            _resources = resources;
        }

        #endregion

        public PropBagTemplate GetPropBagTemplate(string resourceKey)
        {
            return GetPropBagTemplate(PbtsFromOurResources, resourceKey);
        }

        public PropBagTemplate GetPropBagTemplate(ResourceDictionary resources, string resourceKey)
        {
            Dictionary<string, PropBagTemplate> pbts = null;

            try
            {
                pbts = GetPropBagTemplates(resources);
            }
            catch
            {
                if (pbts == null)
                {
                    System.Diagnostics.Debug.WriteLine($"PropBagTemplates was not populated while trying to fetch the PropBagTemplate for {resourceKey}");
                }
                throw;
            }

            PropBagTemplate result = GetPropBagTemplate(pbts, resourceKey);
            return result;
        }

        private PropBagTemplate GetPropBagTemplate(Dictionary<string, PropBagTemplate> pbts, string resourceKey)
        {
            try
            {
                PropBagTemplate result = pbts[resourceKey];
                return result;
            }
            catch
            {
                if (pbts == null)
                {
                    System.Diagnostics.Debug.WriteLine($"PropBagTemplates was not populated while trying to fetch the PropBagTemplate for {resourceKey}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to find PropBagTemplate with key = {resourceKey}");
                }
                throw;
            }
        }

        public Dictionary<string, PropBagTemplate> GetPropBagTemplates(ResourceDictionary resources)
        {
            Dictionary<string, PropBagTemplate> result = new Dictionary<string, PropBagTemplate>();

            // TODO: build an enumerator that walks the tree of resource dictionaries.
            //ResourceDictionary resources = System.Windows.Application.Current.Resources;

            foreach (ResourceDictionary rd in resources.MergedDictionaries)
            {
                foreach (object objKey in rd.Keys)
                {
                    object rdEntry = rd[objKey];

                    if (rdEntry is PropBagTemplate pbTemplate)
                    {
                        string strKey = (string)objKey;

                        if(strKey != pbTemplate.ClassName && strKey != pbTemplate.FullClassName)
                        {
                            System.Diagnostics.Debug.WriteLine("PropBagTemplate Resource Warning: ResourceKey does not match the ClassName or FullClassName.");
                        }

                        result.Add(strKey, pbTemplate);
                    }
                }
            }

            if(result.Count == 0)
            {
                result = null;
            }

            return result;
        }

        public Dictionary<string, MapperRequest> GetMapperRequests(ResourceDictionary resources)
        {
            Dictionary<string, MapperRequest> result = new Dictionary<string, MapperRequest>();

            // TODO: build an enumerator that walks the tree of resource dictionaries.
            //ResourceDictionary resources = System.Windows.Application.Current.Resources;

            foreach (ResourceDictionary rd in resources.MergedDictionaries)
            {
                foreach (object objKey in rd.Keys)
                {
                    object rdEntry = rd[objKey];

                    if (rdEntry is MapperRequest mr)
                    {
                        result.Add((string) objKey, mr);
                    }
                }
            }

            if (result.Count == 0)
            {
                result = null;
            }

            return result;
        }

        public MapperRequest GetMapperRequest(string resourceKey)
        {
            return GetMapperRequest(MrFromOurResources, resourceKey);
        }

        public MapperRequest GetMapperRequest(ResourceDictionary resources, string resourceKey)
        {
            Dictionary<string, MapperRequest> mrRequests = null;

            try
            {
                mrRequests = GetMapperRequests(resources);
            }
            catch
            {
                if (mrRequests == null)
                {
                    System.Diagnostics.Debug.WriteLine($"MapperRequests was not populated while trying to fetch the MapperRequest for {resourceKey}");
                }
                throw;
            }

            MapperRequest result = GetMapperRequest(mrRequests, resourceKey);
            return result;
        }

        private MapperRequest GetMapperRequest(Dictionary<string, MapperRequest> mrRequests, string resourceKey)
        {
            try
            {
                MapperRequest result = mrRequests[resourceKey];
                return result;
            }
            catch
            {
                if (mrRequests == null)
                {
                    System.Diagnostics.Debug.WriteLine($"MapperRequests was not populated while trying to fetch the MapperRequest for {resourceKey}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to find MapperRequest with key = {resourceKey}");
                }
                throw;
            }
        }
    }
}
