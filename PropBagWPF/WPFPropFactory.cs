using DRM.PropBag;
using DRM.PropBag.Caches;
using DRM.TypeSafePropertyBag;
using System;

using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.PropBagWPF
{
    #region Type Aliases
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    #endregion

    public class WPFPropFactory : AbstractPropFactory
    {
        public override bool ProvidesStorage => true;

        override public int DoSetCacheCount => DelegateCacheProvider.DoSetDelegateCache.Count;
        override public int CreatePropFromStringCacheCount => DelegateCacheProvider.CreatePropFromStringCache.Count;
        override public int CreatePropWithNoValCacheCount => DelegateCacheProvider.CreatePropWithNoValCache.Count;

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

        #region Collection-type property creators


        #endregion

        #region Propety-type property creators

        #endregion

        #region Generic property creators


        #endregion
    }
}
