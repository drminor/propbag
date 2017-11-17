using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using PropBagLib.Tests.BusinessModel;
using System;

namespace PropBagLib.Tests.PerformanceDb
{
    public partial class DestinationModel1 : PropBag
    {
        public DestinationModel1(PropBagTypeSafetyMode typeSafetyMode, IPropFactory propFactory, string fullClassName)
            : base(typeSafetyMode, propFactory, fullClassName)
        {
            AddProp<int>("Id", null, false, null, null, 0);
            AddProp<string>("FirstName", null, false, null, null, null);
            AddProp<string>("LastName", null, false, null, null, null);
            AddProp<string>("CityOfResidence", null, false, null, null, null);
            AddProp<Profession>("Profession", null, false, null, null, Profession.Default);
        }

        public DestinationModel1(PropModel propModel, string fullClassName, IPropFactory propFactory)
            : base(propModel, fullClassName, propFactory)
        {
        }

    }

    public partial class DestinationModel5 : PropBag
    {
        public DestinationModel5(PropBagTypeSafetyMode typeSafetyMode, IPropFactory propFactory, string fullClassName)
            : base(typeSafetyMode, propFactory, fullClassName)
        {
            AddProp<Guid>("ProductId", null, false, null, null, Guid.NewGuid());
        }

        public DestinationModel5(PropModel propModel, string fullClassName, IPropFactory propFactory)
            : base(propModel, fullClassName, propFactory)
        {
        }

    }

}
