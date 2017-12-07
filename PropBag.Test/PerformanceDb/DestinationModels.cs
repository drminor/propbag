using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using PropBagLib.Tests.BusinessModel;
using System;
using System.Collections.Generic;

namespace PropBagLib.Tests.PerformanceDb
{
    public partial class DestinationModel1 : PropBag, ICloneable
    {
        public DestinationModel1(PropBagTypeSafetyMode typeSafetyMode, IPropFactory propFactory, string fullClassName)
            : base(typeSafetyMode, propFactory, fullClassName)
        {
            AddProp<int>("Id", null, null, 0);
            AddProp<string>("FirstName", null, null, null);
            AddProp<string>("LastName", null, null, null);
            AddProp<string>("CityOfResidence", null, null, null);
            AddProp<Profession>("Profession", null, null, Profession.Default);
        }

        public DestinationModel1(PropModel propModel, string fullClassName, IPropFactory propFactory)
            : base(propModel, fullClassName, propFactory)
        {
        }

        public DestinationModel1(DestinationModel1 copySource)
            : base(copySource)
        {
        }

        new public object Clone()
        {
            return new DestinationModel1(this);
        }
    }

    public partial class DestinationModel5 : PropBag
    {
        public DestinationModel5(PropBagTypeSafetyMode typeSafetyMode, IPropFactory propFactory, string fullClassName)
            : base(typeSafetyMode, propFactory, fullClassName)
        {
            AddProp<Guid>("ProductId", null, null, Guid.NewGuid());
        }

        public DestinationModel5(PropModel propModel, string fullClassName, IPropFactory propFactory)
            : base(propModel, fullClassName, propFactory)
        {
        }

    }

    public partial class DestinationModel6 : PropBag
    {
        //public DestinationModel6(PropBagTypeSafetyMode typeSafetyMode, IPropFactory propFactory, string fullClassName)
        //    : base(typeSafetyMode, propFactory, fullClassName)
        //{
        //    AddProp<Guid>("ProductId", null, false, null, null, Guid.NewGuid());
        //}

        public DestinationModel6(PropModel propModel, string fullClassName, IPropFactory propFactory)
            : base(propModel, fullClassName, propFactory)
        {
        }

    }

}
