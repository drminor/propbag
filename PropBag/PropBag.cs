using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using DRM.ReferenceEquality;

using DRM.Ipnwv;

namespace DRM.PropBag
{
    #region Summary and Remarks

    /// <summary>
    /// The contents of this code file were designed and created by David R. Minor, Pittsboro, NC.
    /// I have chosen to provide others free access to this intellectual property using the terms set forth
    /// by the well known Code Project Open License.
    /// Please refer to the file in this same folder named CPOP.htm for the exact set of terms that govern this release.
    /// Although not included as a condition of use, I would prefer that this text, 
    /// or a similar text which covers all of the points, be included along with a copy of cpol.htm
    /// in the set of artifacts deployed with any product
    /// wherein this source code, or a derivative thereof, is used to build the product.
    /// </summary>

    /// <remarks>
    /// While writing this code, I learned much and was guided by the material found at the following locations.
    /// http://northhorizon.net/2011/the-right-way-to-do-inotifypropertychanged/ (Daniel Moore)
    /// https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/ (Jon Skeet)
    /// </remarks>

    #endregion

    public class PropBag : INotifyPropertyChanged, INotifyPropertyChanging, INotifyPropertyChangedWithVals
    {
        // TO-DOC: Explain since we may subscribe to our own events we would like to have these initialized.
        // confirm that on post construction events are initialized for us if we don't
        // alternative could be that they are initialized on first assignment.
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public event PropertyChangingEventHandler PropertyChanging = delegate { };
        public event PropertyChangedWithValsHandler PropertyChangedWithVals = delegate { };

        private readonly Dictionary<string, ValueWithType> tVals;

        /// <summary>
        /// If true, attempting to set a property for which no call to AddProp has been made, will cause an exception to thrown.
        /// </summary>
        public readonly bool AllPropsMustBeRegistered;

        /// <summary>
        /// If not true, attempting to set a property, not previously set with a call to AddProp or SetIt<typeparamref name="T"/>, will cause an exception to be thrown.
        /// </summary>
        public readonly bool AllowSetsViaThisForNewProps;

        /// <summary>
        /// Used to create Delegates when the type of the value is not known at run time.
        /// </summary>
        static Type GMT_TYPE = typeof(GenericMethodTemplates);

        #region Constructor

        public PropBag() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered) { }

        public PropBag(PropBagTypeSafetyMode typeSafetyMode)
        {
            switch (typeSafetyMode)
            {
                case PropBagTypeSafetyMode.AllPropsMustBeRegistered:
                    {
                        AllPropsMustBeRegistered = true;
                        AllowSetsViaThisForNewProps = false;
                        break;
                    }
                case PropBagTypeSafetyMode.AllPropsMustBeFirstSetWithSetIt:
                    {
                        AllPropsMustBeRegistered = false;
                        AllowSetsViaThisForNewProps = false;
                        break;
                    }
                case PropBagTypeSafetyMode.Loose:
                    {
                        AllPropsMustBeRegistered = false;
                        AllowSetsViaThisForNewProps = true;
                        break;
                    }
                default:
                    throw new ApplicationException("Unexpected value for typeSafetyMode parameter.");
            }

            tVals = new Dictionary<string, ValueWithType>();
        }

        // When we are being destructed, remove all of the handlers that we provisioned.
        ~PropBag()
        {
            foreach (KeyValuePair<string, ValueWithType> kvp in tVals)
            {
                foreach (PropertyChangedWithValsHandler h in kvp.Value.PropChangedWithValsHandlerList)
                {
                    this.PropertyChangedWithVals -= h;
                }
            }
        }

        public void RemoveProp(string propertyName)
        {
            ValueWithType vwt = GetValueWithType(propertyName);

            foreach (PropertyChangedWithValsHandler h in vwt.PropChangedWithValsHandlerList)
            {
                this.PropertyChangedWithVals -= h;
            }

            tVals.Remove(propertyName);
        }

        #endregion

        #region Public Methods and Properties

        public object this[string propertyName]
        {
            get
            {
                return GetIt(propertyName);
            }
            set
            {
                SetIt(value, propertyName);
            }
        }

        public object GetIt([CallerMemberName] string propertyName = null)
        {
            // This will throw an exception if no value has been added to the _tVals dictionary with a key of propertyName,
            // either by calling AddProp or SetIt.
            ValueWithType vwt = GetValueWithType(propertyName);

            // This uses reflection.
            return vwt.Value;
        }

        public T GetIt<T>([CallerMemberName] string propertyName = null)
        {
            // This will throw an exception if no value has been added to the _tVals dictionary with a key of propertyName,
            // either by calling AddProp or SetIt.
            ValueWithType vwt = GetValueWithType(propertyName);

            IProp<T> prop = (IProp<T>)(vwt.Prop);
            return prop.Value;
        }

        public void SetIt(object value, [CallerMemberName] string propertyName = null)
        {
            ValueWithType vwt;
            try
            {
                vwt = GetValueWithType(propertyName);
            }
            catch (KeyNotFoundException)
            {
                if (AllPropsMustBeRegistered)
                    throw new KeyNotFoundException(string.Format("Property: {0} has not been declared by calling AddProp, nor has its value been set by calling SetIt<T>. Cannot use this method in this case. Declare by calling AddProp, or use the SetIt<T> method.", propertyName));

                if (!AllowSetsViaThisForNewProps)
                {
                    throw new ApplicationException(string.Format("Property: {0} has not been defined with a call to AddProp or any SetIt<T> call and the operation setting 'AllowSetsViaThisForNewProps' is set to false.", propertyName));
                }

                // This uses reflection.
                vwt = ValueWithType.CreateValueInferType(value);
                this.tVals.Add(propertyName, vwt);

                // No point in calling DoSet, it would find that the value is the same and do nothing.
                return;
            }

            if (value != null)
            {
                Type newType = value.GetType();
                if (!vwt.TypeIsSolid) //  && newType != typeof(object) && (null != Nullable.GetUnderlyingType(newType)))
                {
                    MakeTypeSolid(propertyName, vwt, value, newType);
                }
                else
                {
                    if (!AreTypesSame(newType, vwt.Type))
                    {
                        throw new ApplicationException(string.Format("Attempting to set property: {0} whose type is {1}, with a call whose type parameter is {2} is invalid.", propertyName, vwt.Type.ToString(), newType.ToString()));
                    }
                }
            }
            else
            {
                if (vwt.TypeIsSolid && vwt.Type.IsValueType)
                {
                    throw new InvalidOperationException(string.Format("Cannot set property: {0} to null, it is a value type.", propertyName));
                }
            }

            // This uses reflection.
            DoSetDelegate setProDel = GetPropSetterDelegate(vwt);
            setProDel(value, this, propertyName, vwt.Prop);
        }


        private DoSetDelegate GetPropSetterDelegate(ValueWithType vwt)
        {
            if (vwt.DoSetProVal == null)
            {
                vwt.DoSetProVal = GetDoSetDelegate(vwt.Type);
            }
            return vwt.DoSetProVal;
        }

        public void SetIt<T>(T value, [CallerMemberName] string propertyName = null)
        {
            ValueWithType vwt;

            try
            {
                vwt = GetValueWithType(propertyName);
            }
            catch (KeyNotFoundException)
            {
                if (AllPropsMustBeRegistered)
                {
                    throw new ApplicationException(string.Format("Property: {0} has not been defined with a call to AddProp() and the operation setting 'AllPropsMustBeRegistered' is set to true.", propertyName));
                }

                // Property has not been defined yet, let's create a definition for it now and initialize the value.
                tVals.Add(propertyName, ValueWithType.Create<T>(value));

                // No reason to call DoSet, it will find no change and do nothing.
                return;
            }

            Type newType = typeof(T);

            if (!vwt.TypeIsSolid && value != null) //  && newType != typeof(object) && (null != Nullable.GetUnderlyingType(newType)))
            {
                MakeTypeSolid(propertyName, vwt, value, newType);
            }
            else
            {
                if (!AreTypesSame(newType, vwt.Type))
                {
                    throw new ApplicationException(string.Format("Attempting to set property: {0} whose type is {1}, with a call whose type parameter is {2} is invalid.", propertyName, vwt.Type.ToString(), typeof(T).ToString()));
                }
            }

            DoSet(value, propertyName, (IProp<T>)vwt.Prop);
        }

        public void SubscribeToPropChanged<T>(Action<T, T> doOnChange, [CallerMemberName] string propertyName = null)
        {
            ValueWithType vwt = GetValueWithType(propertyName);

            // TODO: Figure out how to handle when the registered type is not solid.

            // Make sure that the Action type is the same as the type registerd for this property.
            if (!AreTypesSame(typeof(T), vwt.Type))
            {
                throw new ApplicationException(string.Format("Attempting to subscribe to PropChanged for property: {0} whose type is {1}, with a call whose type parameter is {2} is invalid.", propertyName, vwt.Type.ToString(), typeof(T).ToString()));
            }

            PropertyChangedWithValsHandler handler = (s, e) =>
            {
                if (string.Equals(e.PropertyName, propertyName, StringComparison.InvariantCulture))
                {
                    doOnChange((T)e.OldValue, (T)e.NewValue);
                }
            };

            this.PropertyChangedWithVals += handler;

            // References are kept here so that we can remove them when this instance is disposed.
            vwt.PropChangedWithValsHandlerList.Add(handler);
        }

        // This is protected, since public callers would know how to prepare the name of the property declaration from which this call is made.
        protected void ConnectToPropChanged<T>(Action<T, T> doOnChange, [CallerMemberName] string subToPropertyName = null)
        {
            string propertyName = GetPropNameFromSubTo(subToPropertyName);
            SubscribeToPropChanged<T>(doOnChange, propertyName);
        }

        /// <summary>
        /// Given a string in the form "SubscribeTo{0}Changed", where {0} is the underlying property name, parse out and return the value of {0}.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private string GetPropNameFromSubTo(string x)
        {
            //SubscribeToPropStringChanged
            return x.Substring(11, x.Length - 18);
        }

        public bool PropertyExists([CallerMemberName] string propertyName = null)
        {
            return tVals.ContainsKey(propertyName);
        }

        public System.Type GetTypeOfProperty([CallerMemberName] string propertyName = null)
        {
            return GetValueWithType(propertyName).Type;
        }

        public List<PropertyChangedWithValsHandler> GetPropChangedWithValsHandlers([CallerMemberName] string propertyName = null)
        {
            return GetValueWithType(propertyName).PropChangedWithValsHandlerList;
        }

        /// <summary>
        /// Returns all of the values in dictionary of objects, keyed by PropertyName.
        /// </summary>
        public IDictionary<string, object> GetAll
        {
            // This uses reflection.
            get
            {
                Dictionary<string, object> result = new Dictionary<string, object>();

                foreach (KeyValuePair<string, ValueWithType> kvp in tVals)
                {
                    result.Add(kvp.Key, kvp.Value.Value);
                }
                return result;
            }
        }

        #endregion

        #region Private Methods and Properties

        private void DoSet<T>(T value, string propertyName, IProp<T> prop)
        {
            bool theSame = prop.CompareTo(value);

            if (!theSame)
            {
                RaisePropertyChangingEvent(propertyName);

                // Save the value before the update.
                T oldVal = prop.Value;

                // Make the update.
                prop.Value = value;

                if (prop.HasCallBack) 
                {
                    if (prop.DoAfterNotify)
                    {
                        //Raise the event -- Then the call back.
                        RaisePropertyChangedEvent(propertyName);
                        RaisePropertyChangedWithValsEvent(propertyName, oldVal, value);

                        prop.DoWHenChanged(oldVal, value);
                    }
                    else
                    {
                        // Run the call back -- then raise the event.
                        prop.DoWHenChanged(oldVal, value);

                        RaisePropertyChangedEvent(propertyName);
                        RaisePropertyChangedWithValsEvent(propertyName, oldVal, value);
                    }
                }
                else
                {
                    //Raise the event
                    RaisePropertyChangedEvent(propertyName);
                    RaisePropertyChangedWithValsEvent(propertyName, oldVal, value);
                }
            }
        }


        private ValueWithType GetValueWithType(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName", "PropertyName is null on call to GetValue.");
            return tVals[propertyName];
        }

        /// <summary>
        /// Updates the ValueWithType instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        private void MakeTypeSolid(string propertyName, ValueWithType vwt, object newValue, Type newType)
        {
            Type currentType = vwt.Type;

            // This is the value from the current prop, using the current type.
            object value = vwt.Value;
            if (value != null) throw new ApplicationException(string.Format("The current value of property: {0} should be null when MakeTypeSolid is called.", propertyName));


            System.Type underlyingtype = Nullable.GetUnderlyingType(newType);

            // Check to see if the new type is a non-nullable, value type.
            // If it is, then it was invalid to set this property to null in the first place.
            // Consider setting value to the type's default value instead of throwing this exception.
            if (underlyingtype == null && newType.IsValueType)
                throw new ApplicationException(string.Format("The property: {0} was originally set to null, now its being set to a value whose type is a value type; value types don't allow setting to null.", propertyName));

            if (newType != currentType)
            {
                vwt.UpdateWithSolidType(newValue, value);
            }
        }

        // Its important to make sure new is new and cur is cur.
        private bool AreTypesSame(Type newType, Type curType)
        {
            Type aUnder = Nullable.GetUnderlyingType(newType);

            if (aUnder != null)
            {
                // Compare the underlying type of this new value and the target property's underlying type.
                Type bUnder = Nullable.GetUnderlyingType(curType);

                return aUnder == bUnder;
            }
            else
            {
                if (newType.IsGenericType)
                {
                    return curType.IsAssignableFrom(newType);
                }
                else
                {
                    return newType == curType;
                }
            }
        }

        #endregion

        #region Add Prop Methods

        /// <summary>
        /// Use when you want to specify an Action<typeparamref name="T", typeparamref name="T"> to be performed
        /// either before or after the PropertyChanged event has been raised.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="doIfChanged"></param>
        /// <param name="doAfterNotify"></param>
        /// <param name="comparer">A instance of a class that implements IEqualityComparer and thus an Equals method.</param>
        /// <param name="initalValue"></param>
        public void AddProp<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false,
            IEqualityComparer<T> comparer = null, T initalValue = default(T))
        {
            tVals.Add(propertyName, ValueWithType.Create<T>(initalValue, doIfChanged, doAfterNotify, comparer));
        }

        public void AddPropObjComp<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false,
            IEqualityComparer<object> comparer = null, T initalValue = default(T))
        {
            tVals.Add(propertyName, ValueWithType.CreateWithObjComparer(initalValue, doIfChanged, doAfterNotify, comparer));
        }

        public void AddProp<T>(string propertyName, T initalValue = default(T))
        {
            tVals.Add(propertyName, ValueWithType.Create<T>(initalValue));
        }

        public void AddProp<T>(string propertyName, IEqualityComparer<T> comparer, T initalValue = default(T))
        {
            tVals.Add(propertyName, ValueWithType.Create<T>(initalValue, null, false, comparer));
        }

        public void AddPropObjComp<T>(string propertyName, IEqualityComparer<object> comparer, T initalValue = default(T))
        {
            tVals.Add(propertyName, ValueWithType.CreateWithObjComparer<T>(initalValue, null, false, comparer));
        }

        #endregion

        #region Set and Raise Event

        /// <summary>
        /// For use when the Property Bag's internal storage is not appropriate. This allows
        /// the property implementor to use a backing store of their choice.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newValue"></param>
        /// <param name="oldValue"></param>
        /// <param name="propertyName"></param>
        /// <returns>True if the value was updated, otherwise false.</returns>
        protected bool SetAndRaise<T>(T newValue, ref T oldValue, [System.Runtime.CompilerServices.CallerMemberName]string propertyName = null)
        {
            // TODO: Need to provide way for caller to specify comparer<T>
            // or change to always call, and make it the responsibility of the caller to only call if changed.

            // TO-DOC: Need to document how these values are compared.
            bool theSame = EqualityComparer<T>.Default.Equals(newValue, oldValue);

            if (!theSame)
            {
                // Save the old value. 
                T oldValueSaved = oldValue;

                // Let callers know we are about to make the change. This gives them a chance to grab the current value. 
                RaisePropertyChangingEvent(propertyName);

                // Make the update.
                oldValue = newValue;

                //Raise the event 
                RaisePropertyChangedEvent(propertyName);
                RaisePropertyChangedWithValsEvent(propertyName, oldValueSaved, newValue);
            }

            //Signal what happened. 
            return !theSame;
        }

        // Raise Standard Events
        protected void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChanged, null, null);

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChangingEvent([System.Runtime.CompilerServices.CallerMemberName]string propertyName = null)
        {
            PropertyChangingEventHandler handler = Interlocked.CompareExchange(ref PropertyChanging, null, null);

            if (handler != null)
                handler(this, new PropertyChangingEventArgs(propertyName));
        }

        // Raise Type Events
        protected void RaisePropertyChangedWithValsEvent(string propertyName, object oldVal, object newVal)
        {
            PropertyChangedWithValsHandler handler = Interlocked.CompareExchange(ref PropertyChangedWithVals, null, null);

            if (handler != null)
                handler(this, new PropertyChangedWithValsEventArgs(propertyName, oldVal, newVal));
        }

        #endregion

        #region Generic Method Support

        private delegate void DoSetDelegate(object value, PropBag target, string propertyName, object prop);

        private DoSetDelegate GetDoSetDelegate(Type typeOfThisValue)
        {
            MethodInfo methInfoSetProp = GetDoSetMethInfo(typeOfThisValue);
            DoSetDelegate result = (DoSetDelegate)Delegate.CreateDelegate(typeof(DoSetDelegate), methInfoSetProp);

            return result;
        }

        private MethodInfo GetDoSetMethInfo(Type typeOfThisValue)
        {
            return GMT_TYPE.GetMethod("DoSetBridge", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
        }

        #endregion

        private class ValueWithType
        {
            #region Member Declarations

            static private Type GMT_TYPE = typeof(GenericMethodTemplates);

            public Type Type { get; private set; }
            public object Prop { get; set; }
            public bool TypeIsSolid { get; set; }

            public List<PropertyChangedWithValsHandler> PropChangedWithValsHandlerList { get; private set; }

            private GetPropValDelegate _doGet;
            private GetPropValDelegate DoGetProVal
            {
                get
                {
                    if (_doGet == null)
                    {
                        _doGet = GetPropGetter(Type);
                    }
                    return _doGet;
                }

                set
                {
                    _doGet = value;
                }
            }

            public DoSetDelegate DoSetProVal { get; set; }

            #endregion

            // Constructor
            public ValueWithType(Type typeOfThisValue, object prop, bool typeIsSolid)
            {
                Type = typeOfThisValue;
                Prop = prop;
                TypeIsSolid = typeIsSolid;
                PropChangedWithValsHandlerList = new List<PropertyChangedWithValsHandler>();
                DoGetProVal = null;
                DoSetProVal = null;
            }

            #region Factory Methods

            static public ValueWithType Create<T>(T value, Action<T,T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
            {
                // Use the implementation which takes IEqualityComparer<T>
                Prop<T> prop = new Prop<T>(value, doWhenChanged, doAfterNotify, comparer);
                return new ValueWithType(typeof(T), prop, typeIsSolid: true);
            }

            static public ValueWithType CreateWithObjComparer<T>(T value, Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<object> comparer = null)
            {
                // Use the Implementation which takes IEqualityComparer<object>
                PropObjComp<T> prop = new PropObjComp<T>(value, doWhenChanged, doAfterNotify);
                return new ValueWithType(typeof(T), prop, typeIsSolid: true);
            }

            static public ValueWithType CreateValueInferType(object value)
            {
                System.Type typeOfThisValue;
                bool typeIsSolid;

                if (value == null)
                {
                    typeOfThisValue = typeof(object);
                    typeIsSolid = false;
                }
                else
                {
                    typeOfThisValue = value.GetType();
                    typeIsSolid = true;
                }

                CreateVWTDelegate createValWithType = GetVWTCreator(typeOfThisValue);
                ValueWithType vwt = createValWithType(value);
                vwt.TypeIsSolid = typeIsSolid;

                return vwt;
            }

            #endregion

            #region Public Methods and Properties

            /// <summary>
            /// Uses Reflection
            /// </summary>
            public object Value
            {
                get
                {
                    return DoGetProVal(Prop);
                }
            }

            public void UpdateWithSolidType(object newValue, object curValue)
            {
                Debug.Assert(newValue != null, "NewValue is null on call to UpdateWithSolidType.");
                Debug.Assert(curValue == null, "CurValue is null on call to UpdateWithSolidType.");

                System.Type typeOfThisValue = newValue.GetType();

                // Create a brand new ValueWithType instance -- we could have created a custom Delegate for this -- but this strategy reuses more code.
                CreateVWTDelegate vwtCreator = GetVWTCreator(typeOfThisValue);
                ValueWithType newVwt = vwtCreator(curValue);

                this.Prop = newVwt.Prop;
                this.Type = typeOfThisValue;
                this.TypeIsSolid = true;
                this.DoGetProVal = null;
                this.DoSetProVal = null;
            }

            #endregion

            #region Delegate declarations

            // Delegate declarations.

            private delegate object GetPropValDelegate(object prop);

            private delegate ValueWithType CreateVWTDelegate(object value);

            #endregion

            #region Helper Methods for the Generic Method Templates

            private static GetPropValDelegate GetPropGetter(Type typeOfThisValue)
            {
                MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetPropValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
                GetPropValDelegate result = (GetPropValDelegate)Delegate.CreateDelegate(typeof(GetPropValDelegate), methInfoGetProp);

                return result;
            }

            private static CreateVWTDelegate GetVWTCreator(Type typeOfThisValue)
            {
                MethodInfo methInfoVWTCreator = GMT_TYPE.GetMethod("CreateVWT", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
                CreateVWTDelegate result = (CreateVWTDelegate)Delegate.CreateDelegate(typeof(CreateVWTDelegate), methInfoVWTCreator);

                return result;
            }

            #endregion

            #region Generic Method Templates

            static class GenericMethodTemplates
            {
                private static object GetPropValue<T>(object prop)
                {
                    return ((IProp<T>)prop).Value;
                }

                private static ValueWithType CreateVWT<T>(object value)
                {
                    return ValueWithType.Create<T>((T)value);
                }
            }

            #endregion
        }

        #region Property Bag Generic Method Templates

        // Method Templates for Property Bag
        static class GenericMethodTemplates
        {
            private static void DoSetBridge<T>(object value, PropBag target, string propertyName, object prop)
            {
                target.DoSet<T>((T)value, propertyName, (IProp<T>)prop);
            }
        }

        #endregion
    }
}
