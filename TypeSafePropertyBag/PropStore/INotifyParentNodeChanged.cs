using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{


    /// <summary>
    /// Notifies clients that a property value has changed and provides the old and new values.
    /// This is used as a generic proxy for INotifyPropertyChangedWithTVals<<typeparamref name="T"/>.
    /// </summary>
    public interface INotifyParentNodeChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event EventHandler<PSNodeParentChangedEventArgs> ParentNodeHasChanged;

    }


}

