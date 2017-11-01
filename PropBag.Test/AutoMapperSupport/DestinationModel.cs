using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;

namespace PropBagLib.Tests.AutoMapperSupport
{
    public partial class DestinationModel : PropBag
    {
        public DestinationModel()
            : this(PropBagTypeSafetyMode.Tight, new AutoMapperHelpers().PropFactory_V1)
        {
        }

        public DestinationModel(PropBagTypeSafetyMode typeSafetyMode, IPropFactory factory)
            : base(typeSafetyMode, factory)
		{
            AddProp<Guid>("ProductId", null, false, null, null, Guid.NewGuid());
            AddProp<int>("Amount", null, false, null, null, 0);
            AddProp<double>("Size", null, false, null, null, 10.1);
            AddProp<MyModel4>("Deep", null, false, null, null, null);
        }

    }
}
