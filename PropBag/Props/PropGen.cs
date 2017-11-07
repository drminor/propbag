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
        public ulong PropId { get; }

        public Type Type { get;}

        /// <summary>
        /// A wrapper for an instance of IProp<typeparam name="T"/>.
        /// </summary>

        public IProp TypedProp { get; }

        public bool IsEmpty => TypedProp == null;

        public bool TypeIsSolid { get { return ((IPropGen)TypedProp).TypeIsSolid; } }

        public bool HasStore { get; }

        // Constructor
        public PropGen(IPropGen typedPropWrapper, ulong? propId)
        {
            if(typedPropWrapper == null)
            {
                TypedProp = null;
                Type = null;
                HasStore = false;
                PropId = 0;
            }
            else
            {
                TypedProp = typedPropWrapper.TypedProp;
                Type = typedPropWrapper.TypedProp.Type;
                HasStore = typedPropWrapper.HasStore;
                PropId = propId.Value;
            }

            PropertyChangedWithGenVals = null; // = delegate { };
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

        public event EventHandler<PCGenEventArgs> PropertyChangedWithGenVals;
        //public event PropertyChangedEventHandler PropertyChanged;

        private List<Tuple<Action<object, object>, EventHandler<PCGenEventArgs>>> _actTable;

        public void SubscribeToPropChanged(Action<object, object> doOnChange)
        {
            EventHandler<PCGenEventArgs> eventHandler = (s, e) => { doOnChange(e.OldValue, e.NewValue); };

            if (GetHandlerFromAction(doOnChange, ref _actTable) == null)
            {
                PropertyChangedWithGenVals += eventHandler;
                if (_actTable == null)
                {
                    _actTable = new List<Tuple<Action<object, object>, EventHandler<PCGenEventArgs>>>();
                }
                _actTable.Add(new Tuple<Action<object, object>, EventHandler<PCGenEventArgs>>(doOnChange, eventHandler));
            }
        }

        public bool UnSubscribeToPropChanged(Action<object, object> doOnChange)
        {
            Tuple<Action<object, object>, EventHandler<PCGenEventArgs>> actEntry = GetHandlerFromAction(doOnChange, ref _actTable);

            if (actEntry == null) return false;

            PropertyChangedWithGenVals -= actEntry.Item2;
            _actTable.Remove(actEntry);
            return true;
        }

        private Tuple<Action<object, object>, EventHandler<PCGenEventArgs>> GetHandlerFromAction(Action<object, object> act,
            ref List<Tuple<Action<object, object>, EventHandler<PCGenEventArgs>>> actTable)
        {
            if (actTable == null) return null;

            for (int i = 0; i < actTable.Count; i++)
            {
                Tuple<Action<object, object>, EventHandler<PCGenEventArgs>> tup = actTable[i];
                if (tup.Item1 == act) return tup;
            }

            return null;
        }

        public void OnPropertyChangedWithVals(string propertyName, object oldVal, object newVal)
        {
            EventHandler<PCGenEventArgs> handler = Interlocked.CompareExchange(ref PropertyChangedWithGenVals, null, null);

            if (handler != null)
            {
                Type propertyType = this.Type;
                handler(this, new PCGenEventArgs(propertyName, propertyType, oldVal, newVal));
            }
        }

        #endregion

        public void CleanUp(bool doTypedCleanup)
        {
            if(doTypedCleanup && TypedProp != null) TypedProp.CleanUpTyped();
            _actTable = null;
            PropertyChangedWithGenVals = null;
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
