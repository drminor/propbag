using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace PropBagLib.Tests.AutoMapperSupport
{
    public partial class DestinationModel : PropBag
    {
        public DestinationModel()
            : this(PropBagTypeSafetyMode.Tight)
        {
        }

        public DestinationModel(PropBagTypeSafetyMode typeSafetyMode)
            : base(typeSafetyMode)
		{
            AddProp<Guid>("ProductId", null, false, null, null, Guid.NewGuid());
            AddProp<int>("Amount", null, false, null, null, 0);
            AddProp<double>("Size", null, false, null, null, 10.1);
            AddProp<MyModel4>("Deep", null, false, null, null, null);
        }

        public DestinationModel(PropModel propModel, string fullClassName, IPropFactory propFactory)
            : base(propModel, fullClassName, propFactory)
        {
        }

    }
}
