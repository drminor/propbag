using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using PropBagTestApp.Infra;
using System;
using System.Collections.Generic;

namespace PropBagTestApp.ViewModels
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public class ReferenceBindViewModelPB : PropBag
    {

        public ReferenceBindViewModelPB()
            : this (GetPropModel("ReferenceBindViewModelPB"))
        {
            System.Diagnostics.Debug.WriteLine
                (
                    "ReferenceBindViewModelPB is being created no params, " +
                    "but loaded using the PropModel and Type Factory."
                );
        }

        private static PropModelType GetPropModel(string fullClassName)
        {
            string fcn = PropStoreServicesForThisApp.GetResourceKeyWithSuffix(fullClassName, PropStoreServicesForThisApp.ConfigPackageNameSuffix);

            if (PropStoreServicesForThisApp.PropModelCache.TryGetPropModel(fcn, out PropModelType mainWindowPropModel))
            {
                return mainWindowPropModel;
            }
            else
            {
                throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fcn}.");
            }
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

