using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    using SubCacheType = ICacheSubscriptions<UInt32>;
    using LocalBinderType = IBindLocalProps<UInt32>;

    public class PropExtStoreFactory : AbstractPropFactory
    {
        object Stuff { get; }

        public override bool ProvidesStorage
        {
            get { return false; }
        }

        #region Constructors

        public PropExtStoreFactory
            (
                object stuff, 
                PSAccessServiceProviderType propStoreAccessServiceProvider,
                //SubCacheType subscriptionManager,
                LocalBinderType localBinder,
                ResolveTypeDelegate typeResolver,
                IConvertValues valueConverter
            )
            : base(propStoreAccessServiceProvider, /*subscriptionManager, */localBinder, typeResolver, valueConverter)
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
            EventHandler<PCTypedEventArgs<CT>> doWhenChangedX = null, bool doAfterNotify = false, Func<CT, CT, bool> comparer = null)
        {
            ICPropPrivate<CT, T> prop = null;
            return prop;
        }

        public override ICPropPrivate<CT, T> CreateWithNoValue<CT, T>(
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            EventHandler<PCTypedEventArgs<CT>> doWhenChangedX = null, bool doAfterNotify = false, Func<CT, CT, bool> comparer = null)
        {
            ICPropPrivate<CT, T> prop = null;
            return prop;
        }

        #endregion

        #region Propety-type property creators

        public override IProp<T> Create<T>(T initialValue,
            PropNameType propertyName, object extraInfo = null,
            bool dummy = true, bool typeIsSolid = true,
            EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with inital values.");

            //return CreateWithNoValue(propertyName, extraInfo, dummy, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
        }

        public override IProp<T> CreateWithNoValue<T>(
            PropNameType propertyName, object extraInfo = null,
            bool dummy = true, bool typeIsSolid = true,
            EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null,
            //EventHandler<PropertyChangedWithTValsEventArgs<T>> doWhenChangedX = null,
            bool doAfterNotify = false, Func<T,T,bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;
            GetDefaultValueDelegate<T> getDefaultValFunc = this.GetDefaultValue<T>;

            PropExternStore<T> propWithExtStore = new PropExternStore<T>(propertyName,
                extraInfo, getDefaultValFunc, typeIsSolid: typeIsSolid, comparer: comparer,
                doWhenChangedX: doWhenChangedX, doAfterNotify: doAfterNotify);

            return propWithExtStore;
        }

        #endregion

        #region Generic Prop Creators

        public override IProp CreateGenFromObject(Type typeOfThisProperty,
            object value,
            PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with inital values.");
        }

        public override IProp CreateGenFromString(Type typeOfThisProperty,
            string value, bool useDefault,
            PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with inital values.");
        }

        public override IProp CreateGenWithNoValue(Type typeOfThisProperty,
            PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            CreatePropWithNoValueDelegate propCreator = GetPropWithNoValueCreator(typeOfThisProperty);
            IProp prop = (IProp)propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);

            return prop;
        }

        #endregion
    }

}
