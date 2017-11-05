using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Reflection;

using System.Runtime.CompilerServices;
using DRM.TypeSafePropertyBag;
using System.ComponentModel;
using System.Windows;

namespace DRM.PropBag
{
    public struct PropGen : IPropGen
    {
        public Type Type { get;}

        /// <summary>
        /// A wrapper for an instance of IProp<typeparam name="T"/>.
        /// </summary>

        public IProp TypedProp { get; }

        public bool IsEmpty => TypedProp == null;

        public bool TypeIsSolid { get { return ((IPropGen)TypedProp).TypeIsSolid; } }

        public bool HasStore { get; }

        // Constructor
        public PropGen(IPropGen typedPropWrapper)
        {
            if(typedPropWrapper == null)
            {
                TypedProp = null;
                Type = null;
                HasStore = false;
            }
            else
            {
                TypedProp = typedPropWrapper.TypedProp;
                Type = typedPropWrapper.TypedProp.Type;
                HasStore = typedPropWrapper.HasStore;
            }

            PropertyChangedWithVals = null; // = delegate { };
            //PropertyChanged = null;
            _actTable = null;
        }

        #region Public Methods and Properties

        /// <summary>
        /// Does not use reflection.
        /// </summary>
        public object Value
        {
            get
            {
                return TypedProp.TypedValueAsObject;
            }
        }

        public ValPlusType ValuePlusType()
        {
            return new ValPlusType(Value, Type);
        }

        public event EventHandler<PropertyChangedWithValsEventArgs> PropertyChangedWithVals;
        //public event PropertyChangedEventHandler PropertyChanged;

        private List<Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>>> _actTable;

        public void SubscribeToPropChanged(Action<object, object> doOnChange)
        {
            EventHandler<PropertyChangedWithValsEventArgs> eventHandler = (s, e) => { doOnChange(e.OldValue, e.NewValue); };

            if (GetHandlerFromAction(doOnChange, ref _actTable) == null)
            {
                PropertyChangedWithVals += eventHandler;
                if (_actTable == null)
                {
                    _actTable = new List<Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>>>();
                }
                _actTable.Add(new Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>>(doOnChange, eventHandler));
            }
        }

        public bool UnSubscribeToPropChanged(Action<object, object> doOnChange)
        {
            Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>> actEntry = GetHandlerFromAction(doOnChange, ref _actTable);

            if (actEntry == null) return false;

            PropertyChangedWithVals -= actEntry.Item2;
            _actTable.Remove(actEntry);
            return true;
        }

        private Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>> GetHandlerFromAction(Action<object, object> act,
            ref List<Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>>> actTable)
        {
            if (actTable == null) return null;

            for (int i = 0; i < actTable.Count; i++)
            {
                Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>> tup = actTable[i];
                if (tup.Item1 == act) return tup;
            }

            return null;
        }

        public void OnPropertyChangedWithVals(string propertyName, object oldVal, object newVal)
        {
            EventHandler<PropertyChangedWithValsEventArgs> handler = Interlocked.CompareExchange(ref PropertyChangedWithVals, null, null);

            if (handler != null)
            {
                Type propertyType = this.Type;
                handler(this, new PropertyChangedWithValsEventArgs(propertyName, propertyType, oldVal, newVal));
            }
        }

        #endregion

        public void CleanUp()
        {
            if(TypedProp != null) TypedProp.CleanUpTyped();
            _actTable = null;
            PropertyChangedWithVals = null;
        }

        //public void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChanged, null, null);

        //    if (handler != null)
        //        handler(this, new PropertyChangedEventArgs(propertyName));
        //}

        //public void SubscribeToPropChanged(EventHandler<PropertyChangedEventArgs> handler)
        //{
        //    WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(this, "PropertyChanged", handler);
        //}
    }
}
