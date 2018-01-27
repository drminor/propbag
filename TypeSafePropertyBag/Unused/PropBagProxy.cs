using System;

namespace DRM.TypeSafePropertyBag.Unused
{
    public class PropBagProxy : IPropBagProxy
    {
        #region Public Properties

        public WeakReference<IPropBagInternal> PropBagRef { get; }

        #endregion

        #region Constructor

        public PropBagProxy(WeakReference<IPropBagInternal> propBagRef)
        {
            PropBagRef = propBagRef ?? throw new ArgumentNullException(nameof(propBagRef));
        }

        public PropBagProxy(IPropBagInternal propBag)
        {
            PropBagRef = new WeakReference<IPropBagInternal>(propBag ?? throw new ArgumentNullException(nameof(propBag)));
        }

        #endregion
    }
}
