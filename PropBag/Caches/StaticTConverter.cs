using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.Caches
{
    public static class StaticTConverterProvider
    {
        public static TypeDescBasedTConverterCache TypeDescBasedTConverterCache { get; }

        static StaticTConverterProvider()
        {
            // TypeDesc<T>-based Converter Cache
            TypeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
        }
    }
}
