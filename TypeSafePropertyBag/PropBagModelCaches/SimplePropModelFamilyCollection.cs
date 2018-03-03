using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using PropModelType = IPropModel<String>;

    public class SimplePropModelFamilyCollection : IPropModelFamilyCollection<String>
    {
        public const long GEN_ZERO = 0;
        private readonly Dictionary<long, PropModelType> _generations;

        object _sync = new object();

        public SimplePropModelFamilyCollection()
        {
            _generations = new Dictionary<long, PropModelType>();
        }

        public long Add(PropModelType propModel)
        {
            long genId = GetNextGenerationId();

            lock(_sync)
            {
                _generations.Add(genId, propModel);
            }

            return genId;
        }

        public IReadOnlyDictionary<long, PropModelType> GetAll()
        {
            return new ReadOnlyDictionary<long, PropModelType>(_generations);
        }

        public bool TryGetPropModel(long generationId, out PropModelType propModel)
        {
            bool result = _generations.TryGetValue(generationId, out propModel);
            return result;
        }

        public bool TryGetPropModel(out PropModelType propModel)
        {
            bool result = TryGetPropModel(GEN_ZERO, out propModel);
            return result;
        }

        public bool Contains(long generationId)
        {
            bool result = _generations.ContainsKey(generationId);
            return result;
        }

        public long Find(PropModelType propModel)
        {
            long result = _generations.Where(x => ReferenceEquals(x, propModel)).FirstOrDefault().Key;
            return result;
        }

        private long _generationIdCounter = -1; 
        private long GetNextGenerationId()
        {
            if (_generationIdCounter == long.MaxValue - 1) throw new InvalidOperationException($"{nameof(SimplePropModelFamilyCollection)} has run out of Ids.");
            return System.Threading.Interlocked.Increment(ref _generationIdCounter);
        }
    }
}
