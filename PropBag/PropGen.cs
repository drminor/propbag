using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Reflection;

using System.Runtime.CompilerServices;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag
{
    public struct PropGen : IPropGen
    {
        public Type Type { get;}

        /// <summary>
        /// An instance of IProp<typeparam name="T"/>.
        /// Our callers could simply cast all instances that inherit from PropGenBase into a IProp<typeparamref name="T"/>
        /// When they need to access the instanace as a IProp<typeparamref name="T"/>, but this makes it more formal.
        /// </summary>

        public IProp TypedProp { get; }

        public bool TypeIsSolid { get { return ((IPropGen)TypedProp).TypeIsSolid; } }

        public bool HasStore { get; }

        // Constructor
        public PropGen(IPropGen typedPropWrapper)
        {
            TypedProp = typedPropWrapper.TypedProp;
            Type = typedPropWrapper.Type;
            HasStore = typedPropWrapper.HasStore;
            PropertyChangedWithVals = delegate { };
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
                //return DoGetProVal(TypedProp);
            }
        }

        public ValPlusType ValuePlusType()
        {
            return new ValPlusType(Value, Type);
        }

        public event PropertyChangedWithValsHandler PropertyChangedWithVals;

        private List<Tuple<Action<object, object>, PropertyChangedWithValsHandler>> _actTable;

        public void SubscribeToPropChanged(Action<object, object> doOnChange)
        {
            PropertyChangedWithValsHandler eventHandler = (s, e) => { doOnChange(e.OldValue, e.NewValue); };

            if (GetHandlerFromAction(doOnChange, ref _actTable) == null)
            {
                if(_actTable == null)
                    _actTable = new List<Tuple<Action<object, object>, PropertyChangedWithValsHandler>>();

                _actTable.Add(new Tuple<Action<object, object>, PropertyChangedWithValsHandler>(doOnChange, eventHandler));
            }
            PropertyChangedWithVals += eventHandler;
        }

        public bool UnSubscribeToPropChanged(Action<object, object> doOnChange)
        {
            PropertyChangedWithValsHandler eventHander = GetHandlerFromAction(doOnChange, ref _actTable);

            if (eventHander == null) return false;

            PropertyChangedWithVals -= eventHander;
            return true;
        }

        private PropertyChangedWithValsHandler GetHandlerFromAction(Action<object, object> act,
            ref List<Tuple<Action<object, object>, PropertyChangedWithValsHandler>> actTable)
        {
            if (actTable == null)
            {
                //actTable = new List<Tuple<Action<object, object>, PropertyChangedWithValsHandler>>();
                return null;
            }

            for (int i = 0; i < actTable.Count; i++)
            {
                Tuple<Action<object, object>, PropertyChangedWithValsHandler> tup = actTable[i];
                if (tup.Item1 == act) return tup.Item2;
            }

            return null;
        }

        public void OnPropertyChangedWithVals(string propertyName, object oldVal, object newVal)
        {
            PropertyChangedWithValsHandler handler = Interlocked.CompareExchange(ref PropertyChangedWithVals, null, null);

            if (handler != null)
                handler(this, new PropertyChangedWithValsEventArgs(propertyName, oldVal, newVal));
        }

        #endregion

        public void CleanUp()
        {
            if(TypedProp != null) TypedProp.CleanUpTyped();
            _actTable = null;
            PropertyChangedWithVals = null;
        }

    }
}
