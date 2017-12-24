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
                ResolveTypeDelegate typeResolver,
                IConvertValues valueConverter
            )
            : this
            (
                  propStoreAccessServiceProvider,
                  typeResolver, 
                  valueConverter,
                  new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates))
            )
        {
        }

        public PropFactory
        (
            PSAccessServiceProviderType propStoreAccessServiceProvider,
            ResolveTypeDelegate typeResolver,
            IConvertValues valueConverter,
            IProvideDelegateCaches delegateCacheProvider
        )  : base
        (
            propStoreAccessServiceProvider,
            delegateCacheProvider,
            typeResolver,
            GetValueConverter(valueConverter, delegateCacheProvider)
        )
        {
        }

        private static IConvertValues GetValueConverter(IConvertValues suppliedValueConverter, IProvideDelegateCaches delegateCacheProvider)
        {
            IConvertValues result = suppliedValueConverter ?? new PropFactoryValueConverter(delegateCacheProvider.TypeDescBasedTConverterCache);
            return result;
        }

        #region Enumerable-Type Prop Creation 

        #endregion

        #region IObsCollection<T> and ObservableCollection<T> Prop Creation

        public override ICProp<CT, T> Create<CT, T>(
            CT initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;
            GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

            ICProp<CT, T> prop = new CProp<CT, T>(initialValue, getDefaultValFunc, typeIsSolid, hasStorage, comparer);
            return prop;
        }

        public override ICPropFB<CT, T> CreateFB<CT, T>(
            CT initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null) 
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;
            GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

            ICPropFB<CT, T> prop = new CPropFB<CT, T>(initialValue, getDefaultValFunc, typeIsSolid, hasStorage, comparer);
            return prop;
        }

        public override ICProp<CT, T> CreateWithNoValue<CT, T>(
        PropNameType propertyName, object extraInfo = null,
        bool hasStorage = true, bool typeIsSolid = true,
        Func<CT, CT, bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;

            GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

            ICProp<CT, T> prop = new CProp<CT, T>(getDefaultValFunc, typeIsSolid, hasStorage, comparer);
            return prop;
        }

        #endregion

        #region CollectionViewSource Prop Creation

        public override IProp CreateCVSProp<TCVS, T>(PropNameType propertyName)
        {
            throw new NotImplementedException("This feature is not implemented by the 'standard' implementation, please use WPFPropfactory or similar.");
        }

        #endregion

        #region Scalar Prop Creation

        public override IProp<T> Create<T>(
            T initialValue,
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<T, T, bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>;
            IProp<T> prop = new Prop<T>(initialValue, getDefaultValFunc, typeIsSolid: typeIsSolid, hasStore: hasStorage, comparer: comparer);
            return prop;
        }

        public override IProp<T> CreateWithNoValue<T>(
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<T, T, bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>;
            IProp<T> prop = new Prop<T>(getDefaultValFunc, typeIsSolid: typeIsSolid, hasStore: hasStorage, comparer: comparer);
            return prop;
        }

        #endregion

        #region Generic Property Creation

        #endregion
    }
}
