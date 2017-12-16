using DRM.PropBag.Caches;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.PropBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    using SubCacheType = ICacheSubscriptions<UInt32>;

    using PropBagType = PropBag;
    using ICreatePropsType = APFGenericMethodTemplates;

    public class PropExtStoreFactory : AbstractPropFactory
    {
        //public IProvideDelegateCaches DelegateCacheProvider { get; }

        object Stuff { get; }

        public override bool ProvidesStorage
        {
            get { return false; }
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

        #region Constructors

        public PropExtStoreFactory
            (
                object stuff,
                PSAccessServiceProviderType propStoreAccessServiceProvider,
                //IProvideDelegateCaches delegateCacheProvider,
                ResolveTypeDelegate typeResolver,
                IConvertValues valueConverter
            )
            : base(propStoreAccessServiceProvider, new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates)), typeResolver, valueConverter)
        {
            // Info to help us set up the getters and setters
            Stuff = stuff;
        }

        #endregion

        #region Collection-type property creators

        public override ICPropPrivate<CT, T> Create<CT, T>(
            CT initialValue,
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null)
        {
            ICPropPrivate<CT, T> prop = null;
            return prop;
        }

        public override ICPropPrivate<CT, T> CreateWithNoValue<CT, T>(
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null)
        {
            ICPropPrivate<CT, T> prop = null;
            return prop;
        }

        #endregion

        #region Propety-type property creators

        public override IProp<T> Create<T>(T initialValue,
            PropNameType propertyName, object extraInfo = null,
            bool dummy = true, bool typeIsSolid = true,
            Func<T,T,bool> comparer = null)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with initial values.");
        }

        public override IProp<T> CreateWithNoValue<T>(
            PropNameType propertyName, object extraInfo = null,
            bool dummy = true, bool typeIsSolid = true,
            Func<T,T,bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;
            GetDefaultValueDelegate<T> getDefaultValFunc = this.GetDefaultValue<T>;

            PropExternStore<T> propWithExtStore = new PropExternStore<T>(propertyName,
                extraInfo, getDefaultValFunc, typeIsSolid: typeIsSolid, comparer: comparer);

            return propWithExtStore;
        }

        #endregion

        #region Generic Prop Creators

        public override IProp CreateGenFromObject(Type typeOfThisProperty,
            object value,
            PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with initial values.");
        }

        public override IProp CreateGenFromString(Type typeOfThisProperty,
            string value, bool useDefault,
            PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with initial values.");
        }

        public override IProp CreateGenWithNoValue(Type typeOfThisProperty,
            PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            CreatePropWithNoValueDelegate propCreator = GetPropWithNoValueCreator(typeOfThisProperty);
            IProp prop = propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                comparer: comparer, useRefEquality: useRefEquality);

            return prop;
        }

        #endregion
    }

}
