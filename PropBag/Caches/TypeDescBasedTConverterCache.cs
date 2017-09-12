using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.Caches
{
    [Serializable]
    public class TypeDescBasedTConverterCache : Dictionary<TypeDescBasedTConverterKey, Delegate>
    {
        // Use our Equality Comparer.
        public TypeDescBasedTConverterCache() : base(TypeDescBasedTConverterKey.GetEquComparer()) { }

        protected TypeDescBasedTConverterCache(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }
}



