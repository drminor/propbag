using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DRM.PropBag
{
    /// <summary>
    /// Base Property Bag Features
    /// </summary>
    public interface IPropBagMin : ITypeSafePropBag, INotifyPropertyChanged, INotifyPropertyChanging, INotifyPropertyChangedWithVals
    {

        // These are defined by ITypeSafePropBag
        //object GetValWithType(string propertyName, Type propertyType);
        //bool SetValWithType(string propertyName, Type propertyType, object value);

        //T GetIt<T>(string propertyName);
        //bool SetIt<T>(T value, string propertyName);

        //Type GetTypeOfProperty(string propertyName);
        //bool TryGetTypeOfProperty(string propertyName, out Type type);

        IProp<T> GetTypedProp<T>(string propertyName);
        IPropGen GetPropGen(string propertyName, Type propertyType);
        ValPlusType GetValPlusType(string propertyName, Type propertyType);

        bool TryGetPropGen(string propertyName, Type propertyType, out IPropGen propGen);
        //IPropGen this[int index] { get; }

        //int IndexOfProp(string propertyName, Type propertyType);

        object this[string typeName, string propertyName] { get; }

        bool SetValWithNoType(string propertyName, object value);

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

        // Consider removing this since we are using the PBDispatch class.
        object GetValueGen(object host, string propertyName, Type propertyType);
        void SetValueGen(object host, string propertyName, Type propertyType, object value);

    }

}
