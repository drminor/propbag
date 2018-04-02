using DRM.TypeSafePropertyBag;
using Swhp.AutoMapperSupport;
using System;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using PropModelType = IPropModel<String>;

    public class PropBagMapperConfigDetails : AutoMapperConfigDetailsBase, IPropBagMapperConfigDetails
    {
        public PropBagMapperConfigDetails(int extensionSourceId, string packageName, PropModelType propModel)
            : base(extensionSourceId, packageName)
        {
            PropModel = propModel;
        }

        public PropModelType PropModel { get; }

        public override object Payload => PropModel;
    }
}
