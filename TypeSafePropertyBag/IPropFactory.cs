using System;
using System.Collections.Generic;
using DRM.TypeSafePropertyBag.EventManagement;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropFactory : IProvideAnEventManager
    {
        bool ProvidesStorage { get; }
        //bool ReturnDefaultForUndefined { get; }
        string IndexerName { get; }

        ResolveTypeDelegate TypeResolver { get; }
        IConvertValues ValueConverter { get; }

        #region Collection-Type Methods Methods

        ICPropPrivate<CT, T> Create<CT, T>(CT initialValue, string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true, Action<CT, CT> doWhenChanged = null, bool doAfterNotify = false,
            Func<CT, CT, bool> comparer = null) where CT : IEnumerable<T>;

        ICPropPrivate<CT, T> CreateWithNoValue<CT, T>(string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true, Action<CT, CT> doWhenChanged = null, bool doAfterNotify = false,
            Func<CT, CT, bool> comparer = null) where CT : IEnumerable<T>;

        #endregion

        #region Property-Type Methods

        IProp<T> Create<T>(T initialValue, string propertyName, object extraInfo = null, 
            bool hasStorage = true, bool typeIsSolid = true, Action<T, T> doWhenChanged = null, bool doAfterNotify = false,
            Func<T, T, bool> comparer = null);

        IProp<T> CreateWithNoValue<T>(string propertyName, object extraInfo = null, 
            bool hasStorage = true, bool typeIsSolid = true, Action<T, T> doWhenChanged = null, bool doAfterNotify = false
            , Func<T, T, bool> comparer = null);

        #endregion

        #region Generic property creators 

        IPropGen CreateGenFromObject(Type typeOfThisProperty, object value, string propertyName, object extraInfo, 
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind, Delegate doWhenChanged, bool doAfterNotify,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        IPropGen CreateGenFromString(Type typeOfThisProperty, string value, bool useDefault, string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind, Delegate doWhenChanged, bool doAfterNotify,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        IPropGen CreateGenWithNoValue(Type typeOfThisProperty, string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind, Delegate doWhenChanged, bool doAfterNotify,
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