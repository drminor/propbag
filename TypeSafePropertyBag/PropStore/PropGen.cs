using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;
    using PropIdType = UInt32;

    public class PropGen : IPropData, IPropDataInternal
    {
        #region Public Properties

        public event EventHandler<PCObjectEventArgs> PropertyChangedWithObjectVals;

        //public PropIdType PropId { get; }
        public CompositeKeyType CompKey { get; private set; }
        public bool IsPropBag { get; }

        ObjectIdType _childObjectId;
        public ObjectIdType ChildObjectId
        {   get { return _childObjectId; }
            private set
            {
                if(value != _childObjectId)
                {
                    ObjectIdType oldValue = ChildObjectId;
                    _childObjectId = value;
                    OnPropertyChangedWithObjectVals(nameof(ChildObjectId), oldValue, value);
                }
            }
        }

        IProp _typedProp;
        public IProp TypedProp
        {   get { return _typedProp; }
            private set
            {
                if(!object.ReferenceEquals(value, _typedProp))
                {
                    IProp oldValue = TypedProp;
                    
                    _typedProp = value;

                    OnPropertyChangedWithObjectVals(nameof(TypedProp), oldValue, value);
                }
            }
        }

        public bool IsEmpty => CompKey == 0;

        //public object Value => TypedProp.TypedValueAsObject;

        #endregion

        #region Constructors

        public PropGen(IProp genericTypedProp, PropIdType propId)
        {
            CompKey = propId;
            //PropId = propId;
            //CompKey = 0;
            ChildObjectId = 0;
            TypedProp = genericTypedProp ?? throw new ArgumentNullException($"{nameof(genericTypedProp)} must be non-null.");

            PropertyChangedWithObjectVals = null;
            if(TypedProp.Type.IsPropBagBased())
            {
                IsPropBag = true;
                // TODO: Our Caller (ie. the propBag) needs to add an event handler subscription
                // for this Properties on (standard) property changed event handler.
                // The event hander needs to set the Parent PropId on the PropStoreAccessor
                // that belongs to the objectId reference by our ChildPropId.
            }
            else
            {
                IsPropBag = false;
            }
        }

        public PropGen()
        {
            //PropId = 0;
            CompKey = 0;
            ChildObjectId = 0;
            TypedProp = null;
            IsPropBag = false;
            PropertyChangedWithObjectVals = null;
        }

        #endregion

        #region Public Methods

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

        #region IPropDataInternal Implementation

        void IPropDataInternal.SetCompKey(CompositeKeyType value)
        {
            CompKey = value;
        }

        void IPropDataInternal.SetTypedProp(IProp value)
        {
            TypedProp = value;
        }

        void IPropDataInternal.SetChildObjectId(ObjectIdType value)
        {
            ChildObjectId = value;
        }

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
