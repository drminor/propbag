using System;

namespace DRM.TypeSafePropertyBag
{
    // Used to signal that a Local Binding Source now points to a different IPropBag.
    public class PSNodeChangedEventArgs : EventArgs
    {
        public PSNodeChangedEventArgs(WeakReference<IPropBag> oldPropItemParent, WeakReference<IPropBag> newPropItemParent)
        {
            OldPropItemParent = oldPropItemParent;
            NewPropItemParent = newPropItemParent;
        }

        // TODO: Consider adding a property that stores the ExKeyT for the old StoreNodeBag
        // and another that stores the ExKeyT for the new StoreNodeBag.

        public WeakReference<IPropBag> OldPropItemParent { get; }
        public WeakReference<IPropBag> NewPropItemParent { get; }

    }
}
