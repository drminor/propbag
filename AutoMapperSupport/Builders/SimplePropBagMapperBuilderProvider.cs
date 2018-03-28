using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimplePropBagMapperBuilderProvider : IPropBagMapperBuilderProvider
    {
        #region Private Properties

        //private readonly ViewModelActivatorInterface _viewModelActivator;
        //private PSAccessServiceCreatorInterface _storeAccessCreator;
        ////private readonly IProvideAutoMappers _autoMapperService;
        //private readonly ICreateWrapperTypes _wrapperTypeCreator;

        #endregion

        #region Constructor

        public SimplePropBagMapperBuilderProvider
            (
            //ViewModelActivatorInterface viewModelActivator,
            //PSAccessServiceCreatorInterface storeAccessCreator,
            ////IProvideAutoMappers autoMapperService,
            //ICreateWrapperTypes wrapperTypesCreator
            )
        {
            //_viewModelActivator = viewModelActivator ?? throw new ArgumentNullException(nameof(viewModelActivator));
            //_storeAccessCreator = storeAccessCreator ?? throw new ArgumentNullException(nameof(storeAccessCreator));
            ////_autoMapperService = autoMapperService ?? throw new ArgumentNullException(nameof(autoMapperService));
            //_wrapperTypeCreator = wrapperTypesCreator ?? throw new ArgumentNullException(nameof(wrapperTypesCreator));
        }

        #endregion

        #region Public Properties

        //public ICreateWrapperTypes WrapperTypeCreator => _wrapperTypeCreator;

        #endregion

        #region Public Methods

        public IBuildPropBagMapper<TSource, TDestination> GetPropBagMapperBuilder<TSource, TDestination>
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            IAutoMapperService autoMapperService
            )
            where TDestination: class, IPropBag
        {
            IBuildPropBagMapper<TSource, TDestination> result
                = new SimplePropBagMapperBuilder<TSource, TDestination>
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
