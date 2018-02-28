using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using GenerationIdType = Int64;

    internal class SimplePropNodeCollectionCache<PropNodeCollectionInterface, L2T, L2TRaw> : ICachePropNodeCollections<PropNodeCollectionInterface, L2T, L2TRaw> where PropNodeCollectionInterface : class, IPropNodeCollection_Internal<L2T, L2TRaw>
    {
        public const GenerationIdType GEN_ZERO = 0;

        #region Private Members

        IDictionary<PropNodeCollectionInterface, Tuple<PropNodeCollectionInterface, GenerationIdType>> _dict;
        object _sync = new object();

        #endregion

        #region Constructor

        public SimplePropNodeCollectionCache()
        {
            _dict = new Dictionary<PropNodeCollectionInterface, Tuple<PropNodeCollectionInterface, GenerationIdType>>();
        }

        #endregion

        #region Public Methods

        public bool TryGetValueAndGenerationId(PropNodeCollectionInterface propItemSet, out PropNodeCollectionInterface basePropItemSet, out GenerationIdType generationId)
        {
            if (_dict.TryGetValue(propItemSet, out Tuple<PropNodeCollectionInterface, GenerationIdType> value))
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

        public bool TryRegisterBasePropItemSet(PropNodeCollectionInterface propItemSet)
        {
            lock (_sync)
            {
                if(_dict.TryGetValue(propItemSet, out Tuple<PropNodeCollectionInterface, GenerationIdType> value))
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot Register Base PropItemSet, it is already registered as {GetDesc(value)}");
                    return false;
                }
                else
                {
                    Tuple<PropNodeCollectionInterface, GenerationIdType> entry = new Tuple<PropNodeCollectionInterface, GenerationIdType>(propItemSet, GEN_ZERO);

                    _dict.Add(propItemSet, entry);
                    return true;
                }
            }
        }

        public bool TryRegisterPropItemSet(PropNodeCollectionInterface propItemSet, PropNodeCollectionInterface basePropItemSet, out GenerationIdType generationId)
        {
            if (_dict.TryGetValue(basePropItemSet, out Tuple<PropNodeCollectionInterface, GenerationIdType> value))
            {
                // TODO: Have the reference base supply the next available generationId.
                generationId = basePropItemSet.GetNextGenerationId();

                Tuple<PropNodeCollectionInterface, GenerationIdType> entry = new Tuple<PropNodeCollectionInterface, GenerationIdType>(propItemSet, generationId);
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

        public bool TryGetGeneration(PropNodeCollectionInterface propItemSet, out GenerationIdType generationId)
        {
            if (_dict.TryGetValue(propItemSet, out Tuple<PropNodeCollectionInterface, GenerationIdType> value))
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

        private string GetDesc(Tuple<PropNodeCollectionInterface, GenerationIdType> value)
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
