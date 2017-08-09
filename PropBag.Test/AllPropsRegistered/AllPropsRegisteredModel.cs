﻿using System;
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

        new public void ClearEventSubscribers()
        {
            base.PClearEventSubscribers();
        }

    }
}
