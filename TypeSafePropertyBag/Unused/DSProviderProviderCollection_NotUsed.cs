using System;
using System.Collections.Concurrent;

namespace DRM.TypeSafePropertyBag.Unused
{
    using PropIdType = UInt32;

    public class DSProviderProviderCollection
    { 
        ConcurrentDictionary<PropIdType, IProvideADataSourceProvider> _dict;

        public DSProviderProviderCollection()
        {
            // TODO: Provide Expected Currency Levels.
            _dict = new ConcurrentDictionary<PropIdType, IProvideADataSourceProvider>();
        }

        public int Count => _dict.Count;

        public IProvideADataSourceProvider GetOrAdd(PropIdType propId, Func<PropIdType, IProvideADataSourceProvider> valueProducer)
        {
            IProvideADataSourceProvider result = _dict.GetOrAdd(propId, valueProducer);
            return result;
        }
    }
}
