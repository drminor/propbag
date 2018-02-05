using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using PropBagLib.Tests.BusinessModel;
using System;
using System.Collections.Generic;

namespace PropBagLib.Tests.PerformanceDb
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public partial class DestinationModel1 : PropBag
    {
        public DestinationModel1(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAccessCreator,
            string fullClassName, IPropFactory propFactory)
            : base(typeSafetyMode, storeAccessCreator, propFactory, fullClassName)
        {
            AddProp<int>("Id", null, null, initialValue: 0);
            AddProp<string>("FirstName", null, null, null);
            AddProp<string>("LastName", null, null, null);
            AddProp<string>("CityOfResidence", null, null, null);
            AddProp<Profession>("Profession", null, null, Profession.Default);
        }

        public DestinationModel1(PropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory, string fullClassName)
            : base(propModel, storeAccessCreator, propFactory, fullClassName)
        {
        }

        public DestinationModel1(DestinationModel1 copySource)
            : base(copySource)
        {
        }

        override public object Clone()
        {
            return new DestinationModel1(this);
        }
    }

    public partial class DestinationModel5 : PropBag
    {
        public DestinationModel5(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAccessCreator,
            string fullClassName, IPropFactory propFactory)
            : base(typeSafetyMode, storeAccessCreator, propFactory, fullClassName)
        {
            AddProp<Guid>("ProductId", null, null, Guid.NewGuid());
        }

        public DestinationModel5(PropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory, string fullClassName)
            : base(propModel, storeAccessCreator, propFactory, fullClassName)
        {
        }

        public DestinationModel5(DestinationModel5 copySource)
            : base(copySource)
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

        public DestinationModel6(PropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory, string fullClassName)
            : base(propModel, storeAccessCreator, propFactory, fullClassName)
        {
        }

        public DestinationModel6(DestinationModel6 copySource)
            : base(copySource)
        {
        }

    }

}
