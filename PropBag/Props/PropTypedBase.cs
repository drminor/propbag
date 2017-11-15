using DRM.TypeSafePropertyBag;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace DRM.PropBag
{
    public abstract class PropTypedBase<T> : PropBase, IPropPrivate<T>
    {
        #region Private Members

        //private List<  Tuple<  Action<T, T>, EventHandler<PCTypedEventArgs<T>>   >   > _actTable = null;

        #endregion

        #region Public Members

        public event EventHandler<PCTypedEventArgs<T>> PropertyChangedWithTVals;

        abstract public T TypedValue { get; set; }

        protected Func<T, T, bool> Comparer { get; set; }

        private Action<T, T> _doWhenChangedAction;
        public Action<T, T> DoWHenChangedAction
        {
            get { return _doWhenChangedAction; }
            private set { _doWhenChangedAction = value; }
        }

        public bool DoAfterNotify { get; set; }

        protected GetDefaultValueDelegate<T> GetDefaultValFunc { get; }
        public virtual bool ReturnDefaultForUndefined => GetDefaultValFunc != null;

        //public object TypedValueAsObject { get { return (object)TypedValue; } }

        //public bool CallBacksHappenAfterPubEvents { get { return DoAfterNotify; } }
        //public bool HasCallBack { get { return DoWHenChangedAction != null; } }
        //public bool HasChangedWithTValSubscribers { get { return PropertyChangedWithTVals != null; } }

        #endregion

        #region Consructors

        protected PropTypedBase(Type typeOfThisValue, bool typeIsSolid, bool hasStore,
            EventHandler<PCTypedEventArgs<T>> doWhenChangedX,
            //Action<T, T> doWhenChanged,
            bool doAfterNotify,
            Func<T,T,bool> comparer,
            GetDefaultValueDelegate<T> getDefaultValFunc,
            PropKindEnum propKind = PropKindEnum.Prop)
            : base(propKind, typeOfThisValue, typeIsSolid, hasStore)
        {

            //DoWHenChangedAction = doWhenChanged;
            DoAfterNotify = doAfterNotify;
            Comparer = comparer;
            PropertyChangedWithTVals = doWhenChangedX;
            GetDefaultValFunc = getDefaultValFunc;
        }

        #endregion

        #region Public Methods

        public void DoWhenChanged(T oldVal, T newVal)
        {
            Action<T, T> doWhenAction = Interlocked.CompareExchange(ref _doWhenChangedAction, null, null);

            if (doWhenAction != null)
                doWhenAction(oldVal, newVal);
        }

        public bool CompareTo(T newValue)
        {
            if (!HasStore)
                throw new NotImplementedException();

            //// Added this behavior on 10/23/2017.
            //if (!ValueIsDefined) return false;

            return Comparer(newValue, TypedValue);
        }

        public bool Compare(T val1, T val2)
        {
            if (!ValueIsDefined) return false;

            return Comparer(val1, val2);
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

        // TODO: See if our value implements IDisposable, and if so, dispose it.
        public new void CleanUpTyped()
        {
            Comparer = null;
            //DoWHenChangedAction = null;
            PropertyChangedWithTVals = null;
            base.CleanUpTyped();
        }

        #endregion

        #region Typed PropertyChangedWithVals support

        //public void SubscribeToPropChanged(Action<T, T> doOnChange)
        //{
        //    EventHandler<PCTypedEventArgs<T>> eventHandler = (s, e) => { doOnChange(e.OldValue, e.NewValue); };

        //    if (GetHandlerFromAction(doOnChange, ref _actTable) == null)
        //    {
        //        PropertyChangedWithTVals += eventHandler;
        //        if (_actTable == null)
        //        {
        //            _actTable = new List<Tuple<Action<T, T>, EventHandler<PCTypedEventArgs<T>>  >>();
        //        }
        //        _actTable.Add(new Tuple<Action<T, T>, EventHandler<PCTypedEventArgs<T>> >(doOnChange, eventHandler));
        //    }
        //}

        //public bool UnSubscribeToPropChanged(Action<T, T> doOnChange)
        //{
        //    Tuple<Action<T, T>, EventHandler<PCTypedEventArgs<T>>> actEntry = GetHandlerFromAction(doOnChange, ref _actTable);

        //    if (actEntry == null) return false;

        //    PropertyChangedWithTVals -= actEntry.Item2;
        //    _actTable.Remove(actEntry);

        //    return true;
        //}

        //private Tuple<Action<T, T>, EventHandler<PCTypedEventArgs<T>>> GetHandlerFromAction(Action<T, T> act,
        //    ref List< Tuple<  Action<T, T>, EventHandler<PCTypedEventArgs<T>> >> actTable)
        //{
        //    if (actTable == null) return null;

        //    for (int i = 0; i < actTable.Count; i++)
        //    {
        //        Tuple<Action<T, T>, EventHandler<PCTypedEventArgs<T>>> tup = actTable[i];
        //        if (tup.Item1 == act) return tup; //.Item2;
        //    }

        //    return null;
        //}

        //public void OnPropertyChangedWithTVals(string propertyName, T oldVal, T newVal)
        //{
        //    EventHandler<PCTypedEventArgs<T>> handler = Interlocked.CompareExchange(ref PropertyChangedWithTVals, null, null);

        //    if (handler != null)
        //        handler(this, new PCTypedEventArgs<T>(propertyName, oldVal, newVal));
        //}

        //public override void OnPropertyChangedWithVals(string propertyName, object oldVal, object newVal)
        //{
        //    EventHandler<PCGenEventArgs> handler = Interlocked.CompareExchange(ref PropertyChangedWithGenVals, null, null);

        //    if (handler != null)
        //        handler(this, new PCGenEventArgs(propertyName, this.Type, oldVal, newVal));
        //}

        #endregion

        #region Raise Events

        public void RaiseEventsForParent(IEnumerable<ISubscriptionGen> typedSubs, object parent,
            string propertyName, object oldVal, object newVal)
        {
            PCTypedEventArgs<T> eArgs = new PCTypedEventArgs<T>(propertyName, (T)oldVal, (T)newVal);

            foreach (Subscription<T> sspt in typedSubs)
            {
                sspt.TypedHandler(parent, eArgs);
            }
        }

        #endregion
    }
}
         