using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DRM.ObjectIdDictionary
{
    public class AbstractObjectIdDictionary<CompT, L1T, L2T, L2TRaw> :
        ConcurrentDictionary<CompT, IPropData>,
        IObjectIdDictionary<CompT, L1T, L2T, L2TRaw, IPropData> 
    {
        #region Private Memebers

        readonly ICKeyMan<CompT, L1T, L2T, L2TRaw> _compKeyManager;

        #endregion

        #region Constructor

        public AbstractObjectIdDictionary(ICKeyMan<CompT, L1T, L2T, L2TRaw> compKeyManager)
        {
            _compKeyManager = compKeyManager ?? throw new ArgumentNullException(nameof(compKeyManager));

            MaxObjectsPerAppDomain = compKeyManager.MaxObjectsPerAppDomain;
            MaxPropsPerObject = compKeyManager.MaxPropsPerObject;
        }

        #endregion

        public long MaxObjectsPerAppDomain { get; }
        public int MaxPropsPerObject { get; }

        #region Level 1 

        // TODO: take a lock during RemoveAll find stage.

        /// <summary>
        /// Removes all entries that have the specified top value.
        /// </summary>
        /// <param name="top"></param>
        /// <returns>The number of entries removed.</returns>
        public int RemoveAll(L1T top)
        {
            //ICollection<KeyValuePair<CompT, TValue>> r = (ICollection<KeyValuePair<CompT, TValue>>) this;

            var thisAsCollection = this as ICollection<KeyValuePair<CompT, IPropBag>>;

            List<KeyValuePair<CompT, IPropBag>> toRemove = new List<KeyValuePair<CompT, IPropBag>>();

            foreach (KeyValuePair<CompT, IPropBag> kvp in thisAsCollection)
            {
                L1T foundTop = _compKeyManager.SplitComp(kvp.Key, out L2T bot);
                if (foundTop.Equals(top))
                {
                    toRemove.Add(kvp);
                }
            }

            int result = 0;
            foreach (KeyValuePair<CompT, IPropBag> kvp in toRemove)
            {
                thisAsCollection.Remove(kvp);
                result++;
            }
            return result;
        }

        #endregion

        #region Level 2 Cooked 

        //public IPropData GetOrAdd(L1T top, L2T bot, IPropData value)
        //{
        //    CompT key = CompKeyManager.JoinComp(top, bot);
        //    return GetOrAdd(key, value);
        //}

        //public bool TryAdd(L1T top, L2T bot, IPropData value)
        //{
        //    CompT key = CompKeyManager.JoinComp(top, bot);
        //    return TryAdd(key, value);
        //}

        //public bool TryGetValue(L1T top, L2T bot, out IPropData value)
        //{
        //    CompT key = CompKeyManager.JoinComp(top, bot);
        //    return TryGetValue(key, out value);
        //}

        //public bool ContainsKey(L1T top, L2T bot)
        //{
        //    CompT key = CompKeyManager.JoinComp(top, bot);
        //    bool result = ContainsKey(key);
        //    return result;
        //}

        //public bool TryRemove(L1T top, L2T bot, out IPropData value)
        //{
        //    CompT key = CompKeyManager.JoinComp(top, bot);
        //    return TryRemove(key, out value);
        //}

        #endregion

        #region Level 2 Raw

        //public bool TryAdd(L1T top, L2TRaw rawBot, IPropData value)
        //{
        //    CompT key = CompKeyManager.JoinComp(top, Level2KeyManager.FromRaw(rawBot));
        //    return TryAdd(key, value);
        //}

        //public IPropData GetOrAdd(L1T top, L2TRaw rawBot, IPropData value)
        //{
        //    CompT key = CompKeyManager.JoinComp(top, Level2KeyManager.FromRaw(rawBot));
        //    return GetOrAdd(key, value);
        //}

        //public bool TryGetValue(L1T top, L2TRaw rawBot, out IPropData value)
        //{
        //    bool result;
        //    if (CompKeyManager.TryJoinComp(top, rawBot, out CompT comp, out L2T bot))
        //    {
        //        result = TryGetValue(comp, out value);
        //        return result;
        //    }
        //    else
        //    {
        //        value = default(IPropData);
        //        return false;
        //    }
        //}

        //public bool ContainsKey(L1T top, L2TRaw rawBot)
        //{
        //    CompT key = CompKeyManager.JoinComp(top, Level2KeyManager.FromRaw(rawBot));
        //    bool result = ContainsKey(key);
        //    return result;
        //}

        //public bool TryRemove(L1T top, L2TRaw rawBot, out IPropData value)
        //{
        //    CompT key = CompKeyManager.JoinComp(top, Level2KeyManager.FromRaw(rawBot));
        //    return TryRemove(key, out value);
        //}




        #endregion
    }

}
