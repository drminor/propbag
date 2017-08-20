using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.Caches
{
    public class TypeDescBasedTConverterCache : Dictionary<TypeDescBasedTConverterKey, Delegate>
    {
        // Use our Equality Comparer.
        public TypeDescBasedTConverterCache() : base(TypeDescBasedTConverterKey.GetEquComparer()) { }

    }
}
