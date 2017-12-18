using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;

namespace PropBagLib.Tests
{
    public class CreateAtRunTimeModel : PropBag
    {

        public CreateAtRunTimeModel() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered, null) { }

		public CreateAtRunTimeModel(PropBagTypeSafetyMode typeSafetyMode) : this(typeSafetyMode, null) { }

        public CreateAtRunTimeModel(PropBagTypeSafetyMode typeSafetyMode, IPropFactory factory)
            : base(typeSafetyMode, factory) { }

        public CreateAtRunTimeModel(PropModel pm, IPropFactory propFactory) : base(pm, null, propFactory)
        {

        }

        //public void RegisterProps(PropModel pm)
        //{
        //    IProp<string> p = ThePropFactory.Create<string>("First string.", "First");
        //    AddProp<string>("PropName", p);
        //}


    }
}
