using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace PropBagLib.Tests.AutoMapperSupport
{
    public partial class DestinationModel3 : PropBag, ICloneable
    {
        public DestinationModel3(PropBagTypeSafetyMode typeSafetyMode, IPropFactory propFactory, string fullClassName)
            : base(typeSafetyMode, propFactory, fullClassName)
		{
            AddProp<Guid>("ProductId", null, null, Guid.NewGuid());
            AddProp<int>("Amount", null, null, initialValue: 0);
            AddProp<double>("Size", null, null, 10.1);
            AddProp<MyModel4>("Deep", null, null, null);
        }

        public DestinationModel3(PropModel propModel, string fullClassName, IPropFactory propFactory)
            : base(propModel, fullClassName, propFactory)
        {
        }

        public DestinationModel3(DestinationModel3 copySource)
            : base(copySource)
        {
        }

        new public object Clone()
        {
            return new DestinationModel3(this);
        }

    }
}
