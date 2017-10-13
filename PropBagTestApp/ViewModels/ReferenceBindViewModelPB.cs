using DRM.PropBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRM.PropBag.ControlModel;

namespace PropBagTestApp.ViewModels
{
    public class ReferenceBindViewModelPB : PropBagMin
    {

        public ReferenceBindViewModelPB() : base(PropBagTypeSafetyMode.HonorUndefined, SettingsExtensions.ThePropFactory)
        {
            System.Diagnostics.Debug.WriteLine("ReferenceBindViewModelPB is being created no params - with Type Factory.");
        }

        public ReferenceBindViewModelPB(byte dummy)
            : base(dummy)
        {
        }

        public ReferenceBindViewModelPB(PropModel pm, IPropFactory propFactory = null)
            : base(pm, propFactory)
        {

        }

        //public ReferenceBindViewModelPB(PropBagTypeSafetyMode typeSafetyMode)
        //    : base(typeSafetyMode)
        //{
        //}

        //public ReferenceBindViewModelPB(PropBagTypeSafetyMode typeSafetyMode, IPropFactory thePropFactory)
        //    : base(typeSafetyMode, thePropFactory)
        //{
        //}
    }
}

