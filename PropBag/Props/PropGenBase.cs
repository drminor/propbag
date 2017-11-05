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
    /// <summary>
    /// A wrapper for an instance of IProp<typeparam name="T"/>.
    /// </summary>
    public abstract class PropGenBase : IPropGen
    {
        #region Private Members

        Attribute[] _attributes;

        List<Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>>> _actTableGen = null;

        #endregion

        #region Public Members

        public abstract event EventHandler<PropertyChangedWithValsEventArgs>  PropertyChangedWithVals;

        public Type Type { get; private set;}

        public IProp TypedProp { get; set; }

        public bool IsEmpty => TypedProp == null;

        public bool TypeIsSolid { get; set; }

        public bool HasStore { get; private set; }


        public virtual Attribute[] Attributes
        {
            get
            {
                return _attributes;
            }
        }

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

        #endregion

        #region Constructors

        public PropGenBase(Type typeOfThisValue, bool typeIsSolid, bool hasStore = true)
        {
            Type = typeOfThisValue;
            TypedProp = null;
            TypeIsSolid = typeIsSolid;
            HasStore = hasStore;
            //PropertyChangedWithVals = null; // = delegate { };
            _actTableGen = null;
            _attributes = new Attribute[] { };
        }

        public PropGenBase(IPropGen typedPropWrapper)
        {
            Type = typedPropWrapper.TypedProp.Type;
            TypedProp = typedPropWrapper.TypedProp;
            TypeIsSolid = typedPropWrapper.TypeIsSolid;
            HasStore = typedPropWrapper.HasStore;
            //PropertyChangedWithVals = null; // = delegate { };
            _actTableGen = null;
            _attributes = new Attribute[] { };
        }

        #endregion

        #region Public Methods

        public ValPlusType ValuePlusType()
        {
            return new ValPlusType(Value, Type);
        }

        public void CleanUp()
        {
            if (TypedProp != null) TypedProp.CleanUpTyped();
            _actTableGen = null;
            //PropertyChangedWithVals = null;
        }

        #endregion

        #region Generic PropertyChangedWithVals support

        public void SubscribeToPropChanged(Action<object, object> doOnChange)
        {
            EventHandler<PropertyChangedWithValsEventArgs> eventHandler = (s, e) => { doOnChange(e.OldValue, e.NewValue); };

            if (GetHandlerFromAction(doOnChange, ref _actTableGen) == null)
            {
                PropertyChangedWithVals += eventHandler;
                if (_actTableGen == null)
                {
                    _actTableGen = new List<Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>>>();
                }
                _actTableGen.Add(new Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>>(doOnChange, eventHandler));
            }
        }

        public bool UnSubscribeToPropChanged(Action<object, object> doOnChange)
        {
            Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>> actEntry = GetHandlerFromAction(doOnChange, ref _actTableGen);

            if (actEntry == null) return false;

            PropertyChangedWithVals -= actEntry.Item2;
            _actTableGen.Remove(actEntry);
            return true;
        }

        private Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>> GetHandlerFromAction(Action<object, object> act,
            ref List<Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>>> actTable)
        {
            if (actTable == null)
            {
                return null;
            }

            for (int i = 0; i < actTable.Count; i++)
            {
                Tuple<Action<object, object>, EventHandler<PropertyChangedWithValsEventArgs>> tup = actTable[i];
                if (tup.Item1 == act) return tup;
            }

            return null;
        }


        #endregion

        #region Raise Events

        public abstract void OnPropertyChangedWithVals(string propertyName, object oldVal, object newVal);


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
