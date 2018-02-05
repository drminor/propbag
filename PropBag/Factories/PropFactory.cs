using DRM.PropBag.Caches;
using DRM.PropBag.Collections;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections.Generic;

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

        #region ObservableCollection<T> Prop Creation

        public override ICProp<CT, T> Create<CT, T>
        (
            CT initialValue,
            string propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null
        ) 
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;
            Func<string, CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

            ICProp<CT, T> prop = new CProp<CT, T>(initialValue, getDefaultValFunc, typeIsSolid, storageStrategy, comparer);
            return prop;
        }

        public override ICProp<CT, T> CreateWithNoValue<CT, T>
        (
            PropNameType propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null
        )
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;

            Func<string, CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

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
            PropNameType propertyName,
            object extraInfo,
            PropStorageStrategyEnum storageStrategy,
            bool typeIsSolid,
            Func<T, T, bool> comparer,
            Func<string, T> getDefaultValFunc)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            if (getDefaultValFunc == null) getDefaultValFunc = ValueConverter.GetDefaultValue<T>;

            //IProp<T> prop = new Prop<T>(initialValue, getDefaultValFunc, typeIsSolid: typeIsSolid, storageStrategy: storageStrategy, comparer: comparer);

            PropTemplateTyped<T> propTemplateTyped = new PropTemplateTyped<T>(PropKindEnum.Prop, storageStrategy, comparer, getDefaultValFunc);

            IProp<T> prop = new Prop<T>(propertyName, initialValue, typeIsSolid, propTemplateTyped);
            return prop;
        }

        //public override IProp<T> Create<T>(
        //    T initialValue,
        //    PropNameType propertyName, object extraInfo = null,
        //    PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
        //    Func<T, T, bool> comparer = null)
        //{
        //    if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

        //    GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>;
        //    IProp<T> prop = new Prop<T>(initialValue, getDefaultValFunc, typeIsSolid: typeIsSolid, storageStrategy: storageStrategy, comparer: comparer);
        //    return prop;
        //}

        public override IProp<T> CreateWithNoValue<T>
            (
            PropNameType propertyName,
            object extraInfo,
            PropStorageStrategyEnum storageStrategy,
            bool typeIsSolid,
            Func<T, T, bool> comparer,
            Func<string, T> getDefaultValFunc
            )
        {
            // Supply a comparer, if one was not supplied by the caller.
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            // Use the Get Default Value function supplied or provided by this Prop Factory.
            if(getDefaultValFunc == null) getDefaultValFunc = ValueConverter.GetDefaultValue<T>;

            PropTemplateTyped<T> propTemplateTyped = new PropTemplateTyped<T>(PropKindEnum.Prop, storageStrategy, comparer, getDefaultValFunc);

            //IProp<T> prop = new Prop_New<T>(propertyName, typeIsSolid, propTemplateTyped);
            //return prop;

            if (storageStrategy == PropStorageStrategyEnum.Internal)
            {
                // Regular Prop with Internal Storage -- Just don't have a value as yet.
                //IProp<T> prop = new Prop<T>(getDefaultValFunc, typeIsSolid: typeIsSolid, storageStrategy: storageStrategy, comparer: comparer);

                IProp<T> prop = new Prop<T>(propertyName, typeIsSolid, propTemplateTyped);

                return prop;
            }
            else
            {
                // Prop With External Store, or this is a Prop that supplies a Virtual (aka Caclulated) value from an internal source or from LocalBindings
                // This implementation simply creates a Property that will always have the default value for type T.
                //IProp<T> prop = new PropNoStore<T>(getDefaultValFunc, typeIsSolid: typeIsSolid, storageStrategy: storageStrategy, comparer: comparer);

                IProp<T> prop = new PropNoStore_New<T>(propertyName, typeIsSolid, propTemplateTyped);
                return prop;
            }
        }

        public override ClrMappedDSP<TDestination> CreateMappedDS<TSource, TDestination>(uint propId, PropKindEnum propKind, IDoCRUD<TSource> dal, IPropStoreAccessService<uint, string> storeAccesor, IPropBagMapper<TSource, TDestination> mapper)
        {
            throw new NotImplementedException();
        }

        public override IProvideADataSourceProvider GetDSProviderProvider(uint propId, PropKindEnum propKind, object iDoCrudDataSource, IPropStoreAccessService<uint, string> storeAccesor, IMapperRequest mr)
        {
            throw new NotImplementedException();
        }


        #endregion

        #region Generic Property Creation

        #endregion
    }
}
