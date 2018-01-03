using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;

    /// <summary>
    /// All implementers of IPropBag that also want to use the 
    /// </summary>
    public interface IPropBagInternal
    {
        PSAccessServiceType ItsStoreAccessor { get; }
        void RaiseStandardPropertyChanged(PropIdType propId, PropNameType propertyName);
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
        IEditableObject,
        ICloneable,
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

        object this[string typeName, PropNameType propertyName] { get; set; }
        object this[Type type, PropNameType propertyName] { get; set; }

        ValPlusType GetValPlusType(PropNameType propertyName, Type propertyType);

        bool SetValWithNoType(string propertyName, object value);
        bool SetIt<T>(T newValue, ref T curValue, PropNameType propertyName);

        bool PropertyExists(PropNameType propertyName);
        bool TryGetPropType(PropNameType propertyName, out PropKindEnum propType);

        //bool TryGetListSource(string propertyName, Type itemType, out IListSource listSource);

        bool SubscribeToPropChanged(PropertyChangedEventHandler handler, PropNameType propertyName, Type propertyType);
        bool UnsubscribeToPropChanged(PropertyChangedEventHandler handler, PropNameType propertyName, Type propertyType);

        bool SubscribeToPropChanging(PropertyChangingEventHandler handler, PropNameType propertyName, Type propertyType);
        bool UnsubscribeToPropChanging(PropertyChangingEventHandler handler, PropNameType propertyName, Type propertyType);

        IDisposable SubscribeToPropChanged<T>(EventHandler<PcTypedEventArgs<T>> eventHandler, string propertyName);
        bool UnSubscribeToPropChanged<T>(EventHandler<PcTypedEventArgs<T>> eventHandler, string propertyName);

        IDisposable SubscribeToPropChanged(EventHandler<PcGenEventArgs> eventHandler, PropNameType propertyName, Type propertyType);
        bool UnSubscribeToPropChanged(EventHandler<PcGenEventArgs> eventHandler, PropNameType propertyName, Type propertyType);

        bool SubscribeToPropChanged(EventHandler<PcObjectEventArgs> eventHandler, PropNameType propertyName);
        bool UnSubscribeToPropChanged(EventHandler<PcObjectEventArgs> eventHandler, PropNameType propertyName);

        bool RegisterBinding<T>(PropNameType nameOfPropertyToUpdate, string pathToSource);
        bool UnregisterBinding<T>(PropNameType nameOfPropertyToUpdate, string pathToSource);

        //bool SubscribeToPropChanged<T>(Action<T, T> doOnChange, PropNameType propertyName);
        //bool UnSubscribeToPropChanged<T>(Action<T, T> doOnChange, PropNameType propertyName);

        //bool SubscribeToPropChanged(Action<object, object> doOnChange, PropNameType propertyName);
        //bool UnSubscribeToPropChanged(Action<object, object> doOnChange, PropNameType propertyName);

        string FullClassName { get; }
        IPropFactory PropFactory { get; }

        // Consider moving these to the TypeSafePropBagMetaData class.
        PropBagTypeSafetyMode TypeSafetyMode { get; }
        IList<PropNameType> GetAllPropertyNames();
        IDictionary<PropNameType, IPropData> GetAllPropertyValues();
        IDictionary<PropNameType, ValPlusType> GetAllPropNamesAndTypes();

        void CloneProps(IPropBag copySource);

        bool TryGetDataSourceProviderProvider(IPropBag propBag, PropNameType propertyName, Type propertyType, out IProvideADataSourceProvider dataSourceProviderProvider);

        bool TryGetDataSourceProvider(IPropBag propBag, PropNameType propertyName, Type propertyType, out DataSourceProvider dataSourceProvider);

        bool TryGetViewManager(IPropBag propBag, PropNameType propertyName, Type propertyType, out IManageCViews cViewManager);

        //IPropGen this[int index] { get; }
        //int IndexOfProp(string propertyName, Type propertyType);
    }

}
