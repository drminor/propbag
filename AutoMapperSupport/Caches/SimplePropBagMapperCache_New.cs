using AutoMapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    //[Synchronization()]
    public class SimplePropBagMapperCache_New : /*ContextBoundObject,*/ ICachePropBagMappers, IDisposable
    {
        private readonly ViewModelFactoryInterface _viewModelFactory;
        private readonly ICacheAutoMappers _autoMappersCache;

        private readonly LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperGen> _propBagMappers;


        public SimplePropBagMapperCache_New(ICacheAutoMappers autoMappersCache, ViewModelFactoryInterface viewModelFactory)
        {
            _autoMappersCache = autoMappersCache;
            _viewModelFactory = viewModelFactory;

            _propBagMappers =
                new LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperGen>(GetPropBagMapperReal);
        }

        public IPropBagMapperKeyGen RegisterPropBagMapperRequest(IPropBagMapperKeyGen mapperRequest)
        {
            IPropBagMapperKeyGen result = _autoMappersCache.RegisterRawAutoMapperRequest(mapperRequest);
            return result;
        }

        public IPropBagMapperGen GetPropBagMapper(IPropBagMapperKeyGen mapRequest)
        {
            IPropBagMapperKeyGen save = mapRequest;

            if (_propBagMappers.TryGetValue(mapRequest, out IPropBagMapperGen result))
            {
                CheckForChanges(save, mapRequest, "PropBagMapperCache - After TryGetValue from our Cache.");
                return result;
            }
            else
            {
                IMapper autoMapper = _autoMappersCache.GetRawAutoMapper(mapRequest);
                mapRequest.AutoMapper = autoMapper;
                result = _propBagMappers.GetOrAdd(mapRequest);

                CheckForChanges(save, mapRequest, "PropBagMapperCache - After GetRaw from AutoMapperCache.");
            }

            return result;
        }

        // TODO: Note: only the sealed mappers are counted.
        public long ClearPropBagMappersCache()
        {
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
        }

        private IPropBagMapperGen GetPropBagMapperReal(IPropBagMapperKeyGen key)
        {
            IPropBagMapperGen result = key.CreateMapper(_viewModelFactory);
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
