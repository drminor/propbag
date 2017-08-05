using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DRM.Ipnwvc;

namespace DRM.PropBag
{
    public class Prop<T> : PropGenBase, IProp<T>
    {
        public Prop(T initalValue, bool hasStore = true, bool typeIsSolid = true, Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
            : base(typeof(T), typeIsSolid, hasStore)
        {
            if(hasStore) TypedValue = initalValue;

            DoWHenChangedAction = doWhenChanged;
            PropertyChangedWithTVals = delegate { };
            DoAfterNotify = doAfterNotify;
            Comparer = comparer ?? EqualityComparer<T>.Default;

            base.TypedProp = this;
        }

        T _value;
        public T TypedValue
        {
            get
            {
                if (HasStore) return _value;
                throw new InvalidOperationException();
            }
            set
            {
                if (!HasStore)
                    throw new InvalidOperationException();
                _value = value;
            }
        }

        IEqualityComparer<T> Comparer { get; set; }

        Action<T, T> _doWhenChangedAction;
        Action<T, T> DoWHenChangedAction {
            get { return _doWhenChangedAction;  }
            set { _doWhenChangedAction = value; }
        }

        public bool DoAfterNotify { get; set; }

        public event PropertyChangedWithTValsHandler<T> PropertyChangedWithTVals;

        public bool HasCallBack
        {
            get
            {
                return DoWHenChangedAction != null;
            }
        }

        public void DoWhenChanged(T oldVal, T newVal)
        {
            Action<T,T> doWhenAction = Interlocked.CompareExchange(ref _doWhenChangedAction, null, null);

            if (doWhenAction != null)
                doWhenAction(oldVal, newVal);
        }

        public void OnPropertyChangedWithTVals(string propertyName, T oldVal, T newVal)
        {
            PropertyChangedWithTValsHandler<T> handler = Interlocked.CompareExchange(ref PropertyChangedWithTVals, null, null);

            if (handler != null)
                handler(this, new PropertyChangedWithTValsEventArgs<T>(propertyName, oldVal, newVal));
        }

        public bool CompareTo(T newValue)
        {
            if (!HasStore)
                throw new NotImplementedException();

            return Comparer.Equals(newValue, TypedValue);
        }

        public bool Compare(T val1, T val2)
        {
            return Comparer.Equals(val1, val2);
        }

        public bool UpdateDoWhenChangedAction(Action<T, T> doWhenChangedAction, bool? doAfterNotify)
        {
            bool hadOnePreviously = doWhenChangedAction != null;

            this.DoWHenChangedAction = doWhenChangedAction;
            if (doAfterNotify.HasValue)
            {
                this.DoAfterNotify = doAfterNotify.Value;
            }
            else
            {
                // If there was a previous value of DoWhenChangedAction, leave the value of doAfterNotify alone,
                // otherwise set it to the default value of false.
                if (!hadOnePreviously)
                {
                    // Set doAfterNotify to the default value of false.
                    this.DoAfterNotify = false;
                }
            }

            return hadOnePreviously;
        }

        ~Prop()
        {
            Comparer = null;
            DoWHenChangedAction = null;
            PropertyChangedWithTVals = delegate {};
        }
    }
}
         