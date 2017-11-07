using System;
using System.Collections.Generic;

using DRM.PropBag;
using DRM.TypeSafePropertyBag;

namespace PropBagLib.Tests
{
    public partial class LooseModel : PropBag
    {
        public bool DoWhenStringChanged_WasCalled { get; set; }
        public string DoWhenStringPropOldVal { get; set; }
        public string DoWhenStringPropNewVal { get; set; }

        //public new object this[string propertyName, string key]
        //{
        //    get { return base[propertyName, key]; }
        //    set { base[propertyName, key] = value; }
        //}

        //public IPropGen GetProp(string propertyName)
        //{
        //    return base.GetPropGen(propertyName, null, out bool wasRegistered,
        //        haveValue: false,
        //        value: null,
        //        alwaysRegister: false,
        //        mustBeRegistered:true,
        //        neverCreate:false,
        //        desiredHasStoreValue:true);
        //}

        public void DoWhenStringChanged(object sender, PropertyChangedWithTValsEventArgs<string> e)
        {
            DoWhenStringChanged_WasCalled = true;
            DoWhenStringPropOldVal = e.OldValue;
            DoWhenStringPropNewVal = e.NewValue;
        }

    }
}
