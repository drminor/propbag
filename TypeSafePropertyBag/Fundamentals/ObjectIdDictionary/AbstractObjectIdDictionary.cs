using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag.Fundamentals.ObjectIdDictionary
{
    public class AbstractObjectIdDictionary<CompT, L1T, L2T, L2TRaw, PropDataT> :
        ConcurrentDictionary<CompT, PropDataT>,
        IObjectIdDictionary<CompT, L1T, L2T, L2TRaw, PropDataT> where PropDataT : IPropGen
    {
        #region Private Memebers

        ICKeyMan<CompT, L1T, L2T, L2TRaw> CompKeyManager { get; }

        IL2KeyMan<L2T, L2TRaw> Level2KeyManager { get; }

        #endregion

        #region Constructor

        public AbstractObjectIdDictionary(ICKeyMan<CompT, L1T, L2T, L2TRaw> compKeyManager, IL2KeyMan<L2T, L2TRaw> level2KeyManager)
        {
            CompKeyManager = compKeyManager ?? throw new ArgumentNullException(nameof(compKeyManager));
            Level2KeyManager = level2KeyManager ?? throw new ArgumentNullException(nameof(level2KeyManager));
        }

        #endregion

        #region Level 1 

        /// <summary>
        /// Removes all entries that have the specified top value.
        /// </summary>
        /// <param name="top"></param>
        /// <returns>The number of entries removed.</returns>
        public int RemoveAll(L1T top)
        {
            //ICollection<KeyValuePair<CompT, TValue>> r = (ICollection<KeyValuePair<CompT, TValue>>) this;

            var thisAsCollection = this as ICollection<KeyValuePair<CompT, PropDataT>>;

            List<KeyValuePair<CompT, PropDataT>> toRemove = new List<KeyValuePair<CompT, PropDataT>>();

            foreach (KeyValuePair<CompT, PropDataT> kvp in thisAsCollection)
            {
                L1T foundTop = CompKeyManager.Split(kvp.Key, out L2T bot);
                if (foundTop.Equals(top))
                {
                    toRemove.Add(kvp);
                }
            }

            int result = 0;
            foreach (KeyValuePair<CompT, PropDataT> kvp in toRemove)
            {
                thisAsCollection.Remove(kvp);
                result++;
            }
            return result;
        }

        #endregion

        #region Level 2 Cooked 

        public PropDataT GetOrAdd(L1T top, L2T bot, PropDataT value)
        {
            CompT key = CompKeyManager.Join(top, bot);
            return GetOrAdd(key, value);
        }

        public bool TryAdd(L1T top, L2T bot, PropDataT value)
        {
            CompT key = CompKeyManager.Join(top, bot);
            return TryAdd(key, value);
        }

        public bool TryGetValue(L1T top, L2T bot, out PropDataT value)
        {
            CompT key = CompKeyManager.Join(top, bot);
            return TryGetValue(key, out value);
        }

        public bool ContainsKey(L1T top, L2T bot)
        {
            CompT key = CompKeyManager.Join(top, bot);
            bool result = ContainsKey(key);
            return result;
        }

        public bool TryRemove(L1T top, L2T bot, out PropDataT value)
        {
            CompT key = CompKeyManager.Join(top, bot);
            return TryRemove(key, out value);
        }

        #endregion

        #region Level 2 Raw

        public bool TryAdd(L1T top, L2TRaw rawBot, PropDataT value)
        {
            CompT key = CompKeyManager.Join(top, Level2KeyManager.FromRaw(rawBot));
            return TryAdd(key, value);
        }

        public PropDataT GetOrAdd(L1T top, L2TRaw rawBot, PropDataT value)
        {
            CompT key = CompKeyManager.Join(top, Level2KeyManager.FromRaw(rawBot));
            return GetOrAdd(key, value);
        }

        public bool TryGetValue(L1T top, L2TRaw rawBot, out PropDataT value)
        {
            bool result;
            if(CompKeyManager.TryJoin(top, rawBot, out CompT comp))
            {
                result = TryGetValue(comp, out value);
                return result;
            }
            else
            {
                value = default(PropDataT);
                return false;
            }
        }

        public bool ContainsKey(L1T top, L2TRaw rawBot)
        {
            CompT key = CompKeyManager.Join(top, Level2KeyManager.FromRaw(rawBot));
            bool result = ContainsKey(key);
            return result;
        }

        public bool TryRemove(L1T top, L2TRaw rawBot, out PropDataT value)
        {
            CompT key = CompKeyManager.Join(top, Level2KeyManager.FromRaw(rawBot));
            return TryRemove(key, out value);
        }


        #endregion
    }

}
