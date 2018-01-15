using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.ViewModelTools
{
    public interface ICreateWrapperTypes
    {
        Type GetWrapperType(string resourceKey, Type typeToCreate);
        Type GetWrapperType(IPropModel propModel, Type typeToCreate);

        Type GetWrapperType<BT>(string resourceKey) where BT : class, IPropBag;
        Type GetWrapperType<BT>(IPropModel propModel) where BT : class, IPropBag;
    }
}
