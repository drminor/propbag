using DRM.PropBag.AutoMapperSupport;
using DRM.PropBagControlsWPF;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM.Infra
{
    public class AMServiceRef : IAMServiceRef
    {
        public AMServiceRef()
        {
        }

        // TODO:A-FixMe
        public IProvideAutoMappers AutoMapperProvider => null; // PropStoreServicesForThisApp.AutoMapperProvider;
    }
}
