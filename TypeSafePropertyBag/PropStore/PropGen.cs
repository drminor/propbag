using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;

    public class PropGen : IPropDataInternal
    {
        #region Public PropGen Properties

        public bool IsEmpty { get; }
        public bool IsPropBag { get; private set; }

        public IProp TypedProp { get; private set; }

        public DoSetDelegate DoSetDelegate
        {
            get
            {
                return TypedProp.PropTemplate.DoSetDelegate;
            }
            set
            {
                TypedProp.PropTemplate.DoSetDelegate = value;
            }
        }

        #endregion

        #region Constructors

        public PropGen(IProp genericTypedProp)
        {
            IsEmpty = false;
            TypedProp = genericTypedProp ?? throw new ArgumentNullException($"{nameof(genericTypedProp)} must be non-null.");
            IsPropBag = genericTypedProp.PropTemplate.Type.IsPropBagBased();
        }

        public PropGen()
        {
            IsEmpty = true;
            TypedProp = null;
            IsPropBag = false;
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

        void IPropDataInternal.SetTypedProp(PropNameType propertyName, IProp value)
        {
            TypedProp = value;
            IsPropBag = value.PropTemplate.Type.IsPropBagBased();
        }

        #endregion
    }
}
