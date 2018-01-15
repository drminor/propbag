using DRM.PropBag.Collections;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

using DRM.PropBag.Caches;

namespace DRM.PropBag
{
    using PropNameType = String;


    public class PropFactory : AbstractPropFactory
    {
        public override bool ProvidesStorage => true;

        //public PropFactory
        //    (
        //        ResolveTypeDelegate typeResolver,
        //        IConvertValues valueConverter
        //    )
        //    : this
        //    (
        //        typeResolver, 
        //        valueConverter,
        //        new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates))
        //    )
        //{
        //}

        public PropFactory
            (
                IProvideDelegateCaches delegateCacheProvider,
                IConvertValues valueConverter,
                ResolveTypeDelegate typeResolver
            )
            : base
            (
                delegateCacheProvider,
                valueConverter, // GetValueConverter(valueConverter, delegateCacheProvider)
                typeResolver
            )
        {
        }

        //// TODO: Require the valueConverter parameter to be supplied.
        //// TODO: Separate out the TypeDescBasedTConverterCache from IProvideDelegateCaches
        ////      and make PropFactory require a supplied TypeDescBasedTConverterCache
        ////      and create an interface that TypeDescBasedTConverterCache can implement.
        //private static IConvertValues GetValueConverter(IConvertValues suppliedValueConverter, IProvideDelegateCaches delegateCacheProvider)
        //{
        //    IConvertValues result = suppliedValueConverter ?? new PropFactoryValueConverter(delegateCacheProvider.TypeDescBasedTConverterCache);
        //    return result;
        //}

        #region Enumerable-Type Prop Creation 

        #endregion

        #region IObsCollection<T> and ObservableCollection<T> Prop Creation

        public override ICProp<CT, T> Create<CT, T>(
            CT initialValue,
            string propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null) 
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;
            GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

            ICProp<CT, T> prop = new CProp<CT, T>(initialValue, getDefaultValFunc, typeIsSolid, storageStrategy, comparer);
            return prop;
        }

        //public override ICPropFB<CT, T> CreateFB<CT, T>(
        //    CT initialValue,
        //    string propertyName, object extraInfo = null,
        //    PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
        //    Func<CT, CT, bool> comparer = null) 
        //{
        //    if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;
        //    GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

        //    ICPropFB<CT, T> prop = new CPropFB<CT, T>(initialValue, getDefaultValFunc, typeIsSolid, hasStorage, comparer);
        //    return prop;
        //}

        public override ICProp<CT, T> CreateWithNoValue<CT, T>(
        PropNameType propertyName, object extraInfo = null,
        PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
        Func<CT, CT, bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;

            GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

            ICProp<CT, T> prop = new CProp<CT, T>(getDefaultValFunc, typeIsSolid, storageStrategy, comparer);
            return prop;
        }

        #endregion

        #region CollectionViewSource Prop Creation

        //public override IProp CreateCVSProp<TCVS, T>(PropNameType propertyName)
        //{
        //    throw new NotImplementedException("This feature is not implemented by the 'standard' implementation, please use WPFPropfactory or similar.");
        //}

        //public override IProp CreateCVProp<T>(string propertyName)
        //{
        //    throw new NotImplementedException("This feature is not implemented by the 'standard' implementation, please use WPFPropfactory or similar.");
        //}

        #endregion

        #region Scalar Prop Creation

        public override IProp<T> Create<T>(
            T initialValue,
            PropNameType propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<T, T, bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>;
            IProp<T> prop = new Prop<T>(initialValue, getDefaultValFunc, typeIsSolid: typeIsSolid, storageStrategy: storageStrategy, comparer: comparer);
            return prop;
        }

        public override IProp<T> CreateWithNoValue<T>(
            PropNameType propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<T, T, bool> comparer = null)
        {
            // Supply a comparer, if one was not supplied by the caller.
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            // Use the Get Default Value function supplied or provided by this Prop Factory.
            GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>;

            if (storageStrategy == PropStorageStrategyEnum.Internal)
            {
                // Regular Prop with Internal Storage -- Just don't have a value as yet.
                IProp<T> prop = new Prop<T>(getDefaultValFunc, typeIsSolid: typeIsSolid, storageStrategy: storageStrategy, comparer: comparer);
                return prop;
            }
            else
            {
                // Prop With External Store, or this is a Prop that supplies a Virtual (aka Caclulated) value from an internal source or from LocalBindings
                // This implementation simply creates a Property that will always have the default value for type T.
                IProp<T> prop = new PropNoStore<T>(getDefaultValFunc, typeIsSolid: typeIsSolid, storageStrategy: storageStrategy, comparer: comparer);
                return prop;
            }
        }

        #endregion

        #region Generic Property Creation

        #endregion
    }
}
