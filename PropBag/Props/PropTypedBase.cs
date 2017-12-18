using DRM.TypeSafePropertyBag;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace DRM.PropBag
{
    using PropIdType = UInt32;
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public abstract class PropTypedBase<T> : PropBase, IPropPrivate<T>
    {
        #region Public and Protected Properties

        public virtual T TypedValue { get; set; }
        public virtual bool ReturnDefaultForUndefined => GetDefaultValFunc != null;

        protected Func<T, T, bool> Comparer { get; }
        protected GetDefaultValueDelegate<T> GetDefaultValFunc { get; }

        public override object TypedValueAsObject => (object)TypedValue;

        public override ValPlusType GetValuePlusType() => new ValPlusType(TypedValue, Type);

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

        public override bool RegisterBinding(IPropBagInternal propBag, PropIdType propId, LocalBindingInfo bindingInfo)
        {
            return propBag.RegisterBinding<T>(propId, bindingInfo);
        }

        public override bool UnregisterBinding(IPropBagInternal propBag, PropIdType propId, LocalBindingInfo bindingInfo)
        {
            return propBag.RegisterBinding<T>(propId, bindingInfo);
        }

        #endregion
    }
}
         