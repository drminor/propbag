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
    /// <summary>
    /// A wrapper for an instance of IProp<typeparam name="T"/>.
    /// </summary>
    public abstract class PropGenBase : IPropGen
    {
        public Type Type {get; set;}

        public IProp TypedProp { get; set; }

        public bool TypeIsSolid { get; set; }

        public bool HasStore { get; set; }

        // Constructor
        public PropGenBase(Type typeOfThisValue, bool typeIsSolid, bool hasStore = true)
        {
            Type = typeOfThisValue;
            TypedProp = null;
            TypeIsSolid = typeIsSolid;
            HasStore = hasStore;
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

        private List<Tuple<Action<object, object>, PropertyChangedWithValsHandler>> _actTable = null;

        public void SubscribeToPropChanged(Action<object, object> doOnChange)
        {
            PropertyChangedWithValsHandler eventHandler = (s, e) => { doOnChange(e.OldValue, e.NewValue); };

            if (GetHandlerFromAction(doOnChange, ref _actTable) == null)
            {
                PropertyChangedWithVals += eventHandler;
                if (_actTable == null)
                {
                    _actTable = new List<Tuple<Action<object, object>, PropertyChangedWithValsHandler>>();
                }
                _actTable.Add(new Tuple<Action<object, object>, PropertyChangedWithValsHandler>(doOnChange, eventHandler));
            }
        }

        public bool UnSubscribeToPropChanged(Action<object, object> doOnChange)
        {
            Tuple<Action<object, object>, PropertyChangedWithValsHandler> actEntry = GetHandlerFromAction(doOnChange, ref _actTable);

            if (actEntry == null) return false;

            PropertyChangedWithVals -= actEntry.Item2;
            _actTable.Remove(actEntry);
            return true;
        }

        private Tuple<Action<object, object>, PropertyChangedWithValsHandler> GetHandlerFromAction(Action<object, object> act,
            ref List<Tuple<Action<object, object>, PropertyChangedWithValsHandler>> actTable)
        {
            if (actTable == null)
            {
                return null;
            }

            for (int i = 0; i < actTable.Count; i++)
            {
                Tuple<Action<object, object>, PropertyChangedWithValsHandler> tup = actTable[i];
                if (tup.Item1 == act) return tup;
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

        //#region Helper Methods for the Generic Method Templates

        //// Delegate declarations.
        //private delegate object GetPropValDelegate(object prop);

        //private static GetPropValDelegate GetPropGetter(Type typeOfThisValue)
        //{
        //    MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetPropValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
        //    GetPropValDelegate result = (GetPropValDelegate)Delegate.CreateDelegate(typeof(GetPropValDelegate), methInfoGetProp);

        //    return result;
        //}

        //#endregion

        public void CleanUp()
        {
            if(TypedProp != null) TypedProp.CleanUpTyped();
            _actTable = null;
            PropertyChangedWithVals = null;
        }

        //#region Generic Method Templates

        //static class GenericMethodTemplates
        //{
        //    private static object GetPropValue<T>(object prop)
        //    {
        //        return ((IProp<T>)prop).TypedValue;
        //    }
        //}

        //#endregion

    }
}
