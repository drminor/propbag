using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DRM.PropBag.ControlsWPF.WPFHelpers
{
    public static class PropBagTemplateResources
    {
        public static Dictionary<string, PropBagTemplate> PropBagTemplates { get; }

        // TODO: Make this Lazy.
        static PropBagTemplateResources()
        {
            PropBagTemplates = GetPropBagTemplates();
        }


        public static PropBagTemplate GetPropBagTemplate(string instanceKey)
        {
            try
            {
                PropBagTemplate result = PropBagTemplates[instanceKey];
                //if(result.ClassName != instanceKey && result.)
                return result;
            }
            catch
            {
                if (PropBagTemplates == null)
                {
                    System.Diagnostics.Debug.WriteLine($"PropBagTemplates was not populated while trying to fetch the PropBagTemplate for {instanceKey}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to find PropBagTemplate with key = {instanceKey}");
                }
                throw;
            }
        }

        public static Dictionary<string, PropBagTemplate> GetPropBagTemplates()
        {
            Dictionary<string, PropBagTemplate> result = new Dictionary<string, PropBagTemplate>();

            // TODO: build an enumerator that walks the tree of resource dictionaries.
            ResourceDictionary resources = System.Windows.Application.Current.Resources;

            foreach (ResourceDictionary rd in resources.MergedDictionaries)
            {
                foreach (object objKey in rd.Keys)
                {
                    object rde = rd[objKey];

                    if (rde is PropBagTemplate)
                    {
                        PropBagTemplate pbTemplate = rde as PropBagTemplate;

                        string strKey = (string)objKey;

                        if(strKey != pbTemplate.InstanceKey && strKey != pbTemplate.ClassName)
                        {
                            System.Diagnostics.Debug.WriteLine("PBTemplate Resource Warning: ResourceKey does not match the ClassName or the InstanceKey.");
                        }
                        // Add using the Instance Key
                        result.Add(pbTemplate.InstanceKey, pbTemplate);
                    }
                }
            }

            return result;
        }
    }
}
