using System;
using System.Collections;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalType = IPropStoreAccessServiceInternal<UInt32, String>;

    internal class PBCollectionDataProvider : DataSourceProvider
    {
        #region Private Properties

        PSAccessServiceInternalType _storeAccessor;
        PropIdType _propId;
        IDisposable _unsubscriber;

        #endregion

        #region Constructor

        public PBCollectionDataProvider(PSAccessServiceInternalType storeAccessor, PropIdType propId/*, bool isAsynchronous*/)
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
            if(TryGetDataFromProp(_storeAccessor, _propId, out object rawData))
            {
                if (rawData is IList data)
                {
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
                throw new InvalidOperationException($"{nameof(PBCollectionDataProvider)} cannot access property with Id = {_propId}.");
            }
        }

        // Other methods that we may override soon.
        //public override IDisposable DeferRefresh()
        //{
        //    return base.DeferRefresh();
        //}

        //protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        //{
        //    base.OnPropertyChanged(e);
        //}

        //protected override void OnQueryFinished(object newData, Exception error, DispatcherOperationCallback completionWork, object callbackArguments)
        //{
        //    base.OnQueryFinished(newData, error, completionWork, callbackArguments);
        //}

        #endregion

        #region Private Methods

        private bool StartWatchingProp(PSAccessServiceInternalType storeAccessor, PropIdType propId, ref IDisposable unsubscriber)
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
            // TODO: Attempt to avoid unsubscribing, just to resubscribe, if the PropItem is the same.
            _unsubscriber?.Dispose();
            _unsubscriber = null;

            if (!IsRefreshDeferred)
            {
                Refresh();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Not refreshing, we are in a Deferred Refresh Cycle.");
            }
        }

        private bool TryGetDataFromProp(PSAccessServiceInternalType storeAccessor, PropIdType propId, out object data)
        {
            StoreNodeProp propNode = _storeAccessor.GetChild(_propId);

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
                //data = null;
                //return false;
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
