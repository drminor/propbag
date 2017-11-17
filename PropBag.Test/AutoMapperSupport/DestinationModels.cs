using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace PropBagLib.Tests.AutoMapperSupport
{
    public partial class DestinationModel3 : PropBag
    {

        public DestinationModel3(PropBagTypeSafetyMode typeSafetyMode, IPropFactory propFactory, string fullClassName)
            : base(typeSafetyMode, propFactory, fullClassName)
		{
            AddProp<Guid>("ProductId", null, false, null, null, Guid.NewGuid());
            AddProp<int>("Amount", null, false, null, null, 0);
            AddProp<double>("Size", null, false, null, null, 10.1);
            AddProp<MyModel4>("Deep", null, false, null, null, null);
        }

        public DestinationModel3(PropModel propModel, string fullClassName, IPropFactory propFactory)
            : base(propModel, fullClassName, propFactory)
        {
        }

    }
}
