using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;
using System;
using DRM.PropBag.AutoMapperSupport;

namespace DRM.PropBagWPF
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;
    using PropModelCacheInterface = ICachePropModels<String>;

    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    /// <summary>
    /// Provides a convenient way to create ViewModels. The PropModelProvider, ViewModelActivator and PropStoreAccessServiceCreation
    /// services are specified when this instance is created and are used on each GetNewViewModel request.
    /// </summary> 
    public class ViewModelHelper
    {
        #region Private Members

        PropModelCacheInterface _propModelCache { get; }
        IViewModelActivator _viewModelActivator { get; }
        PSAccessServiceCreatorInterface _storeAccessCreator;
        IProvideAutoMappers _autoMapperService;

        #endregion

        #region Constructors

        public ViewModelHelper
            (
            PropModelCacheInterface propModelCache,
            IViewModelActivator viewModelActivator,
            PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService
            )
        {
            _propModelCache = propModelCache ?? throw new ArgumentNullException(nameof(propModelCache));
            _viewModelActivator = viewModelActivator ?? throw new ArgumentNullException(nameof(viewModelActivator));
            _storeAccessCreator = storeAccessCreator ?? throw new ArgumentNullException(nameof(storeAccessCreator));
            _autoMapperService = autoMapperService ?? throw new ArgumentNullException(nameof(autoMapperService));
        }

        #endregion

        #region Public Methods 

        public object GetNewViewModel(string resourceKey)
        {
            PropModelType pm = _propModelCache.GetPropModel(resourceKey);
            object result = GetNewViewModel(pm, null, null);
            return result;
        }

        /// <summary>
        /// Uses the specified propFactory instead of the one specified by the PropModel referenced by the given resourceKey.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="propFactory"></param>
        /// <returns></returns>
        public object GetNewViewModel(string resourceKey, IPropFactory propFactory)
        {
            PropModelType pm = _propModelCache.GetPropModel(resourceKey);
            object result = GetNewViewModel(pm, propFactory, null);
            return result;
        }

        private object GetNewViewModel(PropModelType pm, IPropFactory propFactory, string fullClassName)
        {
            object result = _viewModelActivator.GetNewViewModel
                (
                typeToCreate: pm.TypeToCreate,
                propModel: pm,
                storeAccessCreator: _storeAccessCreator,
                autoMapperService: _autoMapperService,
                propFactory: propFactory,
                fullClassName: fullClassName
                );

            return result;
        }

        #endregion
    }
}
