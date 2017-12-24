using System;
using System.Collections.Concurrent;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    using PropIdType = UInt32;

    public class DSProviderProviderCollection
    { 
        ConcurrentDictionary<PropIdType, IProvideADataSourceProviderGen> _dict;

        public DSProviderProviderCollection()
        {
            // TODO: Provide Expected Currency Levels.
            _dict = new ConcurrentDictionary<PropIdType, IProvideADataSourceProviderGen>();
        }

        public int Count => _dict.Count;

        public IProvideADataSourceProviderGen GetOrAdd(PropIdType propId, Func<PropIdType, IProvideADataSourceProviderGen> valueProducer)
        {
            IProvideADataSourceProviderGen result = _dict.GetOrAdd(propId, valueProducer);
            return result;
        }
    }
}
