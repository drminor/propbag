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

        //// TODO: Ultimately this should be removed -- the PropModelProvider should make sure that every PropModel has a PropFactory.
        //// Or better yet, solve the "The Default PropFactory Cannot Resolve Local Types" problem.
        //IPropFactory FallBackPropFactory { get; }

        #endregion

        #region Constructors

        public ViewModelHelper(IPropModelProvider propModelProvider)
        {
            PropModelProvider = propModelProvider ?? throw new ArgumentNullException(nameof(propModelProvider));
            //FallBackPropFactory = fallBackPropFactory ?? throw new ArgumentNullException(nameof(fallBackPropFactory));

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

            // If the PropModel just fetched does not have a propFactory
            // then use the one provided by the caller.
            //pm.PropFactory = pm.PropFactory ?? propFactory;

            //IPropFactory thePropFactoryToUse = propFactory ?? pm.PropFactory ?? FallBackPropFactory;

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
