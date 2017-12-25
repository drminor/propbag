using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;
    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;

    public interface IPropFactory 
    {
        #region Public Properties and Methods

        bool ProvidesStorage { get; }
        string IndexerName { get; }
        ResolveTypeDelegate TypeResolver { get; }
        IConvertValues ValueConverter { get; }

        PSAccessServiceType CreatePropStoreService(IPropBagInternal propBag);

        IProvideDelegateCaches DelegateCacheProvider { get; }

        bool IsCollection(IProp prop);
        bool IsCollection(PropKindEnum propKind);

        bool IsReadOnly(IProp prop);
        bool IsReadOnly(PropKindEnum propKind);

        #endregion

        #region Enumerable-Type Prop Creation 

        #endregion

        #region IObsCollection<T> and ObservableCollection<T> Prop Creation

        ICProp<CT, T> Create<CT, T>(CT initialValue, PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;

        //ICPropFB<CT, T> CreateFB<CT, T>(CT initialValue, PropNameType propertyName, object extraInfo = null,
        //    bool hasStorage = true, bool typeIsSolid = true,
        //    Func<CT, CT, bool> comparer = null) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;

        ICProp<CT, T> CreateWithNoValue<CT, T>(PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;

        #endregion

            #region CollectionViewSource Prop Creation

        IProp CreateCVSProp<TCVS, T>(PropNameType propertyName) where TCVS : class;

        IProp CreateCVProp<T>(PropNameType propertyName);

        #endregion

        #region Scalar Prop Creation

        IProp<T> Create<T>(T initialValue, PropNameType propertyName, object extraInfo = null, 
            bool hasStorage = true, bool typeIsSolid = true, Func<T, T, bool> comparer = null);

        IProp<T> CreateWithNoValue<T>(PropNameType propertyName, object extraInfo = null, 
            bool hasStorage = true, bool typeIsSolid = true, Func<T, T, bool> comparer = null);

        #endregion

        #region Generic Property Creation

        IProp CreateGenFromObject(Type typeOfThisProperty, object value, PropNameType propertyName, object extraInfo, 
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        IProp CreateGenFromString(Type typeOfThisProperty, string value, bool useDefault, PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        IProp CreateGenWithNoValue(Type typeOfThisProperty, PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        IProp CreateCVSPropFromString(Type typeOfThisProperty, PropNameType propertyName);
        IProp CreateCVPropFromString(Type typeofThisProperty, PropNameType propertyName);

        //IPropGen CreatePropInferType(object value, PropNameType propertyName, object extraInfo, bool hasStorage);

        #endregion

        #region Default Value and Type Support

        Func<T, T, bool> GetRefEqualityComparer<T>();

        object GetDefaultValue(Type propertyType, PropNameType propertyName = null);

        T GetDefaultValue<T>(PropNameType propertyName = null);

        T GetValueFromObject<T>(object value);

        T GetValueFromString<T>(PropNameType value);

        CT GetValueFromString<CT, T>(PropNameType value) where CT : class;

        Type GetTypeFromValue(object value);

        bool IsTypeSolid(object value, Type propertyType);

        #endregion

        #region Diagnostics

        int DoSetCacheCount { get; }
        int CreatePropFromStringCacheCount { get; }
        int CreatePropWithNoValCacheCount { get; }

        #endregion
    }
}