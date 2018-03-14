using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;
using System;

namespace MVVM_Sample1.Infra
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
