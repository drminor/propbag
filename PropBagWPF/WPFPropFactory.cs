using DRM.PropBag;
using DRM.PropBag.Caches;
using DRM.TypeSafePropertyBag;
using System;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    using PropNameType = String;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;

    public class WPFPropFactory : PropFactory
    {
        #region Constructor

        public WPFPropFactory
            (
                PSAccessServiceProviderType propStoreAccessServiceProvider,
                ResolveTypeDelegate typeResolver,
                IConvertValues valueConverter
            )
            : base(propStoreAccessServiceProvider, typeResolver, valueConverter, new SimpleDelegateCacheProvider(typeof(PropBag.PropBag), typeof(APFGenericMethodTemplates)))
        {
        }

        #endregion

        #region Collection-type property creators

        //public override IETypedProp<CT, T> Create<CT, T>
        //    (
        //    CT initialValue,
        //    string propertyName,
        //    object extraInfo = null,
        //    bool hasStorage = true,
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
        //    bool hasStorage = true,
        //    bool typeIsSolid = true,
        //    Func<CT, CT, bool> comparer = null
        //    ) where CT: IOListCollectionView<T>
        //{
        //    IListCViewProp<CT, T> result = new ListCViewProp<CT, T>(initialValue, defaultValFunc, typeIsSolid, hasStorage, comparer);

        //    return result;
        //}

        #endregion

        #region CollectionViewSource property creators

        public override IProp CreateCVSProp<TCVS, T>(PropNameType propertyName) 
        {
            ICViewPropWPF<CollectionViewSource, T> result = new CViewSourceProp<T>(null, propertyName);

            return (IProp)result;
        }

        public override IProp CreateCVProp<T>(string propertyName)
        {
            CViewProp<T> result = new CViewProp<T>(propertyName);
            return result;
        }

        #endregion

        #region Property-type property creators

        #endregion

        #region Generic property creators

        public override IProp CreateCVSPropFromString(Type typeOfThisProperty, PropNameType propertyName)
        {
            CreateCVSPropDelegate propCreator = GetCVSPropCreator(typeof(object), typeOfThisProperty);
            IProp prop = propCreator(this, propertyName);
            return prop;
        }

        public override IProp CreateCVPropFromString(Type typeofThisProperty, PropNameType propertyName)
        {
            CreateCVPropDelegate propCreator = GetCVPropCreator(typeofThisProperty);
            IProp prop = propCreator(this, propertyName);
            return prop;
        }

        #endregion
    }
}
