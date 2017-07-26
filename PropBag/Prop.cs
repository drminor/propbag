using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.ReferenceEquality;

namespace DRM.PropBag
{
    public class Prop<T> : IProp<T>
    {

        public Prop(T curValue, Action<T,T> doWhenChanged, bool doAfterNotify, IEqualityComparer<T> comparer)
        {
            Value = curValue;
            DoWHenChanged = doWhenChanged;
            DoAfterNotify = doAfterNotify;
            Comparer = comparer;
            UseReferenceEquality = false;
        }

        public Prop(T curValue, Action<T, T> doWhenChanged, bool doAfterNotify, bool useReferenceEquality)
        {
            Value = curValue;
            DoWHenChanged = doWhenChanged;
            DoAfterNotify = doAfterNotify;
            Comparer = null;
            UseReferenceEquality = useReferenceEquality;
        }


        public T Value { get; set; }
        public Action<T, T> DoWHenChanged { get; set; }
        public IEqualityComparer<T> Comparer { get; private set; }
        public bool UseReferenceEquality { get; private set; }
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
            if (UseReferenceEquality)
            {
                return ReferenceEqualityComparer.Default.Equals(newValue, Value);
            }
            else
            {
                if (Comparer != null)
                {
                    return Comparer.Equals(newValue, Value);
                }
                return EqualityComparer<T>.Default.Equals(newValue, Value);
            }
        }

    }
}
