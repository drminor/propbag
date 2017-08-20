using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DRM.PropBag.Caches
{
    public class DelegateCacheProvider
    {
        static Lazy<TypeDescBasedTConverterCache> theSingleTypeDescBasedTConverterCache;
        public static TypeDescBasedTConverterCache TypeDescBasedTConverterCache { get { return theSingleTypeDescBasedTConverterCache.Value; } }

        // Add additional member fields here for other Caches which contain delegates for other purposes.

        static DelegateCacheProvider()
        {
            theSingleTypeDescBasedTConverterCache = new Lazy<TypeDescBasedTConverterCache>(() => new TypeDescBasedTConverterCache(), LazyThreadSafetyMode.PublicationOnly);
        }

        private DelegateCacheProvider() { } // Mark as private to disallow creation of an instance of this class.
    }
}
