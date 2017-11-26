using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;


    public class PropGen : IPropDataInternal
    {
        #region Public PropGen Properties

        public event EventHandler<PCObjectEventArgs> PropertyChangedWithObjectVals = delegate {};

        public PropIdType PropId { get; }

        IProp _typedProp;
        public IProp TypedProp
        {
            get { return _typedProp; }
            //private set
            //{
            //    if (!object.ReferenceEquals(value, _typedProp))
            //    {
            //        IProp oldValue = TypedProp;

            //        _typedProp = value;

            //        OnPropertyChangedWithObjectVals(nameof(TypedProp), oldValue, value);
            //    }
            //}
        }

        public bool IsEmpty => _cKey.isEmpty;

        #endregion

        #region Public IPropDataInternal Properties

        private ExKeyT _cKey;
        ExKeyT IPropDataInternal.CKey => _cKey;

        private bool _isPropBag;
        bool IPropDataInternal.IsPropBag => _isPropBag;

        #endregion

        #region Constructors

        public PropGen(IProp genericTypedProp, ExKeyT cKey, PropIdType propId, PropNameType propertyName)
        {
            _cKey = cKey;
            PropId = propId;
            if(genericTypedProp == null) throw new ArgumentNullException($"{nameof(genericTypedProp)} must be non-null.");
            ((IPropDataInternal)this).SetTypedProp(propertyName, genericTypedProp);
        }

        private void GenericTypedProp_PropertyChangedWithObjectVals(object sender, PCObjectEventArgs e)
        {
            OnPropertyChangedWithObjectVals(e);
        }

        public PropGen()
        {
            _cKey = new SimpleExKey(0);
            PropId = 0;
            _typedProp = null;
            _isPropBag = false;
            PropertyChangedWithObjectVals = null;
        }

        //event EventHandler<PCObjectEventArgs> INotifyPCObject.PropertyChangedWithObjectVals
        //{
        //    add
        //    {

        //        throw new NotImplementedException();
        //    }

        //    remove
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        #endregion

        #region PropGen Public Methods

        public ValPlusType ValuePlusType()
        {
            return new ValPlusType(TypedProp.TypedValueAsObject, TypedProp.Type);
        }

        public void CleanUp(bool doTypedCleanup)
        {
            IProp typedProp = TypedProp;

            PropertyChangedWithObjectVals = null;

            if (typedProp != null)
            {
                //if (typedProp.GetType().IsPropBagBased())
                //{
                //    // Remove our handler from the old value.
                //    typedProp.PropertyChangedWithObjectVals -= GenericTypedProp_PropertyChangedWithObjectVals;
                //}

                if (doTypedCleanup) typedProp.CleanUpTyped();
            }
        }

        #endregion

        #region IPropDataInternal Methods

        void IPropDataInternal.SetTypedProp(PropNameType propertyName, IProp value)
        {
            //if (!object.ReferenceEquals(value, _typedProp))
            //{
            //    IProp oldValue = _typedProp;
            //    if (oldValue != null && oldValue.GetType().IsPropBagBased())
            //    {
            //        // Remove our handler from the old value.
            //        oldValue.PropertyChangedWithObjectVals -= GenericTypedProp_PropertyChangedWithObjectVals;
            //    }

            //    _typedProp = value;
            //    OnPropertyChangedWithObjectVals(propertyName, oldValue?.TypedValueAsObject, value?.TypedValueAsObject);

            //    if (_typedProp.TypedValueAsObject != null && _typedProp.TypedValueAsObject.GetType().IsPropBagBased())
            //    {
            //        _isPropBag = true;
            //        // Subscribe to the new value's PropertyChanged event.
            //        _typedProp.PropertyChangedWithObjectVals += GenericTypedProp_PropertyChangedWithObjectVals;
            //    }
            //    else
            //    {
            //        _isPropBag = false;
            //    }
            //}

            _typedProp = value;
            _isPropBag = _typedProp.Type.IsPropBagBased();
        }

        #endregion

        #region Private Methods

        public void OnPropertyChangedWithObjectVals(string propertyName, object oldValue, object newValue)
        {
            Interlocked.CompareExchange(ref PropertyChangedWithObjectVals, null, null)
                ?.Invoke(this, new PCObjectEventArgs(propertyName, oldValue, newValue));
        }

        public void OnPropertyChangedWithObjectVals(PCObjectEventArgs e)
        {
            Interlocked.CompareExchange(ref PropertyChangedWithObjectVals, null, null)
                ?.Invoke(this, e);
        }

        #endregion
    }
}
