using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using IRegisterBindingsFowarderType = IRegisterBindingsForwarder<UInt32>;

    public abstract class PropTypedBase<T> : PropBase, IProp<T>
    {
        #region Public and Protected Properties

        public virtual T TypedValue { get; set; }
        public override bool ReturnDefaultForUndefined => GetDefaultValFunc != null;

        public Func<T, T, bool> Comparer { get; }
        public GetDefaultValueDelegate<T> GetDefaultValFunc { get; }

        public override object TypedValueAsObject => TypedValue;

        public override ValPlusType GetValuePlusType()
        {
            ValPlusType result;

            if(StorageStrategy == PropStorageStrategyEnum.Internal)
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

        protected PropTypedBase(Type typeOfThisValue, bool typeIsSolid, PropStorageStrategyEnum storageStrategy, bool valueIsDefined,
            Func<T,T,bool> comparer, GetDefaultValueDelegate<T> defaultValFunc, PropKindEnum propKind)
            : base(propKind, typeOfThisValue, typeIsSolid, storageStrategy, valueIsDefined)
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
            if(StorageStrategy == PropStorageStrategyEnum.Internal)
            {
                return Comparer(newValue, TypedValue);
            }
            else
            {
                throw new NotImplementedException();
            }
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
         