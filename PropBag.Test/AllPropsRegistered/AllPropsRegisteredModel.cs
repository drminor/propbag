using DRM.PropBag;
using DRM.TypeSafePropertyBag; using Swhp.Tspb.PropBagAutoMapperService;
using System;
using System.Collections.Generic;

namespace PropBagLib.Tests
{
    public partial class AllPropsRegisteredModel : PropBag
    {
        public bool DoWhenStringChanged_WasCalled { get; set; }
        public string DoWhenStringPropOldVal { get; set; }
        public string DoWhenStringPropNewVal { get; set; }

        public bool DoWhenNullIntChanged_WasCalled { get; set; }
        public bool DoWhenICollectionIntChanged_WasCalled { get; set; }

        // This is used to test adding a property that has not been registered via a call to AddProp
        //public new object this[string propertyName, string key]
        //{
        //    get { return base[propertyName, key]; }
        //    set { base[propertyName, key] = value; }
        //}

        public void DoWhenStringChanged(object sender, PcTypedEventArgs<string> e)
        {
            DoWhenStringChanged_WasCalled = true;
            DoWhenStringPropOldVal = e.OldValue;
            DoWhenStringPropNewVal = e.NewValue;
        }

        private void DoWhenNullIntChanged(object sender, PcTypedEventArgs<Nullable<int>> e)
        {
            DoWhenNullIntChanged_WasCalled = true;
        }

        private void DoWhenICollectionIntChanged(object sender, PcTypedEventArgs<ICollection<int>> e)
        {
            DoWhenICollectionIntChanged_WasCalled = true;
        }

    }
}
