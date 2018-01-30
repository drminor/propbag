using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag.PropStore
{
    using GenerationIdType = UInt32;

    internal class SimpleLevel2KeyManRepository<L2KeyManType, L2T, L2TRaw> where L2KeyManType : class, IL2KeyMan<L2T, L2TRaw>
    {
        public const GenerationIdType GEN_ZERO = 0;

        IDictionary<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> _level2KeyManagers;
        object _sync;

        public SimpleLevel2KeyManRepository()
        {
            _level2KeyManagers = new Dictionary<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType>();
        }

        public bool TryRegisterBaseL2KeyMan(L2KeyManType level2KeyManager)
        {
            Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, uint>(level2KeyManager, GEN_ZERO);

            lock (_sync)
            {
                if(IsRegistered_Private(key, out L2KeyManType refBase))
                {
                    // Already exists, cannot register it again.
                    return false;
                }
                else
                {
                    KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> kvp = GetKeyPair(level2KeyManager);
                    _level2KeyManagers.Add(kvp);
                    return true;
                }
            }
        }

        public bool IsRegistered(L2KeyManType level2KeyManager, GenerationIdType generationId, out KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> kvp)
        {
            Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, uint>(level2KeyManager, generationId);

            lock (_sync)
            {
                bool result = IsRegistered_Private(key, out L2KeyManType refBase);
                if(result)
                {
                    kvp = GetKeyPair(level2KeyManager, generationId, refBase);
                    return true;
                }
                else
                {
                    kvp = GetEmptyKeyPair();
                    return false;
                }
            }
        }

        public bool TryRegisterL2KeyMan(L2KeyManType level2KeyManager, L2KeyManType level2KeyManagerReferenceBase, out GenerationIdType generationId)
        {
            Tuple<L2KeyManType, GenerationIdType> kvpRefBase = new Tuple<L2KeyManType, GenerationIdType>(level2KeyManagerReferenceBase, GEN_ZERO);

            lock (_sync)
            {
                // Find the base in our Dictionary. (It will be keyed using it (its object referenced) and GenerationId = 0.
                if(IsRegistered_Private(kvpRefBase, out L2KeyManType refBase))
                {
                    // TODO: Have the reference base supply the next available generationId.
                    generationId = 1;

                    KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> newKvp = GetKeyPair(level2KeyManager, generationId, level2KeyManagerReferenceBase);

                    _level2KeyManagers.Add(newKvp); // TODO: Consider using a try / catch to check for a previously added entry with the "new" generationId.
                    return true;
                }
                else
                {
                    // Could not find base, cannot register a derived L2KeyMan.
                    generationId = GenerationIdType.MaxValue;
                    return false;
                }
            }
        }

        public bool TryGetReferenceBase(L2KeyManType level2KeyManager, GenerationIdType generationId, out L2KeyManType level2KeyManagerReferenceBase)
        {
            Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, GenerationIdType>(level2KeyManager, generationId);

            if (IsRegistered_Private(key, out level2KeyManagerReferenceBase)) 
            {
                return true;
            }
            else
            {
                level2KeyManagerReferenceBase = null;
                return false;
            }
        }

        #region Private Methods

        /// <summary>
        /// Used for registering Base Managers
        /// </summary>
        /// <param name="level2KeyManager"></param>
        /// <returns></returns>
        private KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> GetKeyPair(L2KeyManType level2KeyManager)
        {
            Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, uint>(level2KeyManager, GEN_ZERO);
            KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> kvp = new KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType>(key, level2KeyManager);
            return kvp;
        }

        /// <summary>
        /// Used for registering Derived Managers.
        /// </summary>
        /// <param name="level2KeyManager"></param>
        /// <param name="generationId"></param>
        /// <param name="level2KeyManagerRefBase"></param>
        /// <returns></returns>
        private KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> GetKeyPair(L2KeyManType level2KeyManager, GenerationIdType generationId, L2KeyManType level2KeyManagerRefBase)
        {
            Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, GenerationIdType>(level2KeyManager, generationId);
            KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> kvp = new KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType>(key, level2KeyManagerRefBase);
            return kvp;
        }

        private Tuple<L2KeyManType, GenerationIdType> GetKey(L2KeyManType level2KeyManager)
        {
            return GetKey(level2KeyManager, GEN_ZERO);
        }

        private Tuple<L2KeyManType, GenerationIdType> GetKey(L2KeyManType level2KeyManager, GenerationIdType generationId)
        {
            Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, uint>(level2KeyManager, generationId);
            return key;
        }


        private KeyValuePair<Tuple<L2KeyManType, UInt32>, L2KeyManType> GetEmptyKeyPair()
        {
            Tuple<L2KeyManType, GenerationIdType> key = new Tuple<L2KeyManType, GenerationIdType>(null, GEN_ZERO);
            KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType> kvp = new KeyValuePair<Tuple<L2KeyManType, GenerationIdType>, L2KeyManType>(key, null);
            return kvp;
        }

        private bool IsRegistered_Private(Tuple<L2KeyManType, GenerationIdType> key, out L2KeyManType refBase)
        {
            if (_level2KeyManagers.TryGetValue(key, out refBase))
            {
                return true;
            }
            else
            {
                refBase = null;
                return false;
            }
        }

        #endregion



    }
}
