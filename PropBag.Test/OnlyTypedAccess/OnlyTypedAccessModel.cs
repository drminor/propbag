using System;
using System.Collections.Generic;

using DRM.PropBag;

namespace PropBagLib.Tests
{
    public partial class OnlyTypedAccessModel : DRM.PropBag.PropBag
    {
        public bool DoWhenStringChanged_WasCalled { get; set; }
        public string DoWhenStringPropOldVal { get; set; }
        public string DoWhenStringPropNewVal { get; set; }

        //public OnlyTypedAccessModel(bool hookupDoWhenStringChanged, bool doAfterNotify)
        //    : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered)
        //{
        //    if(hookupDoWhenStringChanged)
        //    {
        //        RegisterDoWhenChanged<string>(DoWhenStringChanged, doAfterNotify, "PropString");
        //    }
        //}

        // This is used to test adding a property that has not been registered via a call to AddProp
        public new object this[string key]
        {
            get { return base[key]; }
            set { base[key] = value; }
        }

        public void DoWhenStringChanged(string oldVal, string newVal)
        {
            DoWhenStringChanged_WasCalled = true;
            DoWhenStringPropOldVal = oldVal;
            DoWhenStringPropNewVal = newVal;
        }

    }
}
