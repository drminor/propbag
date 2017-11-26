using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using PropNameType = String;

    using L2KeyManType = IL2KeyMan<UInt32, String>;
    #endregion

    public class PropBagProxy : IPropBagProxy
    {
        #region Public Properties

        public WeakReference<IPropBagInternal> PropBagRef { get; }
        //public ObjectIdType ObjectId { get; }
        public L2KeyManType Level2KeyManager { get; }

        #endregion

        #region Constructor

        public PropBagProxy(WeakReference<IPropBagInternal> propBagRef, L2KeyManType level2KeyManager)
        {
            PropBagRef = propBagRef ?? throw new ArgumentNullException(nameof(propBagRef));
            //ObjectId = objectId;
            Level2KeyManager = level2KeyManager ?? throw new ArgumentNullException(nameof(level2KeyManager));
        }

        #endregion

        //public bool TryGetTarget(out IPropBagInternal target)
        //{
        //    bool result = PropBagRef.TryGetTarget(out target);
        //    return result;
        //}

    }
}
