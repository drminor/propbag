using System;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;

    internal class PropItemWatcherGen : IWatchAPropItemGen
    {
        #region Private Properties

        WeakReference<PSAccessServiceInternalInterface> _storeAccessor_wr;
        //PSAccessServiceInternalInterface _storeAccessor;
        PropIdType _propId;

        Object _sync = new object();

        #endregion

        #region Event Declarations

        public event EventHandler<PcGenEventArgs> PropertyChangedWithGenVals
        {
            add
            {
                lock (_sync)
                {
                    if (_storeAccessor_wr.TryGetTarget(out PSAccessServiceInternalInterface storeAccessor))
                    {
                        storeAccessor.RegisterHandler(_propId, value, priorityGroup: SubscriptionPriorityGroup.Standard, keepRef: false);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Could not RegisterHandler: PropertyChangedWithGenVal on this PropItemWatcher -- the StoreAccess has been garbage collected.");
                    }
                }
            }
            remove
            {
                lock (_sync)
                {
                    if (_storeAccessor_wr.TryGetTarget(out PSAccessServiceInternalInterface storeAccessor))
                    {
                        storeAccessor.UnregisterHandler(_propId, value);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Could not RegisterHandler: PropertyChangedWithGenVal on this PropItemWatcher -- the StoreAccess has been garbage collected.");
                    }
                }
            }
        }

        #endregion

        #region Constructor

        public PropItemWatcherGen(PSAccessServiceInternalInterface storeAccessor, PropIdType propId/*, bool isAsynchronous*/)
        {
            _storeAccessor_wr = new WeakReference<PSAccessServiceInternalInterface>(storeAccessor);
            _propId = propId;
            //IsAsynchronous = isAsynchronous;
        }

        #endregion

        #region Public Properties

        public bool IsAsynchronous => false;

        #endregion

        #region Public Methods

        public bool TryGetValue(out object value)
        {
            bool result = TryGetDataFromProp(_storeAccessor_wr, this._propId, out value);
            return result;
        }

        public object GetValue()
        {
            if (!TryGetDataFromProp(_storeAccessor_wr, this._propId, out object result))
            {
                throw new InvalidOperationException("Could not retrieve the value."); // TODO: Improve this exception message.
            }
            return result;
        }

        #endregion

        #region Private Methods

        private bool TryGetDataFromProp(WeakReference<PSAccessServiceInternalInterface> storeAccessor_wr, PropIdType propId, out object data)
        {
            if(storeAccessor_wr.TryGetTarget(out PSAccessServiceInternalInterface storeAccessor))
            {
                PropNode propNode = storeAccessor.GetChild(propId);
                IPropDataInternal propDataHolder = propNode.PropData_Internal;
                IProp typedProp = propDataHolder.TypedProp;
                data = typedProp.TypedValueAsObject;
                return true;
            }
            else
            {
                data = null;
                return false;
            }
        }

        #endregion
    }
}
