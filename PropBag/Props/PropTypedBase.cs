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
        #region Private Members

        //private List<  Tuple<  Action<T, T>, EventHandler<PCTypedEventArgs<T>>   >   > _actTable = null;

        #endregion

        #region Public Members

        abstract public T TypedValue { get; set; }

        protected Func<T, T, bool> Comparer { get; set; }

        protected GetDefaultValueDelegate<T> GetDefaultValFunc { get; }
        public virtual bool ReturnDefaultForUndefined => GetDefaultValFunc != null;

        #endregion

        #region Constructors

        protected PropTypedBase(Type typeOfThisValue, bool typeIsSolid, bool hasStore,
            Func<T,T,bool> comparer, GetDefaultValueDelegate<T> defaultValFunc, PropKindEnum propKind)
            : base(propKind, typeOfThisValue, typeIsSolid, hasStore)
        {
            Comparer = comparer;
            GetDefaultValFunc = defaultValFunc;
        }

        #endregion

        #region Public Methods

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
            if (!ValueIsDefined) return false;

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
         