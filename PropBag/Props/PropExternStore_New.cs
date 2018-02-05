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
    public class PropExternStore_New<T> : PropTypedBase_New<T>
    {
        object _extraInfo { get; }

        public PropExternStore_New(string propertyName, object extraInfo, bool typeIsSolid, IPropTemplate<T> template)
            : base(propertyName, typeIsSolid, template)
        {
            _extraInfo = extraInfo;
            Tag = Guid.NewGuid(); // tag;
            Getter = null; // getter;
            Setter = null; // setter;

            ValueIsDefined = Getter != null;
        }

        override public T TypedValue
        {
            get
            {
                return Getter(Tag);
            }
            set
            {
                Setter(Tag, value);
            }
        }

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

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override void CleanUpTyped()
        {
            // TODO: Consider including in the constructor a way to opt out of this behaviour.
            base.CleanUpTyped();
        }
    }
}
