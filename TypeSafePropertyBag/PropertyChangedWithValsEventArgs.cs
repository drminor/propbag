using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    /// <summary>
    /// Provides typed value change information for the <see cref="INotifyPropertyChangedWithTVals<typeparamref name="T"/>.PropertyChanged"/> event.
    /// </summary>
    /// <typeparam name="T">The type of the property that changed.</typeparam>
    public class PCTypedEventArgs<T> : PcGenEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PCTypedEventArgs<typeparamref name="T"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        public PCTypedEventArgs(string propertyName, T oldValue, T newValue)
            : base(propertyName, typeof(T), oldValue, newValue)
        {
        }

        public PCTypedEventArgs(string propertyName, T newValue)
            : base(propertyName, typeof(T), newValue)
        {
        }

        public PCTypedEventArgs(string propertyName, T newValue, bool newValueIsUndefined)
            : base(propertyName, typeof(T), newValue, newValueIsUndefined)
        {
        }

        public PCTypedEventArgs(string propertyName, bool oldValueIsUndefined, bool newValueIsUndefined)
            : base(propertyName, typeof(T), oldValueIsUndefined, newValueIsUndefined)
        {
        }

        /// <summary>
        /// Gets the old value of the property.
        /// </summary>
        public new T OldValue
        {
            get { return (T)base.OldValue; }
            set
            {
                base.OldValue = value;
            }
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public new T NewValue
        {
            get { return (T)base.NewValue; }
            set
            {
                base.NewValue = value;
            }
        }
    }

    /// <summary>
    /// Provides un-typed value change information for the <see cref="INotifyPCGen.PropertyChanged"/> event.
    /// Object instances of this class are used as a generic proxy for instances of <see cref="PCTypedEventArgs<typeparamref name="T"/>.
    /// </summary>
    public class PcGenEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PcGenEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        public PcGenEventArgs(string propertyName, Type propertyType, object oldValue, object newValue)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
            PropertyType = propertyType;
            OldValueIsUndefined = false;
            NewValueIsUndefined = false;
        }

        public PcGenEventArgs(string propertyName, Type propertyType, object newValue)
            : base(propertyName)
        {
            OldValueIsUndefined = true;
            NewValue = newValue;
            PropertyType = propertyType;
        }

        public PcGenEventArgs(string propertyName, Type propertyType, object oldValue, bool newValueIsUndefined)
            : base(propertyName)
        {
            OldValue = oldValue;
            OldValueIsUndefined = false;
            NewValueIsUndefined = newValueIsUndefined;
            PropertyType = propertyType;
        }

        public PcGenEventArgs(string propertyName, Type propertyType, bool oldValueIsUndefined, bool newValueIsUndefined)
            : base(propertyName)
        {
            OldValueIsUndefined = oldValueIsUndefined;
            NewValueIsUndefined = newValueIsUndefined;
            PropertyType = propertyType;
        }

        public Type PropertyType { get; }

        /// <summary>
        /// Gets the old value of the property.
        /// </summary>
        public object OldValue { get; protected set; }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public object NewValue { get; protected set; }

        public bool OldValueIsUndefined { get; protected internal set; }
        public bool NewValueIsUndefined { get; protected internal set; }
    }

    /// <summary>
    /// Provides value change information for the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    public class PcObjectEventArgs :  PropertyChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PcObjectEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        public PcObjectEventArgs(string propertyName, object oldValue, object newValue)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public PcObjectEventArgs(string propertyName, object newValue)
            : base(propertyName)
        {
            OldValueIsUndefined = true;
            NewValue = newValue;
        }

        public PcObjectEventArgs(string propertyName, object oldValue, bool newValueIsUndefined)
            : base(propertyName)
        {
            OldValue = oldValue;
            OldValueIsUndefined = false;
            NewValueIsUndefined = newValueIsUndefined;
        }

        /// <summary>
        /// Gets the old value of the property.
        /// </summary>
        public object OldValue { get; protected set; }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public object NewValue { get; protected set; }

        public bool OldValueIsUndefined { get; protected internal set; }
        public bool NewValueIsUndefined { get; protected internal set; }
    }

    ///// <summary> Using PCObjectEventArgs instead.
    ///// Provides value change information for the <see cref="INotifyPropDataChanged.PropChanged"/> event.
    ///// </summary>
    //public class PropDataValSetEventArgs : PropertyChangedEventArgs
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="PCObjectEventArgs"/> class.
    //    /// </summary>
    //    /// <param name="propName">The PropertyName.</param>
    //    /// <param name="propId">The PropId</param>
    //    /// <param name="propValueState">The new state of the value.</param>
    //    public PropDataValSetEventArgs(PropNameType propName, PropIdType propId, PropValueStateEnum propValueState)
    //        : base(propName)
    //    {
    //        PropValueState = propValueState;
    //    }

    //    /// <summary>
    //    /// Gets the old value of the property.
    //    /// </summary>
    //    public PropValueStateEnum PropValueState { get; protected set; }
    //}

    //public enum PropValueStateEnum
    //{
    //    Bindable,
    //    Unbindable
    //}
}
