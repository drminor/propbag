using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;
using System.Reflection;

namespace PropBagLib.Tests
{
    public class CreateAtRunTimeModel : PropBag
    {

        public CreateAtRunTimeModel() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered, null) { }

		public CreateAtRunTimeModel(PropBagTypeSafetyMode typeSafetyMode) : this(typeSafetyMode, null) { }

        public CreateAtRunTimeModel(PropBagTypeSafetyMode typeSafetyMode, IPropFactory factory)
            : base(typeSafetyMode, factory) { }

        public CreateAtRunTimeModel(PropModel pm) : base(pm)
        {

        }

        //public void RegisterProps(PropModel pm)
        //{
        //    IProp<string> p = ThePropFactory.Create<string>("First string.", "First");
        //    AddProp<string>("PropName", p);
        //}


    }
}
