using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public class Prop<T> : PropTypedBase<T>
    {
        public Prop(T initalValue,
            GetDefaultValueDelegate<T> defaultValFunc,
            bool typeIsSolid,
            bool hasStore,
            Func<T, T, bool> comparer)
            : base(typeof(T), typeIsSolid, hasStore, true, comparer, defaultValFunc, PropKindEnum.Prop)
        {
            if (hasStore)
            {
                _value = initalValue;
            }
        }

        public Prop(GetDefaultValueDelegate<T> defaultValFunc,
            bool typeIsSolid,
            bool hasStore,
            Func<T, T, bool> comparer)
            : base(typeof(T), typeIsSolid, hasStore, false, comparer, defaultValFunc, PropKindEnum.Prop)
        {
            //if (hasStore)
            //{
            //    ValueIsDefined = false;
            //}
        }

        T _value;
        override public T TypedValue
        {
            get
            {
                if (HasStore)
                {
                    if (!ValueIsDefined)
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
                    ValueIsDefined = true;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        //public override object TypedValueAsObject => (object)TypedValue;

        //public override ValPlusType GetValuePlusType()
        //{
        //    return new ValPlusType(TypedValue, Type);
        //}

        //private bool _valueIsDefined;
        //override public bool ValueIsDefined
        //{
        //    get
        //    {
        //        return _valueIsDefined;
        //    }
        //}

        //override public bool SetValueToUndefined()
        //{
        //    bool oldSetting = ValueIsDefined;
        //    ValueIsDefined = false;

        //    return oldSetting;
        //}

        //override public IListSource ListSource => throw new NotSupportedException($"This PropBag property is of PropKind = {PropKind}. It cannot provide a ListSource.");

        public override object Clone()
        {
            Prop<T> result;
            if (!ValueIsDefined)
            {
                result = new Prop<T>(defaultValFunc: GetDefaultValFunc, typeIsSolid: TypeIsSolid, hasStore: HasStore, comparer: Comparer);
            }
            else if (TypedValue == null || TypedValue is ValueType)
            {
                result = new Prop<T>(initalValue: TypedValue, defaultValFunc: GetDefaultValFunc, typeIsSolid: TypeIsSolid, hasStore: HasStore, comparer: Comparer);
            }
            else if (TypedValue is ICloneable ic)
            {
                result = new Prop<T>(initalValue: (T)ic.Clone(), defaultValFunc: GetDefaultValFunc, typeIsSolid: TypeIsSolid, hasStore: HasStore, comparer: Comparer);
            }
            else if (typeof(T) == typeof(string))
            {
                if(TryGetStringCopy(TypedValue, out T copy))
                {
                    result = new Prop<T>(initalValue: copy, defaultValFunc: GetDefaultValFunc, typeIsSolid: TypeIsSolid, hasStore: HasStore, comparer: Comparer);
                }
                else
                {
                    throw new InvalidOperationException("Although the value is a string type, could not make a copy using the available type converters.");
                }
            }
            else
            {
                //throw new InvalidOperationException("The Prop's value is not null, nor is it undefined nor does it implement ICloneable.");
                result = new Prop<T>(initalValue: default(T), defaultValFunc: GetDefaultValFunc, typeIsSolid: TypeIsSolid, hasStore: HasStore, comparer: Comparer);
            }

            return result;
        }

        private bool TryGetStringCopy(T val, out T copy)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));

            if (converter.CanConvertTo(typeof(string)))
            {
                string temp = (string)converter.ConvertToString(val);

                if(converter.CanConvertFrom(typeof(string)))
                {
                    copy = (T) converter.ConvertFrom(string.Copy(temp));
                    return true;
                }
                else
                {
                    converter = TypeDescriptor.GetConverter(typeof(string));

                    if(converter.CanConvertTo(typeof(T)))
                    {
                        copy = (T)converter.ConvertTo(string.Copy(temp), typeof(T));
                        return true;
                    }
                }
            }
            else
            {
                converter = TypeDescriptor.GetConverter(typeof(string));

                if (converter.CanConvertFrom(typeof(T)))
                {
                    string temp = (string) converter.ConvertFrom(val);
                    if (converter.CanConvertTo(typeof(T)))
                    {
                        copy = (T)converter.ConvertTo(string.Copy(temp), typeof(T));
                        return true;
                    }
                    else
                    {
                        converter = TypeDescriptor.GetConverter(typeof(T));

                        if (converter.CanConvertFrom(typeof(string)))
                        {
                            copy = (T)converter.ConvertTo(string.Copy(temp), typeof(T));
                            return true;
                        }
                    }
                }
            }

            copy = default(T);
            return false;
        }

        public override void CleanUpTyped()
        {
            // There's nothing to clean up.
        }

    }
}
         