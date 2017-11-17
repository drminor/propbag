﻿using DRM.PropBag.Collections;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.TypeSafePropertyBag;

namespace DRM.PropBag
{
    #region Type Aliases
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    //using SubCacheType = ICacheSubscriptions<SimpleExKey, UInt64, UInt32, UInt32, String>;
    using LocalBinderType = IBindLocalProps<UInt32>;
    #endregion

    public class PropFactory : AbstractPropFactory
    {
        public override bool ProvidesStorage
        {
            get { return true; }
        }

        public PropFactory
            (
                PSAccessServiceProviderType propStoreAccessServiceProvider,
                //SubCacheType subscriptionManager,
                LocalBinderType localBinder,
                ResolveTypeDelegate typeResolver,
                IConvertValues valueConverter
            )
            : base(propStoreAccessServiceProvider, /*subscriptionManager, */localBinder, typeResolver, valueConverter)
        {
        }

        // TODO: This is temporary just for testing.
        //public override Type GetTypeFromName(string typeName)
        //{
        //    throw new ApplicationException("All instances of PropFactory need to be supplied with a value of typeResolver.");
        //}


        #region Collection-type property creators

        public override ICPropPrivate<CT, T> Create<CT, T>(
            CT initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            EventHandler<PCTypedEventArgs<CT>> doWhenChangedX = null, bool doAfterNotify = false, Func<CT, CT, bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;
            GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

            ICPropPrivate<CT, T> prop = new CProp<CT, T>(initialValue, getDefaultValFunc, typeIsSolid, hasStorage,
                comparer, doWhenChangedX, doAfterNotify);
            return prop;
        }

        public override ICPropPrivate<CT, T> CreateWithNoValue<CT, T>(
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            EventHandler<PCTypedEventArgs<CT>> doWhenChangedX = null, bool doAfterNotify = false, Func<CT, CT, bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;

            GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>; // this.GetDefaultValue<T>;

            ICPropPrivate<CT, T> prop = new CProp<CT, T>(getDefaultValFunc, typeIsSolid, hasStorage,
                comparer, doWhenChangedX, doAfterNotify);
            return prop;
        }

        #endregion

        #region Propety-type property creators

        // TODO: Need to create IPropPrivate<T> 
        public override IProp<T> Create<T>(
            T initialValue,
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>; // this.GetDefaultValue<T>;
            IProp<T> prop = new Prop<T>(initialValue, getDefaultValFunc, typeIsSolid: typeIsSolid,
                hasStore: hasStorage, comparer: comparer, doWhenChangedX: doWhenChangedX, doAfterNotify: doAfterNotify);
            return prop;
        }

        public override IProp<T> CreateWithNoValue<T>(
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>; // this.GetDefaultValue<T>;
            IProp<T> prop = new Prop<T>(getDefaultValFunc, typeIsSolid: typeIsSolid,
                hasStore: hasStorage, comparer: comparer, doWhenChangedX: doWhenChangedX,
                doAfterNotify: doAfterNotify);
            return prop;
        }

        #endregion

        #region Generic property creators

        public override IProp CreateGenFromObject(Type typeOfThisProperty,
            object value,
            PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            if (propKind == PropKindEnum.Prop)
            {
                CreatePropFromObjectDelegate propCreator = GetPropCreator(typeOfThisProperty);
                IProp prop = (IProp)propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else if (propKind == PropKindEnum.Collection)
            {
                CreateCPropFromObjectDelegate propCreator = GetCPropCreator(typeOfThisProperty, itemType);
                IProp prop = (IProp)propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else
            {
                throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
            }
        }

        public override IProp CreateGenFromString(Type typeOfThisProperty,
            string value, bool useDefault,
            PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            if (propKind == PropKindEnum.Prop)
            {
                CreatePropFromStringDelegate propCreator = GetPropFromStringCreator(typeOfThisProperty);
                IProp prop = (IProp)propCreator(this, value, useDefault, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            } 
            else if(propKind == PropKindEnum.Collection)
            {
                CreateCPropFromStringDelegate propCreator = GetCPropFromStringCreator(typeOfThisProperty, itemType);
                IProp prop = (IProp)propCreator(this, value, useDefault, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else
            {
                throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
            }
        }

        public override IProp CreateGenWithNoValue(Type typeOfThisProperty,
            PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            if (propKind == PropKindEnum.Prop)
            {
                CreatePropWithNoValueDelegate propCreator = GetPropWithNoValueCreator(typeOfThisProperty);
                IProp prop = (IProp)propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else if (propKind == PropKindEnum.Collection)
            {
                CreateCPropWithNoValueDelegate propCreator = GetCPropWithNoValueCreator(typeOfThisProperty, itemType);
                IProp prop = (IProp)propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else
            {
                throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
            }
        }

        #endregion

    }

}
