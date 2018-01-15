using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace DRM.TypeWrapper.TypeDesc
{
    public class SimpleTypeDescriptionProvider : ITypeDescriptionProvider
    {
        private string _defaultNamespaceName { get; }

        public SimpleTypeDescriptionProvider()
        {
        }

        public TypeDescription GetTypeDescription(IPropModel propModel, Type typeToWrap, string className)
        {
            NewTypeRequest request = new NewTypeRequest(propModel, typeToWrap, className);

            TypeDescription result = GetTypeDescription(request);
            return result;
        }

        public TypeDescription GetTypeDescription(NewTypeRequest newTypeRequest)
        {
            string nsName = newTypeRequest.PropModel.NamespaceName;

            TypeName tn = new TypeName(newTypeRequest.TypeToWrap.Name, nsName);

            IEnumerable<PropertyDescription> propDescs = GetPropertyDescriptions(newTypeRequest.PropModel);

            TypeDescription result = new TypeDescription(tn, newTypeRequest.TypeToWrap, propDescs);

            return result;
        }

        private IEnumerable<PropertyDescription> GetPropertyDescriptions(IPropModel pm)
        {
            List<PropertyDescription> result = new List<PropertyDescription>();

            foreach (IPropItem pi in pm.Props)
            {
                result.Add(new PropertyDescription(pi.PropertyName, pi.PropertyType, canWrite: true));
            }
            return result;
        }
    }
}
