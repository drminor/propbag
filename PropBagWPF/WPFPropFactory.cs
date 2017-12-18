using DRM.PropBag;
using DRM.PropBag.Caches;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBagWPF
{
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;

    public class WPFPropFactory : AbstractPropFactory
    {
        public override bool ProvidesStorage => true;

        public WPFPropFactory
            (
                PSAccessServiceProviderType propStoreAccessServiceProvider,
                ResolveTypeDelegate typeResolver,
                IConvertValues valueConverter
            )
            : base(propStoreAccessServiceProvider, new SimpleDelegateCacheProvider(typeof(PropBag.PropBag), typeof(APFGenericMethodTemplates)), typeResolver, valueConverter)
        {
        }

        #region Collection-type property creators

        public override ICPropPrivate<CT, T> Create<CT, T>
            (
            CT initialValue,
            string propertyName,
            object extraInfo = null,
            bool hasStorage = true,
            bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null
            )
        {
            ICPropPrivate<CT, T> newProp;
            if(typeof(OListCollectionView<T>).IsAssignableFrom(initialValue.GetType()))
            {
                OListCollectionView<T> newVal = initialValue as OListCollectionView<T>;

                // Use the default EqualityComparer defined for the OListCollectionView type.
                //Func<OListCollectionView<T>, OListCollectionView<T>, bool> newComparer = EqualityComparer<OListCollectionView<T>>.Default.Equals;

                // Use reference equality.
                Func<OListCollectionView<T>, OListCollectionView<T>, bool> newComparer = RefEqualityComparer<OListCollectionView<T>>.Default.Equals;

                newProp = (ICPropPrivate<CT, T>)CreateListViewProp<OListCollectionView<T>, T>(newVal, propertyName, extraInfo, hasStorage, typeIsSolid, newComparer);
            }
            else
            {
                newProp = base.Create<CT, T>(initialValue, propertyName, extraInfo, hasStorage, typeIsSolid, comparer);
            }
            return newProp;
        }

        private IListCViewProp<CT,T> CreateListViewProp<CT, T>
            (
            CT initialValue,
            string propertyName,
            object extraInfo = null,
            bool hasStorage = true,
            bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null
            ) where CT: IOListCollectionView<T>
        {
            IListCViewProp<CT, T> result = null;

            return result;
        }

        #endregion

        #region Propety-type property creators

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

        #endregion

        #region Generic property creators


        #endregion
    }
}
