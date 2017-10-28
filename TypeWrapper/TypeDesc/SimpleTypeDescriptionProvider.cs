using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRM.PropBag.ControlModel;

namespace DRM.TypeWrapper.TypeDesc
{
    public class SimpleTypeDescriptionProvider : ITypeDescriptionProvider
    {
        public const string DEFAULT_NAMESPACE_NAME = "PropBagWrappers";

        private string _defaultNamespaceName { get; }

        public SimpleTypeDescriptionProvider(string defaultNamespaceName = DEFAULT_NAMESPACE_NAME)
        {
            _defaultNamespaceName = defaultNamespaceName;
        }

        public TypeDescription GetTypeDescription(PropModel propModel, Type typeToWrap, string className)
        {
            NewTypeRequest request = new NewTypeRequest(propModel, typeToWrap, className);

            TypeDescription result = GetTypeDescription(request);
            return result;
        }

        public TypeDescription GetTypeDescription(NewTypeRequest newTypeRequest)
        {
            string nsName = newTypeRequest.PropModel.NamespaceName ?? DEFAULT_NAMESPACE_NAME;

            TypeName tn = new TypeName(newTypeRequest.TypeToWrap.Name, nsName);

            IEnumerable<PropertyDescription> propDescs = GetPropertyDescriptions(newTypeRequest.PropModel);

            TypeDescription result = new TypeDescription(tn, newTypeRequest.TypeToWrap, propDescs);

            return result;
        }

        private IEnumerable<PropertyDescription> GetPropertyDescriptions(PropModel pm)
        {
            List<PropertyDescription> result = new List<PropertyDescription>();

            foreach (PropItem pi in pm.Props)
            {
                result.Add(new PropertyDescription(pi.PropertyName, pi.PropertyType, canWrite: true));
            }
            return result;
        }
    }
}
