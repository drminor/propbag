using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

/// <remarks>
/// This was copied whole sale from the AutoMapper project on GitHub.
/// DRM Added the following features:
///     IEnumerable<typeparamref name="TValue"/> implementation,
///     IEqualityComparer<typeparam name="TKey"></typeparam> constructor parameter,
///     TryRemoveValue and Clear methods.
/// </remarks>

///<remarks>
/// Note: Since this uses the version of GetOrAdd that takes a delegate,
/// Locks are not held by the ConcurrentDictionary while the delegate is being invoked.
/// 
/// TODO: Need to determine that since the Lazy constructor is thread-safe, if the overall
/// operation is thread-safe.
///</remarks>

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    /// <summary>
    /// Wraps a ConcurrentDictionary. Values are created by evaluating a 'factory' Func<typeparamref name="TKey"/>, Lazy<typeparamref name="TValue"/>> 
    /// specified when an instance of this class is created.
    /// GetOrAdd(<typeparamref name="TValue"/> key, only calls the factory Func when their is no existing entry in the Dictionary.
    /// If an entry does exist, instead of executing the factory Func, the existing value is returned from the Dictionary.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public struct LockingConcurrentDictionary<TKey, TValue> : IEnumerable<TValue>
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

        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
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

        public IEnumerator<TValue> GetEnumerator()
        {
            IEnumerable<TValue> list = _dictionary.Values.Select(x => x.Value);
            IEnumerator<TValue> result = list.GetEnumerator();
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerator<TValue> typedEnumerator = GetEnumerator();
            return typedEnumerator;
        }
    }

}
