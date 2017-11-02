﻿
namespace DRM.PropBag.AutoMapperSupport
{
    public interface IPropBagMapperBuilderProvider
    {
        IBuildPropBagMapper<TSource, TDestination> GetPropBagMapperBuilder<TSource, TDestination>
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder
            )
            where TDestination : class, IPropBag;
    }
}
