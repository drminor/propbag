using System;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public class PSNodeParentChangedEventArgs : EventArgs
    {
        public PSNodeParentChangedEventArgs(ExKeyT propId, IPropBagProxy oldValue, IPropBagProxy newValue)
        {
            PropId = propId;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public ExKeyT PropId { get; set; }
        public IPropBagProxy OldValue { get; set; }
        public IPropBagProxy NewValue { get; set; }


    }
}
