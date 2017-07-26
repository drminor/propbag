using System;

using System.Collections.Generic;
using System.ComponentModel;

using DRM.PropBag;
//using PropBagLib;
using System.Collections.ObjectModel;


namespace PropBagLib.Tests
{
    public partial class SetAndGetModel : PropBag
    {
        public SetAndGetModel(PropBagTypeSafetyMode typeSafetyMode) : base(typeSafetyMode)
        {
            AddProp<object>("PropObject", initalValue:null);

            AddProp<bool>("PropBool", false);
        }
    }

    public partial class RefEqualityModel : PropBag
    {

        public bool ItGotUpdated = false;

        public RefEqualityModel(PropBagTypeSafetyMode typeSafetyMode) : base(typeSafetyMode)
        {
            AddPropObjComp<string>("PropString", doIfChanged: DoWhenUpdated, doAfterNotify: false, comparer: null, initalValue: "Test");
        }

        public void DoWhenUpdated(string oldVal, string newVal)
        {
            ItGotUpdated = true;
        }

    }

    public partial class NullableModel : PropBag
    {

        public bool ItGotUpdated = false;

        public NullableModel(PropBagTypeSafetyMode typeSafetyMode)
            : base(typeSafetyMode)
        {
            AddProp<Nullable<int>>("PropNullableInt", doIfChanged: DoWhenUpdated, doAfterNotify: false, comparer: null, initalValue: new Nullable<int>());

            AddPropObjComp<ICollection<int>>("PropICollectionInt", doIfChanged: DoWhenUpdatedCol, doAfterNotify: false, comparer: null, initalValue: new Collection<int>());
        }

        public void DoWhenUpdated(Nullable<int> oldVal, Nullable<int> newVal)
        {
            ItGotUpdated = true;
        }

        public void DoWhenUpdatedCol(ICollection<int> oldVal, ICollection<int> newVal)
        {
            ItGotUpdated = true;
        }

    }

    public partial class SandGLoosetModel : PropBag
    {
        public SandGLoosetModel(PropBagTypeSafetyMode typeSafetyMode)
            : base(typeSafetyMode)
        {
            AddProp<object>("PropObject", initalValue: null);

            AddProp<string>("PropString");

            AddProp<bool>("PropBool", false);

            AddProp<int>("PropInt");

            AddProp<TimeSpan>("PropTimeSpan");

            AddProp<Uri>("PropUri");

            AddProp<Lazy<int>>("PropLazyInt");
        }


    }

}
