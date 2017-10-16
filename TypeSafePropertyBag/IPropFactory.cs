using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropFactory
    {
        bool ProvidesStorage { get; }
        bool ReturnDefaultForUndefined { get; }
        string IndexerName { get; }

        ResolveTypeDelegate TypeResolver { get; }

        IProp<T> Create<T>(T initialValue, string propertyName, object extraInfo = null, bool hasStorage = true, bool typeIsSolid = true, Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T, T, bool> comparer = null);

        IProp<T> CreateWithNoValue<T>(string propertyName, object extraInfo = null, bool hasStorage = true, bool typeIsSolid = true, Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T, T, bool> comparer = null);

        IPropGen CreateGenFromObject(Type typeOfThisProperty, object value, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false);

        IPropGen CreateGenFromString(Type typeOfThisProperty, string value, bool useDefault, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false);

        IPropGen CreateGenWithNoValue(Type typeOfThisProperty, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false);

        IPropGen CreatePropInferType(object value, string propertyName, object extraInfo, bool hasStorage);

        Func<T, T, bool> GetRefEqualityComparer<T>();

        object GetDefaultValue(Type propertyType, string propertyName = null);

        T GetDefaultValue<T>(string propertyName = null);

        T GetValueFromObject<T>(object value);

        T GetValueFromString<T>(string value);

        bool IsTypeSolid(object value, Type propertyType);

        IConvertValues ValueConverter { get; }
    }
}