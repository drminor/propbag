using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

    public class PropBagBase : IPropBag
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

        protected AbstractPropFactory thePropFactory { get; private set; }

        private readonly Dictionary<string, IPropGen> tVals;

        private readonly Dictionary<Type, DoSetDelegate> doSetDelegateDict;

        public PropBagTypeSafetyMode TypeSafetyMode { get; protected set; }


        /// <summary>
        /// If true, attempting to set a property for which no call to AddProp has been made, will cause an exception to thrown.
        /// </summary>
        private bool AllPropsMustBeRegistered;

        /// <summary>
        /// If not true, attempting to set a property, not previously set with a call to AddProp or SetIt<typeparamref name="T"/>, will cause an exception to be thrown.
        /// </summary>
        private bool OnlyTypedAccess;

        /// <summary>
        /// Used to create Delegates when the type of the value is not known at run time.
        /// </summary>
        static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        #endregion

        #region Constructor

        public PropBagBase() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered, null) { }

        public PropBagBase(PropBagTypeSafetyMode typeSafetyMode) : this(typeSafetyMode, null) { }

        public PropBagBase(PropBagTypeSafetyMode typeSafetyMode, AbstractPropFactory thePropFactory)
        {
            TypeSafetyMode = typeSafetyMode;
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

            // Use the "built-in" property factory, if the caller did not supply one.
            this.thePropFactory = thePropFactory ?? new PropFactory();

            tVals = new Dictionary<string, IPropGen>();
            doSetDelegateDict = new Dictionary<Type, DoSetDelegate>();
        }

        // When we are being destructed, remove all of the handlers that we provisioned.
        ~PropBagBase()
        {
            //foreach (KeyValuePair<string,IPropGen> kvp in tVals)
            //{
            //    IPropGen genProp = kvp.Value;

            //    foreach (PropertyChangedWithValsHandler h in genProp.PropChangedWithValsHandlerList)
            //    {
            //        this.PropertyChangedWithVals -= h;
            //    }
            //}

            // Maybe not necessary, but since each of these delegates refer back to this instance of PropBag,
            // it may make it a little easier on the garbage collector.
            doSetDelegateDict.Clear();

            // Really not necessary, but while were here...
            tVals.Clear();
        }

        #endregion

        #region Propety Access Methods

        protected object PGetIt(string propertyName)
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

        protected T PGetIt<T>(string propertyName)
        {
            // This will throw an exception if no value has been added to the _tVals dictionary with a key of propertyName,
            // either by calling AddProp or SetIt.
            IPropGen genProp = GetGenProp(propertyName);

            IProp<T> prop = CheckTypeInfo<T>(genProp, propertyName, tVals);

            return prop.TypedValue;
        }

        protected void PSetIt(object value, string propertyName)
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
                        throw new ApplicationException(string.Format("The property: {0} was originally set to null, now its being set to a value whose type is a value type; value types don't allow setting to null.", propertyName), ice);
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

        protected void PSetIt<T>(T value, string propertyName)
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
        protected bool PSetIt<T>(T newValue, ref T curValue, [CallerMemberName]string propertyName = null)
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
                genProp = thePropFactory.CreateWithNoValue<T>(propertyName, hasStorage: false, typeIsSolid: true);

                tVals.Add(propertyName, genProp);
            }

            IProp<T> prop = CheckTypeInfo<T>(genProp, propertyName, tVals);

            bool theSame = prop.Compare(newValue, curValue);

            if (!theSame)
            {
                // Save the value before the update.
                T oldValue = curValue;

                POnPropertyChanging(propertyName);

                // Make the update.
                curValue = newValue;

                // Raise notify events.
                DoNotifyWork<T>(oldValue, newValue, propertyName, prop);
            }

            // Return true, if the new value was found to be different than the current value.
            return !theSame;
        }

        #endregion

        #region Subscribe to Property Changed Event Helpers

        // This is used to allow the caller to get notified only when a particular property is changed with values.
        // It can be used in any of the three modes, but is especially handy for Loose mode.
        public void SubscribeToPropChanged(Action<object, object> doOnChange, string propertyName)
        {
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

                genProp = thePropFactory.CreateWithNoValue<object>(propertyName, null, true, false, null, false, null);
                tVals.Add(propertyName, genProp);
            }

            genProp.SubscribeToPropChanged(doOnChange);

        }

        public bool UnSubscribeToPropChanged(Action<object, object> doOnChange, string propertyName)
        {
            IPropGen genProp;
            try
            {
                genProp = GetGenProp(propertyName);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            return genProp.UnSubscribeToPropChanged(doOnChange);
        }

        // Allow callers to easily subscribe to PropertyChangedWithTVals.
        public void SubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName)
        {
            IProp<T> prop = GetPropDef<T>(propertyName);

            prop.SubscribeToPropChanged(doOnChange);
        }

        public bool UnSubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName)
        {
            IPropGen genProp;
            try
            {
                genProp = GetGenProp(propertyName);

            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            IProp<T> prop = (IProp<T>)genProp;

            return prop.UnSubscribeToPropChanged(doOnChange);
        }

        public void SubscribeToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, string propertyName)
        {
            IProp<T> prop = GetPropDef<T>(propertyName);

            prop.PropertyChangedWithTVals += eventHandler;
        }

        public void UnSubscribeToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, string propertyName)
        {
            IPropGen genProp = GetGenProp(propertyName);

            IProp<T> prop = CheckTypeInfo<T>(genProp, propertyName, tVals);

            prop.PropertyChangedWithTVals -= eventHandler;
        }

        /// <summary>
        /// Uses the name of the property or event accessor of the calling method to indentify the property,
        /// if the propertyName argument is not specifed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="eventPropertyName"></param>
        public void AddToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, [CallerMemberName] string eventPropertyName = null)
        {
            string propertyName = GetPropNameFromEventProp(eventPropertyName);
            SubscribeToPropChanged<T>(eventHandler, propertyName);
        }

        /// <summary>
        /// Uses the name of the property or event accessor of the calling method to indentify the property,
        /// if the propertyName argument is not specifed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="eventPropertyName"></param>
        public void RemoveFromPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, [CallerMemberName] string eventPropertyName = null)
        {
            string propertyName = GetPropNameFromEventProp(eventPropertyName);
            UnSubscribeToPropChanged<T>(eventHandler, propertyName);
        }

        #endregion

        #region Public Methods

        public bool PropertyExists([CallerMemberName] string propertyName = null)
        {
            return tVals.ContainsKey(propertyName);
        }

        public System.Type GetTypeOfProperty([CallerMemberName] string propertyName = null)
        {
            return GetGenProp(propertyName).Type;
        }

        #endregion

        #region Property Management

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
        protected IProp<T> PAddProp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            IEqualityComparer<T> comparer = null, object extraInfo = null, T initalValue = default(T))
        {
            IProp<T> pg = thePropFactory.Create<T>(initalValue, propertyName, extraInfo, true, true, doIfChanged, doAfterNotify, comparer);
            tVals.Add(propertyName, pg);

            return pg;
        }

        protected IProp<T> PAddPropObjComp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            object extraInfo = null, T initalValue = default(T))
        {
            RefEqualityComparer<T> refComp = RefEqualityComparer<T>.Default;

            IProp<T> pg = thePropFactory.Create<T>(initalValue, propertyName, extraInfo, true, true, doIfChanged, doAfterNotify, refComp);
            tVals.Add(propertyName, pg);

            return pg;
        }

        protected IProp<T> PAddPropNoValue<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            IEqualityComparer<T> comparer = null, object extraInfo = null)
        {
            IProp<T> pg = thePropFactory.CreateWithNoValue<T>(propertyName, extraInfo, true, true, doIfChanged, doAfterNotify, comparer);
            tVals.Add(propertyName, pg);

            return pg;
        }

        protected IProp<T> PAddPropObjCompNoValue<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            object extraInfo = null)
        {
            RefEqualityComparer<T> refComp = RefEqualityComparer<T>.Default;

            IProp<T> pg = thePropFactory.CreateWithNoValue<T>(propertyName, extraInfo, true, true, doIfChanged, doAfterNotify, refComp);
            tVals.Add(propertyName, pg);

            return pg;
        }

        protected IProp<T> PAddPropNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false,
            IEqualityComparer<T> comparer = null, object extraInfo = null)
        {
            IProp<T> pg = thePropFactory.CreateWithNoValue<T>(propertyName, extraInfo, false, true, doIfChanged, doAfterNotify, comparer);
            tVals.Add(propertyName, pg);

            return pg;
        }

        protected IProp<T> PAddPropObjCompNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false, object extraInfo = null)
        {
            RefEqualityComparer<T> refComp = RefEqualityComparer<T>.Default;

            IProp<T> pg = thePropFactory.CreateWithNoValue<T>(propertyName, extraInfo, false, true, doIfChanged, doAfterNotify, refComp);
            tVals.Add(propertyName, pg);

            return pg;
        }

        protected void PRemoveProp(string propertyName)
        {
            //IPropGen genProp = GetGenProp(propertyName);

            //foreach (PropertyChangedWithValsHandler h in genProp.PropChangedWithValsHandlerList)
            //{
            //    this.PropertyChangedWithVals -= h;
            //}

            tVals.Remove(propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doWhenChanged"></param>
        /// <param name="doAfterNotify"></param>
        /// <param name="propertyName"></param>
        /// <returns>True, if there was an existing Action in place for this property.</returns>
        protected bool PRegisterDoWhenChanged<T>(Action<T, T> doWhenChanged, bool doAfterNotify, string propertyName)
        {
            IProp<T> prop = GetPropDef<T>(propertyName);
            return prop.UpdateDoWhenChangedAction(doWhenChanged, doAfterNotify);
        }

        //protected List<PropertyChangedWithValsHandler> GetPropChangedWithValsHandlers(string propertyName)
        //{
        //    return GetGenProp(propertyName).PropChangedWithValsHandlerList;
        //}

        /// <summary>
        /// Returns all of the values in dictionary of objects, keyed by PropertyName.
        /// </summary>
        protected IDictionary<string, object> PGetAllPropertyValues()
        {
            // This uses reflection.
            Dictionary<string, object> result = new Dictionary<string, object>();

            foreach (KeyValuePair<string, IPropGen> kvp in tVals)
            {
                result.Add(kvp.Key, kvp.Value.Value);
            }
            return result;
        }

        protected IList<string> PGetAllPropertyNames()
        {
            List<string> result = new List<string>();

            foreach (var y in tVals.Keys)
            {
                result.Add(y);
            }
            return result;
        }

        #endregion

        #region Private Methods and Properties

        private void DoSet<T>(T newValue, string propertyName, IProp<T> prop)
        {
            if (!prop.ValueIsDefined)
            {
                // Update and only raise the standard OnPropertyChanged
                // Since there's no way to pass an undefined value to the other OnPropertyChanged event subscribers.
                prop.TypedValue = newValue;

                // Raise the standard PropertyChanged event
                POnPropertyChanged(propertyName);
            }
            else
            {
                bool theSame = prop.CompareTo(newValue);

                if (!theSame)
                {
                    // Save the value before the update.
                    T oldValue = prop.TypedValue;

                    POnPropertyChanging(propertyName);

                    // Make the update.
                    prop.TypedValue = newValue;

                    // Raise notify events.
                    DoNotifyWork(oldValue, newValue, propertyName, prop);
                }
            }
        }

        private void DoNotifyWork<T>(T oldVal, T newValue, string propertyName, IProp<T> prop)
        {
            if (prop.DoAfterNotify)
            {
                // Raise the standard PropertyChanged event
                POnPropertyChanged(propertyName);

                // The typed, PropertyChanged event defined on the individual property.
                prop.OnPropertyChangedWithTVals(propertyName, oldVal, newValue);

                // The un-typed, PropertyChanged event defined on the individual property.
                prop.OnPropertyChangedWithVals(propertyName, oldVal, newValue);

                // The un-typed, PropertyChanged shared event.
                POnPropertyChangedWithVals(propertyName, oldVal, newValue);

                // then perform the call back.
                prop.DoWhenChanged(oldVal, newValue);
            }
            else
            {
                // Peform the call back,
                prop.DoWhenChanged(oldVal, newValue);

                // Raise the standard PropertyChanged event
                POnPropertyChanged(propertyName);

                // The typed, PropertyChanged event defined on the individual property.
                prop.OnPropertyChangedWithTVals(propertyName, oldVal, newValue);

                // The un-typed, PropertyChanged event defined on the individual property.
                prop.OnPropertyChangedWithVals(propertyName, oldVal, newValue);

                // The un-typed, PropertyChanged shared event.
                POnPropertyChangedWithVals(propertyName, oldVal, newValue);
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

        /// <summary>
        /// This should only be called from methods that do not include a new or initial value for the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="genProp"></param>
        /// <returns></returns>
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

                genProp = thePropFactory.CreateWithNoValue<T>(propertyName);
                tVals.Add(propertyName, genProp);
            }

            IProp<T> prop = CheckTypeInfo<T>(genProp, propertyName, tVals);

            return prop;
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

            if (!doSetDelegateDict.ContainsKey(typeOfThisProperty))
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

        #region Methods to Raise Events

        // Raise Standard Events
        protected void POnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChanged, null, null);

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void POnPropertyChanging([System.Runtime.CompilerServices.CallerMemberName]string propertyName = null)
        {
            PropertyChangingEventHandler handler = Interlocked.CompareExchange(ref PropertyChanging, null, null);

            if (handler != null)
                handler(this, new PropertyChangingEventArgs(propertyName));
        }

        protected void POnPropertyChangedWithVals(string propertyName, object oldVal, object newVal)
        {
            PropertyChangedWithValsHandler handler = Interlocked.CompareExchange(ref PropertyChangedWithVals, null, null);

            if (handler != null)
                handler(this, new PropertyChangedWithValsEventArgs(propertyName, oldVal, newVal));
        }

        #endregion

        #region Generic Method Support

        private delegate void DoSetDelegate(object value, PropBagBase target, string propertyName, object prop);

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
            private static void DoSetBridge<T>(object value, PropBagBase target, string propertyName, object prop)
            {
                target.DoSet<T>((T)value, propertyName, (IProp<T>)prop);
            }
        }

        #endregion

    }
}
