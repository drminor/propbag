using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DRM.TypeSafePropertyBag;
using DRM.PropBag;

namespace DRM.PropBag.Caches
{
    public static class DelegateCacheProvider
    {

        #region Private Backing Members

        static Lazy<TypeDescBasedTConverterCache> theSingleTypeDescBasedTConverterCache;

        static Lazy<LockingConcurrentDictionary<Type, DoSetDelegate>> theSingleDoSetDelegateCache;

        #endregion

        #region Public Accessors

        // TypeDesc<T>-based Converter Cache
        public static TypeDescBasedTConverterCache TypeDescBasedTConverterCache
        {
            get { return theSingleTypeDescBasedTConverterCache.Value; }
        }

        // DoSet Delegate Cache
        internal static LockingConcurrentDictionary<Type, DoSetDelegate> DoSetDelegateCache
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
            Func<Type, DoSetDelegate> valueFactory = PropBag.GenericMethodTemplates.GetDoSetDelegate;
            theSingleDoSetDelegateCache = 
                new Lazy<LockingConcurrentDictionary<Type, DoSetDelegate>>
                (
                    () => new LockingConcurrentDictionary<Type, DoSetDelegate>(valueFactory),
                    LazyThreadSafetyMode.PublicationOnly
                );
        }
        #endregion

        #region Instance Constructors

        // Mark as private to disallow instances of this class to be created.
        //private DelegateCacheProvider() { }
        
        #endregion
    }
}
