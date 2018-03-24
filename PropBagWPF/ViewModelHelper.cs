using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;
using System;
using DRM.PropBag.AutoMapperSupport;
using System.Collections.Generic;
using DRM.PropBag.TypeWrapper;

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
        ICreateWrapperTypes _wrapperTypeCreator;

        #endregion

        #region Constructors

        public ViewModelHelper
            (
            PropModelCacheInterface propModelCache,
            IViewModelActivator viewModelActivator,
            PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService,
            ICreateWrapperTypes wrapperTypeCreator
            )
        {
            _propModelCache = propModelCache ?? throw new ArgumentNullException(nameof(propModelCache));
            _viewModelActivator = viewModelActivator ?? throw new ArgumentNullException(nameof(viewModelActivator));
            _storeAccessCreator = storeAccessCreator ?? throw new ArgumentNullException(nameof(storeAccessCreator));
            _autoMapperService = autoMapperService ?? throw new ArgumentNullException(nameof(autoMapperService));
            _wrapperTypeCreator = wrapperTypeCreator ?? throw new ArgumentNullException(nameof(wrapperTypeCreator));
        }

        #endregion

        #region Public Methods 

        public object GetNewViewModel(string fullClassName)
        {
            //PropModelType pm = _propModelCache.GetPropModel(resourceKey);

            if (_propModelCache.TryGetPropModel(fullClassName, out PropModelType pm))
            {
                object result = GetNewViewModel(pm, null, null);
                return result;
            }
            else
            {
                throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fullClassName}.");
            }
        }

        /// <summary>
        /// Uses the specified propFactory instead of the one specified by the PropModel referenced by the given resourceKey.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="propFactory"></param>
        /// <returns></returns>
        public object GetNewViewModel(string fullClassName, IPropFactory propFactory)
        {
            //PropModelType pm = _propModelCache.GetPropModel(resourceKey);

            if(!_propModelCache.TryGetPropModel(fullClassName, out PropModelType pm))
            {
                object result = GetNewViewModel(pm, propFactory, null);
                return result;
            }
            else
            {
                throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fullClassName}.");
            }

        }

        private object GetNewViewModel(PropModelType pm, IPropFactory propFactory, string fullClassName)
        {
            object result = _viewModelActivator.GetNewViewModel
                (
                typeToCreate: pm.TypeToCreate,
                propModel: pm,
                storeAccessCreator: _storeAccessCreator,
                autoMapperService: _autoMapperService,
                wrapperTypeCreator: _wrapperTypeCreator,
                propFactory: propFactory,
                fullClassName: fullClassName
                );

            return result;
        }

        #endregion
    }
}
