using System;

namespace DRM.TypeSafePropertyBag
{
    public class PropBagProxy_Unused : IPropBagProxy
    {
        #region Public Properties

        public WeakReference<IPropBagInternal> PropBagRef { get; }

        #endregion

        #region Constructor

        public PropBagProxy_Unused(WeakReference<IPropBagInternal> propBagRef)
        {
            PropBagRef = propBagRef ?? throw new ArgumentNullException(nameof(propBagRef));
        }

        public PropBagProxy_Unused(IPropBagInternal propBag)
        {
            PropBagRef = new WeakReference<IPropBagInternal>(propBag ?? throw new ArgumentNullException(nameof(propBag)));
        }

        #endregion
    }
}
