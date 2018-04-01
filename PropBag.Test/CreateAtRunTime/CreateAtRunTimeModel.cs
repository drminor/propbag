using DRM.PropBag;
using DRM.PropBag.ViewModelTools;
using System;

namespace PropBagLib.Tests
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public class CreateAtRunTimeModel : PropBag
    {
        //public CreateAtRunTimeModel(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAccessCreator,
        //    IPropFactory propFactory, string fullClassName)
        //    : base(typeSafetyMode, storeAccessCreator, propFactory, fullClassName)
        //{
        //}

        public CreateAtRunTimeModel(PropModel pm, ViewModelFactoryInterface viewModelFactory)
            : base(pm, viewModelFactory, null, propFactory: null, fullClassName: "PropBagLib.Tests.CreateAtRunTimeModel")
        {
        }

    }
}
