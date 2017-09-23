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

        public new object this[string propertyName, string key]
        {
            get { return base[propertyName, key]; }
            set { base[propertyName, key] = value; }
        }

        public IPropGen GetProp(string propertyName)
        {
            return base.GetGenProp(propertyName, null, out bool wasRegistered,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustRegister:true,
                desiredHasStoreValue:ThePropFactory.ProvidesStorage);
        }

        public void DoWhenStringChanged(string oldVal, string newVal)
        {
            DoWhenStringChanged_WasCalled = true;
            DoWhenStringPropOldVal = oldVal;
            DoWhenStringPropNewVal = newVal;
        }

    }
}
