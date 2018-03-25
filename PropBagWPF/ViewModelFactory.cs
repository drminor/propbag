using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.TypeWrapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace DRM.PropBagWPF
{
    using PropModelCacheInterface = ICachePropModels<String>;
    using PropModelType = IPropModel<String>;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using ViewModelActivatorInterface = IViewModelActivator<UInt32, String>;

    /// <summary>
    /// Provides a convenient way to create ViewModels. The PropModel Provider, ViewModel Activator, PropStoreAccessService Creator,
    /// AutoMapper service and Wrapper Type Creator are specified when this instance is created
    /// and are used on each GetNewViewModel request.
    /// </summary> 
    public class ViewModelFactory
    {
        #region Private Members

        PropModelCacheInterface _propModelCache { get; }
        ViewModelActivatorInterface _viewModelActivator { get; }
        PSAccessServiceCreatorInterface _storeAccessCreator;
        IProvideAutoMappers _autoMapperService;
        ICreateWrapperTypes _wrapperTypeCreator;

        #endregion

        #region Constructors

        public ViewModelFactory
            (
            PropModelCacheInterface propModelCache,
            ViewModelActivatorInterface viewModelActivator,
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
            //if (_propModelCache.TryGetPropModel(fullClassName, out PropModelType propModel))
            //{
            //    object result = GetNewViewModel(propModel, null, null);
            //    return result;
            //}
            //else
            //{
            //    throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fullClassName}.");
            //}
            object result = GetNewViewModel(fullClassName, null, null);
            return result;
        }

        /// <summary>
        /// Uses the specified propFactory instead of the one specified by the PropModel referenced by the given resourceKey.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="pfOverride"></param>
        /// <returns></returns>
        public object GetNewViewModel(string fullClassName, IPropFactory pfOverride)
        {
            //if(!_propModelCache.TryGetPropModel(fullClassName, out PropModelType propModel))
            //{
            //    object result = GetNewViewModel(propModel, propFactory, null);
            //    return result;
            //}
            //else
            //{
            //    throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fullClassName}.");
            //}

            object result = GetNewViewModel(fullClassName, pfOverride, null);
            return result;

        }

        /// <summary>
        /// Uses the specified propFactory instead of the one specified by the PropModel referenced by the given resourceKey.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="pfOverride"></param>
        /// <returns></returns>
        public object GetNewViewModel(string fullClassName, IPropFactory pfOverride, string fcnOverride)
        {
            if (!_propModelCache.TryGetPropModel(fullClassName, out PropModelType propModel))
            {
                object result = GetNewViewModel(propModel, pfOverride, null);
                return result;
            }
            else
            {
                throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fullClassName}.");
            }
        }

        public object GetNewViewModel(PropModelType propModel, IPropFactory pfOverride, string fcnOverride)
        {
            object result = _viewModelActivator.GetNewViewModel
                (
                typeToCreate: propModel.TypeToCreate,
                propModel: propModel,
                storeAccessCreator: _storeAccessCreator,
                autoMapperService: _autoMapperService,
                wrapperTypeCreator: _wrapperTypeCreator,
                pfOverride: pfOverride,
                fcnOverride: fcnOverride
                );

            return result;
        }

        #endregion
    }
}
