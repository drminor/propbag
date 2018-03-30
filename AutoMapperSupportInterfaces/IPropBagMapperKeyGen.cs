using AutoMapper;
using DRM.PropBag.ViewModelTools;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public interface IPropBagMapperKeyGen
    {
        Type SourceType { get; }
        Type DestinationType { get; }

        IMapTypeDefinitionGen SourceTypeGenDef { get; }
        IMapTypeDefinitionGen DestinationTypeGenDef { get; }

        IAutoMapperRequestKeyGen AutoMapperRequestKeyGen { get; }

        IMapper AutoMapper { get; set; }

        Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> PropBagMapperCreator { get; }

        IPropBagMapperGen CreatePropBagMapper(ViewModelFactoryInterface viewModelFactory);
    }


    // Convenient Reference
    //public interface IAutoMapperRequestKeyGen
    //{
    //    IMapTypeDefinitionGen SourceTypeGenDef { get; }
    //    IMapTypeDefinitionGen DestinationTypeGenDef { get; }

    //    IMapper CreateRawAutoMapper();

    //    IMapper AutoMapper { get; set; }

    //    Func<IAutoMapperRequestKeyGen, IMapper> RawAutoMapperCreator { get; }
    //}
}
