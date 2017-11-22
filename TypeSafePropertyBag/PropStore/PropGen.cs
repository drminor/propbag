using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;

    public class PropGen : IPropDataInternal
    {
        #region Public PropGen Properties

        public event EventHandler<PCObjectEventArgs> PropertyChangedWithObjectVals;

        public PropIdType PropId { get; }

        IProp _typedProp;
        public IProp TypedProp
        {
            get { return _typedProp; }
            private set
            {
                if (!object.ReferenceEquals(value, _typedProp))
                {
                    IProp oldValue = TypedProp;

                    _typedProp = value;

                    OnPropertyChangedWithObjectVals(nameof(TypedProp), oldValue, value);
                }
            }
        }

        public bool IsEmpty => _cKey == 0;

        #endregion

        #region Public IPropDataInternal Properties

        CompositeKeyType _cKey;
        CompositeKeyType IPropDataInternal.CKey => _cKey;

        //ObjectIdType _childObjectId;
        //ObjectIdType IPropDataInternal.ChildObjectId
        //{
        //    get { return _childObjectId; }
        //    set
        //    {
        //        if (value != _childObjectId)
        //        {
        //            ObjectIdType oldValue = ((IPropDataInternal)this).ChildObjectId;
        //            _childObjectId = value;
        //            OnPropertyChangedWithObjectVals(nameof(IPropDataInternal.ChildObjectId), oldValue, value);
        //        }
        //    }
        //}

        private bool _isPropBag;
        bool IPropDataInternal.IsPropBag => _isPropBag;

        #endregion

        #region Constructors

        public PropGen(IProp genericTypedProp, CompositeKeyType cKey, PropIdType propId)
        {
            _cKey = cKey;
            PropId = propId;
            //_childObjectId = 0;
            TypedProp = genericTypedProp ?? throw new ArgumentNullException($"{nameof(genericTypedProp)} must be non-null.");

            PropertyChangedWithObjectVals = null;
            if(TypedProp.Type.IsPropBagBased())
            {
                _isPropBag = true;
                IProp ip = TypedProp;
                //
                // TODO: Our Caller (ie. the propBag) needs to add an event handler subscription
                // for this Properties on (standard) property changed event handler.
                // The event hander needs to set the Parent PropId on the PropStoreAccessor
                // that belongs to the objectId reference by our ChildPropId.
            }
            else
            {
                _isPropBag = false;
            }
        }

        ///// <summary>
        ///// Used to create an objectRefHolder for the tree-based global store.
        ///// </summary>
        ///// <param name="cKey"></param>
        //public PropGen(CompositeKeyType cKey)
        //{
        //    _cKey = cKey;
        //    PropId = 0;
        //    _childObjectId = 0;
        //    TypedProp = null;
        //    _isPropBag = true;
        //    PropertyChangedWithObjectVals = null;
        //}

        public PropGen()
        {
            _cKey = 0;
            PropId = 0;
            //_childObjectId = 0;
            TypedProp = null;
            _isPropBag = false;
            PropertyChangedWithObjectVals = null;
        }

        event EventHandler<PCObjectEventArgs> INotifyPCObject.PropertyChangedWithObjectVals
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region PropGen Public Methods

        public ValPlusType ValuePlusType()
        {
            return new ValPlusType(TypedProp.TypedValueAsObject, TypedProp.Type);
        }

        public void CleanUp(bool doTypedCleanup)
        {
            if(doTypedCleanup && TypedProp != null) TypedProp.CleanUpTyped();
            PropertyChangedWithObjectVals = null;
        }

        #endregion

        #region IPropDataInternal Methods

        //void IPropDataInternal.SetCompKey(CompositeKeyType value)
        //{
        //    _cKey = value;
        //}

        void IPropDataInternal.SetTypedProp(IProp value)
        {
            TypedProp = value;
        }

        //void IPropDataInternal.SetChildObjectId(ObjectIdType value)
        //{
        //    ChildObjectId = value;
        //}

        #endregion

        #region Private Methods

        public void OnPropertyChangedWithObjectVals(string propertyName, object oldValue, object newValue)
        {
            EventHandler<PCObjectEventArgs> handler = Interlocked.CompareExchange(ref PropertyChangedWithObjectVals, null, null);

            if (handler != null)
                handler(this, new PCObjectEventArgs(propertyName, oldValue, newValue));
        }

        #endregion
    }
}
