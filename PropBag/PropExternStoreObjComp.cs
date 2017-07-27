using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.ReferenceEquality;

namespace DRM.PropBag
{

    public class PropExternStoreObjComp<T> : IProp<T>
    {
        public PropExternStoreObjComp(Guid tag, GetExtVal<T> getter, SetExtVal<T> setter, Action<T, T> doWhenChanged, bool doAfterNotify, IEqualityComparer<object> comparer)
        {
            Tag = tag;
            Getter = getter;
            Setter = setter;
            DoWHenChanged = doWhenChanged;
            DoAfterNotify = doAfterNotify;
            Comparer = comparer ?? ReferenceEqualityComparer.Default;
        }

        public T Value {
            get
            {
                return Getter(Tag);
            }
            set
            {
                Setter(Tag, value);
            }
        }

        public Guid Tag { get; private set; }
        private GetExtVal<T> Getter { get; set; }
        private SetExtVal<T> Setter { get; set; }

        public Action<T, T> DoWHenChanged { get; set; }
        public IEqualityComparer<object> Comparer { get; private set; }
        public bool DoAfterNotify { get; set; }

        public bool HasCallBack
        {
            get
            {
                return DoWHenChanged != null;
            }
        }

        public bool CompareTo(T newValue)
        {
            return Comparer.Equals(newValue, Value);
        }

        public bool Compare(T val1, T val2)
        {
            return Comparer.Equals(val1, val2);
        }

    }
}
