using DRM.PropBag.TypeWrapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class SimplePropBagMapperBuilderProvider : IPropBagMapperBuilderProvider
    {
        #region Private Properties

        private readonly ICreateWrapperTypes _wrapperTypeCreator;
        private readonly IViewModelActivator _viewModelActivator;
        private PSAccessServiceCreatorInterface _storeAccessCreator;

        #endregion

        #region Constructor

        public SimplePropBagMapperBuilderProvider
            (
            ICreateWrapperTypes wrapperTypesCreator,
            IViewModelActivator viewModelActivator, 
            PSAccessServiceCreatorInterface storeAccessCreator
            )
        {
            _wrapperTypeCreator = wrapperTypesCreator ?? throw new ArgumentNullException(nameof(wrapperTypesCreator)); // GetSimpleWrapperTypeCreator();
            _viewModelActivator = viewModelActivator ?? throw new ArgumentNullException(nameof(viewModelActivator)); // new SimpleViewModelActivator();
            _storeAccessCreator = storeAccessCreator ?? throw new ArgumentNullException(nameof(storeAccessCreator));
        }

        #endregion

        #region Public Properties

        public ICreateWrapperTypes WrapperTypeCreator => _wrapperTypeCreator;

        #endregion

        #region Public Methods

        public IBuildPropBagMapper<TSource, TDestination> GetPropBagMapperBuilder<TSource, TDestination>
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            IProvideAutoMappers autoMapperService
            )
            where TDestination: class, IPropBag
        {
            IBuildPropBagMapper<TSource, TDestination> result
                = new SimplePropBagMapperBuilder<TSource, TDestination>
                (
                    mapperConfigurationBuilder: mapperConfigurationBuilder,
                    wrapperTypeCreator: _wrapperTypeCreator,
                    viewModelActivator: _viewModelActivator,
                    storeAccessCreator: _storeAccessCreator,
                    autoMapperService: autoMapperService
                );

            return result;
        }

        // TODO: Note: The WrapperTypeCreator hold two caches, the result provide is only from the cache of emitted types.
        // The number of entries in the cache of TypeDescriptors is not included.
        public long ClearTypeCache()
        {
            return _wrapperTypeCreator.ClearTypeCache();
        }

        #endregion
    }
}
