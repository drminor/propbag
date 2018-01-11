using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace PropBagLib.Tests
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class CreateAtRunTimeModel : PropBag
    {
  //      public CreateAtRunTimeModel() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered, null) { }

		//public CreateAtRunTimeModel(PropBagTypeSafetyMode typeSafetyMode) : this(typeSafetyMode, null) { }

        public CreateAtRunTimeModel(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAccessCreator,
            string fullClassName, IPropFactory propFactory)
            : base(typeSafetyMode, storeAccessCreator, propFactory, fullClassName) { }

        public CreateAtRunTimeModel(PropModel pm, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory)
            : base(pm, storeAccessCreator, propFactory, null)
        {

        }

        //public void RegisterProps(PropModel pm)
        //{
        //    IProp<string> p = ThePropFactory.Create<string>("First string.", "First");
        //    AddProp<string>("PropName", p);
        //}


    }
}
