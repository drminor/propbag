using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections.Generic;


namespace MVVMApplication.ServiceAdapters
{
    public class CrudWithMapping<TDal, TSource, TDestination> : IDisposable where TDal: IDoCRUD<TSource> where TSource : class
    {
        TDal _dataAccessLayer;

        private IPropBagMapper<TSource, TDestination> _mapper;

        public CrudWithMapping(TDal dataAccessLayer, IPropBagMapper<TSource, TDestination> mapper)
        {
            _mapper = mapper;
            _dataAccessLayer = dataAccessLayer;
        }

        public IEnumerable<TDestination> Get()
        {
            IEnumerable<TSource> unmappedItems = _dataAccessLayer.Get();
            //IEnumerable<TSource> unmappedItems = temp.Cast<TSource>();
            IEnumerable<TDestination> mappedItems = _mapper.MapToDestination(unmappedItems);

            return mappedItems;
        }

        public IEnumerable<TDestination> Get(int top)
        {
            IEnumerable<TSource> unmappedItems = _dataAccessLayer.Get(top);
            //var temp = _dataAccessLayer.Get(top);
            //IEnumerable<TSource> unmappedItems = temp.Cast<TSource>();
            IEnumerable<TDestination> mappedItems = _mapper.MapToDestination(unmappedItems);

            return mappedItems;
        }

        public IEnumerable<TDestination> Get<TKey>(int start, int count, Func<TSource, TKey> keySelector)
        {
            IEnumerable<TSource> unmappedItems = _dataAccessLayer.Get<TKey>(start, count, keySelector);
            IEnumerable<TDestination> mappedItems = _mapper.MapToDestination(unmappedItems);
            //List<TDestination> test = mappedItems.ToList();

            return mappedItems;
        }

        public void Delete(TDestination itemToDelete)
        {
            TSource unMappedItemToDelete = GetUnmappedItem(itemToDelete);
            _dataAccessLayer.Delete(unMappedItemToDelete);
        }

        public void Update(TDestination updatedItem)
        {
            TSource unmappedItemToUpdate = GetUnmappedItem(updatedItem);

            _dataAccessLayer.Update(unmappedItemToUpdate);
        }

        public TDestination GetNewItem()
        {
            TDestination result = _mapper.GetNewDestination();
            return result;
        }

        private TSource GetUnmappedItem(TDestination mappedItem)
        {
            TSource result;
            if (mappedItem != null)
            {
                result = _mapper?.MapToSource(mappedItem);
            }
            else
            {
                result = null;
            }

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
                    // TODO: dispose managed state (managed objects).
                    if (_dataAccessLayer != null) _dataAccessLayer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Business() {
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
