using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DRM.PropBagWPF.Unused
{
    public class CrudWithMapping<TSource, TDestination> : IDoCRUD<TDestination>, IDisposable /*where TDal: IDoCRUD<TSource>*/ where TSource : class
    {
        #region Private Members

        IDoCRUD<TSource> _dataAccessLayer;
        private IPropBagMapper<TSource, TDestination> _mapper;

        #endregion

        #region Constructor

        public CrudWithMapping(IDoCRUD<TSource> dataAccessLayer, IPropBagMapper<TSource, TDestination> mapper)
        {
            _dataAccessLayer = dataAccessLayer;
            _mapper = mapper;
            _dataAccessLayer.DataSourceChanged += _dataAccessLayer_DataSourceChanged;
        }

        private void _dataAccessLayer_DataSourceChanged(object sender, EventArgs e)
        {
            OnDataSourceChanged(sender, e);
        }

        #endregion

        #region IDoCrud<TDestination> Implementation

        public event EventHandler<EventArgs> DataSourceChanged;

        public IEnumerable<TDestination> Get()
        {
            IEnumerable<TSource> unmappedItems = _dataAccessLayer.Get();
            IEnumerable<TDestination> mappedItems = _mapper.MapToDestination(unmappedItems);
            return mappedItems;
        }

        public IEnumerable<TDestination> Get(int top)
        {
            IEnumerable<TSource> unmappedItems = _dataAccessLayer.Get(top);

            List<TSource> test = unmappedItems.ToList();

            IEnumerable<TDestination> mappedItems = _mapper.MapToDestination(unmappedItems);
            return mappedItems;
        }

        //public IEnumerable<TDestination> Get<TKey>(int start, int count, Func<TSource, TKey> keySelector)
        //{
        //    IEnumerable<TSource> unmappedItems = _dataAccessLayer.Get<TKey>(start, count, keySelector);
        //    IEnumerable<TDestination> mappedItems = _mapper.MapToDestination(unmappedItems);
        //    //List<TDestination> test = mappedItems.ToList();

        //    return mappedItems;
        //}

        //public IEnumerable<TDestination> Get<TKey>(int start, int count, Func<TDestination, TKey> keySelector)
        //{
        //    Func<TSource, TKey> rawKeySelector = (x => x.Id);
        //    IEnumerable<TSource> unmappedItems = _dataAccessLayer.Get<TKey>(start, count, keySelector);
        //    IEnumerable<TDestination> mappedItems = _mapper.MapToDestination(unmappedItems);
        //    //List<TDestination> test = mappedItems.ToList();

        //    return mappedItems;
        //}

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

        #endregion

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

        private void OnDataSourceChanged(object sender, EventArgs e)
        {
            Interlocked.CompareExchange(ref DataSourceChanged, null, null)?.Invoke(sender, e);
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
                    if (_mapper != null) _mapper.Dispose();
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
