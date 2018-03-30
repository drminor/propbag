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
        private readonly LockingConcurrentDictionary<IAutoMapperRequestKeyGen, IAutoMapperRequestKeyGen> _unSealedAutoMappers;
        private readonly LockingConcurrentDictionary<IAutoMapperRequestKeyGen, IMapper> _sealedAutoMappers;

        private int pCntr = 0;

        public SimpleAutoMapperCache()
        {
            _unSealedAutoMappers = 
                new LockingConcurrentDictionary<IAutoMapperRequestKeyGen, IAutoMapperRequestKeyGen>(RequestFactory);

            _sealedAutoMappers =
                new LockingConcurrentDictionary<IAutoMapperRequestKeyGen, IMapper>(MapperFactory);
        }

        /// <summary>
        /// Queues a request to create an IMapper.
        /// </summary>
        /// <param name="mapperRequest"></param>
        /// <returns>The 'true' key present in the dictionary. This will be the key given if no previous matching registration has occured.</returns>

        public IAutoMapperRequestKeyGen RegisterRawAutoMapperRequest(IAutoMapperRequestKeyGen mapperRequest)
        {
            IAutoMapperRequestKeyGen result;

            if (_sealedAutoMappers.ContainsKey(mapperRequest))
            {
                //ICollection<IAutoMapperRequestKeyGen> keys = _sealedAutoMappers.Keys;

                //// TODO: This is relatively resource intensive, perhaps there's a better way.
                //IAutoMapperRequestKeyGen existingKey = keys.FirstOrDefault(x => x.DestinationTypeGenDef.Equals(autoMapperRequestKeyGen.DestinationTypeGenDef));

                //if(!ReferenceEquals(existingKey, autoMapperRequestKeyGen))
                //{
                //    System.Diagnostics.Debug.WriteLine("The mapRequest given to RegisterRawAutoMapperRequest was not the original key.");
                //}
                //result = existingKey;
                result = mapperRequest;
            }
            else
            {
                _unSealedAutoMappers.GetOrAdd(mapperRequest);
                result = mapperRequest;
            }

            return result;
        }

        public IMapper GetRawAutoMapper(IAutoMapperRequestKeyGen mapperRequest)
        {
            IAutoMapperRequestKeyGen save = mapperRequest;

            if (_sealedAutoMappers.TryGetValue(mapperRequest, out IMapper result))
            {
                CheckForChanges(save, mapperRequest, "Find in Sealed -- First Check.");
                return result;
            }

            _unSealedAutoMappers.GetOrAdd(mapperRequest);
            CheckForChanges(save, mapperRequest, "GetOrAdd to UnSealed");

            // Seal all pending mapper requests.
            int numberInThisBatch = SealThis(pCntr++);

            CheckForChanges(save, mapperRequest, "Seal");

            if (!_sealedAutoMappers.TryGetValue(mapperRequest, out result))
            {
                CheckForChanges(save, mapperRequest, "Find in Sealed -- second check.");
                result = null;
            }

            return result;
        }

        // Note: Creating a Typed GetRawAutoMapper would only offer a very small benefit. 
        // Remember if we create on IMapper, we need to process all pending requests in a single batch.
        // These requests have different values for TSource and TDestination and we would still have to
        // use the 'generic way' for all but the one referenced in this request.
        //public IMapper GetRawAutoMapper<TSource, TDestination>(IAutoMapperRequestKey<TSource, TDestination> mapperRequest) where TDestination : class, IPropBag
        //{
        //    IMapper result = null;

        //    return result;
        //}

        // TODO: Note: only the sealed mappers are counted.
        public long ClearRawAutoMappersCache()
        {
            long result = _sealedAutoMappers.Count;
            foreach (IAutoMapperRequestKeyGen mapperRequest in _sealedAutoMappers)
            {
                if (mapperRequest is IDisposable disable)
                {
                    disable.Dispose();
                }
            }
            _unSealedAutoMappers.Clear();

            foreach (IAutoMapperRequestKeyGen mapper in _sealedAutoMappers)
            {
                if (mapper is IDisposable disable)
                {
                    disable.Dispose();
                }
            }
            _sealedAutoMappers.Clear();

            return result;
        }

        private void CheckForChanges(IAutoMapperRequestKeyGen original, IAutoMapperRequestKeyGen current, string operationName)
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
            foreach (IAutoMapperRequestKeyGen key in _unSealedAutoMappers.Keys)
            {
                IMapper mapper = _sealedAutoMappers.GetOrAdd(key);
                key.AutoMapper = mapper;

                if (!(_unSealedAutoMappers.TryRemoveValue(key, out IAutoMapperRequestKeyGen dummyKey)))
                {
                    System.Diagnostics.Debug.WriteLine("Couldn't remove mappper request from list of registered, pending to be created, mapper requests.");
                }
                result++;
            }
            return result;
        }

        private IMapper MapperFactory(IAutoMapperRequestKeyGen key)
        {
            IMapper result = key.CreateRawAutoMapper();
            return result;
        }

        private IAutoMapperRequestKeyGen RequestFactory(IAutoMapperRequestKeyGen key)
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
