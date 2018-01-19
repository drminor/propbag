using System;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;

    internal class PropItemWatcher<T> : IWatchAPropItem<T>
    {
        #region Private Properties

        PSAccessServiceInternalInterface _storeAccessor;
        PropIdType _propId;

        Object _sync = new object();

        #endregion

        #region Event Declarations

        public event EventHandler<PcTypedEventArgs<T>> PropertyChangedWithTVals
        {
            add
            {
                lock(_sync)
                {
                    _storeAccessor.RegisterHandler(_propId, value, priorityGroup: SubscriptionPriorityGroup.Standard, keepRef: false);
                }
            }
            remove
            {
                lock (_sync)
                {
                    _storeAccessor.UnregisterHandler(_propId, value);
                }
            }
        }

        public event EventHandler<PcGenEventArgs> PropertyChangedWithGenVals
        {
            add
            {
                lock (_sync)
                {
                    _storeAccessor.RegisterHandler(_propId, value, priorityGroup: SubscriptionPriorityGroup.Standard, keepRef: false);
                }
            }
            remove
            {
                lock (_sync)
                {
                    _storeAccessor.UnregisterHandler(_propId, value);
                }
            }
        }

        #endregion

        #region Constructor

        public PropItemWatcher(PSAccessServiceInternalInterface storeAccessor, PropIdType propId/*, bool isAsynchronous*/)
        {
            _storeAccessor = storeAccessor;
            _propId = propId;
            //IsAsynchronous = isAsynchronous;
        }

        #endregion

        #region Public Properties

        public bool IsAsynchronous => false;

        #endregion

        #region Public Methods

        public T GetValue()
        {
            if (TryGetDataFromProp(this._storeAccessor, this._propId, out object rawValue))
            {
                T result = (T)rawValue;
                return result;
            }
            else
            {
                throw new InvalidOperationException("Could not retrieve the value."); // TODO: Improve this exception message.
            }
        }

        public bool TryGetValue(out object value)
        {
            bool result = TryGetDataFromProp(this._storeAccessor, this._propId, out value);
            return result;
        }

        object IWatchAPropItemGen.GetValue()
        {
            if (!TryGetDataFromProp(this._storeAccessor, this._propId, out object result))
            {
                throw new InvalidOperationException("Could not retrieve the value."); // TODO: Improve this exception message.
            }
            return result;
        }

        #endregion

        #region Private Methods

        public bool TryGetValue(out T value)
        {
            if(TryGetDataFromProp(this._storeAccessor, this._propId, out object rawValue))
            {
                value = (T)rawValue;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        private bool TryGetDataFromProp(PSAccessServiceInternalInterface storeAccessor, PropIdType propId, out object data)
        {
            StoreNodeProp propNode = _storeAccessor.GetChild(propId);
            IPropDataInternal propDataHolder = propNode.Int_PropData;
            IProp typedProp = propDataHolder.TypedProp;
            data = typedProp.TypedValueAsObject;
            return true;
        }

        #endregion
    }
}
