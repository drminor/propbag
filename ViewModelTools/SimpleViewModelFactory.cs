using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.ViewModelTools
{
    using PropModelCacheInterface = ICachePropModels<String>;
    using PropModelType = IPropModel<String>;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using ViewModelActivatorInterface = IViewModelActivator<UInt32, String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    /// <summary>
    /// Provides a convenient way to create ViewModels. The PropModel Provider, ViewModel Activator, PropStoreAccessService Creator,
    /// AutoMapper service and Wrapper Type Creator are specified when this instance is created
    /// and are used on each GetNewViewModel request.
    /// </summary> 
    public class SimpleViewModelFactory : ViewModelFactoryInterface
    {
        #region Private Members

        private readonly PropModelCacheInterface _propModelCache;
        private readonly ViewModelActivatorInterface _viewModelActivator;
        private readonly PSAccessServiceCreatorInterface _storeAccessCreator;
        private readonly IProvideAutoMappers _autoMapperService;
        private readonly ICreateWrapperTypes _wrapperTypeCreator;

        #endregion

        #region Constructors

        /// <summary>
        /// This is provided for use by clients that require no loading of PropModels by FullClassName,
        /// or will otherwise retrieve PropModels using some other mechanisim.
        /// </summary>
        /// <param name="viewModelActivator"></param>
        /// <param name="storeAccessCreator"></param>
        public SimpleViewModelFactory
            (
            ViewModelActivatorInterface viewModelActivator,
            PSAccessServiceCreatorInterface storeAccessCreator
            )
        {
            _propModelCache = null;
            _viewModelActivator = viewModelActivator ?? throw new ArgumentNullException(nameof(viewModelActivator));
            _storeAccessCreator = storeAccessCreator ?? throw new ArgumentNullException(nameof(storeAccessCreator));

            _autoMapperService = null;
            _wrapperTypeCreator = null;
        }

        /// <summary>
        /// This is provided for clients that will not need any AutoMapping services.
        /// </summary>
        /// <param name="propModelCache"></param>
        /// <param name="viewModelActivator"></param>
        /// <param name="storeAccessCreator"></param>
        public SimpleViewModelFactory
            (
            PropModelCacheInterface propModelCache,
            ViewModelActivatorInterface viewModelActivator,
            PSAccessServiceCreatorInterface storeAccessCreator
            )
        {
            _propModelCache = propModelCache ?? throw new ArgumentNullException(nameof(propModelCache));
            _viewModelActivator = viewModelActivator ?? throw new ArgumentNullException(nameof(viewModelActivator));
            _storeAccessCreator = storeAccessCreator ?? throw new ArgumentNullException(nameof(storeAccessCreator));

            _autoMapperService = null;
            _wrapperTypeCreator = null;
        }

        /// <summary>
        /// Full service, all arguments must have a non-null value.
        /// </summary>
        /// <param name="propModelCache"></param>
        /// <param name="viewModelActivator"></param>
        /// <param name="storeAccessCreator"></param>
        /// <param name="autoMapperService"></param>
        /// <param name="wrapperTypeCreator"></param>
        public SimpleViewModelFactory
            (
            PropModelCacheInterface propModelCache,
            ViewModelActivatorInterface viewModelActivator,
            PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService,
            ICreateWrapperTypes wrapperTypeCreator
            )
            : this(propModelCache, viewModelActivator, storeAccessCreator)
        {
            _autoMapperService = autoMapperService ?? throw new ArgumentNullException(nameof(autoMapperService));
            _wrapperTypeCreator = wrapperTypeCreator ?? throw new ArgumentNullException(nameof(wrapperTypeCreator));
        }

        #endregion

        #region Public Properties

        public bool HasAutoMapperServices => _autoMapperService != null;

        public PropModelCacheInterface PropModelCache => _propModelCache;
        public PSAccessServiceCreatorInterface PropStoreAccessServiceCreator => _storeAccessCreator;
        public ViewModelActivatorInterface ViewModelActivator => _viewModelActivator;
        public IProvideAutoMappers AutoMapperService => _autoMapperService;
        public ICreateWrapperTypes WrapperTypeCreator => _wrapperTypeCreator;

        #endregion

        #region Public Methods 

        public object GetNewViewModel(string fullClassName)
        {
            object result = GetNewViewModel(fullClassName, null, null);
            return result;
        }

        public object GetNewViewModel(string fullClassName, IPropFactory pfOverride)
        {
            object result = GetNewViewModel(fullClassName, pfOverride, null);
            return result;
        }

        /// <summary>
        /// Creates a new View Model using the PropModel that has the specified Class Name.
        /// Fetches the PropModel whose class and namespace match the fullClassName argument.
        /// The names must match exactly: if no namespace is included, only PropModels that have a blank (null or empty string) namespace will be matched.
        /// </summary>
        /// <param name="fullClassName">The class and optional namespace of the PropModel to retrieve.</param>
        /// <param name="pfOverride">The PropFactory to use instead of the one specified in the PropModel. Can be null.</param>
        /// <param name="fcnOverride">If provided this will replace the full class name specified in the PropModel record.</param>
        /// <returns></returns>
        public object GetNewViewModel(string fullClassName, IPropFactory pfOverride, string fcnOverride)
        {

            if (_propModelCache.TryGetPropModel(fullClassName, out PropModelType propModel))
            {
                object result = GetNewViewModel(propModel, pfOverride, null);
                return result;
            }
            else
            {
                throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fullClassName}.");
            }
        }

        /// <summary>
        /// Creates a new ViewModel using the specified PropModel. The PropFactory and FullClassName values recorded in
        /// the PropModel may be overridden by the pfOverrid and fcnOverride arguments. If these values should not be
        /// overridden set the arguments to null.
        /// </summary>
        /// <param name="propModel"></param>
        /// <param name="pfOverride"></param>
        /// <param name="fcnOverride"></param>
        /// <returns></returns>
        public object GetNewViewModel(PropModelType propModel, IPropFactory pfOverride, string fcnOverride)
        {
            object result = _viewModelActivator.GetNewViewModel
                (
                typeToCreate: propModel.TypeToCreate,
                propModel: propModel,
                viewModelFactory: this,
                pfOverride: pfOverride,
                fcnOverride: fcnOverride
                );

            return result;
        }

        #endregion
    }
}
