using System.ComponentModel;

/// <remarks>
/// This code is heavily based on code developed by Daniel Moore in his WPF Igniter project.
/// https://github.com/danielmoore/WpfIgniter
/// </remarks>

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// Provides typed value change information for the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <typeparam name="T">The type of the property that changed.</typeparam>
    public class PropertyChangedWithTValsEventArgs<T> : PropertyChangedWithValsEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedEventArgs{T}"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        public PropertyChangedWithTValsEventArgs(string propertyName, T oldValue, T newValue) : base(propertyName, oldValue, newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the old value of the property.
        /// </summary>
        public virtual new T OldValue
        {
            get
            {
                return (T)base.OldValue;
            }

            protected set
            {
                base.OldValue = value;
            }
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public virtual new T NewValue
        {
            get
            {
                return (T)base.NewValue;
            }

            protected set
            {
                base.NewValue = value;
            }
        }
    }

    /// <summary>
    /// Provides un-typed value change information for the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    public class PropertyChangedWithValsEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedEventArgs{T}"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        public PropertyChangedWithValsEventArgs(string propertyName, object oldValue, object newValue)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the old value of the property.
        /// </summary>
        public virtual object OldValue { get; protected set; }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public virtual object NewValue { get; protected set; }
    }
}
