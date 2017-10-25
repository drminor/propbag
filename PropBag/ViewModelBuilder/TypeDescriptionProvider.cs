using DRM.PropBag.ControlModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ViewModelBuilder
{
    public class TypeDescriptionProvider
    {
        public const string DEFAULT_NAMESPACE_NAME = "PropBagWrappers";

        private string _defaultNamespaceName { get; }

        public TypeDescriptionProvider(string defaultNamespaceName = DEFAULT_NAMESPACE_NAME)
        {
            _defaultNamespaceName = defaultNamespaceName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtViewModelType">The type (usually known at compile-time for which the proxy is being created.</param>
        /// <param name="namespaceName">The namespace to use when creating the proxy.</param>
        /// <param name="pm">The propmodel from which to get the list of properties to define for the proxy.</param>
        /// <returns></returns>
        public TypeDescription BuildTypeDesc(PropModel pm, Type dtViewModelType, string namespaceName = null)
        {
            string nsName = namespaceName ?? DEFAULT_NAMESPACE_NAME;

            TypeName tn = new TypeName(dtViewModelType.Name, nsName);

            IEnumerable<PropertyDescription> propDescs = GetPropertyDescriptions(pm);

            TypeDescription td = new TypeDescription(tn, dtViewModelType, propDescs);
            return td;
        }

        public IEnumerable<PropertyDescription> GetPropertyDescriptions(PropModel pm)
        {
            List<PropertyDescription> result = new List<PropertyDescription>();

            foreach (PropItem pi in pm.Props)
            {
                result.Add(new PropertyDescription(pi.PropertyName, pi.PropertyType, canWrite: true));
            }
            return result;
        }

        //public IEnumerable<PropertyDescription> GetPropertyDescriptions(IEnumerable<KeyValuePair<string, Type>> propDefs)
        //{
        //    List<PropertyDescription> result = new List<PropertyDescription>();

        //    foreach (KeyValuePair<string, Type> kvp in propDefs)
        //    {
        //        result.Add(new PropertyDescription(kvp.Key, kvp.Value));
        //    }
        //    return result;
        //}

        //public Dictionary<string, Type> GetPropertyDefs(PropModel pm)
        //{
        //    Dictionary<string, Type> result = new Dictionary<string, Type>();
        //    foreach (PropItem pi in pm.Props)
        //    {
        //        result.Add(pi.PropertyName, pi.PropertyType);
        //    }
        //    return result;
        //}
    }
}
