using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    public class CrudWithMapping<TDal, TSource, TDestination> : IDoCRUD<TDestination>, IDisposable, IDoCrudWithMapping<TDestination> where TDal: class, IDoCRUD<TSource> where TSource : class
    {
        #region Private Members

        TDal _dataAccessLayer;
        IPropBagMapper<TSource, TDestination> _mapper;

        #endregion

        #region Constructor

        public CrudWithMapping(IWatchAPropItem<TDal> propItemWatcher, IPropBagMapper<TSource, TDestination> mapper)
        {
            _mapper = mapper;

            _dataAccessLayer = propItemWatcher.GetValue();
            if (_dataAccessLayer != null)
            {
                _dataAccessLayer.DataSourceChanged += _dataAccessLayer_DataSourceChanged;
            }

            propItemWatcher.PropertyChangedWithGenVals += PropItemWatcher_PropertyChangedWithGenVals; // .PropertyChangedWithTVals += PropItemWatcher_PropertyChangedWithTVals; 
        }

        private void PropItemWatcher_PropertyChangedWithGenVals(object sender, PcGenEventArgs e)
        {
            // TODO: Consider using the following statment, instead of the expanded version in place now.
            //DataAccessLayer = e.NewValue as TDal;

            if (e.NewValueIsUndefined/* || e.NewValue == null*/)
            {
                DataAccessLayer = null;
            }
            else
            {
                DataAccessLayer = e.NewValue as TDal;
            }

            //OnDataSourceChanged(this, EventArgs.Empty);
        }

        // TODO: Note: we could use PcTypedEventArgs if the ViewModel registered the source PropItem as type: IDoCRUD<T> instead of 
        // some derived type such as SourceDal : IDoCRUD<TSource>.

        ////private void PropItemWatcher_PropertyChangedWithTVals(object sender, PropertyChangedEventArgs e)
        //private void PropItemWatcher_PropertyChangedWithTVals(object sender, PcTypedEventArgs<TDal> e)
        //{
        //    DataAccessLayer = e.NewValueIsUndefined ? null : e.NewValue;
        //    OnDataSourceChanged(this, EventArgs.Empty);

        //    //if (e is PcTypedEventArgs<TDal> ts)
        //    //{
        //    //    DataAccessLayer = ts.NewValueIsUndefined ? null : ts.NewValue;
        //    //    OnDataSourceChanged(this, EventArgs.Empty);
        //    //}
        //    //else if (e is PcTypedEventArgs<IDoCRUD<TDestination>> td)
        //    //{
        //    //    //DataAccessLayer = ts.NewValueIsUndefined ? null : ts.NewValue;

        //    //    if (td.NewValueIsUndefined)
        //    //    {
        //    //        DataAccessLayer = null;
        //    //    }
        //    //    else
        //    //    {
        //    //        // TODO: Convert from IDoCRUD<TD> to IDoCRUD<TS>
        //    //        System.Diagnostics.Debug.WriteLine("Expected IDoCRUD<TSource>, but got IDoCRUD<TDestination>.");
        //    //    }

        //    //    OnDataSourceChanged(this, EventArgs.Empty);
        //}

        private void _dataAccessLayer_DataSourceChanged(object sender, EventArgs e)
        {
            // Note: Most DataSources, for example: PersonDAL, never changes its DataSource and therefore will never raise this event.
            OnDataSourceChanged(sender, e);
        }

        #endregion

        public IPropBagMapper<TSource, TDestination> Mapper => _mapper;

        public TDal DataAccessLayer
        {
            get
            {
                return _dataAccessLayer;
            }
            set
            {
                if(!ReferenceEquals(_dataAccessLayer, value))
                {
                    if(_dataAccessLayer != null)
                    {
                        _dataAccessLayer.DataSourceChanged -= _dataAccessLayer_DataSourceChanged;
                    }

                    _dataAccessLayer = value;

                    if(_dataAccessLayer != null)
                    {
                        _dataAccessLayer.DataSourceChanged += _dataAccessLayer_DataSourceChanged;
                    }

                    OnDataSourceChanged(this, EventArgs.Empty);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"CrudWithMapping was assigned a new DataAccessLayer, but is it the same object as the one previously assigned: The DataSourceChanged event is not being raised.");
                }
            }
        }

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
            if(_dataAccessLayer == null)
            {
                return new List<TDestination>();
            }

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
            TSource unMappedItemToDelete = GetUnmappedItem_int(itemToDelete);
            _dataAccessLayer.Delete(unMappedItemToDelete);
        }

        public void Update(TDestination updatedItem)
        {
            TSource unmappedItemToUpdate = GetUnmappedItem_int(updatedItem);
            _dataAccessLayer.Update(unmappedItemToUpdate);
        }

        public TDestination GetNewItem()
        {
            TDestination result = _mapper.GetNewDestination();
            return result;
        }

        #endregion

        private TSource GetUnmappedItem_int(TDestination mappedItem)
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

        public object GetUnmappedItem(TDestination mappedItem) => GetUnmappedItem_int(mappedItem);

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

        public bool TryGetNewItem(out object newItem)
        {
            newItem = _dataAccessLayer.GetNewItem();
            return true;
        }
        #endregion
    }
}
