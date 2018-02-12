using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;
using System;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM.Infra
{
    using PSServiceSingletonProviderInterface = IProvidePropStoreServiceSingletons<UInt32, String>;

    public class PSServiceRef : IPSServiceRef
    {
        public PSServiceRef()
        {
        }

        // TODO:A-FixMe
        public PSServiceSingletonProviderInterface PropStoreServices => null; // PropStoreServicesForThisApp.PropStoreServices;
    }
}
