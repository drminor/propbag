using System;
using System.Collections.Generic;

using System.ComponentModel;

using DRM.Inpcwv;
using System.Runtime.CompilerServices;

namespace DRM.PropBag
{
    /// <summary>
    /// Base Property Bag Features
    /// </summary>
    public interface IPropBag : INotifyPropertyChanged, INotifyPropertyChanging, INotifyPropertyChangedWithVals
    {
        PropBagTypeSafetyMode TypeSafetyMode { get; }

        object GetIt([CallerMemberName] string propertyName = null, Type propertyType = null);
        T GetIt<T>([CallerMemberName] string propertyName = null);

        bool SetItWithNoType(object value, [CallerMemberName] string propertyName = null);
        bool SetItWithType(object value, Type propertyType = null, [CallerMemberName] string propertyName = null);
        bool SetIt<T>(T value, [CallerMemberName] string propertyName = null);
        bool SetIt<T>(T newValue, ref T curValue, [CallerMemberName]string propertyName = null);

        bool PropertyExists(string propertyName);

        Type GetTypeOfProperty(string propertyName);

        void SubscribeToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, string propertyName);
        void UnSubscribeToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, string propertyName);

        void SubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName);
        bool UnSubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName);

        void SubscribeToPropChanged(Action<object, object> doOnChange, string propertyName);
        bool UnSubscribeToPropChanged(Action<object, object> doOnChange, string propertyName);

        IList<string> GetAllPropertyNames();
        IDictionary<string, object> GetAllPropertyValues();
        IDictionary<string, ValPlusType> GetAllPropNamesAndTypes();

        
    }
}
