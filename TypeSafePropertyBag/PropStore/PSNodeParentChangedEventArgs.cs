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
        public PSNodeParentChangedEventArgs(ExKeyT propId, ExKeyT oldValue, ExKeyT newValue)
        {
            PropId = propId;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public ExKeyT PropId { get; set; }
        public ExKeyT OldValue { get; set; }
        public ExKeyT NewValue { get; set; }


    }
}
