using System;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;

namespace DRM.PropBag.AutoMapperSupport
{
    using PropModelType = IPropModel<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public delegate IPropBagMapperGen PropBagMapperCreator(IMapperRequest mr);


    // TODO: Change Register<xxx> to Submit<xxx>. (We are submitting a notice that we will need this at some point -- remember: GetMapper creates a mapper for all pending requests on first access to any request.)
    public interface IAutoMapperService : ICachePropBagMappers
    {
        IPropBagMapperKey<TSource, TDestination> SubmitMapperRequest<TSource, TDestination>
            (
            PropModelType propModel,
            object viewModelFactory,
            Type typeToWrap,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest = null,
            IPropFactory propFactory = null
            )
            where TDestination : class, IPropBag;

        // The PropModel supplies the TDestination type and the Type that will be wrapped.
        IPropBagMapperKeyGen SubmitMapperRequest
            (
            PropModelType propModel,
            ViewModelFactoryInterface viewModelFactory,
            Type sourceType,
            string configPackageName,
            IAutoMapperService autoMapperService
            );


        // Execute the Request and get the Mapper
        IPropBagMapper<TSource, TDestination> GetMapper<TSource, TDestination>
            (
            IPropBagMapperKey<TSource, TDestination> mapperRequest
            )
            where TDestination : class, IPropBag;
    }
}