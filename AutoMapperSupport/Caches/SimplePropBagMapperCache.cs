using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    //[Synchronization()]
    public class SimplePropBagMapperCache : /*ContextBoundObject,*/ ICachePropBagMappers, IDisposable
    {
        private readonly ViewModelFactoryInterface _viewModelFactory;

        private readonly LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperKeyGen> _requests;
        private readonly LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperGen> _propBagMappers;


        public SimplePropBagMapperCache(ViewModelFactoryInterface viewModelFactory)
        {
            _viewModelFactory = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));

            _requests =
                new LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperKeyGen>(RequestFactory);

            _propBagMappers =
                new LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperGen>(MapperFactory);
        }

        /// <summary>
        /// Queues a request to create and IMapper and a new PropBagMapper to be created from the IMapper.
        /// </summary>
        /// <param name="mapperRequest"></param>
        /// <returns>The 'true' key present in the dictionary. This will be the key given if no previous matching registration has occured.</returns>
        public IPropBagMapperKeyGen RegisterPropBagMapperRequest(IPropBagMapperKeyGen mapperRequest)
        {
            IPropBagMapperKeyGen result;

            if (_propBagMappers.ContainsKey(mapperRequest))
            {
                //ICollection<IPropBagMapperKeyGen> keys = _propBagMappers.Keys;

                //// TODO: This is relatively resource intensive, perhaps there's a better way.
                //IPropBagMapperKeyGen existingRequest = keys.FirstOrDefault
                //    (
                //    x => x.DestinationTypeGenDef.Equals(mapperRequest.DestinationTypeGenDef)
                //    );

                //result = existingRequest;
                result = mapperRequest;
            }
            else
            {
                IPropBagMapperKeyGen newOrExistingRequest = _requests.GetOrAdd(mapperRequest);
                result = newOrExistingRequest;
            }

            return result;
        }

        public IPropBagMapperGen GetPropBagMapper(IPropBagMapperKeyGen mapperRequest)
        {
            if(mapperRequest.AutoMapper == null)
            {
                throw new InvalidOperationException($"The {nameof(SimplePropBagMapperCache)} was asked to GetPropBagMapper, however the mapperRequest has a null AutoMapper.");
            }

            IPropBagMapperKeyGen save = mapperRequest;

            if (_propBagMappers.TryGetValue(mapperRequest, out IPropBagMapperGen result))
            {
                return result;
            }
            else
            {
                //IPropBagMapperKeyGen newOrExistingRequest = _Requests.GetOrAdd(mapperRequest);
                //result = _propBagMappers.GetOrAdd(newOrExistingRequest);

                // Remove the request from the request que if present.
                _requests.TryRemoveValue(mapperRequest, out IPropBagMapperKeyGen dummyExistingRequest);

                result = _propBagMappers.GetOrAdd(mapperRequest);
                CheckForChanges(save, mapperRequest, "GetOrAdd PropBagMapper.");
            }

            return result;
        }

        // Typed Get Mapper
        public IPropBagMapper<TSource, TDestination> GetPropBagMapper<TSource, TDestination>
        (
            IPropBagMapperKey<TSource, TDestination> mapperRequest
        )
        where TDestination : class, IPropBag
        {
            IPropBagMapper<TSource, TDestination> result;

            if (mapperRequest.AutoMapper == null)
            {
                throw new InvalidOperationException($"The {nameof(SimplePropBagMapperCache)} was asked to GetPropBagMapper, however the mapperRequest has a null AutoMapper.");
            }

            if (_propBagMappers.TryGetValue(mapperRequest, out IPropBagMapperGen genMapper))
            {
                result = (IPropBagMapper<TSource, TDestination>)genMapper;
            }
            else
            {
                result = mapperRequest.GeneratePropBagMapper(mapperRequest, _viewModelFactory);

                // Remove the request from the request que if present.
                _requests.TryRemoveValue(mapperRequest, out IPropBagMapperKeyGen dummyExistingRequest);
            }

            return result;
        }

        public long ClearPropBagMappersCache()
        {
            // TODO: Clear the request que as well.

            foreach (IPropBagMapperKeyGen mapperRequest in _requests)
            {
                if (mapperRequest is IDisposable disable)
                {
                    disable.Dispose();
                }
            }

            _requests.Clear();

            long result = _propBagMappers.Count;
            foreach (IPropBagMapperGen mapper in _propBagMappers)
            {
                if (mapper is IDisposable disable)
                {
                    disable.Dispose();
                }
            }

            _propBagMappers.Clear();

            return result;
        }

        private void CheckForChanges(IPropBagMapperKeyGen original, IPropBagMapperKeyGen current, string operationName)
        {
            if(!ReferenceEquals(original,current))
            {
                System.Diagnostics.Debug.WriteLine($"The mapRequest object was replaced by method call: {operationName}.");
            }

            if (original != current)
            {
                System.Diagnostics.Debug.WriteLine($"The mapRequest was updated by method call: {operationName}.");
            }
            else
            {
                System.Diagnostics.Debug.Assert(1 == 1, "Remove this.");
            }
        }

        private IPropBagMapperKeyGen RequestFactory(IPropBagMapperKeyGen key)
        {
            return key;
        }

        private IPropBagMapperGen MapperFactory(IPropBagMapperKeyGen key)
        {
            IPropBagMapperGen result = key.CreatePropBagMapper(_viewModelFactory);
            return result;
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
                    ClearPropBagMappersCache();
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
