using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;
using System;

namespace MVVMApplication.Infra
{
    using PSServiceSingletonProviderInterface = IProvidePropStoreServiceSingletons<UInt32, String>;

    public class PSServiceRef : IPSServiceRef
    {
        public PSServiceRef()
        {
        }

        public PSServiceSingletonProviderInterface PropStoreServices => PropStoreServicesForThisApp.PropStoreServices;
    }
}
