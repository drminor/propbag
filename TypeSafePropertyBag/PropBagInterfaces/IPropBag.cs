using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    /// <summary>
    /// All implementers of IPropBag that also want to use a shared property store must
    /// implement this interface.
    /// </summary>
    public interface IPropBagInternal : IPropBag
    {
        PSAccessServiceInterface ItsStoreAccessor { get; }
        //IPropFactory PropFactory { get; }
        //void RaiseStandardPropertyChanged(PropIdType propId, PropNameType propertyName);
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
        INotifyItemEndEdit,
        IDisposable,
        ICloneable
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

        // Consider moving these to the TypeSafePropBagMetaData class.
        PropBagTypeSafetyMode TypeSafetyMode { get; }
        IList<PropNameType> GetAllPropertyNames();
        IDictionary<PropNameType, IPropData> GetAllPropertyValues();
        IDictionary<PropNameType, ValPlusType> GetAllPropNamesAndTypes();

        void RaiseStandardPropertyChanged(PropNameType propertyName);

        bool TryGetDataSourceProvider(PropNameType propertyName, Type propertyType, out DataSourceProvider dataSourceProvider);

        bool TryGetViewManager(PropNameType propertyName, Type propertyType, out IManageCViews cViewManager);


        // Use when the property is a ICViewProp<CVT> where CVT : ICollectionView
        IManageCViews GetOrAddViewManager(PropNameType propertyName, Type propertyType);

        // Use when the property is a IDoCRUD<TSource>
        IManageCViews GetOrAddViewManager<TDal, TSource, TDestination>
        (
            PropNameType propertyName,
            Type propertyType,
            IPropBagMapper<TSource, TDestination> mapper,
            out IManageCViews cViewManager
        )
            where TDal : IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit;

        IProvideACViewManager GetOrAddCViewManagerProvider<TDal, TSource, TDestination>(IViewManagerProviderKey viewManagerProviderKey)
        //(
        //    //LocalBindingInfo bindingInfo,   // The name of the property that holds the data (of type IDoCRUD<TSource>.)
        //    //IMapperRequest mr   // The (non-generic) information necessary to create a AutoMapper Mapper request.
        //)
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit;


        //IPropGen this[int index] { get; }
        //int IndexOfProp(string propertyName, Type propertyType);
    }

}
