using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using GenerationIdType = Int64;

    internal class SimplePropNodeCollectionCache<PropNodeCollectionIntInterface, L2T, L2TRaw> : ICachePropNodeCollections<PropNodeCollectionIntInterface, L2T, L2TRaw> where PropNodeCollectionIntInterface : class, IPropNodeCollection_Internal<L2T, L2TRaw>
    {
        public const GenerationIdType GEN_ZERO = 0;

        #region Private Members

        IDictionary<PropNodeCollectionIntInterface, Tuple<PropNodeCollectionIntInterface, GenerationIdType>> _dict;
        object _sync = new object();

        #endregion

        #region Constructor

        public SimplePropNodeCollectionCache()
        {
            _dict = new Dictionary<PropNodeCollectionIntInterface, Tuple<PropNodeCollectionIntInterface, GenerationIdType>>();
        }

        #endregion

        #region Public Methods

        public bool TryGetValueAndGenerationId(PropNodeCollectionIntInterface propItemSet, out PropNodeCollectionIntInterface basePropItemSet, out GenerationIdType generationId)
        {
            if (_dict.TryGetValue(propItemSet, out Tuple<PropNodeCollectionIntInterface, GenerationIdType> value))
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

        public bool TryRegisterBasePropItemSet(PropNodeCollectionIntInterface propItemSet)
        {
            lock (_sync)
            {
                if(_dict.TryGetValue(propItemSet, out Tuple<PropNodeCollectionIntInterface, GenerationIdType> value))
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot Register Base PropItemSet, it is already registered as {GetDesc(value)}");
                    return false;
                }
                else
                {
                    Tuple<PropNodeCollectionIntInterface, GenerationIdType> entry = new Tuple<PropNodeCollectionIntInterface, GenerationIdType>(propItemSet, GEN_ZERO);

                    _dict.Add(propItemSet, entry);
                    return true;
                }
            }
        }

        public bool TryRegisterPropItemSet(PropNodeCollectionIntInterface propItemSet, PropNodeCollectionIntInterface basePropItemSet, out GenerationIdType generationId)
        {
            if (_dict.TryGetValue(basePropItemSet, out Tuple<PropNodeCollectionIntInterface, GenerationIdType> value))
            {
                // TODO: Have the reference base supply the next available generationId.
                generationId = basePropItemSet.GetNextGenerationId();

                Tuple<PropNodeCollectionIntInterface, GenerationIdType> entry = new Tuple<PropNodeCollectionIntInterface, GenerationIdType>(propItemSet, generationId);
                _dict.Add(propItemSet, entry);
                return true;
            }
            else
            {
                // Could not find base, cannot register a derived PropItemSet.
                generationId = GenerationIdType.MaxValue;
                return false;
            }
        }

        public bool TryGetGeneration(PropNodeCollectionIntInterface propItemSet, out GenerationIdType generationId)
        {
            if (_dict.TryGetValue(propItemSet, out Tuple<PropNodeCollectionIntInterface, GenerationIdType> value))
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

        #endregion

        #region Private Methods

        private string GetDesc(Tuple<PropNodeCollectionIntInterface, GenerationIdType> value)
        {
            if (value.Item2 == 0)
            {
                return $"PropItemSet: {value.Item1}";
            }
            else
            {
                return $"Version: {value.Item2} of PropItemSet: {value.Item1}";
            }
        }

        #endregion
    }
}
