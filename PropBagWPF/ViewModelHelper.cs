using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.ViewModelTools;
using System;

namespace DRM.PropBagWPF
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class ViewModelHelper
    {
        #region Private Members

        IPropModelProvider PropModelProvider { get; }
        IViewModelActivator ViewModelActivator { get; }
        PSAccessServiceCreatorInterface _storeAccessCreator;

        #endregion

        #region Constructors

        public ViewModelHelper(IPropModelProvider propModelProvider, IViewModelActivator viewModelActivator, PSAccessServiceCreatorInterface storeAccessCreator)
        {
            PropModelProvider = propModelProvider ?? throw new ArgumentNullException(nameof(propModelProvider));
            ViewModelActivator = viewModelActivator ?? throw new ArgumentNullException(nameof(viewModelActivator));
            _storeAccessCreator = storeAccessCreator ?? throw new ArgumentNullException(nameof(storeAccessCreator));
        }

        #endregion

        #region Public Methods 

        // TODO: Do we use the propFactory as a backup, in case the PropModel does not have one?
        // Or do we use it as an override, overriding any value that may be present in the PropModel?
        // Currently we are using it as an override.
        public object GetNewViewModel(string resourceKey, IPropFactory propFactory = null)
        {
            PropModel pm = PropModelProvider.GetPropModel(resourceKey);

            Type typeToCreate = pm.TypeToCreate;
            string fullClassName = pm.FullClassName;

            object result = ViewModelActivator.GetNewViewModel
                (
                propModel: pm,
                storeAccessCreator: _storeAccessCreator,
                typeToCreate: typeToCreate,
                propFactory: propFactory ?? pm.PropFactory
,
                fullClassName: fullClassName);

            return result;
        }

        #endregion
    }
}
