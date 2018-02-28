﻿using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;
using System;
using DRM.PropBag.AutoMapperSupport;

namespace DRM.PropBagWPF
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    /// <summary>
    /// Provides a convenient way to create ViewModels. The PropModelProvider, ViewModelActivator and PropStoreAccessServiceCreation
    /// services are specified when this instance is created and are used on each GetNewViewModel request.
    /// </summary> 
    public class ViewModelHelper
    {
        #region Private Members

        IProvidePropModels _propModelProvider { get; }
        IViewModelActivator _viewModelActivator { get; }
        PSAccessServiceCreatorInterface _storeAccessCreator;
        IProvideAutoMappers _autoMapperService;

        #endregion

        #region Constructors

        public ViewModelHelper
            (
            IProvidePropModels propModelProvider,
            IViewModelActivator viewModelActivator,
            PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService
            )
        {
            _propModelProvider = propModelProvider ?? throw new ArgumentNullException(nameof(propModelProvider));
            _viewModelActivator = viewModelActivator ?? throw new ArgumentNullException(nameof(viewModelActivator));
            _storeAccessCreator = storeAccessCreator ?? throw new ArgumentNullException(nameof(storeAccessCreator));
            _autoMapperService = autoMapperService ?? throw new ArgumentNullException(nameof(autoMapperService));
        }

        #endregion

        #region Public Methods 

        public object GetNewViewModel(string resourceKey)
        {
            IPropModel pm = _propModelProvider.GetPropModel(resourceKey);
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
            IPropModel pm = _propModelProvider.GetPropModel(resourceKey);
            object result = GetNewViewModel(pm, propFactory, null);
            return result;
        }

        private object GetNewViewModel(IPropModel pm, IPropFactory propFactory, string fullClassName)
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
