using DRM.PropBag.ControlModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ViewModelBuilder
{
    public static class VMBuilderPropModelExtensions
    {

        //public static Dictionary<string, Type> GetPropertyDefs(this PropModel pm)
        //{
        //    Dictionary<string, Type> result = new Dictionary<string, Type>();
        //    foreach (PropItem pi in pm.Props)
        //    {
        //        result.Add(pi.PropertyName, pi.PropertyType);
        //    }
        //    return result;
        //}

        public static IEnumerable<PropertyDescription> GetPropertyDescriptions(this PropModel pm)
        {
            List<PropertyDescription> result = new List<PropertyDescription>();

            foreach (PropItem pi in pm.Props)
            {
                result.Add(new PropertyDescription(pi.PropertyName, pi.PropertyType, canWrite:true));
            }
            return result;
        }

        //public static IEnumerable<PropertyDescription> GetPropertyDescriptions(IEnumerable<KeyValuePair<string, Type>> propDefs)
        //{
        //    List<PropertyDescription> result = new List<PropertyDescription>();

        //    foreach (KeyValuePair<string, Type> kvp in propDefs)
        //    {
        //        result.Add(new PropertyDescription(kvp.Key, kvp.Value));
        //    }
        //    return result;
        //}

    }
}
