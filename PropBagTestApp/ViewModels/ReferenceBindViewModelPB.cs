using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using PropBagTestApp.Infra;

namespace PropBagTestApp.ViewModels
{
    public class ReferenceBindViewModelPB : PropBag
    {

        public ReferenceBindViewModelPB()
            : this (JustSayNo.PropModelProvider.GetPropModel("ReferenceBindViewModelPB"))
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
        public ReferenceBindViewModelPB(PropModel pm) : base(pm, null, null, null)
        {

        }
    }
}

