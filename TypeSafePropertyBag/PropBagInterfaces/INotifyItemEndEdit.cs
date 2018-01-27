using System;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// Notifies clients that an item (that implements the IEditableObject interface) has performed a EndEdit operation. (Someone has called CommitEdit on a collection that implements IEditableCollectionView.)
    /// </summary>
    public interface INotifyItemEndEdit 
    {
        /// <summary>
        /// Occurs when EditEdit has been called signaling that the edits should be committed.
        /// </summary>
        event EventHandler<EventArgs> ItemEndEdit;
    }

}

