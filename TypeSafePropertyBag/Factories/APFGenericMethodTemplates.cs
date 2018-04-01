using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;

    public class APFGenericMethodTemplates
    {
        #region Enumerable-Type Prop Creation 

        #endregion

        #region ObservableCollection<T> Prop Creation

        // From Object
        private static ICProp<CT, T> CreateCPropFromObject<CT, T>(IPropFactory propFactory,
            object value,
            string propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
            Delegate comparer, bool useRefEquality = false) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
        {
            CT initialValue = propFactory.GetValueFromObject<CT>(value);

            return propFactory.Create<CT, T>(initialValue, propertyName, extraInfo, storageStrategy, isTypeSolid,
                GetComparerForCollections<CT>(comparer, propFactory, useRefEquality));
        }

        // From String
        private static ICProp<CT, T> CreateCPropFromString<CT, T>(IPropFactory propFactory,
            string value, bool useDefault,
            string propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
            Delegate comparer, bool useRefEquality = true) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
        {
            CT initialValue;
            if (useDefault)
            {
                initialValue = propFactory.ValueConverter.GetDefaultValue<CT, T>(propertyName);
            }
            else
            {
                initialValue = propFactory.GetValueFromString<CT, T>(value);
            }

            return propFactory.Create<CT, T>(initialValue, propertyName, extraInfo, storageStrategy, isTypeSolid,
                GetComparerForCollections<CT>(comparer, propFactory, useRefEquality));
        }

        // With No Value
        private static ICProp<CT, T> CreateCPropWithNoValue<CT, T>(IPropFactory propFactory,
            bool useDefault,
            PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
            Delegate comparer, bool useRefEquality = true) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
        {
            return propFactory.CreateWithNoValue<CT, T>(propertyName, extraInfo, storageStrategy, isTypeSolid,
                GetComparerForCollections<CT>(comparer, propFactory, useRefEquality));
        }

        private static Func<CT, CT, bool> GetComparerForCollections<CT>(Delegate comparer, IPropFactory propFactory, bool useRefEquality)
        {
            Func<CT, CT, bool> compr;
            if (useRefEquality || comparer == null)
                return propFactory.GetRefEqualityComparer<CT>();
            else
                return compr = (Func<CT, CT, bool>)comparer;
        }

        #endregion

        #region CollectionViewSource Prop Creation

        //// CollectionViewSource
        //private static IProp CreateCVSProp(IPropFactory propFactory, PropNameType propertyName, IProvideAView viewProvider, IPropTemplate propTemplate)
        //{
        //    return propFactory.CreateCVSProp(propertyName, viewProvider, propTemplate);
        //}

        //// CollectionView
        //private static IProp CreateCVProp(IPropFactory propFactory, PropNameType propertyName, IProvideAView viewProvider, IPropTemplate propTemplate)
        //{
        //    return propFactory.CreateCVProp(propertyName, viewProvider, propTemplate);
        //}

        #endregion

        #region DataSource creators

        //// TODO: replace IPropBagMapperGen with a IMapperRequest.
        //// So that the Mapper gets created only if it's needed.
        //private static IProvideADataSourceProvider CreateMappedDSPProvider<TSource, TDestination>
        //    (
        //    IPropFactory propFactory,
        //    PropIdType propId,
        //    PropKindEnum propKind,
        //    object genDal, // presumably, the value of the propItem.
        //    PSAccessServiceInterface propStoreAccessService,
        //    IPropBagMapperGen genMapper  //, out CrudWithMapping<TSource, TDestination> mappedDs
        //    ) where TSource : class where TDestination : INotifyItemEndEdit
        //{
        //    // Cast the genDal object back to it's original type.
        //    IDoCRUD<TSource> dal = (IDoCRUD<TSource>)genDal;

        //    // Cast the genMapper to it's typed-counterpart (All genMappers also implement IPropBagMapper<TS, TD>
        //    IPropBagMapper<TSource, TDestination> mapper = (IPropBagMapper<TSource, TDestination>)genMapper;

        //    // Now that we have performed the type casts, we can call the propFactory using "compile-time" type parameters.
        //    ClrMappedDSP<TDestination> mappedDSP = propFactory.CreateMappedDS<TSource, TDestination>(propId, propKind, dal, propStoreAccessService, mapper);

        //    //IProvideADataSourceProvider result = mappedDSP;
        //    return mappedDSP; // result;
        //}

        //private static ClrMappedDSP<TDestination> CreateMappedDS_Typed<TSource, TDestination>
        //    (
        //    IPropFactory propFactory,
        //    PropIdType propId,
        //    PropKindEnum propKind,
        //    IDoCRUD<TSource> dal,
        //    PSAccessServiceInterface propStoreAccessService,
        //    IPropBagMapper<TSource, TDestination> mapper  //, out CrudWithMapping<TSource, TDestination> mappedDs
        //    ) where TSource : class where TDestination : INotifyItemEndEdit
        //{

        //    ClrMappedDSP<TDestination> result = propFactory.CreateMappedDS<TSource, TDestination>(propId, propKind, dal, propStoreAccessService,  mapper);

        //    //mappedDs = null;
        //    return result;
        //}

        #endregion

        #region Scalar Prop Creation

        // From Object
        private static IProp<T> CreateProp<T>
        (
            IPropFactory propFactory,
            bool haveValue,
            object value,
            bool useDefault,
            PropNameType propertyName,
            object extraInfo,
            PropStorageStrategyEnum storageStrategy,
            bool isTypeSolid,
            Delegate comparer,
            bool useRefEquality,
            Delegate getDefaultValFunc
        )
        {
            Func<T, T, bool> comparerToUse = GetComparerForProps<T>(comparer, propFactory, useRefEquality);

            Func<string, T> getDefaultValFuncToUse = (Func<string, T>) getDefaultValFunc ?? propFactory.ValueConverter.GetDefaultValue<T>;

            if (haveValue || useDefault)
            {
                T initialValue;
                if (useDefault)
                {
                    initialValue = getDefaultValFuncToUse(propertyName);
                }
                else
                {
                    initialValue = propFactory.GetValueFromObject<T>(value);
                }

                return propFactory.Create(initialValue, propertyName, extraInfo, storageStrategy, isTypeSolid,
                    comparerToUse, useRefEquality, getDefaultValFuncToUse);
            }
            else
            {
                return propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, isTypeSolid,
                    comparerToUse, useRefEquality, getDefaultValFuncToUse);
            }
        }

        //// From String
        //private static IProp<T> CreatePropFromString<T>(IPropFactory propFactory,
        //    string value, bool useDefault,
        //    PropNameType propertyName, object extraInfo,
        //    PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
        //    Delegate comparer, bool useRefEquality = false)
        //{
        //    T initialValue;
        //    if (useDefault)
        //    {
        //        initialValue = propFactory.ValueConverter.GetDefaultValue<T>(propertyName);
        //    }
        //    else
        //    {
        //        initialValue = propFactory.GetValueFromString<T>(value);
        //    }

        //    return propFactory.Create<T>(initialValue, propertyName, extraInfo, storageStrategy, isTypeSolid,
        //        GetComparerForProps<T>(comparer, propFactory, useRefEquality));
        //}

        //// With No Value
        //private static IProp<T> CreatePropWithNoValue<T>(IPropFactory propFactory,
        //    PropNameType propertyName, object extraInfo,
        //    PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
        //    Delegate comparer, bool useRefEquality = false)
        //{
        //    return propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, isTypeSolid,
        //        GetComparerForProps<T>(comparer, propFactory, useRefEquality));
        //}

        private static Func<T, T, bool> GetComparerForProps<T>(Delegate comparer, IPropFactory propFactory, bool useRefEquality)
        {
            if (useRefEquality)
            {
                return propFactory.GetRefEqualityComparer<T>();
            }
            else if (comparer == null)
            {
                return null;
            }
            else
            {
                return (Func<T, T, bool>)comparer;
            }
        }

        #endregion
    }
}

