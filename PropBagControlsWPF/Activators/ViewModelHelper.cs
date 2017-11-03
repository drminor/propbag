using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.ViewModelTools;
using System;

namespace DRM.PropBag.ControlsWPF
{
    public class ViewModelHelper
    {
        #region Private Members

        IPropModelProvider PropModelProvider { get; }
        IViewModelActivator ViewModelActivator { get; }

        #endregion

        #region Constructors

        public ViewModelHelper(IPropModelProvider propModelProvider)
        {
            PropModelProvider = propModelProvider ?? throw new ArgumentNullException(nameof(propModelProvider));

            ViewModelActivator = new SimpleViewModelActivator(propModelProvider);
        }

        #endregion

        #region Public Methods 

        // TODO: Do we use the propFactory as a backup, in case the PropModel does not have one?
        // Or do we use it as an override, overriding any value that may be present in the PropModel?
        // Currently we are using it as an override.
        public object GetNewViewModel(string resourceKey, IPropFactory propFactory = null)
        {
            PropModel pm = PropModelProvider.GetPropModel(resourceKey);

            Type typeToCreate = pm.TargetType;
            string fullClassName = pm.FullClassName;

            object result = ViewModelActivator.GetNewViewModel
                (
                propModel: pm,
                typeToCreate: typeToCreate,
                fullClassName: fullClassName,
                propFactory: propFactory ?? pm.PropFactory
                );

            return result;
        }

        #endregion
    }
}
