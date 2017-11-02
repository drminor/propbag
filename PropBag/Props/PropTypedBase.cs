﻿using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;


namespace DRM.PropBag
{
    public abstract class PropTypedBase<T> : PropGenBase, IPropPrivate<T>
    {
        protected PropTypedBase(Type typeOfThisValue, bool typeIsSolid, bool hasStore,
            Action<T, T> doWhenChanged, bool doAfterNotify,
            Func<T,T,bool> comparer, GetDefaultValueDelegate<T> getDefaultValFunc, PropKindEnum propType = PropKindEnum.Prop)
            : base(typeOfThisValue, typeIsSolid, hasStore)
        {
            DoWHenChangedAction = doWhenChanged;
            DoAfterNotify = doAfterNotify;
            Comparer = comparer;
            PropertyChangedWithTVals = null;
            base.TypedProp = this;
            GetDefaultValFunc = getDefaultValFunc;
            PropKind = propType;
        }

        public PropKindEnum PropKind
        {
            get;
            protected set;
        }

        abstract public IListSource ListSource { get; }

        abstract public T TypedValue { get; set; }

        abstract public bool ValueIsDefined { get; }

        abstract public bool SetValueToUndefined();

        protected Func<T,T,bool> Comparer { get; set; }

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

        protected GetDefaultValueDelegate<T> GetDefaultValFunc { get; }

        public virtual bool ReturnDefaultForUndefined
        {
            get
            {
                return GetDefaultValFunc != null;
            }
        }

        private List<Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>>> _actTable = null;

        public void SubscribeToPropChanged(Action<T, T> doOnChange)
        {
            PropertyChangedWithTValsHandler<T> eventHandler = (s, e) => { doOnChange(e.OldValue, e.NewValue); };

            if (GetHandlerFromAction(doOnChange, ref _actTable) == null)
            {
                PropertyChangedWithTVals += eventHandler;
                if(_actTable == null)
                {
                    _actTable = new List<Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>>>();
                }
                _actTable.Add(new Tuple<Action<T,T>,PropertyChangedWithTValsHandler<T>>(doOnChange, eventHandler));
            }
        }

        public bool UnSubscribeToPropChanged(Action<T, T> doOnChange)
        {
            Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>> actEntry = GetHandlerFromAction(doOnChange, ref _actTable);

            if (actEntry == null) return false;

            PropertyChangedWithTVals -= actEntry.Item2;
            _actTable.Remove(actEntry);

            return true;
        }

        private Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>> GetHandlerFromAction(Action<T, T> act,
            ref List<Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>>> actTable)
        {
            if (actTable == null) return null;

            for (int i = 0; i < actTable.Count; i++)
            {
                Tuple<Action<T, T>, PropertyChangedWithTValsHandler<T>> tup = actTable[i];
                if (tup.Item1 == act) return tup; //.Item2;
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

        public object TypedValueAsObject { get { return (object) TypedValue; } }

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
         