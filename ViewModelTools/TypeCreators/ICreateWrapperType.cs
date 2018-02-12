using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.ViewModelTools
{
    public interface ICreateWrapperTypes
    {
        // This could easily be re-instated.
        //Type GetWrapperType(string resourceKey, Type typeToCreate);
        Type GetWrapperType(IPropModel propModel, Type typeToCreate);

        // This could easily be re-instated.
        //Type GetWrapperType<BT>(string resourceKey) where BT : class, IPropBag;
        Type GetWrapperType<BT>(IPropModel propModel) where BT : class, IPropBag;

        long ClearTypeCache();
    }
}
