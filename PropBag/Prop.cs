using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DRM.ReferenceEquality;
using DRM.Ipnwvc;

namespace DRM.PropBag
{
    public class Prop<T> : IProp<T>
    {
        public Prop(T curValue, Action<T, T> doWhenChanged, bool doAfterNotify, IEqualityComparer<T> comparer)
        {
            Value = curValue;
            DoWHenChanged = doWhenChanged;
            PropertyChangedWithTVals = null;
            DoAfterNotify = doAfterNotify;
            Comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public T Value { get; set; }
        public IEqualityComparer<T> Comparer { get; private set; }

        public Action<T, T> DoWHenChanged { get; set; }
        public bool DoAfterNotify { get; set; }

        public event PropertyChangedWithTValsHandler<T> PropertyChangedWithTVals;

        public bool HasCallBack
        {
            get
            {
                return DoWHenChanged != null;
            }
        }

        public void OnPropertyChangedWithTVals(string propertyName, T oldVal, T newVal)
        {
            PropertyChangedWithTValsHandler<T> handler = Interlocked.CompareExchange(ref PropertyChangedWithTVals, null, null);

            if (handler != null)
                handler(this, new PropertyChangedWithTValsEventArgs<T>(propertyName, oldVal, newVal));
        }

        public bool CompareTo(T newValue)
        {
            return Comparer.Equals(newValue, Value);
        }

        public bool Compare(T val1, T val2)
        {
            return Comparer.Equals(val1, val2);
        }

        ~Prop()
        {
            Comparer = null;
            DoWHenChanged = null;
            PropertyChangedWithTVals = delegate {};
        }
    }
}
         