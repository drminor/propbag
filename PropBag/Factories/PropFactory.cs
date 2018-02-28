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
        #region Constructor

        public PropFactory
        (
            IProvideDelegateCaches delegateCacheProvider,
            IConvertValues valueConverter,
            ResolveTypeDelegate typeResolver
        )
        : base
            (
                delegateCacheProvider,
                valueConverter,
                typeResolver
            )
        {
        }

        #endregion

        #region Public Properties

        public override bool ProvidesStorage => true;

        #endregion

        #region Enumerable-Type Prop Creation 

        #endregion

        #region ObservableCollection<T> Prop Creation

        public override ICProp<CT, T> Create<CT, T>
        (
            CT initialValue,
            string propertyName,
            object extraInfo,
            PropStorageStrategyEnum storageStrategy,
            bool typeIsSolid,
            Func<CT, CT, bool> comparer
        ) 
        {
            IPropTemplate<CT> propTemplate = GetPropTemplate<CT>(PropKindEnum.ObservableCollection, storageStrategy, comparer, null);

            ICProp<CT, T> prop = new CProp<CT, T>(propertyName, initialValue, typeIsSolid, propTemplate);
            return prop;
        }

        public override ICProp<CT, T> CreateWithNoValue<CT, T>
        (
            PropNameType propertyName,
            object extraInfo,
            PropStorageStrategyEnum storageStrategy,
            bool typeIsSolid,
            Func<CT, CT, bool> comparer
        )
        {
            IPropTemplate<CT> propTemplate = GetPropTemplate<CT>(PropKindEnum.ObservableCollection, storageStrategy, comparer, null);

            ICProp<CT, T> prop = new CProp<CT, T>(propertyName, typeIsSolid, propTemplate);
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

        public override IProp<T> Create<T>
        (
            T initialValue,
            PropNameType propertyName,
            object extraInfo,
            PropStorageStrategyEnum storageStrategy,
            bool typeIsSolid,
            Func<T, T, bool> comparer,
            Func<string, T> getDefaultValFunc
        )
        {
            IPropTemplate<T> propTemplate = GetPropTemplate<T>(PropKindEnum.Prop, storageStrategy, comparer, getDefaultValFunc);
            propTemplate.PropCreator = CookedScalarPropCreator<T>;

            IProp<T> prop = new Prop<T>(propertyName, initialValue, typeIsSolid, propTemplate);
            return prop;
        }

        private static IProp CookedScalarPropCreator<T>(string propertyName2, object initialValue2, bool typeIsSolid2, IPropTemplate propTemplate2)
        {
            IProp<T> prop2 = new Prop<T>(propertyName2, (T)initialValue2, typeIsSolid2, (IPropTemplate<T>)propTemplate2);
            return prop2;
        }

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
            IPropTemplate<T> propTemplate = GetPropTemplate<T>(PropKindEnum.Prop, storageStrategy, comparer, getDefaultValFunc);

            if(storageStrategy == PropStorageStrategyEnum.Internal)
            {
                propTemplate.PropCreator = CookedScalarPropCreatorNoVal<T>;
            }
            else
            {
                propTemplate.PropCreator = CookedScalarPropCreatorNoStore<T>;
            }

            if (storageStrategy == PropStorageStrategyEnum.Internal)
            {
                // Regular Prop with Internal Storage -- Just don't have a value as yet.
                IProp<T> prop = new Prop<T>(propertyName, typeIsSolid, propTemplate);
                return prop;
            }
            else
            {
                // Prop With External Store, or this is a Prop that supplies a Virtual (aka Caclulated) value from an internal source or from LocalBindings
                // This implementation simply creates a Property that will always have the default value for type T.
                IProp<T> prop = new PropNoStore<T>(propertyName, typeIsSolid, propTemplate);
                return prop;
            }
        }

        private static IProp CookedScalarPropCreatorNoVal<T>(string propertyName2, object initialValue2, bool typeIsSolid2, IPropTemplate propTemplate2)
        {
            // Regular Prop with Internal Storage -- Just don't have a value as yet.
            IProp<T> prop = new Prop<T>(propertyName2, typeIsSolid2, (IPropTemplate<T>)propTemplate2);
            return prop;
        }

        private static IProp CookedScalarPropCreatorNoStore<T>(string propertyName2, object initialValue2, bool typeIsSolid2, IPropTemplate propTemplate2)
        {
            // Prop With External Store, or this is a Prop that supplies a Virtual (aka Caclulated) value from an internal source or from LocalBindings
            // This implementation simply creates a Property that will always have the default value for type T.
            IProp<T> prop = new PropNoStore<T>(propertyName2, typeIsSolid2, (IPropTemplate<T>)propTemplate2);
            return prop;
        }


        #endregion

        #region DataSource Provider Creation

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
