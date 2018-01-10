using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Linq;



namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;

    // This class takes an IEnumerable<T> of CLR objects that implement the IEditableObject interface
    // comming from a IDoCRUD data source...
    // and produces an ObservableCollection<T> that raises the ItemEndEdit event.

    internal class ClrMappedDSP<T> : DataSourceProvider, INotifyItemEndEdit, IProvideADataSourceProvider where T: INotifyItemEndEdit
    {
        #region Private Properties

        PSAccessServiceInternalInterface _storeAccessor;
        PropIdType _propId;
        IDisposable _unsubscriber;

        IDoCRUD<T> _dataAccessLayer;

        #endregion

        public event EventHandler<EventArgs> ItemEndEdit;

        #region Constructor

        public ClrMappedDSP(PSAccessServiceInternalInterface storeAccessor, PropIdType propId/*, IDoCRUD<T> dataAccessLayer*//*, bool isAsynchronous*/)
        {
            _storeAccessor = storeAccessor;
            _propId = propId;

            _dataAccessLayer = GetCurrentDS(storeAccessor, propId);
            //IsAsynchronous = isAsynchronous;

            if (!(StartWatchingProp(_storeAccessor, _propId, ref _unsubscriber)))
            {
                throw new InvalidOperationException($"PB Collection could not subscribe to PropertyChanged (EventHandler<PcGenEventArgs) on the property with Id: {_propId}.");
            }

        }

        private IDoCRUD<T> GetCurrentDS(PSAccessServiceInternalInterface storeAccessor, PropIdType propId)
        {
            StoreNodeProp propNode = _storeAccessor.GetChild(_propId);
            if(propNode.Int_PropData.TypedProp is IDoCRUD<T> dal)
            {
                return dal;
            }
            else
            {
                if(propNode.Int_PropData.TypedProp != null)
                {
                    throw new InvalidOperationException($"The PropId: provided to CLR_Mapped_DSP refers to a PropItem: {propNode.CompKey} whose value does not implement: {nameof(IDoCRUD<T>)}.");
                }
                return null;
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
                        // TODO: Fix the warning message: WrappedData does not implement INotifyItemEndEdit.
                        System.Diagnostics.Debug.WriteLine($"Warning: Wrapped Data for property: FixMe does not implement INotifyItemEndEdit.");
                    }

                    if (wrappedData is INotifyCollectionChanged inccNew)
                    {
                        inccNew.CollectionChanged += Incc_CollectionChanged;
                    }

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

        private bool StartWatchingProp(PSAccessServiceInternalInterface storeAccessor, PropIdType propId, ref IDisposable unsubscriber)
        {
            if (unsubscriber != null)
            {
                unsubscriber.Dispose();
                //System.Diagnostics.Debug.WriteLine("The PBCollectionDataProvider has a previous subscription, that is not being unsubscribed.");
            }

            unsubscriber = storeAccessor.RegisterHandler(propId, DoWhenListSourceIsReset, SubscriptionPriorityGroup.Internal, keepRef: false);
            return true;
        }

        private void DoWhenListSourceIsReset(object sender, PcGenEventArgs args)
        {
            if(!args.NewValueIsUndefined)
            {
                IDoCRUD<T> dal = args.NewValue as IDoCRUD<T>;
                if(dal == null && args.NewValue != null)
                {
                    throw new InvalidOperationException($"CLR_Mapped_DSP is handling a DoWhenListSourceIsReset and the new value does not implement {nameof(IDoCRUD<T>)}.");
                }
                _dataAccessLayer = dal;
            }
           
            if (!IsRefreshDeferred)
            {
                Refresh();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Not refreshing, we are in a Deferred Refresh Cycle.");
            }
        }

        private bool TryGetDataFromProp(IDoCRUD<T> dataAccessLayer, out IEnumerable<T> data)
        {
            data = dataAccessLayer.Get(200);
            return true;
        }

        public bool IsCollection() => true;


        public bool IsReadOnly() => false;

        #endregion
    }
}
