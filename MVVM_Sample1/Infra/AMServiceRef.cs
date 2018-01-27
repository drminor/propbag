using DRM.PropBag.AutoMapperSupport;
using DRM.PropBagControlsWPF;

namespace MVVMApplication.Infra
{
    public class AMServiceRef : IAMServiceRef
    {
        public AMServiceRef()
        {
        }

        public IProvideAutoMappers AutoMapperProvider => PropStoreServicesForThisApp.AutoMapperProvider;
    }
}
