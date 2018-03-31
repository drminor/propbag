using AutoMapper;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ViewModelTools;
using System;

namespace DRM.TypeSafePropertyBag
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public interface IPropBagMapperRequestKeyGen
    {
        Type SourceType { get; }
        Type DestinationType { get; }

        IMapTypeDefinition SourceTypeDef { get; }
        IMapTypeDefinition DestinationTypeDef { get; }

        IAutoMapperRequestKeyGen AutoMapperRequestKeyGen { get; }

        IMapper AutoMapper { get; set; }

        Func<IPropBagMapperRequestKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> PropBagMapperCreator { get; }

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
