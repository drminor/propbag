using System;

namespace DRM.TypeSafePropertyBag
{
    // TODO: Consider making the INotifyPropertyChangedWithTVals interface
    // also implement the INotifyPropertyChanged<TEventArgs> where TEventArgs : EventArgs
    // which would look like this.
    //     INotifyPropertyChangedWithTVals<T> : INotifyPropertyChanged<PropertyChangedWithTValsEventArgs<T>> 


    /// <summary>
    /// Notifies clients that a property value has changed and provides the old and new values in a type-safe manner.
    /// </summary>
    public interface INotifyPropertyChangedWithTVals<T>
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event EventHandler<PropertyChangedWithTValsEventArgs<T>> PropertyChangedWithTVals;
    }

    /// <summary>
    /// Notifies clients that a property value has changed and provides the old and new values.
    /// </summary>
    public interface INotifyPropertyChangedWithVals
        : INotifyPropertyChanged<PropertyChangedWithValsEventArgs>
    {
    }

    public interface INotifyPropertyChanged<TEventArgs> where TEventArgs : EventArgs
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event EventHandler<TEventArgs> PropertyChangedWithVals;
    }
}

