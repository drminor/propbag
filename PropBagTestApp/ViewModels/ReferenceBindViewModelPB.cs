using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;

namespace PropBagTestApp.ViewModels
{
    public class ReferenceBindViewModelPB : PropBag
    {

        public ReferenceBindViewModelPB() : this
            (
                JustSayNo.PropModelProvider.GetPropModel("ReferenceBindViewModelPB"),
                JustSayNo.ThePropFactory
            )
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

        public ReferenceBindViewModelPB(PropModel pm, IPropFactory propFactory)
            : base(pm, propFactory)
        {

        }
    }
}

