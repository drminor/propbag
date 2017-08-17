using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DRM.Inpcwv;

namespace DRM.PropBag
{
    public abstract class PropTypedBase<T> : PropGenBase, IProp<T>
    {
        protected PropTypedBase(Type typeOfThisValue, bool typeIsSolid, bool hasStore,
            Action<T, T> doWhenChanged, bool doAfterNotify,
            IEqualityComparer<T> comparer)
            : base(typeOfThisValue, typeIsSolid, hasStore)
        {
            DoWHenChangedAction = doWhenChanged;
            DoAfterNotify = doAfterNotify;
            Comparer = comparer;
            PropertyChangedWithTVals = null;
            base.TypedProp = this;
        }


        abstract public T TypedValue { get; set; }

        abstract public bool ValueIsDefined { get; }

        abstract public bool SetValueToUndefined();

        protected IEqualityComparer<T> Comparer { get; set; }

        private Action<T, T> _doWhenChangedAction;
        protected Action<T, T> DoWHenChangedAction {
            get { return _doWhenChangedAction;  }
            set { _doWhenChangedAction = value; }
        }

        public bool DoAfterNotify { get; set; }

        public void DoWhenChanged(T oldVal, T newVal)
        {
            Action<T,T> doWhenAction = Interlocked.CompareExchange(ref _doWhenChangedAction, null, null);

            if (doWhenAction != null)
                doWhenAction(oldVal, newVal);
        }

        public event PropertyChangedWithTValsHandler<T> PropertyChangedWithTVals;

        private List<Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>>> actTable = null;

        public void SubscribeToPropChanged(Action<T, T> doOnChange)
        {
            PropertyChangedWithTValsHandler<T> eventHandler = (s, e) => { doOnChange(e.OldValue, e.NewValue); };

            if (GetHandlerFromAction(doOnChange, ref actTable) == null)
            {
                PropertyChangedWithTVals += eventHandler;
                actTable.Add(new Tuple<Action<T,T>,PropertyChangedWithTValsHandler<T>>(doOnChange, eventHandler));
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

            for (int i = 0; i < actTable.Count; i++)
            {
                Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>> tup = actTable[i];
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
            if (!HasStore)
                throw new NotImplementedException();

            return Comparer.Equals(newValue, TypedValue);
        }

        public bool Compare(T val1, T val2)
        {
            if (!ValueIsDefined) return false;

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

        public object TypedValueAsObject { get { return Value; } }

        public bool CallBacksHappenAfterPubEvents { get { return DoAfterNotify; } }
        public bool HasCallBack { get { return DoWHenChangedAction != null; } }
        public bool HasChangedWithTValSubscribers { get { return PropertyChangedWithTVals != null; } }

        public void CleanUpTyped()
        {
            Comparer = null;
            DoWHenChangedAction = null;
            PropertyChangedWithTVals = null;
        }

    }
}
         