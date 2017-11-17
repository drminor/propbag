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
    public abstract class PropBase : IProp
    {
        #region Public Members

        public event EventHandler<PCGenEventArgs>  PropertyChangedWithGenVals;
        public event EventHandler<PCObjectEventArgs> PropertyChangedWithObjectVals;

        public PropKindEnum PropKind { get; private set; }
        public Type Type { get; private set; }
        public bool TypeIsSolid { get; set; }
        public bool HasStore { get; private set; }

        Attribute[] _attributes;
        public virtual Attribute[] Attributes => _attributes;

        public abstract bool ValueIsDefined { get; }
        public abstract object TypedValueAsObject { get; }
        public abstract IListSource ListSource { get; }

        #endregion

        #region Constructors

        public PropBase(PropKindEnum propKind, Type typeOfThisValue, bool typeIsSolid, bool hasStore = true)
        {
            PropKind = propKind;
            Type = typeOfThisValue;
            TypeIsSolid = typeIsSolid;
            HasStore = hasStore;
            _attributes = new Attribute[] { };
            PropertyChangedWithObjectVals = null;
            PropertyChangedWithObjectVals = null;
        }

        #endregion

        #region Public Methods 

        public abstract ValPlusType GetValuePlusType();

        public abstract bool SetValueToUndefined();

        public void CleanUpTyped()
        {
            //PropertyChangedWithGenVals = null;
            PropertyChangedWithObjectVals = null;
        }

        #endregion

        #region Object PropertyChangedWithVals support

        //List<Tuple<Action<object, object>, EventHandler<PCGenEventArgs>>> _actTableGen = null;

        //public event PropertyChangedEventHandler PropertyChanged;

        //private List<Tuple<Action<object, object>, EventHandler<PCGenEventArgs>>> _actTable;

        //public void SubscribeToPropChanged(Action<object, object> doOnChange)
        //{
        //    EventHandler<PCGenEventArgs> eventHandler = (s, e) => { doOnChange(e.OldValue, e.NewValue); };

        //    if (GetHandlerFromAction(doOnChange, ref _actTable) == null)
        //    {
        //        PropertyChangedWithGenVals += eventHandler;
        //        if (_actTable == null)
        //        {
        //            _actTable = new List<Tuple<Action<object, object>, EventHandler<PCGenEventArgs>>>();
        //        }
        //        _actTable.Add(new Tuple<Action<object, object>, EventHandler<PCGenEventArgs>>(doOnChange, eventHandler));
        //    }
        //}

        //public bool UnSubscribeToPropChanged(Action<object, object> doOnChange)
        //{
        //    Tuple<Action<object, object>, EventHandler<PCGenEventArgs>> actEntry = GetHandlerFromAction(doOnChange, ref _actTable);

        //    if (actEntry == null) return false;

        //    PropertyChangedWithGenVals -= actEntry.Item2;
        //    _actTable.Remove(actEntry);
        //    return true;
        //}

        //private Tuple<Action<object, object>, EventHandler<PCGenEventArgs>> GetHandlerFromAction(Action<object, object> act,
        //    ref List<Tuple<Action<object, object>, EventHandler<PCGenEventArgs>>> actTable)
        //{
        //    if (actTable == null) return null;

        //    for (int i = 0; i < actTable.Count; i++)
        //    {
        //        Tuple<Action<object, object>, EventHandler<PCGenEventArgs>> tup = actTable[i];
        //        if (tup.Item1 == act) return tup;
        //    }

        //    return null;
        //}

        public void OnPropertyChangedWithGenVals(string propertyName, object oldVal, object newVal)
        {
            EventHandler<PCGenEventArgs> handler = Interlocked.CompareExchange(ref PropertyChangedWithGenVals, null, null);

            if (handler != null)
            {
                Type propertyType = this.Type;
                handler(this, new PCGenEventArgs(propertyName, propertyType, oldVal, newVal));
            }
        }

        public void OnPropertyChangedWithObjectVals(string propertyName, object oldVal, object newVal)
        {
            EventHandler<PCObjectEventArgs> handler = Interlocked.CompareExchange(ref PropertyChangedWithObjectVals, null, null);

            if (handler != null)
            {
                Type propertyType = this.Type;
                handler(this, new PCObjectEventArgs(propertyName, oldVal, newVal));
            }
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
