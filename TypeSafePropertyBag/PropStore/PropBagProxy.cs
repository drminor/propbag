using System;

namespace DRM.TypeSafePropertyBag
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
            if (propBag == null) throw new ArgumentNullException(nameof(propBag));
            PropBagRef = new WeakReference<IPropBagInternal>(propBag);
        }

        #endregion

        //public bool TryGetTarget(out IPropBagInternal target)
        //{
        //    bool result = PropBagRef.TryGetTarget(out target);
        //    return result;
        //}

    }
}
