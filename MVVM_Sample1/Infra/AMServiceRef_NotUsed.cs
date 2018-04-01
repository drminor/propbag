using Swhp.Tspb.PropBagAutoMapperService;
using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;

namespace MVVM_Sample1.Infra
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
