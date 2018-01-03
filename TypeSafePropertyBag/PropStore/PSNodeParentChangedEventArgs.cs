using System;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public class PSNodeParentChangedEventArgs : EventArgs
    {
        public PSNodeParentChangedEventArgs(ExKeyT propId, ExKeyT oldParent, ExKeyT newParent)
        {
            PropId = propId; // The node whose parent is being changed.
            OldParent = oldParent; // The old parent.
            NewParent = newParent; // The new parent.
        }

        public ExKeyT PropId { get; set; }
        public ExKeyT OldParent { get; set; }
        public ExKeyT NewParent { get; set; }


    }
}
