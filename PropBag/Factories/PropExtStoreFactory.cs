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
        #region Private Members

        object Stuff { get; }

        #endregion

        #region Public Properties

        public override bool ProvidesStorage => false;

        #endregion

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

        #region ObservableCollection<T> Prop Creation

        // TODO: Implement Create Collection With Initial Value.
        public override ICProp<CT, T> Create<CT, T>(
            CT initialValue,
            PropNameType propertyName,
            object extraInfo,
            PropStorageStrategyEnum storageStrategy,
            bool typeIsSolid,
            Func<CT, CT, bool> comparer)
        {
            throw new NotImplementedException("PropExtStoreFactory has not implemented the Create Collection Prop with Initial Value.");
        }

        // TODO: Implement Create Collection With No Value.
        public override ICProp<CT, T> CreateWithNoValue<CT, T>(
            PropNameType propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null)
        {
            throw new NotImplementedException("PropExtStoreFactory has not implemented the Create Collection Prop with No Value.");
        }

        #endregion

        #region CollectionViewSource Prop Creation

        #endregion

        #region DataSource Creation

        //public override ClrMappedDSP<TDestination> CreateMappedDS<TSource, TDestination>(uint propId, PropKindEnum propKind, IDoCRUD<TSource> dal, IPropStoreAccessService<uint, string> storeAccesor, IPropBagMapper<TSource, TDestination> mapper)
        //{
        //    throw new NotImplementedException();
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
            bool comparerIsRefEquality,
            Func<string, T> getDefaultValFunc)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with initial values.");
        }

        public override IProp<T> CreateWithNoValue<T>
            (
            PropNameType propertyName,
            object extraInfo,
            PropStorageStrategyEnum storageStrategy,
            bool typeIsSolid,
            Func<T, T, bool> comparer,
            bool comparerIsRefEquality,
            Func<string, T> getDefaultValFunc
            )
        {
            IPropTemplate<T> propTemplate = GetPropTemplate<T>(PropKindEnum.Prop, storageStrategy, comparer, comparerIsRefEquality, getDefaultValFunc);

            IProp<T> prop;

            switch (storageStrategy)
            {
                case PropStorageStrategyEnum.Internal:
                    {
                        // Regular Prop with Internal Storage -- Just don't have a value as yet.
                        prop = new Prop<T>(propertyName, typeIsSolid, propTemplate);
                        break;
                    }
                case PropStorageStrategyEnum.External:
                    {
                        // Create a Prop that uses an external storage source.
                        prop = new PropExternStore<T>(propertyName, extraInfo, typeIsSolid, propTemplate);
                        break;
                    }
                case PropStorageStrategyEnum.Virtual:
                    {
                        // This is a Prop that supplies a Virtual (aka Caclulated) value from an internal source or from LocalBindings
                        // This implementation simply creates a Property that will always have the default value for type T.
                        prop = new PropNoStore<T>(propertyName, typeIsSolid, propTemplate);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException($"{storageStrategy} is not supported or is not recognized.");
                    }
            }

            return prop;
        }

        #endregion

        #region Generic Prop Creators

        //public override IProp CreateGenFromObject(Type typeOfThisProperty,
        //    object value,
        //    PropNameType propertyName, object extraInfo,
        //    PropStorageStrategyEnum storageStrategy, bool isTypeSolid, PropKindEnum propKind,
        //    Delegate comparer, bool useRefEquality = false, Type itemType = null)
        //{
        //    throw new NotImplementedException("External Store Factory doesn't know how to create properties with initial values.");
        //}

        #endregion
    }

}
