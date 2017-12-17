using DRM.PropBag;
using DRM.PropBag.Caches;
using DRM.PropBag.Collections;
using DRM.TypeSafePropertyBag;
using System;
using System.Windows.Data;



namespace DRM.PropBagWPF
{
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;

    public class WPFPropFactory : AbstractPropFactory
    {
        public override bool ProvidesStorage => true;

        public override int DoSetCacheCount => DelegateCacheProvider.DoSetDelegateCache.Count;
        public override int CreatePropFromStringCacheCount => DelegateCacheProvider.CreatePropFromStringCache.Count;
        public override int CreatePropWithNoValCacheCount => DelegateCacheProvider.CreatePropWithNoValCache.Count;

        public WPFPropFactory
            (
                PSAccessServiceProviderType propStoreAccessServiceProvider,
                //IProvideDelegateCaches delegateCacheProvider,
                ResolveTypeDelegate typeResolver,
                IConvertValues valueConverter
            )
            : base(propStoreAccessServiceProvider, new SimpleDelegateCacheProvider(typeof(PropBag.PropBag), typeof(APFGenericMethodTemplates)), typeResolver, valueConverter)
        {
        }

        //public override IProp<T> Create<T>
        //    (
        //    T initialValue,
        //    string propertyName,
        //    object extraInfo = null,
        //    bool hasStorage = true,
        //    bool typeIsSolid = true,
        //    Func<T, T, bool> comparer = null
        //    )
        //{
        //    IProp<T> result;

        //    if (typeof(T) == typeof(ListCollectionView))
        //    {
        //        ListCollectionView temp = initialValue as ListCollectionView;
        //        result = (IProp<T>) new CViewProp(temp);
        //    }
        //    else
        //    {
        //        result = base.Create(initialValue, propertyName, extraInfo, hasStorage, typeIsSolid, comparer);
        //    }
        //    return result;
        //}

        #region Collection-type property creators

        //public override ICPropPrivate<CT, T> Create<CT, T>
        //    (
        //    CT initialValue,
        //    string propertyName,
        //    object extraInfo = null,
        //    bool hasStorage = true,
        //    bool typeIsSolid = true,
        //    Func<CT, CT, bool> comparer = null
        //    )
        //{
        //    if(initialValue is )
        //    ICPropPrivate<CT, T> input = (ICPropPrivate<CT, T>)extraInfo;
        //    CViewProp<CT, T> result = new CViewProp<CT, T>(input);

        //    ICPropPrivate<CT, T> r2 = (ICPropPrivate<CT, T>)result;

        //    return r2;

        //}

        #endregion

        #region Propety-type property creators

        #endregion

        #region Generic property creators


        #endregion
    }
}
