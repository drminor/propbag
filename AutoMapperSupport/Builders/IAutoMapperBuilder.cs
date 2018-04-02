using System;
using AutoMapper;

namespace Swhp.AutoMapperSupport
{
    public interface IAutoMapperBuilder<TSource, TDestination> 
    {
        Func<IAutoMapperRequestKeyGen, IMapper> AutoMapperBuilderGen { get; }

        IMapper BuildAutoMapper(IAutoMapperRequestKey<TSource, TDestination> mapperRequestKey);
    }
}
