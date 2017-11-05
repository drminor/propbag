using System;


namespace DRM.TypeSafePropertyBag
{
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
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event EventHandler<PropertyChangedWithValsEventArgs> PropertyChangedWithVals;

    }
}

