using DRM.TypeSafePropertyBag;
using Swhp.AutoMapperSupport;
using System;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using PropModelType = IPropModel<String>;

    public class PropBagMapperConfigDetails : AutoMapperConfigDetailsBase
    {
        public const int PROP_BAG_MAPPER_DETAIL_EXTENSION_ID = 1;

        protected override int _extensionSourceId => PROP_BAG_MAPPER_DETAIL_EXTENSION_ID;

        public PropBagMapperConfigDetails(string packageName, PropModelType propModel)
            : base(packageName)
        {
            PropModel = propModel;
        }

        public PropModelType PropModel { get; }

        public override object PayLoad => PropModel;
    }
}
