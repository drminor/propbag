using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Linq;
using System.Collections.ObjectModel;

namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    // This class takes an IEnumerable<T> of CLR objects that implement the IEditableObject interface
    // comming from a IDoCRUD data source...
    // and produces an ObservableCollection<T> that raises the ItemEndEdit event.

    //public class ClrMappedDSP<T> : DataSourceProvider, IMappedDSP<T> where T : INotifyItemEndEdit
    public class ClrMappedDSP<T> : DataSourceProvider, IDisposable, INotifyItemEndEdit, IProvideADataSourceProvider, IHaveACrudWithMapping<T> where T : INotifyItemEndEdit
    {
        #region Private Properties

        IDoCRUD<T> _dataAccessLayer;
        BetterLCVCreatorDelegate<T> _betterLCVCreatorDelegate;

        #endregion

        public event EventHandler<EventArgs> ItemEndEdit;

        #region Constructor

        public ClrMappedDSP(IDoCRUD<T> dataAccessLayer, BetterLCVCreatorDelegate<T> betterLCVCreatorDelegate/*, bool isAsynchronous*/)
        {
            _dataAccessLayer = dataAccessLayer;
            _betterLCVCreatorDelegate = betterLCVCreatorDelegate;
            //IsAsynchronous = isAsynchronous;

            _dataAccessLayer.DataSourceChanged += _dataAccessLayer_DataSourceChanged;
        }

        private void _dataAccessLayer_DataSourceChanged(object sender, EventArgs e)
        {
            if (!IsRefreshDeferred)
            {
                Refresh();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Not refreshing, we are in a Deferred Refresh Cycle.");
            }
        }

        #endregion

        #region Public Properties

        public bool TryGetTypedData(out IList data)
        {
            if (TryGetDataFromProp(_dataAccessLayer, out IEnumerable<T> rawData))
            {
                data = (IList)rawData;
                return true;
            }
            else
            {
                data = null;
                return false;
            }
        }

        public bool IsAsynchronous => false;

        public DataSourceProvider DataSourceProvider => this;

        public IDoCrudWithMapping<T> CrudWithMapping => _dataAccessLayer as IDoCrudWithMapping<T>;

        public Type CollectionItemRunTimeType => typeof(T);

        #endregion

        #region DataSourceProvider Overrides

        protected override void BeginQuery()
        {
            // Note: Data holds a reference the previously fetched data, if any.

            try
            {
                if (Data is INotifyItemEndEdit inieeCurrent)
                {
                    // XXTemp
                    inieeCurrent.ItemEndEdit -= Iniee_ItemEndEdit;
                }

                if (Data is INotifyCollectionChanged inccCurrent)
                {
                    // XXTemp
                    inccCurrent.CollectionChanged -= Incc_CollectionChanged;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Could not remove ItemEndEdit handler. The exception description is {e.Message}.");
            }

            DisposeOldData(Data);

            try
            {
                if (TryGetDataFromProp(_dataAccessLayer, out IEnumerable<T> rawData))
                {
                    EndEditWrapper<T> wrappedData = new EndEditWrapper<T>(rawData, _betterLCVCreatorDelegate);

                    if (wrappedData is INotifyItemEndEdit inieeNew)
                    {
                        // XXTemp
                        inieeNew.ItemEndEdit += Iniee_ItemEndEdit;
                    }
                    else if (wrappedData != null)
                    {
                        // TODO: CLR_Mapped_DSP needs to be given the name of the property from which the data is coming, if appropriate.
                        // TODO: Fix the warning message: WrappedData does not implement INotifyItemEndEdit.
                        System.Diagnostics.Debug.WriteLine($"Warning: Wrapped Data for property: 'FixMe' does not implement INotifyItemEndEdit.");
                    }

                    if (wrappedData is INotifyCollectionChanged inccNew)
                    {
                        inccNew.CollectionChanged += Incc_CollectionChanged;
                    }

                    // This will raise the DataSourceProvider.DataChanged event.
                    OnQueryFinished(wrappedData);
                }
                else
                {
                    // TODO: Fix this error message -- need to identify the data source and path.
                    throw new InvalidOperationException($"{nameof(ClrMappedDSP<T>)} cannot access property with Id = FixMe.");
                }
            }
            catch (Exception e2)
            {
                // TODO: Fix this error message -- need to identify the data source and path.
                throw new InvalidOperationException($"{nameof(ClrMappedDSP<T>)} cannot access property with Id = FixMe.", e2);
            }

            //IList<T> temp = new List<T>();
            //EndEditWrapper<T> wrappedData = new EndEditWrapper<T>(temp);
            //OnQueryFinished(wrappedData);
        }

        private void Iniee_ItemEndEdit(object sender, EventArgs e)
        {
            //T selectedItem = (T)sender;
            //if (selectedItem == null) return;

            if(sender is T selectedItem)
            {
                _dataAccessLayer.Update(selectedItem);
            }
            else
            {
                // TODO: Complete this warning message.
                System.Diagnostics.Debug.WriteLine($"Fix Me.");
            }

            OnItemEndEdit(sender, e);
        }

        private void Incc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (T item in e.OldItems.OfType<T>())
                {
                    if(item != null)
                    {
                        _dataAccessLayer.Delete(item);
                    }
                }
            }

            if(e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                // TODO: Handle new items.
            }
            else if(e.Action == NotifyCollectionChangedAction.Reset)
            {
                //DisposeOldData(Data);
            }
        }

        #endregion

        #region Private Methods

        protected void OnItemEndEdit(object sender, EventArgs e)
        {
            ItemEndEdit?.Invoke(sender, e);
        }

        private bool TryGetDataFromProp(IDoCRUD<T> dataAccessLayer, out IEnumerable<T> data)
        {
            data = dataAccessLayer.Get(200);
            return true;
        }

        public bool IsCollection() => true;
        public bool IsReadOnly() => false;

        public bool TryGetNewItem(out object newItem)
        {
            if(CrudWithMapping != null)
            {
                newItem = CrudWithMapping.GetNewItem();
                return true;
            }
            else
            {
                newItem = null;
                return false;
            }
        }

        //// This is simply so that we can see when these events occur for diagnostic reasons.
        //// TODO: Production code should not include these.
        //protected override void BeginInit()
        //{
        //    base.BeginInit();
        //}

        //protected override void EndInit()
        //{
        //    base.EndInit();
        //}

        private void DisposeOldData(object data)
        {
            if(data is IDisposable collectionDisable)
            {
                collectionDisable.Dispose();
            }
            else if (data is IEnumerable list)
            {
                foreach (object o in list)
                {
                    if (o is IDisposable disable)
                    {
                        // TODO: Consider wrapping this in a Try/Catch.
                        disable.Dispose();
                    }
                }
            }
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    DisposeOldData(this.Data);

                    _dataAccessLayer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.
                _dataAccessLayer = null;

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
