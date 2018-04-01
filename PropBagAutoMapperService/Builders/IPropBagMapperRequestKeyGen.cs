using AutoMapper;
using Swhp.AutoMapperSupport;
using DRM.PropBag.ViewModelTools;
using System;
using DRM.TypeSafePropertyBag;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using PropModelType = IPropModel<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public interface IPropBagMapperRequestKeyGen
    {
        // The PropModel that defines the Destination Type. Will be null if this mapping target a standard Type.
        PropModelType PropModel { get; }

        // Our link to the AutoMapperService
        IAutoMapperRequestKeyGen AutoMapperRequestKeyGen { get; }

        // Returns a function that when called creates a IPropBagMapper
        Func<IPropBagMapperRequestKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> PropBagMapperCreator { get; }

        // A function that creates a IPropBagMapper.
        IPropBagMapperGen CreatePropBagMapper(ViewModelFactoryInterface viewModelFactory);

        // -----------------
        //
        //    Convenience Properties that source from the AutoMapperRequestKeyGen.
        //
        // -----------------
        IMapper AutoMapper { get; set; }

        IMapTypeDefinition SourceTypeDef { get; }
        IMapTypeDefinition DestinationTypeDef { get; }

        Type SourceType { get; }
        Type DestinationType { get; }
    }

}
