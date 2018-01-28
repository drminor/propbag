using System;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;

    internal class PropItemWatcherGen : IWatchAPropItemGen
    {
        #region Private Properties

        PSAccessServiceInternalInterface _storeAccessor;
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

        public PropItemWatcherGen(PSAccessServiceInternalInterface storeAccessor, PropIdType propId/*, bool isAsynchronous*/)
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

        public bool TryGetValue(out object value)
        {
            bool result = TryGetDataFromProp(this._storeAccessor, this._propId, out value);
            return result;
        }

        public object GetValue()
        {
            if (!TryGetDataFromProp(this._storeAccessor, this._propId, out object result))
            {
                throw new InvalidOperationException("Could not retrieve the value."); // TODO: Improve this exception message.
            }
            return result;
        }

        #endregion

        #region Private Methods

        private bool TryGetDataFromProp(PSAccessServiceInternalInterface storeAccessor, PropIdType propId, out object data)
        {
            StoreNodeProp propNode = _storeAccessor.GetChild(propId);
            IPropDataInternal propDataHolder = propNode.PropData_Internal;
            IProp typedProp = propDataHolder.TypedProp;
            data = typedProp.TypedValueAsObject;
            return true;
        }

        #endregion
    }
}
