using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    using PropNameType = String;

    public class WPFPropFactory : PropFactory
    {
        #region Constructor

        public WPFPropFactory
            (
                ResolveTypeDelegate typeResolver,
                IConvertValues valueConverter,
                IProvideDelegateCaches delegateCacheProvider
            )
            : base(typeResolver, valueConverter, delegateCacheProvider)
        {
        }

        #endregion

        #region CollectionViewSource property creators

        #endregion

        #region Collection-type property creators

        //public override IETypedProp<CT, T> Create<CT, T>
        //    (
        //    CT initialValue,
        //    string propertyName,
        //    object extraInfo = null,
        //    PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal,
        //    bool typeIsSolid = true,
        //    Func<CT, CT, bool> comparer = null
        //    )
        //{
        //    IETypedProp<CT, T> newProp;
        //    if(typeof(CT).IOListCollectionViewBased())
        //    {
        //        IOListCollectionView<T> newVal;
        //        if (initialValue == null)
        //        {
        //            newVal = new OListCollectionView<T>(new ObservableCollection<T>());
        //        }
        //        else
        //        {
        //            newVal = initialValue as IOListCollectionView<T>;
        //        }

        //        // Use reference equality.
        //        Func<IOListCollectionView<T>, IOListCollectionView<T>, bool> newComparer = RefEqualityComparer<IOListCollectionView<T>>.Default.Equals;

        //        // Get the function used to create default values from our ValueConverter.
        //        GetDefaultValueDelegate<IOListCollectionView<T>> defaultValFunc = ValueConverter.GetDefaultValue<IOListCollectionView<T>>;

        //        IListCViewProp<IOListCollectionView<T>, T> fancyNewProp = 
        //            CreateListViewProp<IOListCollectionView<T>, T>(newVal, defaultValFunc,  propertyName, extraInfo, hasStorage, typeIsSolid, newComparer);

        //        newProp = (IETypedProp<CT,T>) fancyNewProp;
        //    }
        //    else
        //    {
        //        newProp = base.Create<CT, T>(initialValue, propertyName, extraInfo, hasStorage, typeIsSolid, comparer);
        //    }

        //    return newProp;
        //}

        //private IListCViewProp<CT,T> CreateListViewProp<CT, T>
        //    (
        //    CT initialValue,
        //    GetDefaultValueDelegate<CT> defaultValFunc,
        //    string propertyName,
        //    object extraInfo = null,
        //    PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal,
        //    bool typeIsSolid = true,
        //    Func<CT, CT, bool> comparer = null
        //    ) where CT: IOListCollectionView<T>
        //{
        //    IListCViewProp<CT, T> result = new ListCViewProp<CT, T>(initialValue, defaultValFunc, typeIsSolid, hasStorage, comparer);

        //    return result;
        //}

        #endregion

        #region CollectionViewSource property creators

        public override CViewProviderCreator GetCViewProviderFactory()
        {
            return CreateAView;
        }

        private IProvideAView CreateAView(string viewName, DataSourceProvider dataSourceProvider)
        {
            ViewProvider result = new ViewProvider(viewName, dataSourceProvider);
            return result;
        }


        public override IProp CreateCVSProp(PropNameType propertyName, IProvideAView viewProvider) 
        {
            ICViewSourceProp<CollectionViewSource> result = new CViewSourceProp(propertyName, viewProvider);
            return (IProp)result;
        }

        public override IProp CreateCVProp(string propertyName, IProvideAView viewProvider)
        {
            //ListCollectionView newVal;
            //if(initialValue == null)
            //{
            //    newVal = null;
            //}
            //else
            //{
            //    if (initialValue is ListCollectionView lcv)
            //    {
            //        newVal = lcv;
            //    }
            //    else
            //    {
            //        throw new ArgumentException($"The initialValue is not a ListCollectionView.", nameof(initialValue));
            //    }
            //}

            CViewProp result = new CViewProp(propertyName, viewProvider);
            return result;
        }

        //public IProp CreateCVPropGen(string propertyName, ICollectionView initialValue)
        //{
        //    CViewProp result = new CViewPropGen(propertyName, initialValue );
        //    return result;
        //}
        #endregion

        #region Property-type property creators

        #endregion

        #region Generic property creators

        //public override IProp CreateCVSPropFromString(Type typeOfThisProperty, PropNameType propertyName, DataSourceProvider initialValue)
        //{
        //    CreateCVSPropDelegate propCreator = GetCVSPropCreator(typeOfThisProperty);
        //    IProp prop = propCreator(this, propertyName, initialValue);
        //    return prop;
        //}

        //public override IProp CreateCVPropFromString(Type typeofThisProperty, PropNameType propertyName, ICollectionView initialValue)
        //{
        //    CreateCVPropDelegate propCreator = GetCVPropCreator(typeofThisProperty);
        //    IProp prop = propCreator(this, propertyName, initialValue);
        //    return prop;
        //}

        #endregion
    }
}
