using DRM.PropBag.ControlModel;
using System;

namespace DRM.TypeWrapper
{
    public interface ITypeDescriptionProvider
    {
        TypeDescription GetTypeDescription(NewTypeRequest newTypeRequest);

        TypeDescription GetTypeDescription(PropModel propModel, Type typeToWrap, string className);
    }
}
