using System;
using AutoMapper;

namespace Swhp.AutoMapperSupport
{
    public interface IBuildAutoMapper<TSource, TDestination> //where TDestination : class, IPropBag
    {
        Func<IAutoMapperRequestKeyGen, IMapper> GenRawAutoMapperCreator { get; }

        IMapper GenerateRawAutoMapper(IAutoMapperRequestKey<TSource, TDestination> mapperRequestKey);
    }
}
