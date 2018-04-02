using AutoMapper;
using System;

namespace Swhp.AutoMapperSupport
{
    public interface IAutoMapperRequestKeyGen
    {
        IMapTypeDefinition SourceTypeDef { get; }
        IMapTypeDefinition DestinationTypeDef { get; }

        IAutoMapperConfigDetails AutoMapperConfigDetails { get; }

        /// <summary>
        /// Returns a function that when called creates an instance of some class that 
        /// implements the AutoMapper.IMapper interface.
        /// </summary>
        Func<IAutoMapperRequestKeyGen, IMapper> AutoMapperBuilder { get; }

        /// <summary>
        /// Creates a new instance of some class that implements the AutoMapper.IMapper interface.
        /// </summary>
        /// <returns></returns>
        IMapper BuildAutoMapper();

        // -----------------
        //
        //    Below are Convenience Properties that source from the SourceTypeDef and DestinationTypeDef
        //
        // -----------------

        Type SourceType { get; }
        Type DestinationType { get; }


        // The AutoMapper.IMapper that was created from this request.
        //IMapper AutoMapper { get; set; }
    }
}
