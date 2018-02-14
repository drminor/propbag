using DRM.PropBag.AutoMapperSupport;
using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM.Infra
{
    public class AMServiceRef : IAMServiceRef
    {
        public AMServiceRef()
        {
            System.Diagnostics.Debug.WriteLine("Here at the Constructor for AMServiceRef.");
        }

        // TODO:A-FixMe
        public IProvideAutoMappers AutoMapperProvider => null; // PropStoreServicesForThisApp.AutoMapperProvider;

        public SimpleExKey ExKey => new SimpleExKey(50, 2012);
    }
}
