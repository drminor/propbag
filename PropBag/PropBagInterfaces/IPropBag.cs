using System;
using System.Collections.Generic;

using System.ComponentModel;
using DRM.TypeSafePropertyBag;

using System.Runtime.CompilerServices;

namespace DRM.PropBag
{
    /// <summary>
    /// Base Property Bag Features
    /// </summary>
    public interface IPropBag : ITypeSafePropBag, INotifyPropertyChanged, INotifyPropertyChanging, INotifyPropertyChangedWithVals
    {
        PropBagTypeSafetyMode TypeSafetyMode { get; }

        bool AllPropsMustBeRegistered { get; }

        bool OnlyTypedAccess { get; }

        ReadMissingPropPolicyEnum ReadMissingPropPolicy { get; }

        bool ReturnDefaultForUndefined { get; }

        // These are defined by ITypeSafePropBag
        //object GetValWithType(string propertyName, Type propertyType);
        //bool SetItWithType(object value, Type propertyType = null, string propertyName);
        //Type GetTypeOfProperty(string propertyName);

        T GetIt<T>(string propertyName);
        IProp<T> GetTypedProp<T>(string propertyName);

        bool SetValWithNoType(string propertyName, object value);

        bool SetIt<T>(T value, string propertyName);
        bool SetIt<T>(T newValue, ref T curValue, string propertyName);

        bool PropertyExists(string propertyName);

        void SubscribeToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, string propertyName);
        void UnSubscribeToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, string propertyName);

        void SubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName);
        bool UnSubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName);

        void SubscribeToPropChanged(Action<object, object> doOnChange, string propertyName);
        bool UnSubscribeToPropChanged(Action<object, object> doOnChange, string propertyName);

        IList<string> GetAllPropertyNames();
        IDictionary<string, object> GetAllPropertyValues();
        IDictionary<string, ValPlusType> GetAllPropNamesAndTypes();

        object GetValueGen(object host, string propertyName, Type propertyType);
        void SetValueGen(object host, string propertyName, Type propertyType, object value);

    }

}
