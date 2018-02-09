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
    // TODO: The ExternStore" contract does not define a way for the external store to create undefined values.
    public class PropExternStore<T> : PropTypedBase<T>
    {
        public PropExternStore(string propertyName,
            object extraInfo,
            Func<string, T> defaultValFunc,
            bool typeIsSolid,
            Func<T, T, bool> comparer)
            : base(propertyName, typeof(T), typeIsSolid, PropStorageStrategyEnum.External, true,  comparer, defaultValFunc, PropKindEnum.Prop)
        {
            Tag = Guid.NewGuid(); // tag;
            Getter = null; // getter;
            Setter = null; // setter;
        }

        override public T TypedValue {
            get
            {
                return Getter(Tag);
            }
            set
            {
                Setter(Tag, value);
            }
        }

        public override object TypedValueAsObject => (object)TypedValue;

        public override ValPlusType GetValuePlusType()
        {
            return new ValPlusType(TypedValue, Type);
        }

        //override public bool ValueIsDefined
        //{
        //    get
        //    {
        //        return Getter != null;
        //    }
        //}

        override public bool SetValueToUndefined()
        {
            throw new InvalidOperationException("PropExternStore does not support setting the value to undefined.");
        }

        public Guid Tag { get; private set; }

        GetExtVal<T> _getter;
        public GetExtVal<T> Getter
        {
            get => _getter;
            set
            {
                if(!ReferenceEquals(_getter, value))
                {
                    ValueIsDefined = value != null;
                }
            }
        }
        public SetExtVal<T> Setter { get; set; }

        //override public IListSource ListSource => throw new NotSupportedException("This PropBag property is not a collection or datatable PropType.");

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override void CleanUpTyped()
        {
            // There's nothing to clean up.
        }
    }
}
