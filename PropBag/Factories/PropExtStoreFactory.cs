using DRM.PropBag.Caches;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DRM.PropBag
{

    using PropNameType = String;

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
                IProvideDelegateCaches delegateCacheProvider,
                IConvertValues valueConverter,
                ResolveTypeDelegate typeResolver,
                object stuff
            )
            : base
            (
                delegateCacheProvider,
                valueConverter,
                typeResolver
            )
        {
            // Info to help us set up the getters and setters
            Stuff = stuff;
        }

        #endregion

        #region Enumerable-Type Prop Creation 

        #endregion

        #region IObsCollection<T> and ObservableCollection<T> Prop Creation

        // TODO: Implement Create Collection With Initial Value.
        public override ICProp<CT, T> Create<CT, T>(
            CT initialValue,
            PropNameType propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null)
        {
            throw new NotImplementedException("PropExtStoreFactory has not implemented the Create Collection Prop with Initial Value.");
        }

        //public override ICPropFB<CT, T> CreateFB<CT, T>(
        //    CT initialValue,
        //    string propertyName, object extraInfo = null,
        //    PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
        //    Func<CT, CT, bool> comparer = null)
        //{
        //    throw new NotImplementedException("PropExtStoreFactory has not implemented the Create Collection Prop with Initial Value.");
        //}

        // TODO: Implement Create Collection With No Value.
        public override ICProp<CT, T> CreateWithNoValue<CT, T>(
            PropNameType propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null)
        {
            throw new NotImplementedException("PropExtStoreFactory has not implemented the Create Collection Prop with No Value.");

            //ICPropPrivate<CT, T> prop = null;
            //return prop;
        }

        #endregion

        #region CollectionViewSource Prop Creation

        //public override IProp CreateCVSProp<TCVS, T>(PropNameType propertyName)
        //{
        //    throw new NotImplementedException("This feature is not implemented by the 'standard' implementation, please use WPFPropfactory or similar.");
        //}

        #region DataSource Creation

        public override ClrMappedDSP<TDestination> CreateMappedDS<TSource, TDestination>(uint propId, PropKindEnum propKind, IDoCRUD<TSource> dal, IPropStoreAccessService<uint, string> storeAccesor, IPropBagMapper<TSource, TDestination> mapper)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Scalar Prop Creation

        public override IProp<T> Create<T>(T initialValue,
            PropNameType propertyName, object extraInfo = null,
            PropStorageStrategyEnum dummy = PropStorageStrategyEnum.External, bool typeIsSolid = true,
            Func<T,T,bool> comparer = null)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with initial values.");
        }

        public override IProp<T> CreateWithNoValue<T>(
            PropNameType propertyName, object extraInfo = null,
            PropStorageStrategyEnum dummy = PropStorageStrategyEnum.External, bool typeIsSolid = true,
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
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with initial values.");
        }

        public override IProp CreateGenFromString(Type typeOfThisProperty,
            string value, bool useDefault,
            PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with initial values.");
        }

        public override IProp CreateGenWithNoValue(Type typeOfThisProperty,
            PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            CreatePropWithNoValueDelegate propCreator = GetPropWithNoValueCreator(typeOfThisProperty);
            IProp prop = propCreator(this, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                comparer: comparer, useRefEquality: useRefEquality);

            return prop;
        }

        // TODO: Implement GetPropWithNoValueCreator
        protected override CreatePropWithNoValueDelegate GetPropWithNoValueCreator(Type typeOfThisValue)
        {
            throw new NotImplementedException("PropExtStoreFactory has not yet implemented the GetPropWithNoValueCreator method.");
        }

        public override IProvideADataSourceProvider GetDSProviderProvider(uint propId, PropKindEnum propKind, object iDoCrudDataSource, IPropStoreAccessService<uint, string> storeAccesor, IMapperRequest mr)
        {
            throw new NotImplementedException();
        }


        #endregion
    }

}
