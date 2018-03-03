using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.TypeWrapper
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public interface ITypeDescriptionProvider
    {
        TypeDescription GetTypeDescription(NewTypeRequest newTypeRequest);

        TypeDescription GetTypeDescription(PropModelType propModel, Type typeToWrap, string className);
    }
}
