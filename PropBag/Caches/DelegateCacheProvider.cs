using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.TypeSafePropertyBag;
using System.Reflection;

namespace DRM.PropBag.Caches
{
    public class DelegateCacheProvider
    {
        #region Private Static Members

        static Lazy<TypeDescBasedTConverterCache> theSingleTypeDescBasedTConverterCache;

        static Lazy<DelegateCache<DoSetDelegate>> theSingleDoSetDelegateCache;

        #endregion

        #region Public Static Properties

        // TypeDesc<T>-based Converter Cache
        public static TypeDescBasedTConverterCache TypeDescBasedTConverterCache
        {
            get { return theSingleTypeDescBasedTConverterCache.Value; }
        }

        // DoSet Delegate Cache
        //internal static DoSetDelegateCache DoSetDelegateCache
        //{
        //    get { return theSingleDoSetDelegateCache.Value; }
        //}

        internal static DelegateCache<DoSetDelegate> DoSetDelegateCache
        {
            get { return theSingleDoSetDelegateCache.Value; }
        }

        #endregion

        #region The Static Constructor

        static DelegateCacheProvider()
        {
            // Create the static instances upon first reference.

            // TypeDesc<T>-based Converter Cache
            theSingleTypeDescBasedTConverterCache = new Lazy<TypeDescBasedTConverterCache>(() => new TypeDescBasedTConverterCache(), LazyThreadSafetyMode.PublicationOnly);


            // Do Set Delegate Cache for PropBagBase
            //Func<Type, DoSetDelegate> valueFactory = PropBag.GenericMethodTemplates.GetDoSetDelegate;
            //theSingleDoSetDelegateCache = 
            //    new Lazy<LockingConcurrentDictionary<Type, DoSetDelegate>>
            //    (
            //        () => new LockingConcurrentDictionary<Type, DoSetDelegate>(valueFactory),
            //        LazyThreadSafetyMode.PublicationOnly
            //    );

            //theSingleDoSetDelegateCache =
            //    new Lazy<DoSetDelegateCache>
            //    (
            //        () => new DoSetDelegateCache(typeof(PropBag)), LazyThreadSafetyMode.PublicationOnly
            //    );

            MethodInfo doSetMethodInfo = typeof(PropBag).GetMethod("DoSetBridge", BindingFlags.Instance | BindingFlags.NonPublic);

            theSingleDoSetDelegateCache =
                new Lazy<DelegateCache<DoSetDelegate>>
                (
                    () => new DelegateCache<DoSetDelegate>(doSetMethodInfo), LazyThreadSafetyMode.PublicationOnly
                );
        }

        #endregion

        #region Instance Constructors

        // Mark as private to disallow instances of this class to be created.
        private DelegateCacheProvider()
        {
        }



        #endregion
    }
}
