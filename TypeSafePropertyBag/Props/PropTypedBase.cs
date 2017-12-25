﻿using System;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using IRegisterBindingsFowarderType = IRegisterBindingsForwarder<UInt32>;

    public abstract class PropTypedBase<T> : PropBase, IProp<T>
    {
        #region Public and Protected Properties

        public virtual T TypedValue { get; set; }
        public virtual bool ReturnDefaultForUndefined => GetDefaultValFunc != null;

        protected Func<T, T, bool> Comparer { get; }
        protected GetDefaultValueDelegate<T> GetDefaultValFunc { get; }

        public override object TypedValueAsObject => (object)TypedValue;

        public override ValPlusType GetValuePlusType()
        {
            ValPlusType result;

            if(HasStore)
            {
                result = ValueIsDefined ? new ValPlusType(true, TypedValue, Type) : new ValPlusType(Type);
            }
            else
            {
                result = new ValPlusType(Type);
            }

            return result;
        }

        #endregion

        #region Constructors

        protected PropTypedBase(Type typeOfThisValue, bool typeIsSolid, bool hasStore, bool valueIsDefined,
            Func<T,T,bool> comparer, GetDefaultValueDelegate<T> defaultValFunc, PropKindEnum propKind)
            : base(propKind, typeOfThisValue, typeIsSolid, hasStore, valueIsDefined)
        {
            Comparer = comparer;
            GetDefaultValFunc = defaultValFunc;
        }

        #endregion

        #region Public Methods

        override public bool SetValueToUndefined()
        {
            bool oldSetting = ValueIsDefined;
            ValueIsDefined = false;

            return oldSetting;
        }

        public bool CompareTo(T newValue)
        {
            if (!HasStore)
                throw new NotImplementedException();

            //// Added this behavior on 10/23/2017.
            //if (!ValueIsDefined) return false;

            return Comparer(newValue, TypedValue);
        }

        public bool Compare(T val1, T val2)
        {
            //if (!ValueIsDefined) return false;

            return Comparer(val1, val2);
        }

        public override bool RegisterBinding(IRegisterBindingsFowarderType forwarder, PropIdType propId, LocalBindingInfo bindingInfo)
        {
            return forwarder.RegisterBinding<T>(propId, bindingInfo);
        }

        public override bool UnregisterBinding(IRegisterBindingsFowarderType forwarder, PropIdType propId, LocalBindingInfo bindingInfo)
        {
            return forwarder.RegisterBinding<T>(propId, bindingInfo);
        }

        #endregion
    }
}
         