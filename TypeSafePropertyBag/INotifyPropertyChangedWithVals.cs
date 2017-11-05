using System;


namespace DRM.TypeSafePropertyBag
{
    //public delegate void EventHandler<TEventArgs>(object sender, TEventArgs e);

    //public interface ITypedEventArgs<TEventArgs> where TEventArgs : EventArgs
    //{

    //}

    //public interface ITypedEventHandler<TT, TEventArgs> where TT : EventHandler<TEventArgs> where TEventArgs : EventArgs
    //{
    //    event EventHandler<TEventArgs> PropertyChanged;
    //}

    //public interface ITypedNotifyPropertyChanged<TEventArgs, TProp> where TEventArgs : EventArgs
    //{
    //    event ITypedEventHandler<TEventArgs> PropertyChanged;

    //}


    public interface INotifyPropertyChanged<TEventArgs> where TEventArgs : EventArgs
    {
        event EventHandler<TEventArgs> PropertyChanged;
    }

    /// <summary>
    /// Notifies clients that a property value has changed and provides the old and new values in a type-safe manner.
    /// </summary>
    public interface INotifyPropertyChangedWithTVals<T,TEventArgs>
        : INotifyPropertyChanged<TEventArgs> where TEventArgs : EventArgs
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event EventHandler<PropertyChangedWithTValsEventArgs<T>> PropertyChangedWithTVals;
    }

    /// <summary>
    /// Notifies clients that a property value has changed and provides the old and new values.
    /// </summary>
    public interface INotifyPropertyChangedWithVals : INotifyPropertyChanged<PropertyChangedWithValsEventArgs>
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event EventHandler<PropertyChangedWithValsEventArgs> PropertyChangedWithVals;

    }
}

