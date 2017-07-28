using System;


namespace DRM.Ipnwv
{
    /// <summary>
    /// Notifies clients that a property value has changed and provides the old and new values in a type-safe manner.
    /// </summary>
    public interface INotifyPropertyChangedWithTVals<T>
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event PropertyChangedWithTValsHandler<T> PropertyChangedWithTVals;
    }

    /// <summary>
    /// Notifies clients that a property value has changed and provides the old and new values.
    /// </summary>
    public interface INotifyPropertyChangedWithVals
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event PropertyChangedWithValsHandler PropertyChangedWithVals;

    }
}

