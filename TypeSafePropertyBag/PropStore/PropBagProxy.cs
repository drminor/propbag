using System;

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using PropNameType = String;

    using L2KeyManType = IL2KeyMan<UInt32, String>;
    using System.Collections.Generic;
    #endregion

    public class PropBagProxy : IPropBagProxy, IEquatable<PropBagProxy>
    {
        #region Public Properties

        public WeakReference<IPropBagInternal> PropBagRef { get; }
        public ObjectIdType ObjectId { get; }
        public L2KeyManType Level2KeyManager { get; }

        #endregion

        #region Constructor

        public PropBagProxy(ObjectIdType objectId, WeakReference<IPropBagInternal> propBagRef, L2KeyManType level2KeyManager)
        {
            PropBagRef = propBagRef ?? throw new ArgumentNullException(nameof(propBagRef));
            ObjectId = objectId;
            Level2KeyManager = level2KeyManager ?? throw new ArgumentNullException(nameof(level2KeyManager));
        }

        public bool TryGetTarget(out IPropBagInternal target)
        {
            bool result = PropBagRef.TryGetTarget(out target);
            return result;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PropBagProxy);
        }

        public bool Equals(PropBagProxy other)
        {
            return other != null &&
                   ObjectId == other.ObjectId;
        }

        public override int GetHashCode()
        {
            return 34855695 + ObjectId.GetHashCode();
        }

        public static bool operator ==(PropBagProxy proxy1, PropBagProxy proxy2)
        {
            return EqualityComparer<PropBagProxy>.Default.Equals(proxy1, proxy2);
        }

        public static bool operator !=(PropBagProxy proxy1, PropBagProxy proxy2)
        {
            return !(proxy1 == proxy2);
        }

        #endregion
    }
}
