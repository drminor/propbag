using AutoMapper;
using DRM.PropBag.ViewModelTools;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public interface IPropBagMapperKeyGen
    {
        IMapTypeDefinitionGen SourceTypeGenDef { get; }
        IMapTypeDefinitionGen DestinationTypeGenDef { get; }

        IPropBagMapperGen CreatePropBagMapper(ViewModelFactoryInterface viewModelFactory);

        IMapper CreateRawAutoMapper();

        IMapper AutoMapper { get; set; }

        Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> PropBagMapperCreator { get; }
        Func<IPropBagMapperKeyGen, IMapper> RawAutoMapperCreator { get; }

    }
}
