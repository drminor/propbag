using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PropIdType = UInt32;
    using PropNameType = String;

    public class PropGen : IPropDataInternal
    {
        #region Private Properties

        //private ExKeyT _cKey;
        private bool _isPropBag;

        #endregion

        #region Public PropGen Properties

        //public PropIdType PropId { get; }
        public IProp TypedProp { get; private set; }
        public bool IsEmpty { get; }

        #endregion

        #region Constructors

        public PropGen(/*ExKeyT cKey, */IProp genericTypedProp)
        {
            IsEmpty = false;
            //_cKey = cKey;
            TypedProp = genericTypedProp ?? throw new ArgumentNullException($"{nameof(genericTypedProp)} must be non-null.");

            //PropId = cKey.Level2Key;
            _isPropBag = genericTypedProp.Type.IsPropBagBased();
        }

        public PropGen()
        {
            IsEmpty = true;
            TypedProp = null;
            //_cKey = new SimpleExKey();
            //PropId = _cKey.Level2Key;
            _isPropBag = false;
        }

        #endregion

        #region PropGen Public Methods

        public ValPlusType GetValuePlusType()
        {
            return TypedProp.GetValuePlusType();  //new ValPlusType(TypedProp.TypedValueAsObject, TypedProp.Type);
        }

        public void CleanUp(bool doTypedCleanup)
        {
            // Note: We have no managed (or unmanaged) resources to cleanup, 
            //all we have to do is call the TypeProp's Cleanup method.
            IProp typedProp = TypedProp;

            if (typedProp != null)
            {
                if (doTypedCleanup) typedProp.CleanUpTyped();
            }
        }

        #endregion

        #region IPropDataInternal Implementation

        //ExKeyT IPropDataInternal.CKey => _cKey;

        bool IPropDataInternal.IsPropBag => _isPropBag;

        void IPropDataInternal.SetTypedProp(PropNameType propertyName, IProp value)
        {
            TypedProp = value;
            _isPropBag = value.Type.IsPropBagBased();
        }

        #endregion
    }
}
