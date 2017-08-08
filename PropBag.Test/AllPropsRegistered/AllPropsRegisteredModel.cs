using System;
using System.Collections.Generic;

using System.Reflection;

using DRM.PropBag;

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
        public new object this[string key]
        {
            get { return base[key]; }
            set { base[key] = value; }
        }

        // This may be used at some point.
        public new bool RegisterDoWhenChanged<T>(Action<T, T> doWhenUpdated, bool doAfterNotify, string propertyName)
        {
            return base.RegisterDoWhenChanged(doWhenUpdated, doAfterNotify, propertyName);
        }

        public void DoWhenStringChanged(string oldVal, string newVal)
        {
            DoWhenStringChanged_WasCalled = true;
            DoWhenStringPropOldVal = oldVal;
            DoWhenStringPropNewVal = newVal;
        }

        private void DoWhenNullIntChanged(Nullable<int> oldVal, Nullable<int> newVal)
        {
            DoWhenNullIntChanged_WasCalled = true;
        }

        private void DoWhenICollectionIntChanged(ICollection<int> oldVal, ICollection<int> newVal)
        {
            DoWhenICollectionIntChanged_WasCalled = true;
        }

    }
}
