
using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace PropBagLib.Tests
{
    public partial class OnlyTypedAccessModel : PropBag
    {
        public bool DoWhenStringChanged_WasCalled { get; set; }
        public string DoWhenStringPropOldVal { get; set; }
        public string DoWhenStringPropNewVal { get; set; }

        public Nullable<int> NiOld { get; set; }
        public Nullable<int> NiNew { get; set; }

        public ICollection<int> IcOld { get; set; }
        public ICollection<int> IcNew { get; set; }

        //public OnlyTypedAccessModel(bool hookupDoWhenStringChanged, bool doAfterNotify)
        //    : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered)
        //{
        //    if(hookupDoWhenStringChanged)
        //    {
        //        RegisterDoWhenChanged<string>(DoWhenStringChanged, doAfterNotify, "PropString");
        //    }
        //}

        // This is used to test adding a property that has not been registered via a call to AddProp
        public new object this[string propertyName, string key]
        {
            get { return base[propertyName, key]; }
            set { base[propertyName, key] = value; }
        }

        public void DoWhenStringChanged(object sender, PCTypedEventArgs<string> e)
        {
            DoWhenStringChanged_WasCalled = true;
            DoWhenStringPropOldVal = e.OldValue;
            DoWhenStringPropNewVal = e.NewValue;
        }

        public void DoWhenNullIntChanged(object sender, PCTypedEventArgs<Nullable<int>> e)
        {
            DoWhenStringChanged_WasCalled = true;
            NiOld = e.OldValue;
            NiNew = e.NewValue;
        }

        public void DoWhenICollectionIntChanged(object sender, PCTypedEventArgs<ICollection<int>> e)
        {
            DoWhenStringChanged_WasCalled = true;
            IcOld = e.OldValue;
            IcNew = e.NewValue;
        }

    }
}
