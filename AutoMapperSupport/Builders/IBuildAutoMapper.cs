using System;
using DRM.TypeSafePropertyBag;
using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IBuildAutoMapper<TSource, TDestination> //where TDestination : class, IPropBag
    {
        Func<IAutoMapperRequestKeyGen, IMapper> GenRawAutoMapperCreator { get; }

        IMapper GenerateRawAutoMapper(IAutoMapperRequestKey<TSource, TDestination> mapperRequestKey);
    }
}
