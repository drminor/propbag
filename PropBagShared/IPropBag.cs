using System;
using System.Collections.Generic;

using System.ComponentModel;

using DRM.Ipnwvc;

namespace DRM.PropBag
{
    /// <summary>
    /// Base Property Bag Features
    /// </summary>
    public interface IPropBag : INotifyPropertyChanged, INotifyPropertyChanging, INotifyPropertyChangedWithVals
    {
        PropBagTypeSafetyMode TypeSafetyMode { get; }

        bool PropertyExists(string propertyName);

        Type GetTypeOfProperty(string propertyName);

        void SubscribeToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, string propertyName);
        void UnSubscribeToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, string propertyName);

        void SubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName);
        bool UnSubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName);

        void SubscribeToPropChanged(Action<object, object> doOnChange, string propertyName);
        bool UnSubscribeToPropChanged(Action<object, object> doOnChange, string propertyName);
    }
}
