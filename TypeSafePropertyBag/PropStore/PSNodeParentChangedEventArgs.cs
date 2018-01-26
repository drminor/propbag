using System;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    // Used to signal that a PropBag (i.e., a View Model) now has a new PropItem host.
    public class PSNodeParentChangedEventArgs : EventArgs
    {
        public PSNodeParentChangedEventArgs(ExKeyT propId, ExKeyT oldPropBagParent, ExKeyT newPropBagParent)
        {
            PropId = propId; // The node whose parent is being changed.
            OldPropBagParent = oldPropBagParent; // The old parent.
            NewPropBagParent = newPropBagParent; // The new parent.
        }

        public ExKeyT PropId { get; set; }
        public ExKeyT OldPropBagParent { get; set; }
        public ExKeyT NewPropBagParent { get; set; }
    }

}
