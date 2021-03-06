﻿using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag
{
    using IRegisterBindingsFowarderType = IRegisterBindingsForwarder<UInt32>;
    using PropIdType = UInt32;
    using PropNameType = String;

    public abstract class PropTypedBase<T> : IProp<T>
    {
        protected IPropTemplate<T> _template { get; private set; }

        public PropTypedBase(PropNameType propertyName, T initalValue, bool typeIsSolid, IPropTemplate<T> template)
        {
            PropertyName = propertyName;
            _value = initalValue;
            TypeIsSolid = typeIsSolid;
            ValueIsDefined = true;
            _template = template;
        }

        public PropTypedBase(PropNameType propertyName, bool typeIsSolid, IPropTemplate<T> template)
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
                    if (ReturnDefaultForUndefined)
                    {
                        return _template.GetDefaultVal(PropertyName);
                    }
                    throw new InvalidOperationException("The value of this property has not yet been set.");
                }
                return _value;
            }
            set
            {
                if (_template.StorageStrategy == PropStorageStrategyEnum.Internal)
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

        public IPropTemplate PropTemplate => _template;

        public IPropTemplate<T> TypedPropTemplate => _template;

        //public Type Type => _template.Type;
        //public PropKindEnum PropKind => _template.PropKind;
        //public PropStorageStrategyEnum StorageStrategy => _template.StorageStrategy;

        public object TypedValueAsObject => TypedValue;

        public DoSetDelegate DoSetDelegate
        {
            get
            {
                return _template.DoSetDelegate;
            }
            set
            {
                _template.DoSetDelegate = value;
            }
        }

        public bool ReturnDefaultForUndefined => _template.GetDefaultVal != null;

        //public Func<string, T> GetDefaultVal => _template.GetDefaultVal;
        //public Func<T, T, bool> Comparer => _template.Comparer;

        //public object GetDefaultValFuncProxy => _template.GetDefaultValFuncProxy;
        //public object ComparerProxy => _template.ComparerProxy;

        //public Attribute[] Attributes => _template.Attributes;

        public abstract object Clone();

        public virtual void CleanUpTyped()
        {
            if (TypedValue is IDisposable disable)
            {
                disable.Dispose();
            }
            _template = null;
        }

        public bool CompareTo(T newValue)
        {
            if (_template.StorageStrategy == PropStorageStrategyEnum.Internal)
            {
                return _template.Comparer(newValue, TypedValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public bool Compare(T val1, T val2)
        {
            //return _template.Compare(val1, val2);
            return _template.Comparer(val1, val2);
        }

        public virtual ValPlusType GetValuePlusType()
        {
            ValPlusType result;

            if (_template.StorageStrategy == PropStorageStrategyEnum.Internal)
            {
                result = ValueIsDefined ? new ValPlusType(true, TypedValue, _template.Type) : new ValPlusType(_template.Type);
            }
            else
            {
                result = new ValPlusType(_template.Type);
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
         