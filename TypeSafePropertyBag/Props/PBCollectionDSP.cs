using System;
using System.Collections;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;

    // Takes a IProp<CT,T> where CT: is an ObservableCollection<T> and where CT raises the ItemEndEdit event.
    // It calls for a refresh, which results in the binder to call our BeginQuery method,
    // which will raise our DataChanged event, if the data produced by BeginQuery is different from the 'current data'.

    internal class PBCollectionDSP : DataSourceProvider, INotifyItemEndEdit
    {
        #region Private Properties

        PSAccessServiceInternalInterface _storeAccessor;
        PropIdType _propId;
        IDisposable _unsubscriber;

        #endregion

        public event EventHandler<EventArgs> ItemEndEdit;

        #region Constructor

        public PBCollectionDSP(PSAccessServiceInternalInterface storeAccessor, PropIdType propId/*, bool isAsynchronous*/)
        {
            _storeAccessor = storeAccessor;
            _propId = propId;
            //IsAsynchronous = isAsynchronous;

            if(!(StartWatchingProp(_storeAccessor, _propId, ref _unsubscriber)))
            {
                throw new InvalidOperationException($"PB Collection could not subscribe to PropertyChanged (EventHandler<PcGenEventArgs) on the property with Id: {_propId}.");
            }
        }

        #endregion

        #region Public Properties

        public bool TryGetTypedData(out IList data)
        {
            if(TryGetDataFromProp(_storeAccessor, _propId, out object rawData))
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

        #endregion

        #region DataSourceProvider Overrides

        protected override void BeginQuery()
        {
            // TODO: We should take a weak reference to the parent PropBag and use it
            // to detect when this data is coming from a new PropBag object.
            // Actually our LocalBinder should take care of this work -- let's test before
            // implementing checks here.

            try
            {
                // Data holds a reference the previously fetched data, if any.
                if (Data is INotifyItemEndEdit inieeCurrent)
                {
                    inieeCurrent.ItemEndEdit -= Iniee_ItemEndEdit;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Could not remove ItemEndEdit handler. The exception description is {e.Message}.");
            }

            if (TryGetDataFromProp(_storeAccessor, _propId, out object rawData))
            {
                if (rawData is IList data)
                {
                    if(data is INotifyItemEndEdit inieeNew)
                    {
                        inieeNew.ItemEndEdit += Iniee_ItemEndEdit;
                    }
                    // This raises our DataChanged event (courtsey of our base class.)
                    OnQueryFinished(data);
                }
                else
                {
                    OnQueryFinished(null);
                }
            } 
            else
            {
                // TODO: Fix this error message -- need a way to better identify the property and parent PropBag.
                throw new InvalidOperationException($"{nameof(PBCollectionDSP)} cannot access property with Id = {_propId}.");
            }
        }

        private void Iniee_ItemEndEdit(object sender, EventArgs e)
        {
            OnItemEndEdit(sender, e);
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
            //// TODO: Attempt to avoid unsubscribing, just to resubscribe, if the PropItem is the same.
            //_unsubscriber?.Dispose();
            //_unsubscriber = null;

            if (!IsRefreshDeferred)
            {
                Refresh();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Not refreshing, we are in a Deferred Refresh Cycle.");
            }
        }

        private bool TryGetDataFromProp(PSAccessServiceInternalInterface storeAccessor, PropIdType propId, out object data)
        {
            StoreNodeProp propNode = _storeAccessor.GetChild(propId);

            IPropDataInternal propDataHolder = propNode.Int_PropData;

            IProp typedProp = propDataHolder.TypedProp;

            // TODO: Create Extensions for PropKindEnum to query IEnumerable, IList, etc.
            if (typedProp.PropKind == PropKindEnum.ObservableCollection)
            {
                data = typedProp.TypedValueAsObject;
                return true;
            }
            else
            {
                throw new InvalidOperationException("The source PropItem is not of kind = ObservableCollection.");
            }
        }

        //private PSAccessServiceType GetRegularService(PSAccessServiceInternalType internalService)
        //{
        //    if (internalService is PSAccessServiceType regularService)
        //    {
        //        return regularService;
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException($"That {nameof(PSAccessServiceInternalType)} instance does not implement {nameof(PSAccessServiceType)}.");
        //    }
        //}

        #endregion
    }
}
