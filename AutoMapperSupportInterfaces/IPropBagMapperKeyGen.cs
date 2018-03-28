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

        IPropBagMapperGen CreateMapper(ViewModelFactoryInterface viewModelFactory);

        IMapper CreateRawAutoMapper();

    }
}
