using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    public interface IPropFactory 
    {
        bool ProvidesStorage { get; }
        //bool ReturnDefaultForUndefined { get; }
        string IndexerName { get; }

        ResolveTypeDelegate TypeResolver { get; }
        IConvertValues ValueConverter { get; }

        IProvidePropStoreAccessService<PropIdType, PropNameType> PropStoreAccessServiceProvider { get; }

        #region Collection-Type Methods Methods

        ICPropPrivate<CT, T> Create<CT, T>(CT initialValue, string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            EventHandler<PCTypedEventArgs<CT>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<CT, CT, bool> comparer = null) where CT : IEnumerable<T>;

        ICPropPrivate<CT, T> CreateWithNoValue<CT, T>(string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            EventHandler<PCTypedEventArgs<CT>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<CT, CT, bool> comparer = null) where CT : IEnumerable<T>;

        #endregion

        #region Property-Type Methods

        IProp<T> Create<T>(T initialValue, string propertyName, object extraInfo = null, 
            bool hasStorage = true, bool typeIsSolid = true, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<T, T, bool> comparer = null);

        IProp<T> CreateWithNoValue<T>(string propertyName, object extraInfo = null, 
            bool hasStorage = true, bool typeIsSolid = true, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false
            , Func<T, T, bool> comparer = null);

        #endregion

        #region Generic property creators 

        IProp CreateGenFromObject(Type typeOfThisProperty, object value, string propertyName, object extraInfo, 
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        IProp CreateGenFromString(Type typeOfThisProperty, string value, bool useDefault, string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        IProp CreateGenWithNoValue(Type typeOfThisProperty, string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        //IPropGen CreatePropInferType(object value, string propertyName, object extraInfo, bool hasStorage);

        #endregion

        Func<T, T, bool> GetRefEqualityComparer<T>();

        object GetDefaultValue(Type propertyType, string propertyName = null);

        T GetDefaultValue<T>(string propertyName = null);

        T GetValueFromObject<T>(object value);

        T GetValueFromString<T>(string value);

        CT GetValueFromString<CT, T>(string value) where CT : class;

        Type GetTypeFromValue(object value);

        bool IsTypeSolid(object value, Type propertyType);

    }
}