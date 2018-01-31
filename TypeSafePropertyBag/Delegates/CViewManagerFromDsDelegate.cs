using System;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;

    public delegate IManageCViews CViewManagerFromDsDelegate(IPropBag target, PropNameType srcPropName, IMapperRequest mr);

    //public delegate IProvideACViewManager CViewManagerProviderFromDsDelegate(IPropBag target, LocalBindingInfo localBindingInfo, IMapperRequest mr);
    public delegate IProvideACViewManager CViewManagerProviderFromDsDelegate(IPropBag target, IViewManagerProviderKey viewManagerProviderKey);
}
