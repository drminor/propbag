using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.TypeWrapper
{
    public interface ITypeDescriptionProvider
    {
        TypeDescription GetTypeDescription(NewTypeRequest newTypeRequest);

        TypeDescription GetTypeDescription(IPropModel propModel, Type typeToWrap, string className);
    }
}
