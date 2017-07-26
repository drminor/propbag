using System;


namespace DRM.Ipnwv
{
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

