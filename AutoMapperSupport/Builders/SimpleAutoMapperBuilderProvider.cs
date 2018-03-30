using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimpleAutoMapperBuilderProvider : IAutoMapperBuilderProvider
    {
        #region Private Properties

        //private readonly ViewModelActivatorInterface _viewModelActivator;
        //private PSAccessServiceCreatorInterface _storeAccessCreator;
        ////private readonly IProvideAutoMappers _autoMapperService;
        //private readonly ICreateWrapperTypes _wrapperTypeCreator;

        #endregion

        #region Constructor

        public SimpleAutoMapperBuilderProvider()
        {
        }

        #endregion

        #region Public Properties

        //public ICreateWrapperTypes WrapperTypeCreator => _wrapperTypeCreator;

        #endregion

        #region Public Methods

        public IBuildAutoMapper<TSource, TDestination> GetAutoMapperBuilder<TSource, TDestination>
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            IAutoMapperService autoMapperService
            )
            //where TDestination: class, IPropBag
        {
            IBuildAutoMapper<TSource, TDestination> result = new SimpleAutoMapperBuilder<TSource, TDestination>
                (
                    mapperConfigurationBuilder: mapperConfigurationBuilder,
                    autoMapperService: autoMapperService
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
