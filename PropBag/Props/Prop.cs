using DRM.TypeSafePropertyBag;
using System;
using System.ComponentModel;

namespace DRM.PropBag
{
    using PropNameType = String;

    public class Prop<T> : PropTypedBase_New<T>
    {
        public Prop(PropNameType propertyName, T initalValue, bool typeIsSolid, IPropTemplate<T> template) 
            : base(propertyName, initalValue, typeIsSolid, template)
        {
        }

        public Prop(PropNameType propertyName, bool typeIsSolid, IPropTemplate<T> template)
            : base(propertyName, typeIsSolid, template)
        {
        }

        public override object Clone()
        {
            Prop<T> result;
            if (!ValueIsDefined)
            {
                //result = new Prop<T>(defaultValFunc: GetDefaultValFunc, typeIsSolid: TypeIsSolid, storageStrategy: StorageStrategy, comparer: Comparer);
                result = new Prop<T>(propertyName: PropertyName, typeIsSolid: TypeIsSolid, template: _template);
            }
            else if (TypedValue == null || TypedValue is ValueType)
            {
                //result = new Prop<T>(initalValue: TypedValue, defaultValFunc: GetDefaultValFunc, typeIsSolid: TypeIsSolid, storageStrategy: StorageStrategy, comparer: Comparer);
                result = new Prop<T>(propertyName: PropertyName, initalValue: TypedValue, typeIsSolid: TypeIsSolid, template: _template);

            }
            else if (TypedValue is ICloneable ic)
            {
                //result = new Prop<T>(initalValue: (T)ic.Clone(), defaultValFunc: GetDefaultValFunc, typeIsSolid: TypeIsSolid, storageStrategy: StorageStrategy, comparer: Comparer);
                result = new Prop<T>(propertyName: PropertyName, initalValue: (T)ic.Clone(), typeIsSolid: TypeIsSolid, template: _template);
            }
            else if (typeof(T) == typeof(string))
            {
                if(TryGetStringCopy(TypedValue, out T copy))
                {
                    //result = new Prop<T>(initalValue: copy, defaultValFunc: GetDefaultValFunc, typeIsSolid: TypeIsSolid, storageStrategy: StorageStrategy, comparer: Comparer);
                    result = new Prop<T>(propertyName: PropertyName, initalValue: copy, typeIsSolid: TypeIsSolid, template: _template);
                }
                else
                {
                    throw new InvalidOperationException("Although the value is a string type, could not make a copy using the available type converters.");
                }
            }
            else
            {
                // TODO: This should throw an invalid operation exeception. Need to test before switching code back.
                //throw new InvalidOperationException("The Prop's value is not null, nor is it undefined nor does it implement ICloneable.");

                T hack = GetDefaultValFunc(PropertyName);
                result = new Prop<T>(propertyName: PropertyName, initalValue: hack, typeIsSolid: TypeIsSolid, template: _template);
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
    }
}
         