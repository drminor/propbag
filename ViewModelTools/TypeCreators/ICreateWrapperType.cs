using DRM.PropBag;
using DRM.PropBag.ControlModel;
using System;

namespace DRM.ViewModelTools
{
    public interface ICreateWrapperType
    {
        Type GetWrapperType(string resourceKey, Type typeToCreate);
        Type GetWrapperType(PropModel propModel, Type typeToCreate);

        Type GetWrapperType<BT>(string resourceKey) where BT : class, IPropBag;
        Type GetWrapperType<BT>(PropModel propModel) where BT : class, IPropBag;
    }
}
