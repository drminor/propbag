using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;
using System;

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

        IProvidePropModels PropModelProvider { get; }
        IViewModelActivator ViewModelActivator { get; }
        PSAccessServiceCreatorInterface _storeAccessCreator;

        #endregion

        #region Constructors

        public ViewModelHelper(IProvidePropModels propModelProvider, IViewModelActivator viewModelActivator, PSAccessServiceCreatorInterface storeAccessCreator)
        {
            PropModelProvider = propModelProvider ?? throw new ArgumentNullException(nameof(propModelProvider));
            ViewModelActivator = viewModelActivator ?? throw new ArgumentNullException(nameof(viewModelActivator));
            _storeAccessCreator = storeAccessCreator ?? throw new ArgumentNullException(nameof(storeAccessCreator));
        }

        #endregion

        #region Public Methods 

        public object GetNewViewModel(string resourceKey)
        {
            IPropModel pm = PropModelProvider.GetPropModel(resourceKey);
            object result = GetNewViewModel(pm, null);
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
            IPropModel pm = PropModelProvider.GetPropModel(resourceKey);
            object result = GetNewViewModel(pm, propFactory);
            return result;
        }

        private object GetNewViewModel(IPropModel pm, IPropFactory propFactory)
        {
            object result = ViewModelActivator.GetNewViewModel
                (
                propModel: pm,
                storeAccessCreator: _storeAccessCreator,
                typeToCreate: pm.TypeToCreate,
                propFactory: propFactory ?? pm.PropFactory,
                fullClassName: pm.FullClassName
                );

            return result;
        }

        #endregion
    }
}
