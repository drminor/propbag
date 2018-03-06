using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.TypeWrapper.TypeDesc
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public class SimpleTypeDescriptionProvider : ITypeDescriptionProvider
    {
        private string _defaultNamespaceName { get; }

        public SimpleTypeDescriptionProvider()
        {
        }

        public TypeDescription GetTypeDescription(PropModelType propModel, Type typeToWrap, string fullClassName)
        {
            NewTypeRequest request = new NewTypeRequest(propModel, typeToWrap, fullClassName);

            TypeDescription result = GetTypeDescription(request);
            return result;
        }

        public TypeDescription GetTypeDescription(NewTypeRequest newTypeRequest)
        {
            //string nsName = newTypeRequest.PropModel.NamespaceName;

            // TODO: Consider using the fullClassName: newTypeRequest.FullClassName.
            //TypeName tn = new TypeName(newTypeRequest.TypeToWrap.Name, nsName);

            TypeName tn = new TypeName(newTypeRequest.FullClassName);

            IEnumerable<PropertyDescription> propDescs = GetPropertyDescriptions(newTypeRequest.PropModel);

            TypeDescription result = new TypeDescription(tn, newTypeRequest.TypeToWrap, propDescs);

            return result;
        }

        private IEnumerable<PropertyDescription> GetPropertyDescriptions(PropModelType pm)
        {
            List<PropertyDescription> result = new List<PropertyDescription>();

            foreach (IPropModelItem pi in pm.GetPropItems())
            {
                result.Add(new PropertyDescription(pi.PropertyName, pi.PropertyType, canWrite: true));
            }
            return result;
        }
    }
}
