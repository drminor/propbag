using AutoMapper;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;

using System.Linq;

namespace DRM.PropBag.AutoMapperSupport
{
    //[Synchronization()]
    public class SimpleAutoMapperCache : /*ContextBoundObject,*/ ICacheAutoMappers, IDisposable
    {
        private readonly LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperKeyGen> _unSealedAutoMappers;
        private readonly LockingConcurrentDictionary<IPropBagMapperKeyGen, IMapper> _sealedAutoMappers;

        private int pCntr = 0;

        public SimpleAutoMapperCache()
        {
            _unSealedAutoMappers = 
                new LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperKeyGen>(GetPropBagMapperPromise);

            _sealedAutoMappers =
                new LockingConcurrentDictionary<IPropBagMapperKeyGen, IMapper>(GetPropBagMapperReal);
        }

        public IPropBagMapperKeyGen RegisterRawAutoMapperRequest(IPropBagMapperKeyGen mapRequest)
        {
            if (_sealedAutoMappers.ContainsKey(mapRequest))
            {
                ICollection<IPropBagMapperKeyGen> keys = _sealedAutoMappers.Keys;

                // TODO: This is relatively resource intensive, perhaps there's a better way.
                IPropBagMapperKeyGen existingKey = keys.FirstOrDefault(x => x.DestinationTypeGenDef.Equals(mapRequest.DestinationTypeGenDef));

                if(!ReferenceEquals(existingKey, mapRequest))
                {
                    System.Diagnostics.Debug.WriteLine("The mapRequest given to RegisterRawAutoMapperRequest was not the original key.");
                }
                return existingKey;
            }
            else
            {
                _unSealedAutoMappers.GetOrAdd(mapRequest);
                return mapRequest;
            }
        }

        public IMapper GetRawAutoMapper(IPropBagMapperKeyGen mapRequest)
        {
            IPropBagMapperKeyGen save = mapRequest;

            if (_sealedAutoMappers.TryGetValue(mapRequest, out IMapper result))
            {
                CheckForChanges(save, mapRequest, "Find in Sealed -- First Check.");
                return result;
            }

            _unSealedAutoMappers.GetOrAdd(mapRequest);
            CheckForChanges(save, mapRequest, "GetOrAdd to UnSealed");

            // Seal all pending mapper requests.
            int numberInThisBatch = SealThis(pCntr++);

            CheckForChanges(save, mapRequest, "Seal");

            if (!_sealedAutoMappers.TryGetValue(mapRequest, out result))
            {
                CheckForChanges(save, mapRequest, "Find in Sealed -- second check.");
                result = null;
            }

            return result;
        }

        // TODO: Note: only the sealed mappers are counted.
        public long ClearRawAutoMappersCache()
        {
            long result = _sealedAutoMappers.Count;
            foreach (IPropBagMapperGen mapper in _sealedAutoMappers)
            {
                if (mapper is IDisposable disable)
                {
                    disable.Dispose();
                }
            }
            _unSealedAutoMappers.Clear();

            foreach (IPropBagMapperGen mapper in _sealedAutoMappers)
            {
                if (mapper is IDisposable disable)
                {
                    disable.Dispose();
                }
            }
            _sealedAutoMappers.Clear();

            return result;
        }

        private void CheckForChanges(IPropBagMapperKeyGen original, IPropBagMapperKeyGen current, string operationName)
        {
            if (!ReferenceEquals(original, current))
            {
                System.Diagnostics.Debug.WriteLine($"The mapRequest object was replaced by method call: {operationName}.");
            }

            if (original != current)
            {
                System.Diagnostics.Debug.WriteLine($"The mapRequest was updated by method call: {operationName}.");
            }
        }

        private int SealThis(int cntr)
        {
            System.Diagnostics.Debug.WriteLine($"Creating Profile_{cntr.ToString()}");

            int result = 0;
            foreach (IPropBagMapperKeyGen key in _unSealedAutoMappers.Keys)
            {
                IMapper mapper = _sealedAutoMappers.GetOrAdd(key);
                key.AutoMapper = mapper;

                if (!(_unSealedAutoMappers.TryRemoveValue(key, out IPropBagMapperKeyGen dummyKey)))
                {
                    System.Diagnostics.Debug.WriteLine("Couldn't remove mappper request from list of registered, pending to be created, mapper requests.");
                }
                result++;
            }
            return result;
        }

        private IMapper GetPropBagMapperReal(IPropBagMapperKeyGen key)
        {
            IMapper result = key.CreateRawAutoMapper();
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
                    ClearRawAutoMappersCache();
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
