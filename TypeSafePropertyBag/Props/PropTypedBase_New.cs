using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag
{
    using IRegisterBindingsFowarderType = IRegisterBindingsForwarder<UInt32>;
    using PropIdType = UInt32;
    using PropNameType = String;

    public abstract class PropTypedBase_New<T> : IProp<T>
    {
        protected IPropTemplate<T> _template { get; }

        public PropTypedBase_New(PropNameType propertyName, T initalValue, bool typeIsSolid, IPropTemplate<T> template)
        {
            PropertyName = propertyName;
            _value = initalValue;
            TypeIsSolid = typeIsSolid;
            ValueIsDefined = true;
            _template = template;
        }

        public PropTypedBase_New(PropNameType propertyName, bool typeIsSolid, IPropTemplate<T> template)
        {
            PropertyName = propertyName;
            TypeIsSolid = typeIsSolid;
            ValueIsDefined = false;
            _template = template;
        }

        public event EventHandler<EventArgs> ValueChanged;

        T _value;
        public virtual T TypedValue
        {
            get
            {
                if (!ValueIsDefined)
                {
                    if (_template.ReturnDefaultForUndefined)
                    {
                        return _template.GetDefaultValFunc(PropertyName);
                    }
                    throw new InvalidOperationException("The value of this property has not yet been set.");
                }
                return _value;
            }
            set
            {
                if (StorageStrategy == PropStorageStrategyEnum.Internal)
                {
                    _value = value;
                    ValueIsDefined = true;
                }
                else
                {
                    throw new InvalidOperationException($"PropItem: {PropertyName} uses a {nameof(_template.StorageStrategy)} store: Its value cannot be set.");
                }
            }
        }

        public PropNameType PropertyName { get; }
        public bool TypeIsSolid { get; set; }
        public bool ValueIsDefined { get; protected set; }

        public Type Type => _template.Type;
        public PropKindEnum PropKind => _template.PropKind;
        public PropStorageStrategyEnum StorageStrategy => _template.StorageStrategy;

        public object TypedValueAsObject => TypedValue;

        public GetDefaultValueDelegate<T> GetDefaultValFunc => _template.GetDefaultValFunc;
        public bool ReturnDefaultForUndefined => _template.ReturnDefaultForUndefined;

        public Func<T, T, bool> Comparer => _template.Comparer;
        public Attribute[] Attributes => _template.Attributes;

        public abstract object Clone();

        public virtual void CleanUpTyped()
        {
            if (TypedValue is IDisposable disable)
            {
                disable.Dispose();
            }
        }

        public bool CompareTo(T value)
        {
            return _template.CompareTo(value);
        }

        public bool Compare(T val1, T val2)
        {
            return _template.Compare(val1, val2);
        }

        public virtual ValPlusType GetValuePlusType()
        {
            ValPlusType result;

            if (StorageStrategy == PropStorageStrategyEnum.Internal)
            {
                result = ValueIsDefined ? new ValPlusType(true, TypedValue, Type) : new ValPlusType(Type);
            }
            else
            {
                result = new ValPlusType(Type);
            }

            return result;
        }

        public virtual bool SetValueToUndefined()
        {
            bool oldSetting = ValueIsDefined;
            ValueIsDefined = false;

            return oldSetting;
        }

        #region Methods to Raise Events

        protected void OnValueChanged()
        {
            Interlocked.CompareExchange(ref ValueChanged, null, null)?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public virtual bool RegisterBinding(IRegisterBindingsFowarderType forwarder, PropIdType propId, LocalBindingInfo bindingInfo)
        {
            return forwarder.RegisterBinding<T>(propId, bindingInfo);
        }

        public virtual bool UnregisterBinding(IRegisterBindingsFowarderType forwarder, PropIdType propId, LocalBindingInfo bindingInfo)
        {
            return forwarder.RegisterBinding<T>(propId, bindingInfo);
        }
    }
}
         