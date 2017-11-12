using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{

    using PropIdType = System.UInt32;

    /// <summary>
    /// Base Property Bag Features
    /// </summary>
    public interface IPropBag : ITypeSafePropBag, ICustomTypeDescriptor, INotifyPropertyChanged, INotifyPropertyChanging, INotifyPCGen, INotifyPCIndividual
    {
        // These are defined by ITypeSafePropBag
        //object GetValWithType(string propertyName, Type propertyType);
        //bool SetValWithType(string propertyName, Type propertyType, object value);

        //T GetIt<T>(string propertyName);
        //bool SetIt<T>(T value, string propertyName);

        //Type GetTypeOfProperty(string propertyName);
        //bool TryGetTypeOfProperty(string propertyName, out Type type);

        //IProp<T> GetTypedProp<T>(string propertyName);
        //IPropGen GetPropGen(string propertyName, Type propertyType);

        bool TryGetPropGen(string propertyName, Type propertyType, out IPropGen propGen);

        object this[string typeName, string propertyName] { get; set; }
        object this[Type type, string propertyName] { get; set; }

        ValPlusType GetValPlusType(string propertyName, Type propertyType);

        bool SetValWithNoType(string propertyName, object value);
        bool SetIt<T>(T newValue, ref T curValue, string propertyName);
        bool SetIt<T>(T value, PropIdType propId);

        bool PropertyExists(string propertyName);
        bool TryGetPropType(string propertyName, out PropKindEnum propType);
        bool TryGetListSource(string propertyName, Type itemType, out IListSource listSource);

        bool SubscribeToPropChanged(EventHandler<PropertyChangedEventArgs> handler, string propertyName, Type propertyType);
        bool UnsubscribeToPropChanged(EventHandler<PropertyChangedEventArgs> handler, string propertyName, Type propertyType);

        bool SubscribeToPropChanged<T>(EventHandler<PCTypedEventArgs<T>> eventHandler, string propertyName);
        bool UnSubscribeToPropChanged<T>(EventHandler<PCTypedEventArgs<T>> eventHandler, string propertyName);

        bool SubscribeToPropChanged(EventHandler<PCGenEventArgs> eventHandler, string propertyName, Type propertyType);
        bool UnSubscribeToPropChanged(EventHandler<PCGenEventArgs> eventHandler, string propertyName, Type propertyType);

        bool SubscribeToPropChanged(EventHandler<PCObjectEventArgs> eventHandler, string propertyName);
        bool UnSubscribeToPropChanged(EventHandler<PCObjectEventArgs> eventHandler, string propertyName);

        bool AddBinding<T>(string propertyName, string targetPropertyName, Action<T, T> ttAction);


        //bool SubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName);
        //bool UnSubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName);

        //bool SubscribeToPropChanged(Action<object, object> doOnChange, string propertyName);
        //bool UnSubscribeToPropChanged(Action<object, object> doOnChange, string propertyName);

        string FullClassName { get; }

        // Consider moving these to the TypeSafePropBagMetaData class.
        IList<string> GetAllPropertyNames();
        IDictionary<string, IPropGen> GetAllPropertyValues();
        IDictionary<string, ValPlusType> GetAllPropNamesAndTypes();

        //IPropGen this[int index] { get; }
        //int IndexOfProp(string propertyName, Type propertyType);

    }

}
