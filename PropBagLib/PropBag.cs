using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using DRM.ReferenceEquality;
using DRM.Ipnwv;

namespace PropBagLib
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

        #endregion

        #region Constructor

        public PropBag() : this(PropBagTypeSafetyModeEnum.AllPropsMustBeRegistered) { }

        public PropBag(PropBagTypeSafetyModeEnum typeSafetyMode)
        {
            switch (typeSafetyMode)
            {
                case PropBagTypeSafetyModeEnum.AllPropsMustBeRegistered:
                    {
                        AllPropsMustBeRegistered = true;
                        AllowSetsViaThisForNewProps = false;
                        break;
                    }
                case PropBagTypeSafetyModeEnum.AllPropsMustBeFirstSetWithSetIt:
                    {
                        AllPropsMustBeRegistered = false;
                        AllowSetsViaThisForNewProps = false;
                        break;
                    }
                case PropBagTypeSafetyModeEnum.Loose:
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
            ValueWithType vwt = GetValue(propertyName);

            foreach (PropertyChangedWithValsHandler h in vwt.PropChangedWithValsHandlerList)
            {
                this.PropertyChangedWithVals -= h;
            }

            tVals.Remove(propertyName);

        }

        #endregion

        #region Public Methods and Properties

        public object GetIt([CallerMemberName] string propertyName = null)
        {
            // This will throw an exception if no value has been added to the _tVals dictionary with a key of propertyName,
            // either by calling AddProp or SetIt.
            return GetValue(propertyName).Value;
        }

        public T GetIt<T>([CallerMemberName] string propertyName = null)
        {
            // This will throw an exception if no value has been added to the _tVals dictionary with a key of propertyName,
            // either by calling AddProp or SetIt.
            return (T)(GetValue(propertyName).Value);
        }

        public void SetIt<T>(T value, [CallerMemberName] string propertyName = null)
        {
            ValueWithType vwt;

            // We are assuming that the vast majority of the time, an entry will exist in the _tVals dictionary with this propertyName,
            // otherwise we would make use Dictionary.ContainsKey to test.

            try
            {
                vwt = GetValue(propertyName);
            }
            catch (KeyNotFoundException)
            {
                if (AllPropsMustBeRegistered)
                {
                    throw new ApplicationException(string.Format("Property: {0} has not been defined with a call to AddProp() and the operation setting 'AllPropsMustBeRegistered' is set to true.", propertyName));
                }

                // Property has not been defined yet, let's create a definition for it now and initialize the value.
                tVals.Add(propertyName, ValueWithType.CreateValueWithType<T>(value, comparer: null, plainComparer: null, doIfChanged: null, doAfterNotify: false, isRegistered: false));

                // No reason to call DoSet, it will find no change and do nothing.
                return;
            }

            DoSet(propertyName, vwt, value, typeof(T));
        }

        /// <summary>
        /// Gets and Sets property values by name. The caller is responsible for casting the value back to the correct type on retrieval.
        /// On set, if the property has not yet been set, the type of the value is used to define the property's type.
        /// If on the first set, the value is null, the type will be object and the definition will be marked as pending.
        /// Once the property has been set with a non-null value, the property's type is fixed.
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object this[string propertyName]
        {
            get
            {
                return GetValue(propertyName).Value;
            }
            set
            {
                // This will throw an exception if a property with this name has not yet been given a value.
                ValueWithType vwt;
                try
                {
                    vwt = GetValue(propertyName);
                }
                catch (KeyNotFoundException)
                {
                    if (AllPropsMustBeRegistered)
                        throw new KeyNotFoundException(string.Format("Property: {0} has not been declared by calling AddProp, nor has its value been set by calling SetIt<T>. Cannot use this method in this case. Declare by calling AddProp, or use the SetIt<T> method.", propertyName));

                    if (!AllowSetsViaThisForNewProps)
                    {
                        throw new ApplicationException(string.Format("Property: {0} has not been defined with a call to AddProp or any SetIt<T> call and the operation setting 'AllowSetsViaThisForNewProps' is set to false.", propertyName));
                    }

                    vwt = ValueWithType.CreateValueInferType(value);
                    this.tVals.Add(propertyName, vwt);

                    // No point in calling DoSet, it would find that the value is the same and do nothing.
                    return;
                }

                DoSet(propertyName, vwt, value, (value == null ? null : value.GetType()));
            }
        }

        public void SubscribeToPropChanged<T>(Action<T, T> doOnChange, [CallerMemberName] string propertyName = null)
        {
            ValueWithType vwt = GetValue(propertyName);

            vwt.CheckTypeSimple(typeof(T), propertyName);

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

        private string GetPropNameFromSubTo(string x)
        {
            //SubscribeToPropStringChanged
            return x.Substring(11, x.Length - 18);
        }

        public bool PropertyExists([CallerMemberName] string propertyName = null)
        {
            return tVals.ContainsKey(propertyName);
        }

        public System.Type GetType([CallerMemberName] string propertyName = null)
        {
            return GetValue(propertyName).Type;
        }

        public System.Type GetUnderlyingType([CallerMemberName] string propertyName = null)
        {
            return GetValue(propertyName).TypeUnderlyingNullableType;
        }

        public List<PropertyChangedWithValsHandler> GetPropChangedWithValsHandlers([CallerMemberName] string propertyName = null)
        {
            return GetValue(propertyName).PropChangedWithValsHandlerList;
        }

        public bool GetIsRegistered([CallerMemberName] string propertyName = null)
        {
            return GetValue(propertyName).IsRegistered;
        }

        /// <summary>
        /// Returns all of the values in dictionary of objects, keyed by PropertyName.
        /// </summary>
        public IDictionary<string, object> GetAll
        {
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

        private void DoSet(string propertyName, ValueWithType vwt, object value, Type newValType)
        {
            if (!vwt.TypeIsSolid && value != null)
            {
                vwt.MakeTypeSolid(value, newValType, propertyName);
            }
            
            vwt.CheckType(value, newValType, propertyName);

            bool theSame = vwt.CompareTo(value);

            if (!theSame)
            {
                RaisePropertyChangingEvent(propertyName);

                // Save the value before the update.
                object oldVal = vwt.Value;

                // Make the update.
                vwt.Value = value;

                if (vwt.HasCallBack)
                {
                    if (vwt.DoAfterNotify)
                    {
                        //Raise the event -- Then the call back.
                        RaisePropertyChangedEvent(propertyName);
                        RaisePropertyChangedWithValsEvent(propertyName, oldVal, value);

                        vwt.DoCallBack(oldVal, value);
                    }
                    else
                    {
                        // Run the call back -- then raise the event.
                        vwt.DoCallBack(oldVal, value);

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

        private ValueWithType GetValue(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName", "PropertyName is null on call to GetValue.");
            return tVals[propertyName];
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
            tVals.Add(propertyName, ValueWithType.CreateValueWithType<T>(initalValue, comparer, null, doIfChanged, doAfterNotify));
        }

        public void AddProp<T>(string propertyName, bool useReferenceEquality, Action<T, T> doIfChanged, bool doAfterNotify = false,
             T initalValue = default(T))
        {
            IEqualityComparer<object> plainComparer = useReferenceEquality ? ReferenceEqualityComparer.Default : null;
            tVals.Add(propertyName, ValueWithType.CreateValueWithType<T>(initalValue, null, plainComparer, doIfChanged, doAfterNotify));
        }

        public void AddProp<T>(string propertyName, T initalValue = default(T))
        {
            tVals.Add(propertyName, ValueWithType.CreateValueWithType<T>(initalValue, null, null, null, false));
        }

        public void AddProp<T>(string propertyName, IEqualityComparer<T> comparer, T initalValue = default(T))
        {
            tVals.Add(propertyName, ValueWithType.CreateValueWithType<T>(initalValue, comparer, null, null, false));
        }

        public void AddProp<T>(string propertyName, bool useReferenceEquality, T initalValue = default(T))
        {
            IEqualityComparer<object> plainComparer = useReferenceEquality ? ReferenceEqualityComparer.Default : null;
            tVals.Add(propertyName, ValueWithType.CreateValueWithType<T>(initalValue, null, plainComparer, null, false));
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

        /// <summary>
        /// Stores the Value along with its type and optionally a method for can be used to test for equality.
        /// Instances should be created using the static factory method: CreateValueWithType<typeparamref name="T"/>
        /// </summary>
        private class ValueWithType
        {
            static Type GMT_TYPE = typeof(GenericMethodTemplates);

            public Type Type { get; private set;}
            public Type TypeUnderlyingNullableType {get; private set;}
            public bool IsRegistered { get; private set; }
            public bool TypeIsSolid { get; private set; }

            DoWhenChangedDelegate DoWhenChanged;
            object DoWhenChangedTarget; 
            public bool DoAfterNotify { get; private set; }

            DoCompareDelegate DoCompare;
            object ComparerTarget;

            public object Value { get; set; }

            public readonly List<PropertyChangedWithValsHandler> PropChangedWithValsHandlerList;

            public bool TypeIsNullable
            {
                get
                {
                    return this.TypeUnderlyingNullableType != null;
                }
            }

            public bool HasCallBack
            {
                get
                {
                    return DoWhenChanged != null;
                }
            }

            public void CheckType(object value, Type newValType, string propertyName)
            {
                if (propertyName == null) throw new ArgumentNullException("PropertyName");

                if (value != null && newValType == null) throw new ArgumentNullException("newValType", "The value's type must be specified if the value is non-null.");

                if (newValType != null)
                {
                    bool typeIsOk;
                    if (this.TypeIsNullable)
                    {
                        // Compare the underlying type of this new value and the target property's underlying type.
                        Type inComingTypeUnderNullableType = Nullable.GetUnderlyingType(newValType);

                        typeIsOk = inComingTypeUnderNullableType == this.TypeUnderlyingNullableType;
                    }
                    else
                    {
                        if (this.Type.IsGenericType)
                        {
                            typeIsOk = this.Type.IsAssignableFrom(newValType);
                        }
                        else
                        {
                            typeIsOk = newValType == this.Type;
                        }
                    }

                    if (!typeIsOk)
                    {
                        string msg = string.Format("Attempting to set value of property {0} failed, property is of type: {1}, incoming value is of type: {2}",
                            propertyName, this.Type.ToString(), newValType.ToString());
                        throw new InvalidCastException(msg);
                    }
                }

                // The Types match, now let's check to see if the caller is trying to assing null to a property whose type is a ValueType.
                // If the target property's type IsNullable, then setting the value to null is ok.
                if (!this.TypeIsNullable)
                {
                    if (value == null && this.Type.IsValueType)
                    {
                        string msg = string.Format("Types of {0} cannot have a null value. PropertyName is {1}", this.Type.ToString(), propertyName);
                        throw new ArgumentNullException(msg);
                    }
                }
            }

            public void CheckTypeSimple(Type newValType, string propertyName)
            {
                if (propertyName == null) throw new ArgumentNullException("PropertyName");

                if (newValType == null) throw new ArgumentNullException("newValType", "The value's type must be specified.");

                bool typeIsOk;
                if (this.TypeIsNullable)
                {
                    // Compare the underlying type of this new value and the target property's underlying type.
                    Type inComingTypeUnderNullableType = Nullable.GetUnderlyingType(newValType);

                    typeIsOk = inComingTypeUnderNullableType == this.TypeUnderlyingNullableType;
                }
                else
                {
                    if (this.Type.IsGenericType)
                    {
                        typeIsOk = this.Type.IsAssignableFrom(newValType);
                    }
                    else
                    {
                        typeIsOk = newValType == this.Type;
                    }
                }

                if (!typeIsOk)
                {
                    string msg = string.Format("Attempting to set value of property {0} failed, property is of type: {1}, incoming value is of type: {2}",
                        propertyName, this.Type.ToString(), newValType.ToString());
                    throw new InvalidCastException(msg);
                }
            }


            public void MakeTypeSolid(object newVal, Type newType, string propertyName)
            {
                if (this.Value != null) throw new ApplicationException(string.Format("The current value of property: {0} should be null when MakeTypeSolid is called.", propertyName));

                System.Type underlyingtype = Nullable.GetUnderlyingType(newType);

                // Check to see if the new type is a non-nullable, value type.
                // If it is, then it was invalid to set this property to null in the first place.
                // Consider setting value to the type's default value instead of throwing this exception.
                if(underlyingtype == null && newType.IsValueType)
                    throw new ApplicationException(string.Format("The property: {0} was originally set to null, now its being set to a value whose type is a value type; value types don't allow setting to null.", propertyName));

                // Build delegate to perform equality test.
                object compareProvider;
                DoCompareDelegate doCompare = GetCompareDelegate(newType, underlyingtype, out compareProvider);
              
                // Only update values, if we have done all of the heavy lifting (and no exceptions have been thrown.)
                this.Type = newType;
                this.TypeUnderlyingNullableType = underlyingtype;
                this.TypeIsSolid = true;

                this.DoCompare = doCompare;
                this.ComparerTarget = compareProvider;
            }

            public bool CompareTo(object a)
            {
                return DoCompare(a, this.Value, this.ComparerTarget);
            }

            public void DoCallBack(object oldVal, object value)
            {
                if(DoWhenChanged != null)
                    DoWhenChanged(oldVal, value, DoWhenChangedTarget);
            }

            #region Constructors

            private ValueWithType()
            {
                throw new ApplicationException("Cannot create an instance of ValueWithType using the default (parameterless) constructor.");
            }

            private ValueWithType(Type typeOfThisValue, object value, System.Type typeUnderlyingNullableType, DoCompareDelegate comparer,
                object comparerTarget, bool isRegistered, bool typeIsSolid)
                : this(typeOfThisValue, value, typeUnderlyingNullableType, comparer,
                    comparerTarget, null, null, false, isRegistered, typeIsSolid) { }


            private ValueWithType(Type typeOfThisValue, object value, System.Type typeUnderlyingNullableType, DoCompareDelegate comparer, 
                object comparerTarget, DoWhenChangedDelegate doWhenChanged, object doWhenChangedTarget, bool doAfterNotify,
                bool isRegistered, bool typeIsSolid)
            {
                Type = typeOfThisValue;
                TypeUnderlyingNullableType = typeUnderlyingNullableType;
                IsRegistered = isRegistered;
                TypeIsSolid = typeIsSolid;

                Value = value;

                DoCompare = comparer;
                ComparerTarget = comparerTarget;

                DoWhenChanged = doWhenChanged;
                DoWhenChangedTarget = doWhenChangedTarget;
                DoAfterNotify = doAfterNotify;

                PropChangedWithValsHandlerList = new List<PropertyChangedWithValsHandler>();
            }

            #endregion

            #region Factory Methods

            public static ValueWithType CreateValueInferType(object value)
            {
                System.Type typeOfThisValue; // This may not be solid.
                System.Type typeUnderlyingNullableType;
                bool typeIsSolid;
                object compareProvider;
                DoCompareDelegate delDoCompare;

                if (value == null)
                {
                    typeOfThisValue = typeof(object);
                    typeUnderlyingNullableType = null;
                    typeIsSolid = false;

                    // Build delegate to perform equality test.
                    IEqualityComparer<object> plainComparer = ReferenceEqualityComparer.Default;
                    delDoCompare = GetCompareDelegate<object>(plainComparer, out compareProvider);
                }
                else
                {
                    typeOfThisValue = value.GetType();

                    typeUnderlyingNullableType = Nullable.GetUnderlyingType(typeOfThisValue);
                    typeIsSolid = true;

                    // Build delegate to perform equality test.
                    delDoCompare = GetCompareDelegate(typeOfThisValue, typeUnderlyingNullableType, out compareProvider);
                }

                return new ValueWithType(typeOfThisValue, value, typeUnderlyingNullableType, delDoCompare, compareProvider, false, typeIsSolid);
            }

            public static ValueWithType CreateValueWithType<T>(T value, IEqualityComparer<T> comparer, IEqualityComparer<object> plainComparer, 
                Action<T, T> doIfChanged, bool doAfterNotify, bool isRegistered = true)
            {
                if (comparer != null && plainComparer != null) throw new ApplicationException("It is invalid to supply a non-null value for both comparer and plain comparer.");

                Type typeOfThisValue = typeof(T);

                // If the type is a Nullable<T>, get the underlying type.
                System.Type typeUnderlyingNullableType = Nullable.GetUnderlyingType(typeOfThisValue);

                // Build delegate to perform equality test.
                object compareProvider;
                DoCompareDelegate delDoCompare;

                if (plainComparer != null)
                {
                    // Will use the one specified by the caller.
                    // Note: plainComparer is of type IEqualityComparer<object>
                    delDoCompare = GetCompareDelegate<object>(plainComparer, out compareProvider);
                }
                else
                {
                    // If comparer is null, will return default comparer provided by the .net runtime.
                    // Otherwise will use the one specified by the caller.
                    delDoCompare = GetCompareDelegate<T>(comparer, out compareProvider);
                }
               
                // Process the Action<T,T> provided by the caller.
                DoWhenChangedDelegate delDoWhenChanged;
                object doWhenChangedTarget;

                if (doIfChanged != null)
                {
                    doWhenChangedTarget = doIfChanged;

                    MethodInfo methInfoCallBackBridge = GetCallBackBridge(typeOfThisValue);
                    delDoWhenChanged = (DoWhenChangedDelegate)Delegate.CreateDelegate(typeof(DoWhenChangedDelegate), methInfoCallBackBridge);
                }
                else
                {
                    doWhenChangedTarget = null;
                    delDoWhenChanged = null;
                }

                return new ValueWithType(typeof(T), value, typeUnderlyingNullableType, delDoCompare, compareProvider, delDoWhenChanged, 
                    doWhenChangedTarget, doAfterNotify, isRegistered, typeIsSolid:true);
            }


            private static DoCompareDelegate GetCompareDelegate<T>(IEqualityComparer<T> comparer, out object compareProvider)
            {
                MethodInfo methInfoComparer = GetComparer(typeof(T));
                DoCompareDelegate result = (DoCompareDelegate)Delegate.CreateDelegate(typeof(DoCompareDelegate), methInfoComparer);

                // If the caller did not provide a EqualityComparer, then use the default implementation provided by the .net runtime.
                compareProvider = comparer ?? EqualityComparer<T>.Default;

                return result;
            }

            /// <summary>
            /// Used when the type is not known at compile time.
            /// </summary>
            /// <param name="typeOfThisValue">The type for which to create the EqualityComparer<typeparamref name="T"/>T</param>
            /// <param name="underlyingType">If not null, the type underneath the Nullable<typeparamref name="T"/></param>
            /// <param name="compareProvider">Upon return, a reference to the instance that provides the IEqualityComparer<typeparamref name="T"/></param>
            /// <returns>A delegate that can be used to compare two values of type typeOfThisValue.</returns>
            private static DoCompareDelegate GetCompareDelegate(Type typeOfThisValue, Type underlyingType, out object compareProvider)
            {
                // If underlying type is not null, use it; otherwise use the "real" type.
                Type typeToUse = underlyingType ?? typeOfThisValue;

                MethodInfo methInfoComparer = GetComparer(typeToUse);
                DoCompareDelegate result = (DoCompareDelegate)Delegate.CreateDelegate(typeof(DoCompareDelegate), methInfoComparer);

                // TO-DOC: Explain why we can't just create a delegate that we can call that incorporates the instance (or type) on which
                // the call needs to be made.
                MethodInfo methInfoCompBridge = GetCompareBridge(typeToUse);

                compareProvider = methInfoCompBridge.Invoke(null, null);

                return result;
            }

            #endregion

            #region Delegate declarations

            // Delegate declarations.
            public delegate void DoWhenChangedDelegate(object oldVal, object newVal, object target); // Action<T,T> on target instance.

            public delegate bool DoCompareDelegate(object a, object b, object provider);

            #endregion

            #region Helper Methods for the Generic Method Templates

            private static MethodInfo GetComparer(Type typeOfThisValue)
            {
                if (typeOfThisValue == typeof(object))
                {
                    // Setup a method that uses IEqualityComparer<object> (for e.g., ReferenceEqualityComparer)
                #if STANDARD
                    return GMT_TYPE.GetMethod("IsEqualUsingPlainComparer", BindingFlags.Static | BindingFlags.NonPublic);
                #endif
                #if CORE
                    return GMT_TYPE.GetMethod("IsEqualUsingPlainComparer");
                #endif

                }
                else
                {

                    // Setup a method that uses IEquitableComparer<T> for the given type.
                #if STANDARD
                    return GMT_TYPE.GetMethod("IsEqualUsingComparer", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
                #endif
                #if CORE
                    return GMT_TYPE.GetMethod("IsEqualUsingComparer").MakeGenericMethod(typeOfThisValue);
                #endif
                }
            }

            private static MethodInfo GetCallBackBridge(Type typeOfThisValue)
            {
            #if STANDARD
                return GMT_TYPE.GetMethod("MakeCallBackBridge", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            #endif
            #if CORE
                return GMT_TYPE.GetMethod("MakeCallBackBridge").MakeGenericMethod(typeOfThisValue);
            #endif
            }

            private static MethodInfo GetCompareBridge(Type typeOfThisValue)
            {
            #if STANDARD
                return GMT_TYPE.GetMethod("MakeComparerBridge", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            #endif
            #if CORE
                return GMT_TYPE.GetMethod("MakeComparerBridge").MakeGenericMethod(typeOfThisValue);
            #endif
            }

            #endregion

            #region Generic Method Templates

            static class GenericMethodTemplates
            {
                // Used when caller provides IEqualityComparer<T>
                private static bool IsEqualUsingComparer<T>(object a, object b, object target)
                {
                    IEqualityComparer<T> x = (IEqualityComparer<T>)target;
                    return x.Equals((T)a, (T)b);
                }

                private static bool IsEqualUsingPlainComparer(object a, object b, object target)
                {
                    IEqualityComparer<object> x = (IEqualityComparer<object>)target;
                    return x.Equals(a, b);
                }

                private static void MakeCallBackBridge<T>(object a, object b, object target)
                {
                    Action<T, T> x = (Action<T, T>)target;
                    x((T)a, (T)b);
                }

                //private static void PropChangedWithValsBridge<T>(object a, object b, object target)
                //{
                //    Action<T, T> x = (Action<T, T>)target;
                //    x((T)a, (T)b);
                //}

                private static IEqualityComparer<T> MakeComparerBridge<T>()
                {
                    IEqualityComparer<T> temp = EqualityComparer<T>.Default;

                    return temp;
                }
            }

            #endregion

        } // End nested class: ValueWWithType

    }


}

