using DRM.PropBag.Collections;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

using DRM.PropBag.Caches;

namespace DRM.PropBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;

    using PropBagType = PropBag;

    public class PropFactory : AbstractPropFactory
    {
        public override bool ProvidesStorage => true;

        public PropFactory
            (
                PSAccessServiceProviderType propStoreAccessServiceProvider,
                //IProvideDelegateCaches delegateCacheProvider,
                ResolveTypeDelegate typeResolver,
                IConvertValues valueConverter
            )
            : base(propStoreAccessServiceProvider, new SimpleDelegateCacheProvider(typeof(PropBag), typeof(APFGenericMethodTemplates)), typeResolver, valueConverter)
        {
        }

        #region Enumerable-Type Prop Creation 

        #endregion

        #region IObsCollection<T> and ObservableCollection<T> Prop Creation

        #endregion

        #region CollectionViewSource Prop Creation

        #endregion

        #region Scalar Prop Creation

        #endregion

        #region Generic Property Creation

        #endregion
    }
}
