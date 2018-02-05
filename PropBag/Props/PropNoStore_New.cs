﻿using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DRM.PropBag
{
    using PropNameType = String;

    public class PropNoStore_New<T> : PropTypedBase_New<T>
    {
        public PropNoStore_New(PropNameType propertyName, bool typeIsSolid, IPropTemplate<T> template)
            : base(propertyName, typeIsSolid, template)
        {
            if(template.StorageStrategy == PropStorageStrategyEnum.Internal)
            {
                throw new InvalidOperationException($"This implementation of IProp<T> does not support the {nameof(PropStorageStrategyEnum.Internal)} StorageStrategy.");
            }

            _haveFetchedValue = false;
        }

        bool _haveFetchedValue;

        T _value;
        override public T TypedValue
        {
            get
            {
                if (!_haveFetchedValue)
                {
                    _value = _template.GetDefaultValFunc(PropertyName);
                    _haveFetchedValue = true;
                }
                return _value;
            }
            set
            {
                // It's as if this call never happened.
            }
        }

        public override object Clone()
        {
            PropNoStore_New<T> result = new PropNoStore_New<T>(this.PropertyName, TypeIsSolid, this._template);
            return result;
        }

        public override void CleanUpTyped()
        {
            if(_haveFetchedValue)
            {
                base.CleanUpTyped();
            }
        }

    }
}
         