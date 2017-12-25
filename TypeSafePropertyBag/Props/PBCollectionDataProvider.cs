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
        }

        #endregion

        #region Public Properties

        public bool IsAsynchronous => false;

        #endregion

        #region DataSourceProvider Overrides

        protected override void BeginQuery()
        {
            if(TryGetDataFromProp(_storeAccessor, _propId, out object rawData))
            {
                if (rawData is IList data)
                {
                    _unsubscriber = StartWatchingProp(_storeAccessor, _propId);
                    OnQueryFinished(data);
                }
                else
                {
                    OnQueryFinished(null);
                }
            } 
            else
            {
                // TODO: Fix this error message.
                throw new NotSupportedException($"{nameof(PBCollectionDataProvider)} cannot access property with Id = {_propId}.");
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

        private IDisposable StartWatchingProp(PSAccessServiceInternalType storeAccessor, PropIdType propId)
        {
            IDisposable result = storeAccessor.RegisterHandler(propId, DoWhenListSourceIsReset, SubscriptionPriorityGroup.Internal, keepRef: false);
            return result;
        }

        private void DoWhenListSourceIsReset(object sender, PcGenEventArgs args)
        {
            // TODO: Attempt to avoid unsubscribing, just to resubscribe, if the PropItem is the same.
            _unsubscriber?.Dispose();

            if (!IsRefreshDeferred) Refresh();
        }

        private bool TryGetDataFromProp(PSAccessServiceInternalType storeAccessor, PropIdType propId, out object data)
        {
            StoreNodeProp propNode = _storeAccessor.GetChild(_propId);

            IPropDataInternal propDataHolder = propNode.Int_PropData;

            IProp typedProp = propDataHolder.TypedProp;

            // TODO: Create Extensions for PropKindEnum to query IE, IList, etc.
            if (typedProp.PropKind == PropKindEnum.ObservableCollection /*|| typedProp.PropKind == PropKindEnum.ObservableCollectionFB*/)
            {
                data = typedProp.TypedValueAsObject;
                return true;
            }
            else
            {
                data = null;
                return false;
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
