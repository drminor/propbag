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
        public SetAndGetModel() : this(PropBagTypeSafetyMode.Loose) { }

        public SetAndGetModel(PropBagTypeSafetyMode typeSafetyMode) : base(typeSafetyMode)
        {
            AddProp<object>("PropObject", initalValue:null);

            AddProp<bool>("PropBool", false);
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

        public new object this[string key]
        {
            get { return base[key]; }
            set { base[key] = value; }
        }


    }



}
