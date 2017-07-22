﻿using System;

using System.Collections.Generic;
using System.ComponentModel;

using PropBagLib;
using System.Collections.ObjectModel;


namespace PropBagLib.Tests
{
    public partial class SetAndGetModel : PropBag
    {
        public SetAndGetModel(PropBagTypeSafetyModeEnum typeSafetyMode) : base(typeSafetyMode)
        {
            AddProp<object>("PropObject", initalValue:null);

            AddProp<bool>("PropBool", false);
        }
    }

    public partial class RefEqualityModel : PropBag
    {

        public bool ItGotUpdated = false;

        public RefEqualityModel(PropBagTypeSafetyModeEnum typeSafetyMode) : base(typeSafetyMode)
        {
            AddProp<string>("PropString", useReferenceEquality: true, doIfChanged: DoWhenUpdated, doAfterNotify: false, initalValue: "Test");
        }

        public void DoWhenUpdated(string oldVal, string newVal)
        {
            ItGotUpdated = true;
        }

    }

    public partial class NullableModel : PropBag
    {

        public bool ItGotUpdated = false;

        public NullableModel(PropBagTypeSafetyModeEnum typeSafetyMode)
            : base(typeSafetyMode)
        {
            AddProp<Nullable<int>>("PropNullableInt", doIfChanged: DoWhenUpdated, doAfterNotify: false, comparer: null, initalValue: new Nullable<int>());

            AddProp<ICollection<int>>("PropICollectionInt", doIfChanged: DoWhenUpdatedCol, doAfterNotify:false, comparer:null, initalValue: new Collection<int>());
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
        public SandGLoosetModel(PropBagTypeSafetyModeEnum typeSafetyMode) : base(typeSafetyMode)
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
