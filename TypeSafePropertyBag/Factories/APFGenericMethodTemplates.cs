using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    public class APFGenericMethodTemplates
    {
        #region Enumerable-Type Prop Creation 

        #endregion

        #region IObsCollection<T> and ObservableCollection<T> Prop Creation

        // From Object
        private static ICProp<CT, T> CreateEPropFromObject<CT, T>(IPropFactory propFactory,
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
        private static ICProp<CT, T> CreateEPropFromString<CT, T>(IPropFactory propFactory,
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

        //// From String FallBack to using ObservableCollection<T>
        //private static ICPropFB<CT, T> CreateEPropFromStringFB<CT, T>(IPropFactory propFactory,
        //    string value, bool useDefault,
        //    string propertyName, object extraInfo,
        //    PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
        //    Delegate comparer, bool useRefEquality = true) where CT : ObservableCollection<T>
        //{
        //    CT initialValue;
        //    if (useDefault)
        //    {
        //        initialValue = propFactory.ValueConverter.GetDefaultValue<CT, T>(propertyName);
        //    }
        //    else
        //    {
        //        initialValue = propFactory.GetValueFromString<CT, T>(value);
        //    }

        //    return propFactory.CreateFB<CT, T>(initialValue, propertyName, extraInfo, hasStorage, isTypeSolid,
        //        GetComparerForCollections<CT>(comparer, propFactory, useRefEquality));
        //}

        // With No Value
        private static ICProp<CT, T> CreateEPropWithNoValue<CT, T>(IPropFactory propFactory,
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

        // CollectionViewSource
        private static IProp CreateCVSProp(IPropFactory propFactory, PropNameType propertyName, IProvideAView viewProvider)
        {
            return propFactory.CreateCVSProp(propertyName, viewProvider);
        }

        // CollectionView
        private static IProp CreateCVProp(IPropFactory propFactory, PropNameType propertyName, IProvideAView viewProvider)
        {
            return propFactory.CreateCVProp(propertyName, viewProvider);
        }

        #endregion

            #region DataSource creators

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

        private static IProvideADataSourceProvider CreateMappedDSPProvider<TSource, TDestination>
            (
            IPropFactory propFactory,
            PropIdType propId,
            PropKindEnum propKind,
            object genDal, // presumably, the value of the propItem.
            PSAccessServiceInterface propStoreAccessService,
            IPropBagMapperGen genMapper  //, out CrudWithMapping<TSource, TDestination> mappedDs
            ) where TSource : class where TDestination : INotifyItemEndEdit
        {
            // Cast the genDal object back to it's original type.
            IDoCRUD<TSource> dal = (IDoCRUD<TSource>)genDal;

            // Cast the genMapper to it's typed-counterpart (All genMappers also implement IPropBagMapper<TS, TD>
            IPropBagMapper<TSource, TDestination> mapper = (IPropBagMapper<TSource, TDestination>)genMapper;

            // Now that we have performed the type casts, we can call the propFactory using "compile-time" type parameters.
            ClrMappedDSP<TDestination> mappedDSP = propFactory.CreateMappedDS<TSource, TDestination>(propId, propKind, dal, propStoreAccessService, mapper);

            IProvideADataSourceProvider result = mappedDSP;

            //mappedDs = null;
            return result;
        }

        #endregion

        #region Scalar Prop Creation

        // From Object
        private static IProp<T> CreatePropFromObject<T>(IPropFactory propFactory,
            object value,
            PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
            Delegate comparer, bool useRefEquality = false)
        {
            T initialValue = propFactory.GetValueFromObject<T>(value);

            return propFactory.Create<T>(initialValue, propertyName, extraInfo, storageStrategy, isTypeSolid,
                GetComparerForProps<T>(comparer, propFactory, useRefEquality));
        }

        // From String
        private static IProp<T> CreatePropFromString<T>(IPropFactory propFactory,
            string value, bool useDefault,
            PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
            Delegate comparer, bool useRefEquality = false)
        {
            T initialValue;
            if (useDefault)
            {
                initialValue = propFactory.ValueConverter.GetDefaultValue<T>(propertyName);
            }
            else
            {
                initialValue = propFactory.GetValueFromString<T>(value);
            }

            return propFactory.Create<T>(initialValue, propertyName, extraInfo, storageStrategy, isTypeSolid,
                GetComparerForProps<T>(comparer, propFactory, useRefEquality));
        }

        // With No Value
        private static IProp<T> CreatePropWithNoValue<T>(IPropFactory propFactory,
            PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
            Delegate comparer, bool useRefEquality = false)
        {
            return propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, isTypeSolid,
                GetComparerForProps<T>(comparer, propFactory, useRefEquality));
        }

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

