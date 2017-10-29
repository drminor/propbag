using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <remarks>
/// This was copied whole sale from the AutoMapper project on GitHub.
/// DRM Added TryRemove.
/// </remarks>

///<summary>
/// Note: Since this uses the version of GetOrAdd that takes a delegate,
/// Locks are not held by the ConcurrentDictionary while the delegate is being invoked.
/// 
/// TODO: Need to determine that since the Lazy constructor is thread-safe, if the overall
/// operation is thread-safe.
///</summary>
namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public struct LockingConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _dictionary;
        private readonly Func<TKey, Lazy<TValue>> _valueFactory;

        public LockingConcurrentDictionary(Func<TKey, TValue> valueFactory, IEqualityComparer<TKey> comparer = null)
        {
            if(comparer != null)
            {
                _dictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>(comparer);
            }
            else
            {
                _dictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
            }
            _dictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
            _valueFactory = key => new Lazy<TValue>(() => valueFactory(key));
        }

        public TValue GetOrAdd(TKey key) => _dictionary.GetOrAdd(key, _valueFactory).Value;

        public TValue this[TKey key]
        {
            get => _dictionary[key].Value;
            set => _dictionary[key] = new Lazy<TValue>(() => value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_dictionary.TryGetValue(key, out Lazy<TValue> lazy))
            {
                value = lazy.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public ICollection<TKey> Keys => _dictionary.Keys;

        public bool TryRemoveValue(TKey key, out TValue value)
        {
            if (_dictionary.TryRemove(key, out Lazy<TValue> lazy))
            {
                value = lazy.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public void Clear() => _dictionary.Clear();
    }

}
