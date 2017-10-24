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
            GetDefaultValueDelegate<T> getDefaultValFunc,
            bool typeIsSolid = true,
            Func<T, T, bool> comparer = null,
            Action<T, T> doWhenChanged = null,
            bool doAfterNotify = false)
            : base(typeof(T), typeIsSolid, false, doWhenChanged, doAfterNotify, comparer, getDefaultValFunc)
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

        override public IListSource ListSource => throw new NotSupportedException("This PropBag property is not a collection or datatable PropType.");

    }
}
