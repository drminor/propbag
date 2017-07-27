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
        #region Member Declarations

        // TO-DOC: Explain since we may subscribe to our own events we would like to have these initialized.
        // confirm that on post construction events are initialized for us if we don't
        // alternative could be that they are initialized on first assignment.
        public event PropertyChangedEventHandler PropertyChanged; // = delegate { };
        public event PropertyChangingEventHandler PropertyChanging; // = delegate { };
        public event PropertyChangedWithValsHandler PropertyChangedWithVals; // = delegate { };

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

        #endregion

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

        #region Protected Methods and Properties

        protected object this[string propertyName]
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

        protected object GetIt([CallerMemberName] string propertyName = null)
        {
            // This will throw an exception if no value has been added to the _tVals dictionary with a key of propertyName,
            // either by calling AddProp or SetIt.
            ValueWithType vwt = GetValueWithType(propertyName);

            // This uses reflection.
            return vwt.Value;
        }

        protected T GetIt<T>([CallerMemberName] string propertyName = null)
        {
            // This will throw an exception if no value has been added to the _tVals dictionary with a key of propertyName,
            // either by calling AddProp or SetIt.
            ValueWithType vwt = GetValueWithType(propertyName);

            IProp<T> prop = (IProp<T>)(vwt.Prop);
            return prop.Value;
        }

        protected void SetIt(object value, [CallerMemberName] string propertyName = null)
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
                if (!vwt.TypeIsSolid) 
                {
                    try
                    {
                        // TODO, we probably need to be more creative when determining the type of this new value.
                        Type newType = value.GetType();
                        MakeTypeSolid(vwt, newType);
                    }
                    catch (InvalidCastException ice)
                    {
                        throw new ApplicationException(string.Format("The property: {0} was originally set to null, now its being set to a value whose type is a value type; value types don't allow setting to null.", propertyName),ice);
                    }
                }
                else
                {
                    // Object.GetType() is sufficent here, since AreTypesSame will handle comparison nuances.
                    Type newType = value.GetType();
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

        protected void SetIt<T>(T value, [CallerMemberName] string propertyName = null)
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

            if (!vwt.TypeIsSolid)
            {
                try
                {
                    MakeTypeSolid(vwt, typeof(T));
                }
                catch (InvalidCastException ice)
                {
                    throw new ApplicationException(string.Format("The property: {0} was originally set to null, now its being set to a value whose type is a value type; value types don't allow setting to null.", propertyName),ice);
                }
            }
            else
            {
                if (!AreTypesSame(typeof(T), vwt.Type))
                {
                    throw new ApplicationException(string.Format("Attempting to set property: {0} whose type is {1}, with a call whose type parameter is {2} is invalid.", propertyName, vwt.Type.ToString(), typeof(T).ToString()));
                }
            }

            DoSet(value, propertyName, (IProp<T>)vwt.Prop);
        }

        /// <summary>
        /// For use when the Property Bag's internal storage is not appropriate. This allows
        /// the property implementor to use a backing store of their choice.
        /// The property must be registered with a call to AddPropNoStore.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newValue">The new value to use to update the property. No operation will be performed if this value is the same as the current value.</param>
        /// <param name="curValue">The current value of the property, must be specified using the ref keyword.</param>
        /// <param name="propertyName"></param>
        /// <returns>True if the value was updated, otherwise false.</returns>
        protected bool SetIt<T>(T newValue, ref T curValue, [CallerMemberName]string propertyName = null)
        {
            ValueWithType vwt;

            try
            {
                vwt = GetValueWithType(propertyName);
            }
            catch (KeyNotFoundException)
            {
                throw new ApplicationException(string.Format("Property: {0} has not been defined with a call to AddProp().", propertyName));
            }

            IProp<T> prop = (IProp<T>)vwt.Prop;

            bool theSame = prop.Compare(newValue, curValue);

            if (!theSame)
            {
                // Save the value before the update.
                T oldValue = curValue;

                OnPropertyChanging(propertyName);

                // Make the update.
                curValue = newValue;

                // Raise notify events.
                DoNotifyWork<T>(oldValue, newValue, propertyName, prop);
            }

            // Return true, if the new value was found to be different than the current value.
            return !theSame;
        }

        protected void SubscribeToPropChanged<T>(Action<T, T> doOnChange, [CallerMemberName] string propertyName = null)
        {
            ValueWithType vwt = GetValueWithType(propertyName);

            if (!vwt.TypeIsSolid)
            {
                try
                {
                    MakeTypeSolid(vwt, typeof(T));
                }
                catch (InvalidCastException ice)
                {
                    throw new ApplicationException(string.Format("The property: {0} was originally set to null, now a subscription to propChanged is being made with an action whose parameters' type is a value type; value types don't allow setting to null.", propertyName), ice);
                }
            }
            else
            {
                // Make sure that the Action type is the same as the type registerd for this property.
                if (!AreTypesSame(typeof(T), vwt.Type))
                {
                    throw new ApplicationException(string.Format("Attempting to subscribe to PropChanged for property: {0} whose type is {1}, with a call whose type parameter is {2} is invalid.", propertyName, vwt.Type.ToString(), typeof(T).ToString()));
                }
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

        protected void ConnectToPropChanged<T>(Action<T, T> doOnChange, [CallerMemberName] string subToPropertyName = null)
        {
            string propertyName = GetPropNameFromSubTo(subToPropertyName);
            SubscribeToPropChanged<T>(doOnChange, propertyName);
        }

        public bool PropertyExists([CallerMemberName] string propertyName = null)
        {
            return tVals.ContainsKey(propertyName);
        }

        public System.Type GetTypeOfProperty([CallerMemberName] string propertyName = null)
        {
            return GetValueWithType(propertyName).Type;
        }

        protected List<PropertyChangedWithValsHandler> GetPropChangedWithValsHandlers([CallerMemberName] string propertyName = null)
        {
            return GetValueWithType(propertyName).PropChangedWithValsHandlerList;
        }

        /// <summary>
        /// Returns all of the values in dictionary of objects, keyed by PropertyName.
        /// </summary>
        protected IDictionary<string, object> GetAllPropertyValues
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

        private void DoSet<T>(T newValue, string propertyName, IProp<T> prop)
        {
            bool theSame = prop.CompareTo(newValue);

            if (!theSame)
            {
                // Save the value before the update.
                T oldValue = prop.Value;

                OnPropertyChanging(propertyName);

                // Make the update.
                prop.Value = newValue;

                // Raise notify events.
                DoNotifyWork(oldValue, newValue, propertyName, prop);
            }
        }

        private void DoNotifyWork<T>(T oldVal, T newValue, string propertyName, IProp<T> prop)
        {
            if (prop.HasCallBack)
            {
                if (prop.DoAfterNotify)
                {
                    // Raise the changed events,
                    OnPropertyChanged(propertyName);
                    OnPropertyChangedWithVals(propertyName, oldVal, newValue);

                    // then perform the call back.
                    prop.DoWHenChanged(oldVal, newValue);
                }
                else
                {
                    // Peform the call back,
                    prop.DoWHenChanged(oldVal, newValue);

                    // then raise the changed events 
                    OnPropertyChanged(propertyName);
                    OnPropertyChangedWithVals(propertyName, oldVal, newValue);
                }
            }
            else
            {
                //Raise the changed events 
                OnPropertyChanged(propertyName);
                OnPropertyChangedWithVals(propertyName, oldVal, newValue);
            }

        }

        private ValueWithType GetValueWithType(string propertyName, bool? desiredHasStoreValue = true)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName", "PropertyName is null on call to GetValue.");
            ValueWithType vwt = tVals[propertyName];

            if (desiredHasStoreValue.HasValue && desiredHasStoreValue.Value != vwt.HasStore)
            {
                if (desiredHasStoreValue.Value)
                    //Caller needs property to have a backing store.
                    throw new InvalidOperationException(string.Format("Property: {0} has no backing store held by this instance of PropBag. This operation can only be performed on properties for which a backing store is held by this instance.", propertyName));
                else
                    throw new InvalidOperationException(string.Format("Property: {0} has a backing store held by this instance of PropBag. This operation can only be performed on properties for which no backing store is kept by PropBag.", propertyName));
            }

            return vwt;
        }

        private void MakeTypeSolid(ValueWithType vwt, Type newType)
        {
            Type currentType = vwt.Type;

            Debug.Assert(vwt.Value == null, "The current value of the property should be null when MakeTypeSolid is called.");

            System.Type underlyingtype = Nullable.GetUnderlyingType(newType);

            // Check to see if the new type is a non-nullable, value type.
            // If it is, then it was invalid to set this property to null in the first place.
            // Consider setting value to the type's default value instead of throwing this exception.
            if (underlyingtype == null && newType.IsValueType)
                throw new InvalidCastException("The new type is a non-nullable value type.");

            // We are using strict equality here, since we have the oportunity to update the type to anything
            // that is assignable from a value of type object (which is everything.)
            if (newType != currentType)
            {
                // Next statement uses reflection.
                object curValue = vwt.Value;
                vwt.UpdateWithSolidType(newType, curValue);
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

        private DoSetDelegate GetPropSetterDelegate(ValueWithType vwt)
        {
            if (vwt.DoSetProVal == null)
            {
                vwt.DoSetProVal = GetDoSetDelegate(vwt.Type);
            }
            return vwt.DoSetProVal;
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

        // This allow the caller to use their own storage to hold the value.
        public Guid AddPropExtStore<T>(string propertyName, GetExtVal<T> getter, SetExtVal<T> setter, IEqualityComparer<T> comparer)
        {
            Guid tag = Guid.NewGuid();
            tVals.Add(propertyName, ValueWithType.CreateWithCustStore<T>(tag, getter, setter, comparer));

            return tag;
        }

        //-- Need ExtStore with ObjComp

        public void AddPropNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false,
            IEqualityComparer<T> comparer = null)
        {
            tVals.Add(propertyName, ValueWithType.CreateWithNoStore<T>(doIfChanged, doAfterNotify, comparer));
        }

        //-- Need NoStore with ObjComp

        // The remainder of the AddProp variants are "sugar" -- they allow for abbreviated calls to what is provided above.
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

        #region Methods to Raise Events

        // Raise Standard Events
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChanged, null, null);

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanging([System.Runtime.CompilerServices.CallerMemberName]string propertyName = null)
        {
            PropertyChangingEventHandler handler = Interlocked.CompareExchange(ref PropertyChanging, null, null);

            if (handler != null)
                handler(this, new PropertyChangingEventArgs(propertyName));
        }

        // Raise Type Events
        private void OnPropertyChangedWithVals(string propertyName, object oldVal, object newVal)
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
            public object Prop { get; private set; }
            public bool TypeIsSolid { get; private set; }
            public bool HasStore { get; private set; }

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
            public ValueWithType(Type typeOfThisValue, object prop, bool typeIsSolid, bool hasStore = true)
            {
                Type = typeOfThisValue;
                Prop = prop;
                TypeIsSolid = typeIsSolid;
                HasStore = hasStore;

                PropChangedWithValsHandlerList = new List<PropertyChangedWithValsHandler>();
                DoGetProVal = null;
                DoSetProVal = null;
            }

            #region Factory Methods

            static public ValueWithType Create<T>(T value, Action<T,T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null, bool typeIsSolid = true)
            {
                // Use the implementation which takes IEqualityComparer<T>
                Prop<T> prop = new Prop<T>(value, doWhenChanged, doAfterNotify, comparer);
                return new ValueWithType(typeof(T), prop, typeIsSolid);
            }

            static public ValueWithType CreateWithObjComparer<T>(T value, Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<object> comparer = null)
            {
                // Use the Implementation which takes IEqualityComparer<object>
                PropObjComp<T> prop = new PropObjComp<T>(value, doWhenChanged, doAfterNotify);
                return new ValueWithType(typeof(T), prop, typeIsSolid: true);
            }

            static public ValueWithType CreateWithCustStore<T>(Guid tag, GetExtVal<T> getter, SetExtVal<T> setter, IEqualityComparer<T> comparer)
            {
                // Use the implementation which allow the caller to provide the backing store.
                PropExternStore<T> prop = new PropExternStore<T>(tag, getter, setter, null, false, comparer);
                return new ValueWithType(typeof(T), prop, typeIsSolid: true);
            }

            static public ValueWithType CreateWithCustStoreObjComp<T>(Guid tag, GetExtVal<T> getter, SetExtVal<T> setter, IEqualityComparer<object> comparer)
            {
                // Use the implementation which allow the caller to provide the backing store.
                PropExternStoreObjComp<T> prop = new PropExternStoreObjComp<T>(tag, getter, setter, null, false, comparer);
                return new ValueWithType(typeof(T), prop, typeIsSolid: true);
            }

            static public ValueWithType CreateWithNoStore<T>(Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
            {
                // Use the implementation which uses no backing store. The caller can only update using call to SetIt<T> where the current and new values are provided.
                PropNoStore<T> prop = new PropNoStore<T>(doWhenChanged, doAfterNotify, comparer);
                return new ValueWithType(typeof(T), prop, typeIsSolid: true, hasStore: false);
            }

            static public ValueWithType CreateWithNoStoreObjComp<T>(Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<object> comparer = null)
            {
                // Use the implementation which uses no backing store. The caller can only update using call to SetIt<T> where the current and new values are provided.
                PropNoStoreObjComp<T> prop = new PropNoStoreObjComp<T>(doWhenChanged, doAfterNotify, comparer);
                return new ValueWithType(typeof(T), prop, typeIsSolid: true, hasStore: false);
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
                ValueWithType vwt = createValWithType(value, typeIsSolid);

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

            public void UpdateWithSolidType(Type typeOfThisValue, object curValue)
            {
                // Create a brand new ValueWithType instance -- we could have created a custom Delegate for this -- but this strategy reuses more code.
                CreateVWTDelegate vwtCreator = GetVWTCreator(typeOfThisValue);
                ValueWithType newVwt = vwtCreator(curValue, true);

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

            private delegate ValueWithType CreateVWTDelegate(object value, bool typeIsSolid);

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

                private static ValueWithType CreateVWT<T>(object value, bool isTypeSolid)
                {
                    return ValueWithType.Create<T>((T)value, null, false, null, isTypeSolid);
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
