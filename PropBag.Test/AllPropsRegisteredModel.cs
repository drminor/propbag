using System;
using System.Collections.Generic;

using DRM.PropBag;

namespace PropBagLib.Tests
{
    public partial class AllPropsRegisteredModel : DRM.PropBag.PropBag
    {
        public bool DoWhenStringChanged_WasCalled { get; set; }
        public string DoWhenStringPropOldVal { get; set; }
        public string DoWhenStringPropNewVal { get; set; }

        public bool DoWhenNullIntChanged_WasCalled { get; set; }
        public bool DoWhenICollectionIntChanged_WasCalled { get; set; }

        public AllPropsRegisteredModel(bool hookupDoWhenStringChanged, bool doAfterNotify) : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered)
        {
            if(hookupDoWhenStringChanged)
            {
                RegisterDoWhenChanged<string>(DoWhenStringChanged, doAfterNotify, "PropString");

                // Use the same hander for PropStringObjComp
                RegisterDoWhenChanged<string>(DoWhenStringChanged, doAfterNotify, "PropStringUseRefComp");

                RegisterDoWhenChanged<Nullable<int>>(DoWhenNullIntChanged, false, "PropNullableInt");

                RegisterDoWhenChanged<ICollection<int>>(DoWhenICollectionIntChanged, false, "PropICollectionInt");
            }
        }

        //public bool RegisterDoWhenUpdated<T>(Action<T, T> doWhenUpdated, bool doAfterNotify, string propertyName)
        //{
        //    return this.RegisterDoWhenChanged(doWhenUpdated, doAfterNotify, propertyName);
        //}

        public void DoWhenStringChanged(string oldVal, string newVal)
        {
            DoWhenStringChanged_WasCalled = true;
            DoWhenStringPropOldVal = oldVal;
            DoWhenStringPropNewVal = newVal;
        }

        public void DoWhenNullIntChanged(Nullable<int> oldVal, Nullable<int> newVal)
        {
            DoWhenNullIntChanged_WasCalled = true;
        }

        public void DoWhenICollectionIntChanged(ICollection<int> oldVal, ICollection<int> newVal)
        {
            DoWhenICollectionIntChanged_WasCalled = true;
        }


    }
}
