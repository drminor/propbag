using DRM.PropBag.AutoMapperSupport;
using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;

namespace MVVMApplication.Infra
{
    public class AMServiceRef : IAMServiceRef
    {
        public AMServiceRef()
        {
        }

        public IProvideAutoMappers AutoMapperProvider => PropStoreServicesForThisApp.AutoMapperProvider;

        public SimpleExKey ExKey => throw new System.NotImplementedException();
    }
}
