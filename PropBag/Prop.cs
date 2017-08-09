using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DRM.Ipnwvc;

namespace DRM.PropBag
{
    public sealed class Prop<T> : PropTypedBase<T>
    {
        public Prop(T initalValue, bool hasStore = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false,
            IEqualityComparer<T> comparer = null)
            : base(typeof(T), typeIsSolid, hasStore, doWhenChanged, doAfterNotify, comparer ?? EqualityComparer<T>.Default)
        {
            if (hasStore)
            {
                TypedValue = initalValue;
                PValueIsDefined = true;
            }
        }

        public Prop(bool hasStore = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false,
            IEqualityComparer<T> comparer = null)
            : base(typeof(T), typeIsSolid, hasStore, doWhenChanged, doAfterNotify, comparer ?? EqualityComparer<T>.Default)
        {
            if (hasStore)
            {
                PValueIsDefined = false;
            }
        }

        T _value;
        override public T TypedValue
        {
            get
            {
                if (HasStore)
                {
                    if (!PValueIsDefined) throw new InvalidOperationException("The value of this property has not yet been set.");
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
                    PValueIsDefined = true;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private bool PValueIsDefined { get; set; }

        override public bool ValueIsDefined
        {
            get
            {
                return PValueIsDefined;
            }
        }

        override public bool SetValueToUndefined()
        {
            bool oldSetting = this.PValueIsDefined;
            PValueIsDefined = false;

            return oldSetting;
        }

    }
}
         