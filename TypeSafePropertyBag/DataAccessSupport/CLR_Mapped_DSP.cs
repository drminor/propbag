using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Linq;

namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    // This class takes an IEnumerable<T> of CLR objects that implement the IEditableObject interface
    // comming from a IDoCRUD data source...
    // and produces an ObservableCollection<T> that raises the ItemEndEdit event.

    public class ClrMappedDSP<T> : DataSourceProvider, INotifyItemEndEdit, IProvideADataSourceProvider, IHaveACrudWithMapping<T> where T: INotifyItemEndEdit
    {
        #region Private Properties

        IDoCRUD<T> _dataAccessLayer;

        #endregion

        public event EventHandler<EventArgs> ItemEndEdit;

        #region Constructor

        public ClrMappedDSP(IDoCRUD<T> dataAccessLayer/*, bool isAsynchronous*/)
        {
            _dataAccessLayer = dataAccessLayer;
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

        #endregion

        #region DataSourceProvider Overrides

        protected override void BeginQuery()
        {
            try
            {
                // Data holds a reference the previously fetched data, if any.
                if (Data is INotifyItemEndEdit inieeCurrent)
                {
                    inieeCurrent.ItemEndEdit -= Iniee_ItemEndEdit;
                }

                if (Data is INotifyCollectionChanged inccCurrent)
                {
                    inccCurrent.CollectionChanged -= Incc_CollectionChanged;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Could not remove ItemEndEdit handler. The exception description is {e.Message}.");
            }

            try
            {
                if (TryGetDataFromProp(_dataAccessLayer, out IEnumerable<T> rawData))
                {
                    EndEditWrapper<T> wrappedData = new EndEditWrapper<T>(rawData);

                    if (wrappedData is INotifyItemEndEdit inieeNew)
                    {
                        inieeNew.ItemEndEdit += Iniee_ItemEndEdit;
                    }
                    else if(wrappedData != null)
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
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (T item in e.OldItems.OfType<T>())
                {
                    if(item != null)
                    {
                        _dataAccessLayer.Delete(item);
                    }
                }
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

        // This is simply so that we can see when these events occur for diagnostic reasons.
        // TODO: Production code should not include these.
        protected override void BeginInit()
        {
            base.BeginInit();
        }

        protected override void EndInit()
        {
            base.EndInit();
        }

        #endregion
    }
}
