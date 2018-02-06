using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;

using System.Linq;

namespace DRM.PropBag.AutoMapperSupport
{
    //[Synchronization()]
    public class SimplePropBagMapperCache : /*ContextBoundObject,*/ ICachePropBagMappers, IDisposable
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
            IPropBagMapperKeyGen save = mapRequest;

            if (_sealedPropBagMappers.TryGetValue(mapRequest, out IPropBagMapperGen result))
            {
                CheckForChanges(save, mapRequest, "Find in Sealed -- First Check.");
                return result;
            }

            _unSealedPropBagMappers.GetOrAdd(mapRequest);
            CheckForChanges(save, mapRequest, "GetOrAdd to UnSealed");

            int numberInThisBatch = SealThis(pCntr++);

            CheckForChanges(save, mapRequest, "Seal");

            if (!_sealedPropBagMappers.TryGetValue(mapRequest, out result))
            {
                CheckForChanges(save, mapRequest, "Find in Sealed -- second check.");
                result = null;
            }

            return result;
        }

        public void Clear()
        {
            foreach (IPropBagMapperGen mapper in _sealedPropBagMappers)
            {
                if (mapper is IDisposable disable)
                {
                    disable.Dispose();
                }
            }
            _unSealedPropBagMappers.Clear();

            foreach (IPropBagMapperGen mapper in _sealedPropBagMappers)
            {
                if (mapper is IDisposable disable)
                {
                    disable.Dispose();
                }
            }
            _sealedPropBagMappers.Clear();
        }

        private void CheckForChanges(IPropBagMapperKeyGen original, IPropBagMapperKeyGen current, string operationName)
        {
            if(!ReferenceEquals(original,current))
            {
                System.Diagnostics.Debug.WriteLine($"The mapRequest was updated by method call: {operationName}.");
            }
        }

        private int SealThis(int cntr)
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

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion


    }
}
