
using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace PropBagLib.Tests
{
    public partial class AllPropsRegisteredModel : IPropBag
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

        public void DoWhenStringChanged(object sender, PCTypedEventArgs<string> e)
        {
            DoWhenStringChanged_WasCalled = true;
            DoWhenStringPropOldVal = e.OldValue;
            DoWhenStringPropNewVal = e.NewValue;
        }

        private void DoWhenNullIntChanged(object sender, PCTypedEventArgs<Nullable<int>> e)
        {
            DoWhenNullIntChanged_WasCalled = true;
        }

        private void DoWhenICollectionIntChanged(object sender, PCTypedEventArgs<ICollection<int>> e)
        {
            DoWhenICollectionIntChanged_WasCalled = true;
        }

        new public void ClearEventSubscribers()
        {
            base.ClearEventSubscribers();
        }

    }
}
