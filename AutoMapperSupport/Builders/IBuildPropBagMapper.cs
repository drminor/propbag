using System;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IBuildPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        bool Validate(IPropBagMapperKey<TSource, TDestination> mapperRequestKey);

        IPropBagMapper<TSource, TDestination> GenerateMapper(IPropBagMapperKey<TSource, TDestination> mapperRequestKey);

        Func<IPropBagMapperKeyGen, IPropBagMapperGen> GenMapperCreator { get; }

        //IViewModelActivator ViewModelActivator { get; }
    }
}
