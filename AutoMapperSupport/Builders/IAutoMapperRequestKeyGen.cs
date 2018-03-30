﻿using AutoMapper;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IAutoMapperRequestKeyGen
    {
        Type SourceType { get; }
        Type DestinationType { get; }

        IMapTypeDefinitionGen SourceTypeGenDef { get; }
        IMapTypeDefinitionGen DestinationTypeGenDef { get; }

        Func<IAutoMapperRequestKeyGen, IMapper> RawAutoMapperCreator { get; }
        IMapper AutoMapper { get; set; }

        IMapper CreateRawAutoMapper();
    }
}