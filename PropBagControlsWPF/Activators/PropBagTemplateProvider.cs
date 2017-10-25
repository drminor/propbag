using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DRM.PropBag.ControlsWPF.WPFHelpers
{
    public class PropBagTemplateProvider : IPropBagTemplateProvider
    {
        private ResourceDictionary _resources;

        public bool HasPbtLookupResources => true;
        public bool CanFindPropBagTemplateWithJustKey => _resources != null;
        
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
            return this.GetPropBagTemplate(_resources, resourceKey);
        }

        public PropBagTemplate GetPropBagTemplate(ResourceDictionary resources, string resourceKey)
        {
            Dictionary<string, PropBagTemplate> pbts = null;

            try
            {
                pbts = GetPropBagTemplates(resources);
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

                    if (rdEntry is PropBagTemplate)
                    {
                        PropBagTemplate pbTemplate = rdEntry as PropBagTemplate;

                        string strKey = (string)objKey;

                        if(strKey != pbTemplate.ClassName && strKey != pbTemplate.FullClassName)
                        {
                            System.Diagnostics.Debug.WriteLine("PropBagTemplate Resource Warning: ResourceKey does not match the ClassName or FullClassName.");
                        }

                        result.Add(strKey, pbTemplate);
                    }
                }
            }

            return result;
        }
    }
}
