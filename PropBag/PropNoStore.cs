using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.ReferenceEquality;

namespace DRM.PropBag
{
    public class PropNoStore<T> : IProp<T>
    {
        public PropNoStore(Action<T, T> doWhenChanged, bool doAfterNotify, IEqualityComparer<T> comparer)
        {
            DoWHenChanged = doWhenChanged;
            DoAfterNotify = doAfterNotify;
            Comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public T Value
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Action<T, T> DoWHenChanged { get; set; }
        public IEqualityComparer<T> Comparer { get; private set; }
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
            throw new NotImplementedException();
        }

        public bool Compare(T val1, T val2)
        {
            return Comparer.Equals(val1, val2);
        }

    }
}
