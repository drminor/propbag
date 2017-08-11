using System;
using System.Collections.Generic;

using DRM.PropBag;

namespace PropBagLib.Tests
{
    public partial class LooseModel : PropBag
    {
        public bool DoWhenStringChanged_WasCalled { get; set; }
        public string DoWhenStringPropOldVal { get; set; }
        public string DoWhenStringPropNewVal { get; set; }

        public new object this[string key]
        {
            get { return base[key]; }
            set { base[key] = value; }
        }

        public IPropGen GetProp(string propertyName)
        {
            return base.GetGenProp(propertyName, null);
        }

        public void DoWhenStringChanged(string oldVal, string newVal)
        {
            DoWhenStringChanged_WasCalled = true;
            DoWhenStringPropOldVal = oldVal;
            DoWhenStringPropNewVal = newVal;
        }

    }
}
