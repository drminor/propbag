using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System.Collections.Generic;

using System.Linq;

namespace DRM.PropBag.AutoMapperSupport
{
    //[Synchronization()]
    public class SimplePropBagMapperCache : /*ContextBoundObject,*/ ICachePropBagMappers
    {
        private int pCntr = 0;
        private LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperKeyGen> _unSealedPropBagMappers;
        private LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperGen> _sealedPropBagMappers;

        public SimplePropBagMapperCache()
        {
            _unSealedPropBagMappers = 
                new LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperKeyGen>
                (GetPropBagMapperPromise);

            _sealedPropBagMappers =
                new LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperGen>
                (GetPropBagMapperReal);
        }

        public IPropBagMapperKeyGen RegisterMapperRequest(IPropBagMapperKeyGen mapRequest)
        {

            if (_sealedPropBagMappers.ContainsKey(mapRequest))
            {
                ICollection<IPropBagMapperKeyGen> keys = _sealedPropBagMappers.Keys;

                // TODO: This is relatively resource intensive, perhaps there's a better way.
                IPropBagMapperKeyGen existingKey = keys.FirstOrDefault(x => x.DestinationTypeGenDef.Equals(mapRequest.DestinationTypeGenDef));
                return existingKey;
            }
            else
            {
                _unSealedPropBagMappers.GetOrAdd(mapRequest);
                return mapRequest;
            }
        }

        public IPropBagMapperGen GetMapper(IPropBagMapperKeyGen mapRequest)
        {
            if (_sealedPropBagMappers.TryGetValue(mapRequest, out IPropBagMapperGen result))
            {
                return result;
            }

            _unSealedPropBagMappers.GetOrAdd(mapRequest);

            IPropBagMapperKeyGen save = mapRequest;

            int numberInThisBatch = SealThis(pCntr++);

            bool ttest = save.Equals(mapRequest);

            if(!_sealedPropBagMappers.TryGetValue(mapRequest, out result))
            {
                result = null;
            }

            return result;
        }

        public int SealThis(int cntr)
        {
            System.Diagnostics.Debug.WriteLine($"Creating Profile_{cntr.ToString()}");

            int result = 0;
            foreach (IPropBagMapperKeyGen key in _unSealedPropBagMappers.Keys)
            {
                IPropBagMapperGen mapper = _sealedPropBagMappers.GetOrAdd(key);
                if (!(_unSealedPropBagMappers.TryRemoveValue(key, out IPropBagMapperKeyGen dummyKey)))
                {
                    System.Diagnostics.Debug.WriteLine("Couldn't remove mappper request from list of registered, pending to be created, mapper requests.");
                }
                result++;
            }
            return result;
        }

        private IPropBagMapperGen GetPropBagMapperReal(IPropBagMapperKeyGen key)
        {
            IPropBagMapperGen result = key.CreateMapper();
            return result;
        }

        private IPropBagMapperKeyGen GetPropBagMapperPromise(IPropBagMapperKeyGen key)
        {
            return key;
        }

    }
}
