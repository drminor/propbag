using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using PropBagTestApp.Infra;
using System;

namespace PropBagTestApp.ViewModels
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public class ReferenceBindViewModelPB : PropBag
    {

        public ReferenceBindViewModelPB()
            : this (PropStoreServicesForThisApp.PropModelCache.GetPropModel("ReferenceBindViewModelPB"))
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
        public ReferenceBindViewModelPB(PropModelType pm) : base(pm, null, null, null)
        {

        }
    }
}

