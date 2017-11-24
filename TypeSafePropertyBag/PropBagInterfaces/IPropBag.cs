using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using IHaveTheKeyIT = IHaveTheKey<UInt64, UInt64, UInt32>;

    using ICKeyManType = ICKeyMan<UInt64, UInt64, UInt32, String>;
    using L2KeyManType = IL2KeyMan<UInt32, String>;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    #endregion

    public interface IPropBagInternal
    { 
        PSAccessServiceType ItsStoreAccessor { get; }
        L2KeyManType Level2KeyManager { get; }
    }

    /// <summary>
    /// Base Property Bag Features
    /// </summary>
    public interface IPropBag
        : ITypeSafePropBag,
        ICustomTypeDescriptor,
        INotifyPropertyChanged,
        INotifyPropertyChanging,
        INotifyPCGen,
        INotifyPCObject,
        //INotifyPCIndividual,
        IDisposable
    {
        // These are defined by ITypeSafePropBag
        //object GetValWithType(string propertyName, Type propertyType);
        //bool SetValWithType(string propertyName, Type propertyType, object value);

        //T GetIt<T>(string propertyName);
        //bool SetIt<T>(T value, string propertyName);

        //Type GetTypeOfProperty(string propertyName);
        //bool TryGetTypeOfProperty(string propertyName, out Type type);

        bool TryGetPropGen(string propertyName, Type propertyType, out IPropData propGen);

        object this[string typeName, string propertyName] { get; set; }
        object this[Type type, string propertyName] { get; set; }

        ValPlusType GetValPlusType(string propertyName, Type propertyType);

        bool SetValWithNoType(string propertyName, object value);
        bool SetIt<T>(T newValue, ref T curValue, string propertyName);

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

        bool RegisterBinding<T>(string nameOfPropertyToUpdate, string pathToSource);
        bool UnregisterBinding<T>(string nameOfPropertyToUpdate, string pathToSource);

        //bool SubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName);
        //bool UnSubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName);

        //bool SubscribeToPropChanged(Action<object, object> doOnChange, string propertyName);
        //bool UnSubscribeToPropChanged(Action<object, object> doOnChange, string propertyName);

        string FullClassName { get; }

        // Consider moving these to the TypeSafePropBagMetaData class.
        IList<string> GetAllPropertyNames();
        IDictionary<string, IPropData> GetAllPropertyValues();
        IDictionary<string, ValPlusType> GetAllPropNamesAndTypes();

        //IPropGen this[int index] { get; }
        //int IndexOfProp(string propertyName, Type propertyType);

    }

}
