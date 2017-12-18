using DRM.PropBag.Collections;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

using DRM.PropBag.Caches;

namespace DRM.PropBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;

    using PropBagType = PropBag;

    public class PropFactory : AbstractPropFactory
    {
        public override bool ProvidesStorage => true;

        public PropFactory
            (
                PSAccessServiceProviderType propStoreAccessServiceProvider,
                //IProvideDelegateCaches delegateCacheProvider,
                ResolveTypeDelegate typeResolver,
                IConvertValues valueConverter
            )
            : base(propStoreAccessServiceProvider, new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates)), typeResolver, valueConverter)
        {
        }

        //#region Collection-type property creators

        //public override ICPropPrivate<CT, T> Create<CT, T>(
        //    CT initialValue,
        //    string propertyName, object extraInfo = null,
        //    bool hasStorage = true, bool typeIsSolid = true,
        //    Func<CT, CT, bool> comparer = null)
        //{
        //    if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;
        //    GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

        //    ICPropPrivate<CT, T> prop = new CProp<CT, T>(initialValue, getDefaultValFunc, typeIsSolid, hasStorage, comparer);
        //    return prop;
        //}

        //public override ICPropPrivate<CT, T> CreateWithNoValue<CT, T>(
        //    PropNameType propertyName, object extraInfo = null,
        //    bool hasStorage = true, bool typeIsSolid = true,
        //    Func<CT, CT, bool> comparer = null)
        //{
        //    if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;

        //    GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

        //    ICPropPrivate<CT, T> prop = new CProp<CT, T>(getDefaultValFunc, typeIsSolid, hasStorage, comparer);
        //    return prop;
        //}

        //#endregion

        //#region Propety-type property creators

        //public override IProp<T> Create<T>(
        //    T initialValue,
        //    PropNameType propertyName, object extraInfo = null,
        //    bool hasStorage = true, bool typeIsSolid = true,
        //    Func<T,T,bool> comparer = null)
        //{
        //    if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

        //    GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>; 
        //    IProp<T> prop = new Prop<T>(initialValue, getDefaultValFunc, typeIsSolid: typeIsSolid, hasStore: hasStorage, comparer: comparer);
        //    return prop;
        //}

        //public override IProp<T> CreateWithNoValue<T>(
        //    PropNameType propertyName, object extraInfo = null,
        //    bool hasStorage = true, bool typeIsSolid = true,
        //    Func<T,T,bool> comparer = null)
        //{
        //    if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

        //    GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>; 
        //    IProp<T> prop = new Prop<T>(getDefaultValFunc, typeIsSolid: typeIsSolid, hasStore: hasStorage, comparer: comparer);
        //    return prop;
        //}

        //#endregion

        //#region Generic property creators

        //public override IProp CreateGenFromObject(Type typeOfThisProperty,
        //    object value,
        //    PropNameType propertyName, object extraInfo,
        //    bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
        //    Delegate comparer, bool useRefEquality = false, Type itemType = null)
        //{
        //    if (propKind == PropKindEnum.Prop)
        //    {
        //        CreatePropFromObjectDelegate propCreator = GetPropCreator(typeOfThisProperty);
        //        IProp prop = propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
        //            comparer: comparer, useRefEquality: useRefEquality);

        //        return prop;
        //    }
        //    else if (propKind == PropKindEnum.Collection)
        //    {
        //        CreateCPropFromObjectDelegate propCreator = GetCPropCreator(typeOfThisProperty, itemType);
        //        IProp prop = propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
        //            comparer: comparer, useRefEquality: useRefEquality);

        //        return prop;
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
        //    }
        //}

        //public override IProp CreateGenFromString(Type typeOfThisProperty,
        //    string value, bool useDefault,
        //    PropNameType propertyName, object extraInfo,
        //    bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
        //    Delegate comparer, bool useRefEquality = false, Type itemType = null)
        //{
        //    if (propKind == PropKindEnum.Prop)
        //    {
        //        CreatePropFromStringDelegate propCreator = GetPropFromStringCreator(typeOfThisProperty);
        //        IProp prop = propCreator(this, value, useDefault, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
        //            comparer: comparer, useRefEquality: useRefEquality);

        //        return prop;
        //    } 
        //    else if(propKind == PropKindEnum.Collection)
        //    {
        //        CreateCPropFromStringDelegate propCreator = GetCPropFromStringCreator(typeOfThisProperty, itemType);
        //        IProp prop = propCreator(this, value, useDefault, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
        //            comparer: comparer, useRefEquality: useRefEquality);

        //        return prop;
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
        //    }
        //}

        //public override IProp CreateGenWithNoValue(Type typeOfThisProperty,
        //    PropNameType propertyName, object extraInfo,
        //    bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
        //    Delegate comparer, bool useRefEquality = false, Type itemType = null)
        //{
        //    if (propKind == PropKindEnum.Prop)
        //    {
        //        CreatePropWithNoValueDelegate propCreator = GetPropWithNoValueCreator(typeOfThisProperty);
        //        IProp prop = propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
        //            comparer: comparer, useRefEquality: useRefEquality);

        //        return prop;
        //    }
        //    else if (propKind == PropKindEnum.Collection)
        //    {
        //        CreateCPropWithNoValueDelegate propCreator = GetCPropWithNoValueCreator(typeOfThisProperty, itemType);
        //        IProp prop = propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
        //            comparer: comparer, useRefEquality: useRefEquality);

        //        return prop;
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
        //    }
        //}

        //#endregion
    }
}
