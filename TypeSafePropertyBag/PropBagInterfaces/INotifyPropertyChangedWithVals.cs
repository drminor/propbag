using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// Notifies clients that a property value has changed and provides the old and new values in a type-safe manner.
    /// </summary>
    /// <typeparam name="T">The property's type who is sending the event args.</typeparam>
    public interface INotifyPCTyped<T> 
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event EventHandler<PCTypedEventArgs<T>> PropertyChangedWithTVals;
    }

    /// <summary>
    /// Notifies clients that a property value has changed and provides the old and new values.
    /// This is used as a generic proxy for INotifyPropertyChangedWithTVals<<typeparamref name="T"/>.
    /// </summary>
    public interface INotifyPCGen //  : INotifyPropertyChanged<PCGenEventArgs>
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event EventHandler<PCGenEventArgs> PropertyChangedWithGenVals;
    }

    /// <summary>
    /// Notifies clients that a property value has changed and provides the old and new values.
    /// This is used as a generic proxy for INotifyPropertyChangedWithTVals<<typeparamref name="T"/>.
    /// </summary>
    public interface INotifyPCObject //  : INotifyPropertyChanged<PCGenEventArgs>
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event EventHandler<PCObjectEventArgs> PropertyChangedWithObjectVals;
    }

    ///// <summary>
    ///// Notifies clients that a property value has changed and provides the old and new values.
    ///// This is used as a generic proxy for INotifyPropertyChangedWithTVals<<typeparamref name="T"/>.
    ///// </summary>
    //public interface INotifyPCIndividual //  : INotifyPropertyChanged<PCGenEventArgs>
    //{
    //    /// <summary>
    //    /// Occurs when a property value changes.
    //    /// </summary>
    //    event EventHandler<PropertyChangedEventArgs> PropertyChangedIndividual;
    //}




    // TODO: Consider making the INotifyPropertyChangedWithTVals interface
    // also implement the INotifyPropertyChanged<TEventArgs> where TEventArgs : EventArgs
    // which would look like this.
    //     INotifyPropertyChangedWithTVals<T> : INotifyPropertyChanged<PropertyChangedWithTValsEventArgs<T>> 
    // I have tried this and it doesn't seem to be very useful. Every delegate is unique,
    // and one cannot provide a delegate, even one with the same signature, in place of another.

    //public interface INotifyPropertyChanged<TEventArgs> where TEventArgs : EventArgs
    //{
    //    /// <summary>
    //    /// Occurs when a property value changes.
    //    /// </summary>
    //    event EventHandler<TEventArgs> PropertyChangedWithVals;
    //}
}

