using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using GenerationIdType = Int64;

    internal class SimplePropItemSetCache<PropItemSetType, L2T, L2TRaw> : ICachePropItemSets<PropItemSetType, L2T, L2TRaw> where PropItemSetType : class, IPropNodeCollection_Internal<L2T, L2TRaw>
    {
        public const GenerationIdType GEN_ZERO = 0;

        #region Private Members

        IDictionary<PropItemSetType, Tuple<PropItemSetType, GenerationIdType>> _dict;
        object _sync = new object();

        #endregion

        #region Constructor

        public SimplePropItemSetCache()
        {
            _dict = new Dictionary<PropItemSetType, Tuple<PropItemSetType, GenerationIdType>>();
        }

        #endregion

        #region Public Methods

        public bool TryGetValueAndGenerationId(PropItemSetType propItemSet, out PropItemSetType basePropItemSet, out GenerationIdType generationId)
        {
            if (_dict.TryGetValue(propItemSet, out Tuple<PropItemSetType, GenerationIdType> value))
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

        public bool TryRegisterBasePropItemSet(PropItemSetType propItemSet)
        {
            lock (_sync)
            {
                if(_dict.TryGetValue(propItemSet, out Tuple<PropItemSetType, GenerationIdType> value))
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot Register Base PropItemSet, it is already registered as {GetDesc(value)}");
                    return false;
                }
                else
                {
                    Tuple<PropItemSetType, GenerationIdType> entry = new Tuple<PropItemSetType, GenerationIdType>(propItemSet, GEN_ZERO);

                    _dict.Add(propItemSet, entry);
                    return true;
                }
            }
        }

        public bool TryRegisterPropItemSet(PropItemSetType propItemSet, PropItemSetType basePropItemSet, out GenerationIdType generationId)
        {
            if (_dict.TryGetValue(basePropItemSet, out Tuple<PropItemSetType, GenerationIdType> value))
            {
                // TODO: Have the reference base supply the next available generationId.
                generationId = basePropItemSet.GetNextGenerationId();

                Tuple<PropItemSetType, GenerationIdType> entry = new Tuple<PropItemSetType, GenerationIdType>(propItemSet, generationId);
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

        public bool TryGetGeneration(PropItemSetType propItemSet, out GenerationIdType generationId)
        {
            if (_dict.TryGetValue(propItemSet, out Tuple<PropItemSetType, GenerationIdType> value))
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

        private string GetDesc(Tuple<PropItemSetType, GenerationIdType> value)
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
