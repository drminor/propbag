using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DRM.Ipnwvc;

namespace DRM.PropBag
{

    public class PropExternStore<T> : PropGenBase, IProp<T>
    {
        public PropExternStore(string propertyName, object extraInfo,
            bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
            : base(typeof(T), typeIsSolid, true)
        {
            Tag = Guid.NewGuid(); // tag;
            Getter = null; // getter;
            Setter = null; // setter;

            DoWHenChangedAction = doWhenChanged;
            PropertyChangedWithTVals = delegate { };
            DoAfterNotify = doAfterNotify;
            Comparer = comparer ?? EqualityComparer<T>.Default;

            base.TypedProp = this;

        }

        public T TypedValue {
            get
            {
                return Getter(Tag);
            }
            set
            {
                Setter(Tag, value);
            }
        }

        public bool ValueIsDefined
        {
            get
            {
                return Getter != null;
            }
            private set
            {
                throw new InvalidOperationException("PropExternStore does not support setting 'ValueIsDefined.'");
            }
        }

        public bool SetValueToUndefined()
        {
            throw new InvalidOperationException("PropExternStore does not support setting 'ValueIsDefined.'");
        }

        public Guid Tag { get; private set; }
        public GetExtVal<T> Getter { get; set; }
        public SetExtVal<T> Setter { get; set; }

        IEqualityComparer<T> Comparer { get; set; }

        Action<T, T> _doWhenChangedAction;
        Action<T, T> DoWHenChangedAction
        {
            get { return _doWhenChangedAction; }
            set { _doWhenChangedAction = value; }
        }

        public bool DoAfterNotify { get; set; }

        //public bool HasCallBack
        //{
        //    get
        //    {
        //        return DoWHenChangedAction != null;
        //    }
        //}

        public void DoWhenChanged(T oldVal, T newVal)
        {
            Action<T, T> doWhenAction = Interlocked.CompareExchange(ref _doWhenChangedAction, null, null);

            if (doWhenAction != null)
                doWhenAction(oldVal, newVal);
        }

        public event PropertyChangedWithTValsHandler<T> PropertyChangedWithTVals;

        private List<Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>>> actTable = null;

        public void SubscribeToPropChanged(Action<T, T> doOnChange)
        {
            PropertyChangedWithTValsHandler<T> eventHandler = (s, e) => { doOnChange(e.OldValue, e.NewValue); };

            if (GetHandlerFromAction(doOnChange, ref actTable) != null)
            {
                PropertyChangedWithTVals += eventHandler;
                actTable.Add(new Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>>(doOnChange, eventHandler));
            }
        }

        public bool UnSubscribeToPropChanged(Action<T, T> doOnChange)
        {
            PropertyChangedWithTValsHandler<T> eventHander = GetHandlerFromAction(doOnChange, ref actTable);

            if (eventHander == null) return false;

            PropertyChangedWithTVals -= eventHander;
            return true;
        }

        private PropertyChangedWithTValsHandler<T> GetHandlerFromAction(Action<T, T> act,
            ref List<Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>>> actTable)
        {
            if (actTable == null)
            {
                actTable = new List<Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>>>();
                return null;
            }

            foreach (Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>> tup in actTable)
            {
                if (tup.Item1 == act) return tup.Item2;
            }
            return null;
        }


        public void OnPropertyChangedWithTVals(string propertyName, T oldVal, T newVal)
        {
            PropertyChangedWithTValsHandler<T> handler = Interlocked.CompareExchange(ref PropertyChangedWithTVals, null, null);

            if (handler != null)
                handler(this, new PropertyChangedWithTValsEventArgs<T>(propertyName, oldVal, newVal));
        }

        public bool CompareTo(T newValue)
        {
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

        ~PropExternStore()
        {
            Comparer = null;
            DoWHenChangedAction = null;
            PropertyChangedWithTVals = null;
        }
    }
}
