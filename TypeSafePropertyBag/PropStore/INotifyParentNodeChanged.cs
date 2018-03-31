using System;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// Notifies clients that a property value has changed and provides the old and new values.
    /// This is used as a generic proxy for INotifyPropertyChangedWithTVals<<typeparamref name="T"/>.
    /// </summary>
    internal interface INotifyParentNodeChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event EventHandler<PSNodeParentChangedEventArgs> ParentNodeHasChanged;

        IDisposable SubscribeToParentNodeHasChanged(EventHandler<PSNodeParentChangedEventArgs> handler);
        bool UnsubscribeToParentNodeHasChanged(EventHandler<PSNodeParentChangedEventArgs> handler);
        bool UnsubscribeToParentNodeHasChanged(ParentNCSubscriptionRequest subRequest);
    }
}

