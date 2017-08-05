using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using DRM.ReferenceEquality;

using DRM.Ipnwvc;

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

        // TO-DOC: Explain that the life time of any sources that listen to the events provided by this class,
        // including the events provided by the IProp instances
        // is determined by the lifetime of the instances of classes that derive from this PropBag class.

        // TO-DOC: Explain since we may subscribe to our own events we would like to have these initialized.
        // confirm that on post construction events are initialized for us if we don't
        // alternative could be that they are initialized on first assignment.
        public event PropertyChangedEventHandler PropertyChanged; // = delegate { };
        public event PropertyChangingEventHandler PropertyChanging; // = delegate { };
        public event PropertyChangedWithValsHandler PropertyChangedWithVals; // = delegate { };

        AbstractPropFactory thePropFactory;

        private readonly Dictionary<string, IPropGen> tVals;

        private readonly Dictionary<Type, DoSetDelegate> doSetDelegateDict;

        /// <summary>
        /// If true, attempting to set a property for which no call to AddProp has been made, will cause an exception to thrown.
        /// </summary>
        public readonly bool AllPropsMustBeRegistered;

        /// <summary>
        /// If not true, attempting to set a property, not previously set with a call to AddProp or SetIt<typeparamref name="T"/>, will cause an exception to be thrown.
        /// </summary>
        public readonly bool OnlyTypedAccess;

        /// <summary>
        /// Used to create Delegates when the type of the value is not known at run time.
        /// </summary>
        static Type GMT_TYPE = typeof(GenericMethodTemplates);

        #endregion

        #region Constructor

        public PropBag() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered, new PropFactory()) { }

        public PropBag(PropBagTypeSafetyMode typeSafetyMode) : this(typeSafetyMode, new PropFactory()) { }

        public PropBag(PropBagTypeSafetyMode typeSafetyMode, AbstractPropFactory thePropFactory)
        {
            switch (typeSafetyMode)
            {
                case PropBagTypeSafetyMode.AllPropsMustBeRegistered:
                    {
                        AllPropsMustBeRegistered = true;
                        OnlyTypedAccess = true;
                        break;
                    }
                case PropBagTypeSafetyMode.OnlyTypedAccess:
                    {
                        AllPropsMustBeRegistered = false;
                        OnlyTypedAccess = true;
                        break;
                    }
                case PropBagTypeSafetyMode.Loose:
                    {
                        AllPropsMustBeRegistered = false;
                        OnlyTypedAccess = false;
                        break;
                    }
                default:
                    throw new ApplicationException("Unexpected value for typeSafetyMode parameter.");
            }

            this.thePropFactory = thePropFactory;
            tVals = new Dictionary<string, IPropGen>();
            doSetDelegateDict = new Dictionary<Type, DoSetDelegate>();
        }

        // When we are being destructed, remove all of the handlers that we provisioned.
        ~PropBag()
        {
            foreach (KeyValuePair<string,IPropGen> kvp in tVals)
            {
                IPropGen genProp = kvp.Value;

                foreach (PropertyChangedWithValsHandler h in genProp.PropChangedWithValsHandlerList)
                {
                    this.PropertyChangedWithVals -= h;
                }
            }

            // Maybe not necessary, but since each of these refers back to this instance of PropBag,
            // it may make it a little easier on the garbage collector.
            doSetDelegateDict.Clear();
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
            if (OnlyTypedAccess)
            {
                throw new InvalidOperationException("Attempt to access property using this method is not allowed when TypeSafetyMode is 'OnlyTypedAccess.'");
            }

            // This will throw an exception if no value has been added to the _tVals dictionary with a key of propertyName,
            // either by calling AddProp or SetIt.
            IPropGen genProp = GetGenProp(propertyName);

            // This uses reflection.
            return genProp.Value;
        }

        protected T GetIt<T>([CallerMemberName] string propertyName = null)
        {
            // This will throw an exception if no value has been added to the _tVals dictionary with a key of propertyName,
            // either by calling AddProp or SetIt.
            IPropGen genProp = GetGenProp(propertyName);

            IProp<T> prop = CheckTypeInfo<T>(genProp, propertyName, tVals);

            return prop.TypedValue;
        }

        protected void SetIt(object value, [CallerMemberName] string propertyName = null)
        {
            if (OnlyTypedAccess)
            {
                throw new InvalidOperationException("Attempt to access property using this method is not allowed when TypeSafetyMode is 'OnlyTypedAccess.'");
            }

            IPropGen genProp;
            try
            {
                genProp = GetGenProp(propertyName);
            }
            catch (KeyNotFoundException)
            {
                if (AllPropsMustBeRegistered)
                    throw new KeyNotFoundException(string.Format("Property: {0} has not been declared by calling AddProp, nor has its value been set by calling SetIt<T>. Cannot use this method in this case. Declare by calling AddProp, or use the SetIt<T> method.", propertyName));

                if (OnlyTypedAccess)
                {
                    throw new ApplicationException(string.Format("Property: {0} has not been defined with a call to AddProp or any SetIt<T> call and the operation setting 'OnlyTypeAccesss' is set to true.", propertyName));
                }

                genProp = thePropFactory.CreatePropInferType(value, propertyName, null, true);
                tVals.Add(propertyName, genProp);

                // No point in calling DoSet, it would find that the value is the same and do nothing.
                return;
            }

            if (value != null)
            {
                if (!genProp.TypeIsSolid) 
                {
                    try
                    {
                        // TODO, we probably need to be more creative when determining the type of this new value.
                        Type newType = value.GetType();

                        MakeTypeSolid(ref genProp, newType, propertyName);
                        tVals[propertyName] = genProp;
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
                    if (!AreTypesSame(newType, genProp.Type))
                    {
                        throw new ApplicationException(string.Format("Attempting to set property: {0} whose type is {1}, with a call whose type parameter is {2} is invalid.", propertyName, genProp.Type.ToString(), newType.ToString()));
                    }
                }
            }
            else
            {
                if (genProp.TypeIsSolid && genProp.Type.IsValueType)
                {
                    throw new InvalidOperationException(string.Format("Cannot set property: {0} to null, it is a value type.", propertyName));
                }
            }

            // This uses reflection on first access.
            DoSetDelegate setProDel = GetPropSetterDelegate(genProp);
            setProDel(value, this, propertyName, genProp);
        }

        protected void SetIt<T>(T value, [CallerMemberName] string propertyName = null)
        {
            IPropGen genProp;

            try
            {
                genProp = GetGenProp(propertyName);
            }
            catch (KeyNotFoundException)
            {
                if (AllPropsMustBeRegistered)
                {
                    throw new ApplicationException(string.Format("Property: {0} has not been defined with a call to AddProp() and the operation setting 'AllPropsMustBeRegistered' is set to true.", propertyName));
                }

                // Property has not been defined yet, let's create a definition for it now and initialize the value.

                genProp = thePropFactory.Create<T>(value, propertyName);
                tVals.Add(propertyName, genProp);

                // No reason to call DoSet, it will find no change and do nothing.
                return;
            }

            IProp<T> prop = CheckTypeInfo<T>(genProp, propertyName, tVals);


            DoSet(value, propertyName, prop);
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
            IPropGen genProp;

            try
            { 
                // This will fail if the property has a backing store.
                genProp = GetGenProp(propertyName, desiredHasStoreValue: false);
            }
            catch (KeyNotFoundException)
            {
                if (AllPropsMustBeRegistered)
                {
                    throw new ApplicationException(string.Format("Property: {0} has not been defined with a call to AddProp() and the operation setting 'AllPropsMustBeRegistered' is set to true.", propertyName));
                }

                // Property has not been defined yet, let's create a definition for it now 
                genProp = thePropFactory.CreateWithNoneOrDefault<T>(propertyName, hasStorage: false, typeIsSolid: true);

                tVals.Add(propertyName, genProp);
            }

            IProp<T> prop = CheckTypeInfo<T>(genProp, propertyName, tVals);

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

        // TODO: Consider making this protected, and then as an option in the PropDefs.xml
        // to expose it using a wrapper in the props.cs file.

        // This is used to allow the caller to get notified only when a particular property is changed with values.
        // It can be used in any of the three modes, but is especially handy for Loose mode.
        public void SubscribeToPropChanged(Action<object, object> doOnChange, [CallerMemberName] string propertyName = null)
        {

            // TODO: consider creating a new property, if one doesn't exist, using just the name.
            IPropGen genProp = GetGenProp(propertyName);

            PropertyChangedWithValsHandler action = (s, e) =>
            {
                if (string.Equals(e.PropertyName, propertyName, StringComparison.InvariantCulture))
                {
                    doOnChange(e.OldValue, e.NewValue);
                }
            };

            this.PropertyChangedWithVals += action;

            // References are kept here so that we can remove them when this instance is disposed.
            genProp.PropChangedWithValsHandlerList.Add(action);
        }

        // TODO: Consider making this protected, and then as an option in the PropDefs.xml
        // to expose it using a wrapper in the props.cs file.

        // This is used to support 'OnlyTypeAccess' mode,
        // to allow callers to easily subscribe to PropertyChangedWithVals.
        public void SubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName)
        {
            IProp<T> prop = GetPropDef<T>(propertyName);

            PropertyChangedWithTValsHandler<T> action = (s, e) =>
            {
                if (string.Equals(e.PropertyName, propertyName, StringComparison.InvariantCulture))
                {
                    doOnChange((T)e.OldValue, (T)e.NewValue);
                }
            };

            prop.PropertyChangedWithTVals += action;
        }

        // This uses "real" PropertyChanged event handers -- not actions.
        // The subscriber is responsible for inspecting the value of the property name.
        public void SubscribeToPropChanged<T>(PropertyChangedWithTValsHandler<T> action, string propertyName)
        {
            IProp<T> prop = GetPropDef<T>(propertyName);

            prop.PropertyChangedWithTVals += action;
        }

        private IPropGen GetTypeCheckedGenProp<T>(string propertyName)
        {
            IPropGen genProp;
            GetPropDef<T>(propertyName, out genProp);
            return genProp;
        }

        private IProp<T> GetPropDef<T>(string propertyName)
        {
            IPropGen genProp;
            return GetPropDef<T>(propertyName, out genProp);
        }

        private IProp<T> GetPropDef<T>(string propertyName, out IPropGen genProp)
        {
            try
            {
                genProp = GetGenProp(propertyName);
            }
            catch (KeyNotFoundException)
            {
                if (AllPropsMustBeRegistered)
                {
                    throw new ApplicationException(string.Format("Property: {0} has not been defined with a call to AddProp() and the operation setting 'AllPropsMustBeRegistered' is set to true.", propertyName));
                }

                // Property has not been defined yet, let's create a definition for it now and initialize the value.

                genProp = thePropFactory.CreateWithNoneOrDefault<T>(propertyName);
                tVals.Add(propertyName, genProp);
            }

            IProp<T> prop = CheckTypeInfo<T>(genProp, propertyName, tVals);

            return prop;
        }

        protected void AddToPropChanged<T>(PropertyChangedWithTValsHandler<T> action, [CallerMemberName] string eventPropertyName = null)
        {
            string propertyName = GetPropNameFromEventProp(eventPropertyName);
            SubscribeToPropChanged<T>(action, propertyName);
        }

        public void UnSubscribeToPropChanged<T>(PropertyChangedWithTValsHandler<T> action, string propertyName)
        {
            IPropGen genProp = GetGenProp(propertyName);

            IProp<T> prop = CheckTypeInfo<T>(genProp, propertyName, tVals);

            prop.PropertyChangedWithTVals -= action;
        }

        protected void RemoveFromPropChanged<T>(PropertyChangedWithTValsHandler<T> action, [CallerMemberName] string eventPropertyName = null)
        {
            string propertyName = GetPropNameFromEventProp(eventPropertyName);
            UnSubscribeToPropChanged<T>(action, propertyName);
        }

        private IProp<T> CheckTypeInfo<T>(IPropGen genProp, string propertyName, IDictionary<string, IPropGen> dict)
        {
            if (!genProp.TypeIsSolid)
            {
                try
                {
                    MakeTypeSolid(ref genProp, typeof(T), propertyName);
                    dict[propertyName] = genProp;
                }
                catch (InvalidCastException ice)
                {
                    throw new ApplicationException(string.Format("The property: {0} was originally set to null, now its being set to a value whose type is a value type; value types don't allow setting to null.", propertyName), ice);
                }
            }
            else
            {
                if (!AreTypesSame(typeof(T), genProp.Type))
                {
                    throw new ApplicationException(string.Format("Attempting to set property: {0} whose type is {1}, with a call whose type parameter is {2} is invalid.", propertyName, genProp.Type.ToString(), typeof(T).ToString()));
                }
            }

            return (IProp<T>)genProp;
        }

        public bool PropertyExists([CallerMemberName] string propertyName = null)
        {
            return tVals.ContainsKey(propertyName);
        }

        public System.Type GetTypeOfProperty([CallerMemberName] string propertyName = null)
        {
            return GetGenProp(propertyName).Type;
        }

        protected List<PropertyChangedWithValsHandler> GetPropChangedWithValsHandlers([CallerMemberName] string propertyName = null)
        {
            return GetGenProp(propertyName).PropChangedWithValsHandlerList;
        }

        protected void RemoveProp(string propertyName)
        {
            IPropGen genProp = GetGenProp(propertyName);

            foreach (PropertyChangedWithValsHandler h in genProp.PropChangedWithValsHandlerList)
            {
                this.PropertyChangedWithVals -= h;
            }

            tVals.Remove(propertyName);
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

                foreach (KeyValuePair<string, IPropGen> kvp in tVals)
                {
                    result.Add(kvp.Key, kvp.Value.Value);
                }
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doWhenChanged"></param>
        /// <param name="doAfterNotify"></param>
        /// <param name="propertyName"></param>
        /// <returns>True, if there was an existing Action in place for this property.</returns>
        //protected bool RegisterDoWhenChanged<T>(Action<T, T> doWhenChanged, bool doAfterNotify = false, [CallerMemberName] string propertyName = null)
        //{
        //    IPropGen genProp = GetTypeCheckedVWT<T>(propertyName);

        //    return genProp.UpdateDoWhenChanged(doWhenChanged, doAfterNotify);
        //}

        #endregion

        #region Private Methods and Properties

        private void DoSet<T>(T newValue, string propertyName, IProp<T> prop)
        {
            bool theSame = prop.CompareTo(newValue);

            if (!theSame)
            {
                // Save the value before the update.
                T oldValue = prop.TypedValue;

                OnPropertyChanging(propertyName);

                // Make the update.
                prop.TypedValue = newValue;

                // Raise notify events.
                DoNotifyWork(oldValue, newValue, propertyName, prop);
            }
        }

        private void DoNotifyWork<T>(T oldVal, T newValue, string propertyName, IProp<T> prop)
        {
            if (prop.DoAfterNotify)
            {
                // Raise the standard PropertyChanged event
                OnPropertyChanged(propertyName);

                // The typed, PropertyChanged event defined on the individual property.
                prop.OnPropertyChangedWithTVals(propertyName, oldVal, newValue);

                // The un-typed, PropertyChanged shared event.
                OnPropertyChangedWithVals(propertyName, oldVal, newValue);

                // then perform the call back.
                prop.DoWhenChanged(oldVal, newValue);
            }
            else
            {
                // Peform the call back,
                prop.DoWhenChanged(oldVal, newValue);

                // Raise the standard PropertyChanged event
                OnPropertyChanged(propertyName);

                // The typed, PropertyChanged event defined on the individual property.
                prop.OnPropertyChangedWithTVals(propertyName, oldVal, newValue);

                // The un-typed, PropertyChanged shared event.
                OnPropertyChangedWithVals(propertyName, oldVal, newValue);
            }
        }

        private IPropGen GetGenProp(string propertyName, bool? desiredHasStoreValue = true)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName", "PropertyName is null on call to GetValue.");
           IPropGen genProp = tVals[propertyName];

            if (desiredHasStoreValue.HasValue && desiredHasStoreValue.Value != genProp.HasStore)
            {
                if (desiredHasStoreValue.Value)
                    //Caller needs property to have a backing store.
                    throw new InvalidOperationException(string.Format("Property: {0} has no backing store held by this instance of PropBag. This operation can only be performed on properties for which a backing store is held by this instance.", propertyName));
                else
                    throw new InvalidOperationException(string.Format("Property: {0} has a backing store held by this instance of PropBag. This operation can only be performed on properties for which no backing store is kept by PropBag.", propertyName));
            }

            return genProp;
        }

        private void MakeTypeSolid(ref IPropGen genProp, Type newType, string propertyName)
        {
            Type currentType = genProp.Type;

            Debug.Assert(genProp.Value == null, "The current value of the property should be null when MakeTypeSolid is called.");

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
                object curValue = genProp.Value;

                IPropGen newwGenProp = thePropFactory.Create(newType, genProp.Value, propertyName, null, true, true);

                //genProp.UpdateWithSolidType(newType, curValue);
                genProp = newwGenProp;
            }
        }

        // Its important to make sure new is new and cur is cur.
        private bool AreTypesSame(Type newType, Type curType)
        {

            if (newType == curType)
                return true;

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
                    return false;
                }
            }
        }

        private DoSetDelegate GetPropSetterDelegate(IPropGen genProp)
        {
            Type typeOfThisProperty = genProp.Type;

            if(!doSetDelegateDict.ContainsKey(typeOfThisProperty))
            {
                DoSetDelegate del = GetDoSetDelegate(typeOfThisProperty);
                doSetDelegateDict[typeOfThisProperty] = del;
                return del;
            }
            return doSetDelegateDict[typeOfThisProperty];
        }

        /// <summary>
        /// Given a string in the form "{0}Changed", where {0} is the underlying property name, parse out and return the value of {0}.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private string GetPropNameFromEventProp(string x)
        {
            //PropStringChanged
            return x.Substring(0, x.Length - 7);
        }

        #endregion

        #region Add Prop Methods

        /// <summary>
        /// Use when you want to specify an Action<typeparamref name="T"/> to be performed
        /// either before or after the PropertyChanged event has been raised.
        /// </summary>
        /// <typeparam name="T">The type of this property's value.</typeparam>
        /// <param name="propertyName"></param>
        /// <param name="doIfChanged"></param>
        /// <param name="doAfterNotify"></param>
        /// <param name="comparer">A instance of a class that implements IEqualityComparer and thus an Equals method.</param>
        /// <param name="initalValue"></param>
        public void AddProp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            IEqualityComparer<T> comparer = null, T initalValue = default(T), object extraInfo = null)
        {
            IPropGen pg = thePropFactory.Create<T>(initalValue, propertyName, extraInfo, true, true, doIfChanged, doAfterNotify, comparer);
            tVals.Add(propertyName, pg);
        }

        public void AddPropObjComp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            T initalValue = default(T), object extraInfo = null)
        {
            RefEqualityComparer<T> refComp = RefEqualityComparer<T>.Default;
            IPropGen pg = thePropFactory.Create<T>(initalValue, propertyName, extraInfo, true, true, doIfChanged, doAfterNotify, refComp);

            tVals.Add(propertyName, pg);
        }

        public void AddPropNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false,
            IEqualityComparer<T> comparer = null, object extraInfo = null)
        {
            IPropGen pg = thePropFactory.CreateWithNoneOrDefault<T>(propertyName, extraInfo, false, true, doIfChanged, doAfterNotify, comparer);
            tVals.Add(propertyName, pg);
        }

        public void AddPropNoStoreObjComp<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false, object extraInfo = null)
            
        {
            RefEqualityComparer<T> refComp = RefEqualityComparer<T>.Default;
            IPropGen pg = thePropFactory.CreateWithNoneOrDefault<T>(propertyName, extraInfo, false, true, doIfChanged, doAfterNotify, refComp);

            tVals.Add(propertyName, pg);
        }


        //// The remainder of the AddProp variants are "sugar" -- they allow for abbreviated calls to what is provided above.

        //// Just name and optional intital value.
        //public void AddProp<T>(string propertyName, T initalValue = default(T))
        //{
        //    tVals.Add(propertyName, ValueWithType.Create<T>(initalValue));
        //}

        //// Use IEqualityComparer<T> and no doIfChanged Action
        //public void AddProp<T>(string propertyName, IEqualityComparer<T> comparer, T initalValue = default(T))
        //{
        //    tVals.Add(propertyName, ValueWithType.Create<T>(initalValue, null, false, comparer));
        //}

        //// Use IEqualityComparer<object> and no doIfChanged action.
        //public void AddPropObjComp<T>(string propertyName, IEqualityComparer<object> comparer, T initalValue = default(T))
        //{
        //    tVals.Add(propertyName, ValueWithType.CreateWithObjComparer<T>(initalValue, null, false, comparer));
        //}


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
