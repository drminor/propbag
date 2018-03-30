using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimplePropBagMapperBuilderProvider : IPropBagMapperBuilderProvider
    {
        #region Constructor

        public SimplePropBagMapperBuilderProvider()
        {
        }

        #endregion

        #region Public Properties

        //public ICreateWrapperTypes WrapperTypeCreator => _wrapperTypeCreator;

        #endregion

        #region Public Methods

        public IBuildPropBagMapper<TSource, TDestination> GetPropBagMapperBuilder<TSource, TDestination>
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            IPropBagMapperService autoMapperService
            )
            where TDestination: class, IPropBag
        {
            IBuildPropBagMapper<TSource, TDestination> result
                = new SimplePropBagMapperBuilder<TSource, TDestination>
                (
                    mapperConfigurationBuilder: mapperConfigurationBuilder,
                    propBagMapperService: autoMapperService
                );

            return result;
        }

        //// TODO: Note: The WrapperTypeCreator hold two caches, the result provided is only from the cache of emitted types.
        //// The number of entries in the cache of TypeDescriptors is not included.
        //public long ClearTypeCache()
        //{
        //    return _wrapperTypeCreator.ClearTypeCache();
        //}

        #endregion
    }
}
