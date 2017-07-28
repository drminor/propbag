using System.ComponentModel;

/// <remarks>
/// This code is heavily based on code developed by Daniel Moore in his WPF Igniter project.
/// https://github.com/danielmoore/WpfIgniter
/// </remarks>

namespace DRM.Ipnwv
{
    /// <summary>
    /// Provides typed value change information for the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <typeparam name="T">The type of the property that changed.</typeparam>
    public class PropertyChangedWithTValsEventArgs<T> : PropertyChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedEventArgs{T}"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        public PropertyChangedWithTValsEventArgs(string propertyName, T oldValue, T newValue) : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the old value of the property.
        /// </summary>
        public T OldValue { get; private set; }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public T NewValue { get; private set; }
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
        public object OldValue { get; private set; }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public object NewValue { get; private set; }
    }
}
