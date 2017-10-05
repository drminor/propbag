﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace DRM.PropBag
{
    public sealed class Prop<T> : PropTypedBase<T>
    {
        public Prop(T initalValue,
            GetDefaultValueDelegate<T> getDefaultValFunc,
            bool typeIsSolid,
            bool hasStore,
            Action<T, T> doWhenChanged = null,
            bool doAfterNotify = false,
            Func<T, T, bool> comparer = null)
            : base(typeof(T), typeIsSolid, hasStore, doWhenChanged, doAfterNotify, comparer, getDefaultValFunc)
        {
            if (hasStore)
            {
                TypedValue = initalValue;
                _valueIsDefined = true;
            }
        }

        public Prop(GetDefaultValueDelegate<T> getDefaultValFunc,
            bool typeIsSolid = true,
            bool hasStore = true,
            Action<T, T> doWhenChanged = null,
            bool doAfterNotify = false,
            Func<T, T, bool> comparer = null)
            : base(typeof(T), typeIsSolid, hasStore, doWhenChanged, doAfterNotify, comparer, getDefaultValFunc)
        {
            if (hasStore)
            {
                _valueIsDefined = false;
            }
        }

        T _value;
        override public T TypedValue
        {
            get
            {
                if (HasStore)
                {
                    if (!_valueIsDefined)
                    {
                        if (ReturnDefaultForUndefined)
                        {
                            return this.GetDefaultValFunc("Prop object doesn't know the prop's name.");
                        }
                        throw new InvalidOperationException("The value of this property has not yet been set.");
                    }
                    return _value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            set
            {
                if (HasStore)
                {
                    _value = value;
                    _valueIsDefined = true;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private bool _valueIsDefined;
        override public bool ValueIsDefined
        {
            get
            {
                return _valueIsDefined;
            }
        }

        override public bool SetValueToUndefined()
        {
            bool oldSetting = this._valueIsDefined;
            _valueIsDefined = false;

            return oldSetting;
        }

    }
}
         