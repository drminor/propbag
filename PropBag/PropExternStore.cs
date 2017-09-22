using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace DRM.PropBag
{

    public sealed class PropExternStore<T> : PropTypedBase<T>
    {
        public PropExternStore(string propertyName, object extraInfo,
            bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null)
            : base(typeof(T), typeIsSolid, false, doWhenChanged, doAfterNotify, comparer)
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

        override public bool ValueIsDefined
        {
            get
            {
                return Getter != null;
            }
        }

        override public bool SetValueToUndefined()
        {
            throw new InvalidOperationException("PropExternStore does not support setting the value to undefined.");
        }

        public Guid Tag { get; private set; }
        public GetExtVal<T> Getter { get; set; }
        public SetExtVal<T> Setter { get; set; }
    }
}
