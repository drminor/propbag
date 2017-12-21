﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases
    using PropIdType = UInt32;
    using PropNameType = String;

    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    #endregion

    public interface IPropFactory 
    {
        bool ProvidesStorage { get; }
        string IndexerName { get; }
        ResolveTypeDelegate TypeResolver { get; }
        IConvertValues ValueConverter { get; }

        PSAccessServiceProviderType PropStoreAccessServiceProvider { get; }

        IProvideDelegateCaches DelegateCacheProvider { get; }

        bool IsPropACollection(PropKindEnum propKind);

        #region Enumerable-Type Methods

        ICProp<CT, T> Create<CT, T>(CT initialValue, PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null) where CT : IObsCollection<T>;

        ICPropFB<CT, T> CreateFB<CT, T>(CT initialValue, PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null) where CT : ObservableCollection<T>;

        ICProp<CT, T> CreateWithNoValue<CT, T>(PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null) where CT : IObsCollection<T>;

        #endregion

        #region Property-Type Methods

        IProp<T> Create<T>(T initialValue, PropNameType propertyName, object extraInfo = null, 
            bool hasStorage = true, bool typeIsSolid = true, Func<T, T, bool> comparer = null);

        IProp<T> CreateWithNoValue<T>(PropNameType propertyName, object extraInfo = null, 
            bool hasStorage = true, bool typeIsSolid = true, Func<T, T, bool> comparer = null);

        #endregion

        #region Generic property creators 

        IProp CreateGenFromObject(Type typeOfThisProperty, object value, PropNameType propertyName, object extraInfo, 
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        IProp CreateGenFromString(Type typeOfThisProperty, PropNameType value, bool useDefault, PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        IProp CreateGenWithNoValue(Type typeOfThisProperty, PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        //IPropGen CreatePropInferType(object value, PropNameType propertyName, object extraInfo, bool hasStorage);

        #endregion

        Func<T, T, bool> GetRefEqualityComparer<T>();

        object GetDefaultValue(Type propertyType, PropNameType propertyName = null);

        T GetDefaultValue<T>(PropNameType propertyName = null);

        T GetValueFromObject<T>(object value);

        T GetValueFromString<T>(PropNameType value);

        CT GetValueFromString<CT, T>(PropNameType value) where CT : class;

        Type GetTypeFromValue(object value);

        bool IsTypeSolid(object value, Type propertyType);

        int DoSetCacheCount { get; }
        int CreatePropFromStringCacheCount { get; }
        int CreatePropWithNoValCacheCount { get; }
    }
}