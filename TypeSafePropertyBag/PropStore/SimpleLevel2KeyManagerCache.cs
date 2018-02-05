using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using GenerationIdType = UInt32;

    internal class SimpleLevel2KeyManagerCache<L2KeyManType, L2T, L2TRaw> : ICacheLevel2KeyManagers<L2KeyManType, L2T, L2TRaw> where L2KeyManType : class, IL2KeyMan<L2T, L2TRaw>
    {
        public const GenerationIdType GEN_ZERO = 0;

        IDictionary<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> _level2KeyManagers;

        IDictionary<L2KeyManType, Tuple<L2KeyManType, GenerationIdType>> _newManagers;

        object _sync = new object();

        public SimpleLevel2KeyManagerCache()
        {
            _level2KeyManagers = new Dictionary<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType>();
            _newManagers = new Dictionary<L2KeyManType, Tuple<L2KeyManType, GenerationIdType>>();
        }

        public bool TryGetValueAndGenerationId(L2KeyManType level2KeyManager, out L2KeyManType basePropItemSet, out GenerationIdType generationId)
        {
            if (_newManagers.TryGetValue(level2KeyManager, out Tuple<L2KeyManType, GenerationIdType> value))
            {
                basePropItemSet = value.Item1;
                generationId = value.Item2;
                return true;
            }
            else
            {
                basePropItemSet = null;
                generationId = GenerationIdType.MaxValue;
                return false;
            }
        }

        //public bool TryRegisterBaseL2KeyMan(L2KeyManType level2KeyManager)
        //{
        //    Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, uint>(level2KeyManager, GEN_ZERO);

        //    lock (_sync)
        //    {
        //        if(IsRegistered_Private(key, out L2KeyManType refBase))
        //        {
        //            // Already exists, cannot register it again.
        //            return false;
        //        }
        //        else
        //        {
        //            KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> kvp = GetKeyPair(level2KeyManager);
        //            _level2KeyManagers.Add(kvp);
        //            return true;
        //        }
        //    }
        //}

        public bool TryRegisterBaseL2KeyMan(L2KeyManType level2KeyManager)
        {
            lock (_sync)
            {
                if(_newManagers.TryGetValue(level2KeyManager, out Tuple<L2KeyManType, GenerationIdType> value))
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot Register Base Level2Key Manager, it is already registered as {GetDesc(value)}");
                    return false;
                }
                else
                {
                    Tuple<L2KeyManType, GenerationIdType> entry = new Tuple<L2KeyManType, GenerationIdType>(level2KeyManager, GEN_ZERO);

                    _newManagers.Add(level2KeyManager, entry);
                    return true;
                }
            }
        }

        public bool TryRegisterL2KeyMan(L2KeyManType level2KeyManager, L2KeyManType basePropItemSet, out GenerationIdType generationId)
        {
            if (_newManagers.TryGetValue(basePropItemSet, out Tuple<L2KeyManType, GenerationIdType> value))
            {
                // TODO: Have the reference base supply the next available generationId.
                generationId = 1;

                Tuple<L2KeyManType, GenerationIdType> entry = new Tuple<L2KeyManType, GenerationIdType>(level2KeyManager, generationId);
                _newManagers.Add(level2KeyManager, entry);
                return true;
            }
            else
            {
                // Could not find base, cannot register a derived L2KeyMan.
                generationId = GenerationIdType.MaxValue;
                return false;
            }
        }

        public bool TryGetGeneration(L2KeyManType level2KeyManager, out GenerationIdType generationId)
        {
            if (_newManagers.TryGetValue(level2KeyManager, out Tuple<L2KeyManType, GenerationIdType> value))
            {
                generationId = value.Item2;
                return true;
            }
            else
            {
                generationId = GenerationIdType.MaxValue;
                return false;
            }
        }

        //public GenerationIdType GetGeneration(L2KeyManType level2KeyManager)
        //{
        //    if (_newManagers.TryGetValue(level2KeyManager, out Tuple<L2KeyManType, GenerationIdType> value))
        //    {
        //        return value.Item2;
        //    }
        //    else
        //    {
        //        return GenerationIdType.MaxValue;
        //    }
        //}

        //public bool IsRegistered(L2KeyManType level2KeyManager, GenerationIdType generationId, out KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> kvp)
        //{
        //    Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, uint>(level2KeyManager, generationId);

        //    lock (_sync)
        //    {
        //        bool result = IsRegistered_Private(key, out L2KeyManType refBase);
        //        if(result)
        //        {
        //            kvp = GetKeyPair(level2KeyManager, generationId, refBase);
        //            return true;
        //        }
        //        else
        //        {
        //            kvp = GetEmptyKeyPair();
        //            return false;
        //        }
        //    }
        //}

        //public bool TryRegisterL2KeyMan(L2KeyManType level2KeyManager, L2KeyManType level2KeyManagerReferenceBase, out GenerationIdType generationId)
        //{
        //    Tuple<L2KeyManType, GenerationIdType> kvpRefBase = new Tuple<L2KeyManType, GenerationIdType>(level2KeyManagerReferenceBase, GEN_ZERO);

        //    lock (_sync)
        //    {
        //        // Find the base in our Dictionary. (It will be keyed using it (its object referenced) and GenerationId = 0.
        //        if(IsRegistered_Private(kvpRefBase, out L2KeyManType refBase))
        //        {
        //            // TODO: Have the reference base supply the next available generationId.
        //            generationId = 1;

        //            KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> newKvp = GetKeyPair(level2KeyManager, generationId, level2KeyManagerReferenceBase);

        //            _level2KeyManagers.Add(newKvp); // TODO: Consider using a try / catch to check for a previously added entry with the "new" generationId.
        //            return true;
        //        }
        //        else
        //        {
        //            // Could not find base, cannot register a derived L2KeyMan.
        //            generationId = GenerationIdType.MaxValue;
        //            return false;
        //        }
        //    }
        //}

        //public bool TryGetReferenceBase(L2KeyManType level2KeyManager, GenerationIdType generationId, out L2KeyManType level2KeyManagerReferenceBase)
        //{
        //    Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, GenerationIdType>(level2KeyManager, generationId);

        //    if (IsRegistered_Private(key, out level2KeyManagerReferenceBase)) 
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        level2KeyManagerReferenceBase = null;
        //        return false;
        //    }
        //}

        #region Private Methods

        //private KeyValuePair<L2KeyManType, Tuple<L2KeyManType, GenerationIdType>> GetKeyPair_N(L2KeyManType level2KeyManager)
        //{
        //    Tuple<L2KeyManType, GenerationIdType> entry = new Tuple<L2KeyManType, GenerationIdType>(level2KeyManager, GEN_ZERO);

        //    KeyValuePair<L2KeyManType, Tuple<L2KeyManType, GenerationIdType>> kvp = 
        //        new KeyValuePair<L2KeyManType, Tuple<L2KeyManType, GenerationIdType>>(level2KeyManager, entry);
                
        //    return kvp;
        //}

        #region OLD

        ///// <summary>
        ///// Used for registering Base Managers
        ///// </summary>
        ///// <param name="level2KeyManager"></param>
        ///// <returns></returns>
        //private KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> GetKeyPair(L2KeyManType level2KeyManager)
        //{
        //    Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, uint>(level2KeyManager, GEN_ZERO);
        //    KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> kvp = new KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType>(key, level2KeyManager);
        //    return kvp;
        //}

        ///// <summary>
        ///// Used for registering Derived Managers.
        ///// </summary>
        ///// <param name="level2KeyManager"></param>
        ///// <param name="generationId"></param>
        ///// <param name="level2KeyManagerRefBase"></param>
        ///// <returns></returns>
        //private KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> GetKeyPair(L2KeyManType level2KeyManager, GenerationIdType generationId, L2KeyManType level2KeyManagerRefBase)
        //{
        //    Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, GenerationIdType>(level2KeyManager, generationId);
        //    KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> kvp = new KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType>(key, level2KeyManagerRefBase);
        //    return kvp;
        //}

        //private Tuple<L2KeyManType, GenerationIdType> GetKey(L2KeyManType level2KeyManager)
        //{
        //    return GetKey(level2KeyManager, GEN_ZERO);
        //}

        //private Tuple<L2KeyManType, GenerationIdType> GetKey(L2KeyManType level2KeyManager, GenerationIdType generationId)
        //{
        //    Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, uint>(level2KeyManager, generationId);
        //    return key;
        //}


        //private KeyValuePair<Tuple<L2KeyManType, UInt32>, L2KeyManType> GetEmptyKeyPair()
        //{
        //    Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, GenerationIdType>(null, GEN_ZERO);
        //    KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> kvp = new KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType>(key, null);
        //    return kvp;
        //}

        //private bool IsRegistered_Private(Tuple<L2KeyManType, GenerationIdType> key, out L2KeyManType refBase)
        //{
        //    if (_level2KeyManagers.TryGetValue(key, out refBase))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        refBase = null;
        //        return false;
        //    }
        //}

        #endregion

        private string GetDesc(Tuple<L2KeyManType, GenerationIdType> value)
        {
            if (value.Item2 > 0)
            {
                return $"L2KMan: {value.Item1}";
            }
            else
            {
                return $"Version: {value.Item2} of L2KMan: {value.Item1}";
            }
        }

        #endregion



    }
}
