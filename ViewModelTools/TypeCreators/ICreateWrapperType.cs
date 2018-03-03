using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.ViewModelTools
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public interface ICreateWrapperTypes
    {
        // This could easily be re-instated.
        //Type GetWrapperType(string resourceKey, Type typeToCreate);
        Type GetWrapperType(PropModelType propModel, Type typeToCreate);

        // This could easily be re-instated.
        //Type GetWrapperType<BT>(string resourceKey) where BT : class, IPropBag;
        Type GetWrapperType<BT>(PropModelType propModel) where BT : class, IPropBag;

        long ClearTypeCache();
    }
}
