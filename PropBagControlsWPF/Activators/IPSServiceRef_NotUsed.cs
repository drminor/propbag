using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBagControlsWPF
{
    using PSServiceSingletonProviderInterface = IProvidePropStoreServiceSingletons<UInt32, String>;

    public interface IPSServiceRef
    {
        PSServiceSingletonProviderInterface PropStoreServices { get; }
    }
}