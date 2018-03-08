using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;

namespace PropBagLib.Tests
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class CreateAtRunTimeModel : PropBag
    {
        //public CreateAtRunTimeModel(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAccessCreator,
        //    IPropFactory propFactory, string fullClassName)
        //    : base(typeSafetyMode, storeAccessCreator, propFactory, fullClassName)
        //{
        //}

        public CreateAtRunTimeModel(PropModel pm, PSAccessServiceCreatorInterface storeAccessCreator)
            : base(pm, storeAccessCreator, propFactory: null, fullClassName: null)
        {
        }

    }
}
