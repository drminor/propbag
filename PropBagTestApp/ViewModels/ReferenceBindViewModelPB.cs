using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using PropBagTestApp.Infra;

namespace PropBagTestApp.ViewModels
{
    public class ReferenceBindViewModelPB : PropBag
    {

        public ReferenceBindViewModelPB()
            : this (PropStoreServicesForThisApp.PropModelProvider.GetPropModel("ReferenceBindViewModelPB"))
        {
            System.Diagnostics.Debug.WriteLine
                (
                    "ReferenceBindViewModelPB is being created no params, " +
                    "but loaded using the PropModel and Type Factory."
                );
        }

        //public ReferenceBindViewModelPB() { }

        //public ReferenceBindViewModelPB(byte dummy)
        //    : base(dummy)
        //{
        //}

        // TODO: AAA
        public ReferenceBindViewModelPB(IPropModel pm) : base(pm, null, null, null)
        {

        }
    }
}

