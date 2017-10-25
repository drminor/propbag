using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace DRM.PropBag
{

    // ToDo: Make the PropFactory supply a default comparer that is suitable for collections.
    // and then remove this class, since the regular PropTypedBase<T> can be used.

    // This provides base support for classes that implement ICProp<CT,T> : IProp<T> where CT: IEnumerable<T>
    // The typeOfThisValue paramarter should be the collection type, for example IList<T>
    public abstract class PropTypedBaseForC<CT> : PropGenBase, IPropPrivate<CT>
    {
        protected PropTypedBaseForC(Type typeOfThisValue, bool typeIsSolid, bool hasStore,
            Action<CT, CT> doWhenChanged, bool doAfterNotify,
            Func<CT, CT, bool> comparer, GetDefaultValueDelegate<CT> getDefaultValFunc)
            : base(typeOfThisValue, typeIsSolid, hasStore)
        {
            DoWHenChangedAction = doWhenChanged;
            DoAfterNotify = doAfterNotify;
            Comparer = comparer;
            PropertyChangedWithTVals = null;
            base.TypedProp = this;
            GetDefaultValFunc = getDefaultValFunc;
            PropKind = PropKindEnum.Prop;
        }

        public PropKindEnum PropKind { get; private set; }

        abstract public IListSource ListSource { get; }

        abstract public CT TypedValue { get; set; }

        abstract public bool ValueIsDefined { get; }

        abstract public bool SetValueToUndefined();

        protected Func<CT, CT, bool> Comparer { get; set; }

        private Action<CT, CT> _doWhenChangedAction;
        protected Action<CT, CT> DoWHenChangedAction {
            get { return _doWhenChangedAction;  }
            set { _doWhenChangedAction = value; }
        }

        public bool DoAfterNotify { get; set; }

        public void DoWhenChanged(CT oldVal, CT newVal)
        {
            Action<CT,CT> doWhenAction = Interlocked.CompareExchange(ref _doWhenChangedAction, null, null);

            if (doWhenAction != null)
                doWhenAction(oldVal, newVal);
        }

        public event PropertyChangedWithTValsHandler<CT> PropertyChangedWithTVals;

        protected GetDefaultValueDelegate<CT> GetDefaultValFunc { get; }

        public virtual bool ReturnDefaultForUndefined
        {
            get
            {
                return GetDefaultValFunc != null;
            }
        }

        private List<Tuple<Action<CT, CT>, PropertyChangedWithTValsHandler<CT>>> _actTable = null;

        public void SubscribeToPropChanged(Action<CT, CT> doOnChange)
        {
            PropertyChangedWithTValsHandler<CT> eventHandler = (s, e) => { doOnChange(e.OldValue, e.NewValue); };

            if (GetHandlerFromAction(doOnChange, ref _actTable) == null)
            {
                PropertyChangedWithTVals += eventHandler;
                if(_actTable == null)
                {
                    _actTable = new List<Tuple<Action<CT, CT>, PropertyChangedWithTValsHandler<CT>>>();
                }
                _actTable.Add(new Tuple<Action<CT, CT>, PropertyChangedWithTValsHandler<CT>>(doOnChange, eventHandler));
            }
        }

        public bool UnSubscribeToPropChanged(Action<CT, CT> doOnChange)
        {
            Tuple<Action<CT, CT>, PropertyChangedWithTValsHandler<CT>> actEntry = GetHandlerFromAction(doOnChange, ref _actTable);

            if (actEntry == null) return false;

            PropertyChangedWithTVals -= actEntry.Item2;
            _actTable.Remove(actEntry);

            return true;
        }

        private Tuple<Action<CT, CT>, PropertyChangedWithTValsHandler<CT>> GetHandlerFromAction(Action<CT, CT> act,
            ref List<Tuple<Action<CT, CT>, PropertyChangedWithTValsHandler<CT>>> actTable)
        {
            if (actTable == null) return null;

            for (int i = 0; i < actTable.Count; i++)
            {
                Tuple<Action<CT, CT>, PropertyChangedWithTValsHandler<CT>> tup = actTable[i];
                if (tup.Item1 == act) return tup; //.Item2;
            }

            return null;
        }

        public void OnPropertyChangedWithTVals(string propertyName, CT oldVal, CT newVal)
        {
            PropertyChangedWithTValsHandler<CT> handler = Interlocked.CompareExchange(ref PropertyChangedWithTVals, null, null);

            if (handler != null)
                handler(this, new PropertyChangedWithTValsEventArgs<CT>(propertyName, oldVal, newVal));
        }

        // TODO: Figure how to compare two ObservableCollections<T>
        public bool CompareTo(CT newValue)
        {
            if (!HasStore)
                throw new NotImplementedException();

            //return Comparer(newValue, TypedValue);
            return object.ReferenceEquals(newValue, TypedValue);
        }

        // TODO: Figure how to compare two ObservableCollections<T>
        public bool Compare(CT val1, CT val2)
        {
            // Added this guard on 10/23/2017.
            if (!HasStore)
                throw new NotImplementedException();

            //if (!ValueIsDefined) return false;

            //return Comparer(val1, val2);
            return object.ReferenceEquals(val1, val2);
        }

        public bool UpdateDoWhenChangedAction(Action<CT, CT> doWhenChangedAction, bool? doAfterNotify)
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
         