﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public class AbstractObjectIdDictionary<ExKeyT, CompT, L1T, L2T, L2TRaw, PropDataT> :
        ConcurrentDictionary<ExKeyT, PropDataT>,
        IObjectIdDictionary<ExKeyT, CompT, L1T, L2T, L2TRaw, PropDataT> where ExKeyT : IExplodedKey<CompT, L1T, L2T> where PropDataT : IPropGen
    {
        #region Private Memebers

        ICKeyMan<ExKeyT, CompT, L1T, L2T, L2TRaw> CompKeyManager { get; }

        IL2KeyMan<L2T, L2TRaw> Level2KeyManager { get; }

        #endregion

        #region Constructor

        public AbstractObjectIdDictionary(ICKeyMan<ExKeyT, CompT, L1T, L2T, L2TRaw> compKeyManager, IL2KeyMan<L2T, L2TRaw> level2KeyManager)
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

            var thisAsCollection = this as ICollection<KeyValuePair<ExKeyT, PropDataT>>;

            List<KeyValuePair<ExKeyT, PropDataT>> toRemove = new List<KeyValuePair<ExKeyT, PropDataT>>();

            foreach (KeyValuePair<ExKeyT, PropDataT> kvp in thisAsCollection)
            {
                L1T foundTop = CompKeyManager.Split(kvp.Key, out L2T bot);
                if (foundTop.Equals(top))
                {
                    toRemove.Add(kvp);
                }
            }

            int result = 0;
            foreach (KeyValuePair<ExKeyT, PropDataT> kvp in toRemove)
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
            ExKeyT key = CompKeyManager.Join(top, bot);
            return GetOrAdd(key, value);
        }

        public bool TryAdd(L1T top, L2T bot, PropDataT value)
        {
            ExKeyT key = CompKeyManager.Join(top, bot);
            return TryAdd(key, value);
        }

        public bool TryGetValue(L1T top, L2T bot, out PropDataT value)
        {
            ExKeyT key = CompKeyManager.Join(top, bot);
            return TryGetValue(key, out value);
        }

        public bool ContainsKey(L1T top, L2T bot)
        {
            ExKeyT key = CompKeyManager.Join(top, bot);
            bool result = ContainsKey(key);
            return result;
        }

        public bool TryRemove(L1T top, L2T bot, out PropDataT value)
        {
            ExKeyT key = CompKeyManager.Join(top, bot);
            return TryRemove(key, out value);
        }

        #endregion

        #region Level 2 Raw

        public bool TryAdd(L1T top, L2TRaw rawBot, PropDataT value)
        {
            ExKeyT key = CompKeyManager.Join(top, Level2KeyManager.FromRaw(rawBot));
            return TryAdd(key, value);
        }

        public PropDataT GetOrAdd(L1T top, L2TRaw rawBot, PropDataT value)
        {
            ExKeyT key = CompKeyManager.Join(top, Level2KeyManager.FromRaw(rawBot));
            return GetOrAdd(key, value);
        }

        public bool TryGetValue(L1T top, L2TRaw rawBot, out PropDataT value)
        {
            bool result;
            if (CompKeyManager.TryJoin(top, rawBot, out ExKeyT comp))
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
            ExKeyT key = CompKeyManager.Join(top, Level2KeyManager.FromRaw(rawBot));
            bool result = ContainsKey(key);
            return result;
        }

        public bool TryRemove(L1T top, L2TRaw rawBot, out PropDataT value)
        {
            ExKeyT key = CompKeyManager.Join(top, Level2KeyManager.FromRaw(rawBot));
            return TryRemove(key, out value);
        }


        #endregion
    }

}
