
namespace DRM.TypeSafePropertyBag
{
    public class SimplePropBagMapperBuilderProvider : IPropBagMapperBuilderProvider
    {
        #region Constructor

        public SimplePropBagMapperBuilderProvider()
        {
        }

        #endregion

        #region Public Methods

        public IBuildPropBagMapper<TSource, TDestination> GetPropBagMapperBuilder<TSource, TDestination>
            (
            //IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            IPropBagMapperService propBagMapperService
            )
            where TDestination: class, IPropBag
        {
            IBuildPropBagMapper<TSource, TDestination> result
                = new SimplePropBagMapperBuilder<TSource, TDestination>
                (
                    //mapperConfigurationBuilder: mapperConfigurationBuilder,
                    propBagMapperService: propBagMapperService
                );

            return result;
        }

        #endregion
    }
}
