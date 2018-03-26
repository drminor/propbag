using System;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public interface ICreateWrapperTypes
    {
        Type GetWrapperType(PropModelType propModel, Type typeToCreate);
        Type GetWrapperType<BT>(PropModelType propModel) where BT : class, IPropBag;
        long ClearTypeCache();
    }
}
