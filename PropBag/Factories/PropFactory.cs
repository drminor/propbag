using DRM.PropBag.Collections;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.TypeSafePropertyBag;
using DRM.PropBag.Caches;

namespace DRM.PropBag
{
    #region Type Aliases
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;

    using PropBagType = PropBag;
    #endregion

    public class PropFactory : AbstractPropFactory
    {
        public DelegateCacheProvider<PropBagType> DelegateCacheProvider { get; }

        public override bool ProvidesStorage
        {
            get { return true; }
        }

        override public int DoSetCacheCount
        {
            get
            {
                return DelegateCacheProvider.DoSetDelegateCache.Count;
            }
        }

        override public int CreatePropFromStringCacheCount
        {
            get
            {
                return DelegateCacheProvider.CreatePropFromStringCache.Count;
            }
        }

        override public int CreatePropWithNoValCacheCount
        {
            get
            {
                return DelegateCacheProvider.CreatePropWithNoValCache.Count;
            }
        }

        public PropFactory
            (
                PSAccessServiceProviderType propStoreAccessServiceProvider,
                ResolveTypeDelegate typeResolver,
                IConvertValues valueConverter
            )
            : base(propStoreAccessServiceProvider, typeResolver, valueConverter)
        {
            DelegateCacheProvider = new DelegateCacheProvider<PropBagType>();

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
            Func<CT, CT, bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;
            GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

            ICPropPrivate<CT, T> prop = new CProp<CT, T>(initialValue, getDefaultValFunc, typeIsSolid, hasStorage,
                comparer);
            return prop;
        }

        public override ICPropPrivate<CT, T> CreateWithNoValue<CT, T>(
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;

            GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>; // this.GetDefaultValue<T>;

            ICPropPrivate<CT, T> prop = new CProp<CT, T>(getDefaultValFunc, typeIsSolid, hasStorage,
                comparer);
            return prop;
        }

        #endregion

        #region Propety-type property creators

        // TODO: Need to create IPropPrivate<T> 
        public override IProp<T> Create<T>(
            T initialValue,
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<T,T,bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>; // this.GetDefaultValue<T>;
            IProp<T> prop = new Prop<T>(initialValue, getDefaultValFunc, typeIsSolid: typeIsSolid,
                hasStore: hasStorage, comparer: comparer);
            return prop;
        }

        public override IProp<T> CreateWithNoValue<T>(
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<T,T,bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>; // this.GetDefaultValue<T>;
            IProp<T> prop = new Prop<T>(getDefaultValFunc, typeIsSolid: typeIsSolid,
                hasStore: hasStorage, comparer: comparer);
            return prop;
        }

        #endregion

        #region Generic property creators

        public override IProp CreateGenFromObject(Type typeOfThisProperty,
            object value,
            PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            if (propKind == PropKindEnum.Prop)
            {
                CreatePropFromObjectDelegate propCreator = GetPropCreator(typeOfThisProperty);
                IProp prop = (IProp)propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else if (propKind == PropKindEnum.Collection)
            {
                CreateCPropFromObjectDelegate propCreator = GetCPropCreator(typeOfThisProperty, itemType);
                IProp prop = (IProp)propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);
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
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            if (propKind == PropKindEnum.Prop)
            {
                CreatePropFromStringDelegate propCreator = GetPropFromStringCreator(typeOfThisProperty);
                IProp prop = (IProp)propCreator(this, value, useDefault, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            } 
            else if(propKind == PropKindEnum.Collection)
            {
                CreateCPropFromStringDelegate propCreator = GetCPropFromStringCreator(typeOfThisProperty, itemType);
                IProp prop = (IProp)propCreator(this, value, useDefault, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);
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
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            if (propKind == PropKindEnum.Prop)
            {
                CreatePropWithNoValueDelegate propCreator = GetPropWithNoValueCreator(typeOfThisProperty);
                IProp prop = (IProp)propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else if (propKind == PropKindEnum.Collection)
            {
                CreateCPropWithNoValueDelegate propCreator = GetCPropWithNoValueCreator(typeOfThisProperty, itemType);
                IProp prop = (IProp)propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else
            {
                throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
            }
        }

        #endregion

        #region Delegate Cache Support

        #region Collection-Type Methods

        //// From Object
        //protected virtual CreateCPropFromObjectDelegate GetCPropCreator(Type collectionType, Type itemType)
        //{
        //    MethodInfo mi = gmtType.GetMethod("CreateCPropFromObject",
        //        BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(collectionType, itemType);

        //    CreateCPropFromObjectDelegate result = (CreateCPropFromObjectDelegate)Delegate.
        //        CreateDelegate(typeof(CreateCPropFromObjectDelegate), mi);

        //    return result;
        //}

        //// From String
        //protected virtual CreateCPropFromStringDelegate GetCPropFromStringCreator(Type collectionType, Type itemType)
        //{
        //    MethodInfo mi = gmtType.GetMethod("CreateCPropFromString",
        //        BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(collectionType, itemType);

        //    CreateCPropFromStringDelegate result = (CreateCPropFromStringDelegate)Delegate.
        //        CreateDelegate(typeof(CreateCPropFromStringDelegate), mi);

        //    return result;
        //}

        //// With No Value
        //protected virtual CreateCPropWithNoValueDelegate GetCPropWithNoValueCreator(Type collectionType, Type itemType)
        //{
        //    MethodInfo mi = gmtType.GetMethod("CreateCPropWithNoValue",
        //        BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(collectionType, itemType);

        //    CreateCPropWithNoValueDelegate result = (CreateCPropWithNoValueDelegate)Delegate.
        //        CreateDelegate(typeof(CreateCPropWithNoValueDelegate), mi);

        //    return result;
        //}

        #endregion

        #region Property-Type Methods

        //// From Object
        //protected virtual CreatePropFromObjectDelegate GetPropCreator(Type typeOfThisValue)
        //{
        //    MethodInfo mi = gmtType.GetMethod("CreatePropFromObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
        //    CreatePropFromObjectDelegate result = (CreatePropFromObjectDelegate)Delegate.CreateDelegate(typeof(CreatePropFromObjectDelegate), mi);

        //    return result;
        //}

        // From String
        protected new CreatePropFromStringDelegate GetPropFromStringCreator(Type typeOfThisValue)
        {
            CreatePropFromStringDelegate result = DelegateCacheProvider.CreatePropFromStringCache.GetOrAdd(typeOfThisValue);
            return result;
        }

        // With No Value
        protected new CreatePropWithNoValueDelegate GetPropWithNoValueCreator(Type typeOfThisValue)
        {
            CreatePropWithNoValueDelegate result = DelegateCacheProvider.CreatePropWithNoValCache.GetOrAdd(typeOfThisValue);
            return result;
        }

        #endregion Property-Type Methods

        #endregion Delegate Cache Support
    }
}
