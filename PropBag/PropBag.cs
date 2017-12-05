using DRM.PropBag.Caches;
using DRM.PropBag.ControlModel;

using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace DRM.PropBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using L2KeyManType = IL2KeyMan<UInt32, String>;

    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    using SubCacheType = ICacheSubscriptions<UInt32>;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;

    using ObjectIdType = UInt64;

    #region Summary and Remarks

    /// <summary>
    /// The contents of this code file were designed and created by David R. Minor, Pittsboro, NC.
    /// I have chosen to provide others free access to this intellectual property using the terms set forth
    /// by the well known Code Project Open License.
    /// Please refer to the file in this same folder named CPOP.htm for the exact set of terms that govern this release.
    /// Although not included as a condition of use, I would prefer that this text, 
    /// or a similar text which covers all of the points made here, be included along with a copy of cpol.htm
    /// in the set of artifacts deployed with any product
    /// wherein this source code, or a derivative thereof, is used.
    /// </summary>

    /// <remarks>
    /// While writing this code, I learned much and was guided by the material found at the following locations.
    /// http://northhorizon.net/2011/the-right-way-to-do-inotifypropertychanged/ (Daniel Moore)
    /// https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/ (Jon Skeet)
    /// </remarks>

    #endregion

    public partial class PropBag : IPropBag, IPropBagInternal
    {
        #region Member Declarations

        // These items are provided to us.
        private readonly IPropFactory _propFactory;
        private readonly PSAccessServiceType _ourStoreAccessor;

        // We are responsible for these
        protected virtual ITypeSafePropBagMetaData OurMetaData { get; }
        private readonly PropBagTypeSafetyMode _typeSafetyMode;
        private readonly L2KeyManType _level2KeyManager;
        private object _sync;
        private readonly PB_EventHolder _eventHolder;
        //private readonly bool _notifyWhenOldIsUndefined;

        // These fulfill the IPropBagInternal contract
        PSAccessServiceType IPropBagInternal.ItsStoreAccessor => _ourStoreAccessor;
        L2KeyManType IPropBagInternal.Level2KeyManager => _level2KeyManager;

        #endregion

        #region Public Events and Properties

        public string FullClassName => OurMetaData.FullClassName;

        public event PropertyChangedEventHandler PropertyChanged; // = delegate { };
        public event PropertyChangingEventHandler PropertyChanging; // = delegate { };

        public event EventHandler<PCGenEventArgs> PropertyChangedWithGenVals
        {
            add
            {
                lock(_sync)
                {
                    WeakEventManager<PB_EventHolder, PCGenEventArgs>.AddHandler(_eventHolder, "PropertyChangedWithGenVals", value);
                }
            }
            remove
            {
                lock (_sync)
                {
                    WeakEventManager<PB_EventHolder, PCGenEventArgs>.RemoveHandler(_eventHolder, "PropertyChangedWithGenVals", value);
                }
            }
        }
            
        public event EventHandler<PCObjectEventArgs> PropertyChangedWithObjectVals;

        #endregion

        #region Constructor

        ///// <summary>
        ///// This is constructor creates an instance with minimal resources, and is not operational.
        ///// It can be called in those cases where an instance is required as a target to use when calling
        ///// a method from the System.Reflection namespace.
        ///// </summary>
        ///// <param name="dummy"></param>
        //public PropBag(byte dummy)
        //{
        //    _typeSafetyMode = PropBagTypeSafetyMode.None;
        //    _propFactory = null;
        //    _ourStoreAccessor = null;
        //    OurMetaData = BuildMetaData(this._typeSafetyMode, classFullName: null, propFactory: null);
        //    _eventHolder = null;
        //}

        protected PropBag()
            : this(PropBagTypeSafetyMode.None, null, null) { }

        protected PropBag(PropBagTypeSafetyMode typeSafetyMode)
            : this(typeSafetyMode, null, null) { }

        // TODO: remove this constructor.
        protected PropBag(PropBagTypeSafetyMode typeSafetyMode, IPropFactory propFactory)
            : this(typeSafetyMode, propFactory, null) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pm"></param>
        /// <param name="propFactory">The PropFactory to use instead of the one specified by the PropModel.</param>
        public PropBag(ControlModel.PropModel pm, string fullClassName = null, IPropFactory propFactory = null)
            : this(pm.TypeSafetyMode, propFactory ?? pm.PropFactory, fullClassName)
        {
            Hydrate(pm);
        }

        protected PropBag(PropBagTypeSafetyMode typeSafetyMode, IPropFactory propFactory, string fullClassName = null)
        {
            _typeSafetyMode = typeSafetyMode;
            _propFactory = propFactory ?? throw new ArgumentNullException(nameof(propFactory));

            OurMetaData = BuildMetaData(_typeSafetyMode, fullClassName, _propFactory);

            _ourStoreAccessor = _propFactory.PropStoreAccessServiceProvider.CreatePropStoreService(this);
            _level2KeyManager = _ourStoreAccessor.Level2KeyManager;

            _sync = new object();
            _eventHolder = new PB_EventHolder();

            //_notifyWhenOldIsUndefined = false;
        }

        protected TypeSafePropBagMetaData BuildMetaData(PropBagTypeSafetyMode typeSafetyMode, string classFullName, IPropFactory propFactory)
        {
            classFullName = classFullName ?? GetFullTypeNameOfThisInstance();
            TypeSafePropBagMetaData result = new TypeSafePropBagMetaData(classFullName, typeSafetyMode, propFactory);
            return result;
        }

        protected string GetFullTypeNameOfThisInstance()
        {
            string fullClassName = this.GetType().FullName; // this.GetType.GetTypeInfo().FullName;
            return fullClassName;
        }

        private string GetClassNameOfThisInstance()
        {
            string className = this.GetType().Name; //this.GetType().GetTypeInfo().Name;
            return className;
        }

        protected void Hydrate(PropModel pm)
        {
            string cName = GetClassNameOfThisInstance();
            string pCName = pm.ClassName;

            if (cName != pCName)
            {
                System.Diagnostics.Debug.WriteLine($"CLR class name: {cName} does not match PropModel class name: {pCName}.");
            }

            foreach (DRM.PropBag.ControlModel.PropItem pi in pm.Props)
            {
                if(pi.PropertyName == "PersonList")
                {
                    System.Diagnostics.Debug.WriteLine("Here we are.");
                }
                object ei = pi.ExtraInfo;

                Delegate comparer;
                bool useRefEquality;

                if (pi.ComparerField == null)
                {
                    comparer = null;
                    useRefEquality = false;
                }
                else
                {
                    comparer = pi.ComparerField.Comparer;
                    useRefEquality = pi.ComparerField.UseRefEquality;
                }

                if (pi.InitialValueField == null)
                {
                    pi.InitialValueField = PropInitialValueField.UndefinedInitialValueField;
                }

                EventHandler<PCGenEventArgs> doWhenChangedAction = pi.DoWhenChangedField?.DoWhenChangedAction;

                if(doWhenChangedAction == null && (pi?.DoWhenChangedField?.DoWhenGenHandlerGetter != null))
                {
                    Func<object, EventHandler<PCGenEventArgs>> rr = pi.DoWhenChangedField.DoWhenGenHandlerGetter;

                    doWhenChangedAction = rr(this);
                }

                IProp pg;

                if (pi.HasStore && !pi.InitialValueField.SetToUndefined)
                {
                    bool useDefault = pi.InitialValueField.SetToDefault;
                    string value;

                    // TODO: Fix this??
                    if (pi.InitialValueField.SetToEmptyString && pi.PropertyType == typeof(Guid))
                    {
                        const string EMPTY_GUID = "00000000-0000-0000-0000-000000000000";
                        value = EMPTY_GUID;
                    }
                    else
                    {
                        value = pi.InitialValueField.GetStringValue();
                    }

                    pg = _propFactory.CreateGenFromString(pi.PropertyType, value, useDefault, pi.PropertyName, ei, pi.HasStore, pi.TypeIsSolid,
                        pi.PropKind, doWhenChangedAction, pi.DoWhenChangedField.DoAfterNotify, comparer, useRefEquality, pi.ItemType);
                }
                else
                {
                    pg = _propFactory.CreateGenWithNoValue(pi.PropertyType, pi.PropertyName, ei, pi.HasStore, pi.TypeIsSolid,
                        pi.PropKind, doWhenChangedAction, pi.DoWhenChangedField.DoAfterNotify, comparer, useRefEquality, pi.ItemType);
                }
                AddProp(pi.PropertyName, pg);
            }
        }

        #endregion

        #region Missing Prop Handler

        // This will always return a valid value -- or throw an exception,
        // unless neverCreate is set, in which case it will
        // never throw an exception and always return an empty PropGen.
        private IPropData HandleMissingProp(PropIdType propId, PropNameType propertyName, Type propertyType, out bool wasRegistered,
            bool haveValue, object value, bool alwaysRegister, bool mustBeRegistered, bool neverCreate)
        {
            ReadMissingPropPolicyEnum thePolicyToUse; //= alwaysRegister ? ReadMissingPropPolicyEnum.Register : ReadMissingPropPolicy;

            // If always register (because we are being called from a set operation,
            // then override the ReadMissingPolicy in place (used for get operations.)

            // If never create (because we are being called from a TryGet or a GetType operation,
            // then override the ReadMissingPolicy in place (used for get operations.)

            Debug.Assert(!(alwaysRegister && mustBeRegistered), "AlwaysRegister and MustBeRegistered are both true, here at HandleMissingProp.");
            Debug.Assert(!(alwaysRegister && neverCreate), "AlwaysRegister and NeverCreate cannot both be true, here at HandleMissingProp.");

            if (neverCreate)
            {
                wasRegistered = false;
                return new PropGen();
            }

            if (alwaysRegister)
            {
                thePolicyToUse = ReadMissingPropPolicyEnum.Register;
            }
            else
            {
                // Use the one in place.
                thePolicyToUse = OurMetaData.ReadMissingPropPolicy;
            }

            switch (thePolicyToUse)
            {
                case ReadMissingPropPolicyEnum.Allowed:
                    {
                        if (mustBeRegistered)
                        {
                            goto case ReadMissingPropPolicyEnum.NotAllowed;
                        }
                        else
                        {
                            wasRegistered = false;
                            return new PropGen();
                        }
                    }
                case ReadMissingPropPolicyEnum.Register:
                    {
                        // TODO: Must determine the PropKind for this new property.
                        IProp genericTypedProp;

                        if (haveValue)
                        {
                            bool typeIsSolid = _propFactory.IsTypeSolid(value, propertyType);


                            genericTypedProp = _propFactory.CreateGenFromObject(propertyType, value,
                                propertyName, null, _propFactory.ProvidesStorage, typeIsSolid, PropKindEnum.Prop, null, false, null);
                        }
                        else
                        {
                            // TODO: This must set the value of the property to the default for its type,
                            // in order to satisfy TypeSafety modes: 'RegisterOnGetLoose' and 'RegisterOnGetSafe.'

                            //object newValue = ThePropFactory.GetDefaultValue(propertyType, propertyName);
                            bool typeIsSolid = true;

                            // On 10/8/17: Changed to use NoValue, instead of trying to generate a default value.
                            genericTypedProp = _propFactory.CreateGenWithNoValue(propertyType, propertyName,
                                null, _propFactory.ProvidesStorage, typeIsSolid, PropKindEnum.Prop, null, false, null);
                        }

                        IPropData propGen = AddProp(propertyName, genericTypedProp);

                        wasRegistered = true;
                        return propGen;
                    }
                case ReadMissingPropPolicyEnum.NotAllowed:
                    {
                        if (OurMetaData.AllPropsMustBeRegistered)
                        {
                            throw new InvalidOperationException(string.Format("Property: {0} has not been declared by calling AddProp. Cannot use this method in this case. Declare by calling AddProp.", propertyName));
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format("No property: {0} exists in this PropBag.", propertyName));
                        }
                    }
                default:
                    {
                        // TODO: create custom exception for this.
                        throw new InvalidOperationException(string.Format("Unrecognized value: {0} for ReadMissingPropPolicyEnum found hile accessing property: {1}.", OurMetaData.ReadMissingPropPolicy.ToString(), propertyName));
                    }
            }
        }

        #endregion

        #region Property Access Methods

        //// Just for testing??
        //protected ObjectIdType GetParentObjectId()
        //{
        //    ObjectIdType result =  OurStoreAccessor.GetParentObjectId(this);
        //    return result;
        //}

        public object this[string typeName, string propertyName]
        {
            get
            {
                Type propertyType = _propFactory.TypeResolver(typeName);
                return GetValWithType(propertyName, propertyType);
            }
            set
            {
                Type propertyType = _propFactory.TypeResolver(typeName);
                SetValWithType(propertyName, propertyType, value);
            }
        }

        public object this[Type propertyType, string propertyName]
        {
            get
            {
                return GetValWithType(propertyName, propertyType);
            }
            set
            {
                SetValWithType(propertyName, propertyType, value);
            }
        }

        private bool ReportNonTypedAccess(string propertyName, string methodName)
        {
            string typeRequiredWhenAccessing = "'OnlyTypedAccess', 'Tight', or 'RegisterOnGetSafe'";
            string msg = $"Attempt to access property {propertyName} via call to {methodName} is not allowed when TypeSafetyMode is one of {typeRequiredWhenAccessing}.";
            throw new InvalidOperationException(msg);
        }

        private bool ReportAccessToMissing(string propertyName, string methodName)
        {
            string accessNotAllowedModes = "'Tight', 'AllPropsMustBeRegistered', or 'OnlyTypedAccess'";
            string msg = $"Attempt to access property {propertyName} via call to {methodName} is not allowed when TypeSafetyMode is one of {accessNotAllowedModes}.";
            throw new InvalidOperationException(msg);
        }

        public bool TryGetPropGen(string propertyName, Type propertyType, out IPropData propGen)
        {
            bool mustBeRegistered = _typeSafetyMode == PropBagTypeSafetyMode.Locked;

            propGen = GetPropGen(propertyName, propertyType, out bool wasRegistered, out PropIdType propId,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: true,
                desiredHasStoreValue: _propFactory.ProvidesStorage);

            if(!propGen.IsEmpty)
            {
                return true;
            }
            else if(_typeSafetyMode == PropBagTypeSafetyMode.Tight)
            {
                return ReportAccessToMissing(propertyName, nameof(TryGetPropGen));
            }
            else
            {
                return false;
            }
        }

        // Public wrapper aroud GetPropGen
        public IPropData GetPropGen(string propertyName, Type propertyType = null)
        {
            if (propertyType == null && OurMetaData.OnlyTypedAccess)
            {
                ReportNonTypedAccess(propertyName, nameof(GetPropGen));
            }

            IPropData PropData = GetPropGen(propertyName, propertyType, out bool wasRegistered, out PropIdType propId,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: true,
                neverCreate: false,
                desiredHasStoreValue: _propFactory.ProvidesStorage);

            if (!wasRegistered)
            {
                if (propertyType != null)
                {
                    if (!PropData.TypedProp.TypeIsSolid)
                    {
                        MakeTypeSolid(propId, propertyName, PropData, propertyType);
                    }
                    else
                    {
                        if (propertyType != PropData.TypedProp.Type)
                        {
                            throw new InvalidOperationException(string.Format("Attempting to get property: {0} whose type is {1}, with a call whose type parameter is {2} is invalid.", propertyName, PropData.TypedProp.Type.ToString(), propertyType.ToString()));
                        }
                    }
                }
            }

            return PropData;
        }

        public object GetValWithType(string propertyName, Type propertyType)
        {
            IPropData pg = (IPropData)GetPropGen(propertyName, propertyType);
            return pg.TypedProp.TypedValueAsObject;
        }

        public ValPlusType GetValPlusType(string propertyName, Type propertyType)
        {
            IPropData pg = GetPropGen(propertyName, propertyType);
            return pg.ValuePlusType();
        }

        public T GetIt<T>(string propertyName)
        {
            //OurStoreAccessor.IncAccess();
            return GetTypedPropPrivate<T>(propertyName, mustBeRegistered: true).TypedValue;
        }

        public IProp<T> GetTypedProp<T>(string propertyName)
        {
            return (IProp<T>)GetTypedPropPrivate<T>(propertyName, mustBeRegistered: true);
        }

        private IPropPrivate<T> GetTypedPropPrivate<T>(string propertyName, bool mustBeRegistered, bool neverCreate = false)
        {
            IPropData PropData = GetGenPropPrivate<T>(propertyName, mustBeRegistered, neverCreate);

            if (!PropData.IsEmpty)
            {
                return (IPropPrivate<T>)PropData.TypedProp;
            }
            else
            {
                return null;
            }
        }

        private IPropData GetGenPropPrivate<T>(string propertyName, bool mustBeRegistered, bool neverCreate = false)
        {
            bool hasStore = _propFactory.ProvidesStorage;

            // TODO: Make this use a different version of GetPropGen: one that takes advantage of the 
            // compile-time type knowlege -- especially if we have to register the property in HandleMissing.

            IPropData PropData = GetPropGen(propertyName, typeof(T), out bool wasRegistered, out PropIdType propId,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: neverCreate,
                desiredHasStoreValue: _propFactory.ProvidesStorage);

            if (wasRegistered)
                return PropData;
            else
            {
                if (!PropData.IsEmpty)
                {
                    CheckTypeInfo<T>(propId, propertyName, PropData, _ourStoreAccessor);
                }
                return PropData;
            }
        }

        public bool SetValWithNoType(string propertyName, object value)
        {
            if (OurMetaData.OnlyTypedAccess)
            {
                ReportNonTypedAccess(propertyName, nameof(SetValWithNoType));
            }
            return SetValWithType(propertyName, null, value);
        }

        //CHK: PropertyType may be null.

        /// <summary>
        /// Set's the value of the property with optional type information.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyType">If unknown, set this parameter to null.</param>
        /// <param name="value"></param>
        public bool SetValWithType(string propertyName, Type propertyType, object value)
        {
            //CHK: PropertyType may be null.
            if (propertyType == null && OurMetaData.OnlyTypedAccess)
            {
                ReportNonTypedAccess(propertyName, nameof(SetValWithType));
            }

            // For Set operations where a type is given, 
            // Register the property if it does not exist, unless the TypeSafetyMode
            // setting is AllPropsMustBe (explictly) registered.
            bool alwaysRegister = !OurMetaData.AllPropsMustBeRegistered;
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData PropData = GetPropGen(propertyName, propertyType, out bool wasRegistered, out PropIdType propId,
                    haveValue: true,
                    value: value,
                    alwaysRegister: alwaysRegister,
                    mustBeRegistered: mustBeRegistered,
                    neverCreate: false,
                    desiredHasStoreValue: _propFactory.ProvidesStorage);

            // No point in calling DoSet, it would find that the value is the same and do nothing.
            if (wasRegistered) return true;

            if (value != null)
            {
                if (!PropData.TypedProp.TypeIsSolid)
                {
                    try
                    {
                        Type newType;
                        if (propertyType != null)
                        {
                            // Use the type provided by the caller.
                            newType = propertyType;
                        }
                        else
                        {
                            newType = _propFactory.GetTypeFromValue(value);
                        }

                        MakeTypeSolid(propId, propertyName, PropData, newType);
                    }
                    catch (InvalidCastException ice)
                    {
                        throw new ApplicationException(string.Format("The property: {0} was originally set to null, now its being set to a value whose type is a value type; value types don't allow setting to null.", propertyName), ice);
                    }
                }
                else
                {
                    Type newType;
                    if (propertyType != null)
                    {
                        // Use the type provided by the caller.
                        newType = propertyType;
                    }
                    else
                    {
                        newType = _propFactory.GetTypeFromValue(value);
                    }

                    if (!AreTypesSame(newType, PropData.TypedProp.Type))
                    {
                        throw new ApplicationException(string.Format("Attempting to set property: {0} whose type is {1}, with a call whose type parameter is {2} is invalid.", propertyName, PropData.TypedProp.Type.ToString(), newType.ToString()));
                    }
                }
            }
            else
            {
                // Check to make sure that we are not attempting to set the value of a ValueType to null.
                if (PropData.TypedProp.TypeIsSolid && PropData.TypedProp.Type.IsValueType)
                {
                    throw new InvalidOperationException(string.Format("Cannot set property: {0} to null, it is a value type.", propertyName));
                }
            }

            DelegateCache<DoSetDelegate> dc = ((PropFactory)_propFactory).DelegateCacheProvider.DoSetDelegateCache;
            DoSetDelegate setPropDel = dc.GetOrAdd(PropData.TypedProp.Type);
            return setPropDel(this, propId, propertyName, PropData.TypedProp, value);
        }

        //public bool SetIt<T>(T value, PropIdType propId)
        //{
        //    PropNameType propertyName = GetPropName(propId); 
        //    IPropData PropData = GetPropGen<T>(propertyName, out PropIdType dummy, desiredHasStoreValue: PropFactory.ProvidesStorage);

        //    IPropPrivate<T> prop = CheckTypeInfo<T>(propId, propertyName, PropData, OurStoreAccessor);

        //    return DoSet(propId, propertyName, prop, value);
        //}

        public bool SetIt<T>(T value, string propertyName)
        {
            // For Set operations where a type is given, 
            // Register the property if it does not exist, unless the TypeSafetyMode
            // setting is AllPropsMustBe (explictly) registered.
            bool alwaysRegister = !OurMetaData.AllPropsMustBeRegistered;
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            // TODO: Create a GetPropGen<T> that uses the _theStore.
            IPropData PropData = GetPropGen(propertyName, typeof(T), out bool wasRegistered, out PropIdType propId,
                haveValue: true,
                value: value,
                alwaysRegister: alwaysRegister,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: _propFactory.ProvidesStorage);

            // No point in calling DoSet, it would find that the value is the same and do nothing.
            if (wasRegistered) return true;

            IPropPrivate<T> prop = CheckTypeInfo<T>(propId, propertyName, PropData, _ourStoreAccessor);
            _ourStoreAccessor.IncAccess();

            T curVal = PropData.TypedProp.ValueIsDefined ? (T) PropData.TypedProp.TypedValueAsObject : default(T);
            bool result = DoSet(propId, propertyName, prop, ref curVal, value);

            return result;
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
        public bool SetIt<T>(T newValue, ref T curValue, string propertyName)
        {
            // For Set operations where a type is given, 
            // Register the property if it does not exist, unless the TypeSafetyMode
            // setting is AllPropsMustBe (explictly) registered.
            bool alwaysRegister = !OurMetaData.AllPropsMustBeRegistered;
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData PropData = GetPropGen(propertyName, typeof(T), out bool wasRegistered, out PropIdType propId,
                    haveValue: true,
                    value: newValue,
                    alwaysRegister: alwaysRegister,
                    mustBeRegistered: mustBeRegistered,
                    neverCreate: false,
                    desiredHasStoreValue: false);

            // No point in calling DoSet, it would find that the value is the same and do nothing.
            if (wasRegistered) return true;

            IPropPrivate<T> typedProp = CheckTypeInfo<T>(propId, propertyName, PropData, _ourStoreAccessor);

            //DoSet<T>(propId, propertyName, typedProp, newValue);

            //bool theSame = typedProp.Compare(newValue, curValue);

            bool result = DoSet<T>(propId, propertyName, typedProp, ref curValue, newValue);

            return result;

            //if (!theSame)
            //{
            //    // Save the value before the update.
            //    T oldValue = curValue;

            //    OnPropertyChanging(PropFactory.IndexerName);

            //    // Make the update.
            //    curValue = newValue;

            //    // Raise notify events.
            //    DoNotifyWork<T>(propId, propertyName, typedProp, oldVal: oldValue, newValue: newValue);
            //}

            //// Return true, if the new value was found to be different than the current value.
            //return !theSame;
        }

        #endregion

        #region Event Subscriptions

        #region Subscribe to Property Changed

        public bool SubscribeToPropChanging(EventHandler<PropertyChangingEventArgs> handler,
            string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;
            IPropData PropData = GetPropGen(propertyName, propertyType, out bool wasRegistered, out PropIdType propId,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: _propFactory.ProvidesStorage);

            if (!PropData.IsEmpty)
            {
                WeakEventManager<INotifyPropertyChanging, PropertyChangingEventArgs>.AddHandler(this, "PropertyChanging", handler);
                return true;
            }
            return false;
        }

        public bool UnsubscribeToPropChanging(EventHandler<PropertyChangingEventArgs> handler,
            string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;
            IPropData PropData = GetPropGen(propertyName, propertyType, out bool wasRegistered, out PropIdType propId,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: _propFactory.ProvidesStorage);

            if (!PropData.IsEmpty)
            {
                WeakEventManager<INotifyPropertyChanging, PropertyChangingEventArgs>.RemoveHandler(this, "PropertyChanging", handler);
                return true;
            }
            return false;
        }

        #endregion

        #region Subscribe to Property Changed

        public bool SubscribeToPropChanged(EventHandler<PropertyChangedEventArgs> handler,
            string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;
            IPropData PropData = GetPropGen(propertyName, propertyType, out bool wasRegistered, out PropIdType propId,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: _propFactory.ProvidesStorage);

            if (!PropData.IsEmpty)
            {
                PropertyChangedEventManager.AddHandler(this, handler, propertyName);
                return true;
            }
            return false;
        }

        public bool UnsubscribeToPropChanged(EventHandler<PropertyChangedEventArgs> handler,
            string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;
            IPropData PropData = GetPropGen(propertyName, propertyType, out bool wasRegistered, out PropIdType propId,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: _propFactory.ProvidesStorage);

            if (!PropData.IsEmpty)
            {
                PropertyChangedEventManager.RemoveHandler(this, handler, propertyName);
                return true;
            }
            return false;
        }

        #endregion

        #region Subscribe to Typed PropertyChanged

        public bool SubscribeToPropChanged<T>(EventHandler<PCTypedEventArgs<T>> eventHandler, string propertyName)
        {
            IPropData PropData = GetPropGen<T>(propertyName, out PropIdType propId, desiredHasStoreValue: _propFactory.ProvidesStorage);
            IPropPrivate<T> prop = CheckTypeInfo<T>(propId, propertyName, PropData, _ourStoreAccessor);

            if (prop != null)
            {
                bool result = _ourStoreAccessor.RegisterHandler<T>(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
                return result;
            }
            else
            {
                return false;
            }
        }

        public bool UnSubscribeToPropChanged<T>(EventHandler<PCTypedEventArgs<T>> eventHandler, string propertyName)
        {
            IPropData PropData = GetPropGen<T>(propertyName, out PropIdType propId, desiredHasStoreValue: _propFactory.ProvidesStorage);

            IPropPrivate<T> prop = CheckTypeInfo<T>(propId, propertyName, PropData, _ourStoreAccessor);

            if (prop != null)
            {
                bool result = _ourStoreAccessor.UnRegisterHandler<T>(this, propId, eventHandler);
                return result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Uses the name of the property or event accessor of the calling method to indentify the property,
        /// if the propertyName argument is not specifed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="eventPropertyName"></param>
        protected bool AddToPropChanged<T>(EventHandler<PCTypedEventArgs<T>> eventHandler, string eventPropertyName)
        {
            string propertyName = GetPropNameFromEventProp(eventPropertyName);
            return SubscribeToPropChanged<T>(eventHandler, propertyName);
        }

        /// <summary>
        /// Uses the name of the property or event accessor of the calling method to indentify the property,
        /// if the propertyName argument is not specifed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="eventPropertyName"></param>
        protected bool RemoveFromPropChanged<T>(EventHandler<PCTypedEventArgs<T>> eventHandler, string eventPropertyName)
        {
            string propertyName = GetPropNameFromEventProp(eventPropertyName);
            return UnSubscribeToPropChanged<T>(eventHandler, propertyName);
        }

        #endregion

        #region Register / Unregister Action<T,T> callbacks.

        //public bool SubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName)
        //{
        //    bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

        //    IProp<T> prop = GetTypedPropPrivate<T>(propertyName, mustBeRegistered: mustBeRegistered, neverCreate: false);

        //    if (prop != null)
        //    {
        //        prop.SubscribeToPropChanged(doOnChange);
        //        return true;
        //    }
        //    return false;

        //}

        //public bool UnSubscribeToPropChanged<T>(Action<T, T> doOnChange, string propertyName)
        //{
        //    bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

        //    IProp<T> prop = GetTypedPropPrivate<T>(propertyName, mustBeRegistered: mustBeRegistered, neverCreate: true);

        //    if (prop != null)
        //        return prop.UnSubscribeToPropChanged(doOnChange);
        //    else
        //        return false;
        //}

        #endregion

        #region Subscribe to Gen PropertyChanged

        public bool SubscribeToPropChanged(EventHandler<PCGenEventArgs> eventHandler, string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData PropData = GetPropGen(propertyName, null, out bool wasRegistered, out PropIdType propId,
                    haveValue: false,
                    value: null,
                    alwaysRegister: false,
                    mustBeRegistered: mustBeRegistered,
                    neverCreate: false,
                    desiredHasStoreValue: _propFactory.ProvidesStorage);

            if (!PropData.IsEmpty)
            {
                bool result = _ourStoreAccessor.RegisterHandler(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
                return result;
            }
            else
            {
                return false;
            }
        }

        public bool UnSubscribeToPropChanged(EventHandler<PCGenEventArgs> eventHandler, string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData PropData = GetPropGen(propertyName, null, out bool wasRegistered, out PropIdType propId,
                    haveValue: false,
                    value: null,
                    alwaysRegister: false,
                    mustBeRegistered: mustBeRegistered,
                    neverCreate: false,
                    desiredHasStoreValue: _propFactory.ProvidesStorage);

            if (!PropData.IsEmpty)
            {
                bool result = _ourStoreAccessor.UnRegisterHandler(this, propId, eventHandler);
                return result;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Subscribe to Object PropertyChanges

        public bool SubscribeToPropChanged(EventHandler<PCObjectEventArgs> eventHandler, string propertyName)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData PropData = GetPropGen(propertyName, null, out bool wasRegistered, out PropIdType propId,
                    haveValue: false,
                    value: null,
                    alwaysRegister: false,
                    mustBeRegistered: mustBeRegistered,
                    neverCreate: false,
                    desiredHasStoreValue: _propFactory.ProvidesStorage);

            if (!PropData.IsEmpty)
            {
                bool result = _ourStoreAccessor.RegisterHandler(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
                return result;
            }
            else
            {
                return false;
            }
        }

        public bool UnSubscribeToPropChanged(EventHandler<PCObjectEventArgs> eventHandler, string propertyName)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData PropData = GetPropGen(propertyName, null, out bool wasRegistered, out PropIdType propId,
                    haveValue: false,
                    value: null,
                    alwaysRegister: false,
                    mustBeRegistered: mustBeRegistered,
                    neverCreate: false,
                    desiredHasStoreValue: _propFactory.ProvidesStorage);

            if (!PropData.IsEmpty)
            {
                bool result = _ourStoreAccessor.UnRegisterHandler(this, propId, eventHandler);
                return result;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Register / Unregister Action<object, object> callbacks.

        //// This is used to allow the caller to get notified only when a particular property is changed with values.
        //// It can be used in any for any TypeSafetyMode, but is especially handy when the TypeSafetyMode = 'none.'
        //public bool SubscribeToPropChanged(Action<object, object> doOnChange, string propertyName)
        //{
        //    bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

        //    IPropGen PropData = GetPropGen(propertyName, null, out bool wasRegistered,
        //            haveValue: false,
        //            value: null,
        //            alwaysRegister: false,
        //            mustBeRegistered: mustBeRegistered,
        //            neverCreate: false,
        //            desiredHasStoreValue: PropFactory.ProvidesStorage);

        //    if (!PropData.IsEmpty)
        //    {
        //        // TODO: Help the caller subscribe using a WeakReference
        //        PropData.SubscribeToPropChanged(doOnChange);
        //        return true;
        //    }
        //    return false;
        //}

        //public bool UnSubscribeToPropChanged(Action<object, object> doOnChange, string propertyName)
        //{
        //    bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

        //    IPropGen PropData = GetPropGen(propertyName, null, out bool wasRegistered,
        //            haveValue: false,
        //            value: null,
        //            alwaysRegister: false,
        //            mustBeRegistered: mustBeRegistered,
        //            neverCreate: false,
        //            desiredHasStoreValue: PropFactory.ProvidesStorage);

        //    if (!PropData.IsEmpty)
        //        return PropData.UnSubscribeToPropChanged(doOnChange);
        //    else
        //        return false;
        //}

        #endregion

        #endregion

        #region Register / Unregister Binding

        public bool RegisterBinding<T>(string nameOfPropertyToUpdate, string pathToSource)
        {
            IPropData PropData = GetPropGen<T>(nameOfPropertyToUpdate, out PropIdType propId, desiredHasStoreValue: _propFactory.ProvidesStorage);

            IPropPrivate<T> prop = CheckTypeInfo<T>(propId, nameOfPropertyToUpdate, PropData, _ourStoreAccessor);

            if (prop != null)
            {
                LocalPropertyPath lpp = new LocalPropertyPath(pathToSource);
                LocalBindingInfo bindingInfo = new LocalBindingInfo(lpp, LocalBindingMode.OneWay);

                List<ISubscriptionGen> sc = GetSubscriptions(propId)?.ToList();

                bool wasAdded = _ourStoreAccessor.RegisterBinding<T>(this, propId, bindingInfo);

                sc = GetSubscriptions(propId)?.ToList();

                return wasAdded;
            }
            else
            {
                return false;
            }
        }

        public bool UnregisterBinding<T>(string nameOfPropertyToUpdate, string pathToSource)
        {
            IPropData PropData = GetPropGen<T>(nameOfPropertyToUpdate, out PropIdType propId, desiredHasStoreValue: _propFactory.ProvidesStorage);

            IPropPrivate<T> prop = CheckTypeInfo<T>(propId, nameOfPropertyToUpdate, PropData, _ourStoreAccessor);

            if (prop != null)
            {
                LocalPropertyPath lpp = new LocalPropertyPath(pathToSource);
                LocalBindingInfo bindingInfo = new LocalBindingInfo(lpp, LocalBindingMode.OneWay);

                bool wasRemoved = _ourStoreAccessor.UnRegisterBinding<T>(this, propId, bindingInfo);
                return wasRemoved;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Public Methods

        public int NumOfDoSetDelegatesInCache
        {
            get
            {
                int result = _propFactory.DoSetCacheCount;
                return result;
            }
        }

        public int CreatePropFromStringCacheCount
        {
            get
            {
                int result = _propFactory.CreatePropFromStringCacheCount;
                return result;
            }
        }

        public int CreatePropWithNoValCacheCount
        {
            get
            {
                int result = _propFactory.CreatePropWithNoValCacheCount;
                return result;
            }
        }

        public virtual ITypeSafePropBagMetaData GetMetaData()
        {
            return OurMetaData;
        }

        /// <summary>
        /// Makes a copy of the core list.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, ValPlusType> GetAllPropNamesAndTypes()
        {
            IEnumerable<KeyValuePair<PropNameType, IPropData>> theStoreAsCollection = _ourStoreAccessor.GetCollection(this);

            IEnumerable<KeyValuePair<string, ValPlusType>> list = theStoreAsCollection.Select(x =>
            new KeyValuePair<string, ValPlusType>(x.Key, x.Value.ValuePlusType())).ToList();

            IDictionary<string, ValPlusType> result = list.ToDictionary(pair => pair.Key, pair => pair.Value);
            return result;
        }

        /// <summary>
        /// Returns all of the values in dictionary of objects, keyed by PropertyName.
        /// </summary>
        public IDictionary<PropNameType, IPropData> GetAllPropertyValues()
        {
            IEnumerable<KeyValuePair<PropNameType, IPropData>> theStoreAsCollection = _ourStoreAccessor.GetCollection(this);
            IDictionary<PropNameType, IPropData> result = theStoreAsCollection.ToDictionary(pair => pair.Key, pair => pair.Value);
            return result;
        }

        public IList<PropNameType> GetAllPropertyNames()
        {
            var result = _ourStoreAccessor.GetKeys(this).ToList();
            return result;
        }

        public bool PropertyExists(string propertyName)
        {
            if (_typeSafetyMode == PropBagTypeSafetyMode.Locked)
            {
                throw new InvalidOperationException("PropertyExists is not allowed when the TypeSafetyMode is set to 'Locked.'");
            }

            if (propertyName.StartsWith("[") && propertyName.EndsWith("]"))
            {
                int pos = propertyName.IndexOf(',');
                if (pos > 0)
                {
                    propertyName = propertyName.Substring(pos + 1, propertyName.Length - (pos + 2)).Trim();
                }
            }

            PropIdType propId = GetPropId(propertyName);

            bool result = _ourStoreAccessor.ContainsKey(this, propId); 
            return result;
        }

        // TODO: PropBagTypeSafetyMode.Locked is not honored here.
        public bool TryGetTypeOfProperty(string propertyName, out Type type)
        {
            PropIdType propId = GetPropId(propertyName);
            if (_ourStoreAccessor.TryGetValue(this, propId, out IPropData value))
            {
                type = value.TypedProp.Type;
                return true;
            }
            else if (_typeSafetyMode == PropBagTypeSafetyMode.Locked)
            {
                type = null;
                return ReportAccessToMissing(propertyName, nameof(TryGetTypeOfProperty));
            }
            else
            {
                type = null;
                return false;
            }
        }

        public System.Type GetTypeOfProperty(string propertyName)
        {
            IPropData pGen = GetPropGen(propertyName, null, out bool wasRegistered, out PropIdType propId,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: false,
                neverCreate: true,
                desiredHasStoreValue: _propFactory.ProvidesStorage);

            if (!pGen.IsEmpty)
            {
                return pGen.TypedProp.Type;
            }
            else if(_typeSafetyMode == PropBagTypeSafetyMode.Locked)
            {
                wasRegistered = false;
                return ReportAccessToMissing(propertyName, nameof(GetTypeOfProperty)).GetType();
            }
            else
            {
                return null;
            }
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
        protected IProp<T> AddProp<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<T, T, bool> comparer = null, object extraInfo = null, T initialValue = default(T))
        {
            bool hasStorage = true;
            bool typeIsSolid = true;
            IProp<T> pg = _propFactory.Create<T>(initialValue, propertyName, extraInfo, hasStorage, typeIsSolid, doWhenChangedX, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropObjComp<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            object extraInfo = null, T initialValue = default(T))
        {
            bool hasStorage = true;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = _propFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = _propFactory.Create<T>(initialValue, propertyName, extraInfo, hasStorage, typeIsSolid, doWhenChangedX, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropNoValue<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            bool hasStorage = true;
            bool typeIsSolid = true;
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doWhenChangedX, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropObjCompNoValue<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            object extraInfo = null)
        {
            bool hasStorage = true;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = _propFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doWhenChangedX, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropNoStore<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            bool hasStorage = false;
            bool typeIsSolid = true;
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doWhenChangedX, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropObjCompNoStore<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            object extraInfo = null)
        {
            bool hasStorage = false;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = _propFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doWhenChangedX, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IPropData AddProp<T>(string propertyName, IProp<T> genericTypedProp,EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false)
        {
            PropIdType propId = GetPropId(propertyName);

            if (!_ourStoreAccessor.TryAdd(this, propId, propertyName, genericTypedProp, out IPropData propGen))
            {
                throw new ApplicationException("Could not add the new propGen to the store.");
            }

            return propGen;
        }

        protected IPropData AddProp(string propertyName, IProp genericTypedProp)
        {
            PropIdType propId = GetPropId(propertyName);

            if (!_ourStoreAccessor.TryAdd(this, propId, propertyName, genericTypedProp, out IPropData propGen))
            {
                throw new ApplicationException("Could not add the new propGen to the store.");
            }
            return propGen;
        }

        protected void RemoveProp(string propertyName, Type propertyType)
        {
            if (propertyType == null && OurMetaData.OnlyTypedAccess)
            {
                ReportNonTypedAccess(propertyName, nameof(SetValWithType));
            }

            bool mustBeRegistered = _typeSafetyMode == PropBagTypeSafetyMode.Locked;

            IPropData pGen = GetPropGen(propertyName, null, out bool wasRegistered, out PropIdType propId,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: false,
                neverCreate: true,
                desiredHasStoreValue: _propFactory.ProvidesStorage);

            if (!pGen.IsEmpty)
            {
                pGen.CleanUp(doTypedCleanup: false);

                if (!_ourStoreAccessor.TryRemove(this, propId, out IPropData foundValue))
                {
                    System.Diagnostics.Debug.WriteLine($"The prop was found, but could not be removed. Property: {propertyName}.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Could not remove property: {propertyName}.");
            }
        }
        
        protected void RemoveProp<T>(string propertyName)
        {
            bool mustBeRegistered = _typeSafetyMode == PropBagTypeSafetyMode.Locked;

            IPropData PropData = GetGenPropPrivate<T>(propertyName, mustBeRegistered: mustBeRegistered, neverCreate: true);

            if(!PropData.IsEmpty)
            {
                PropData.CleanUp(doTypedCleanup: true);

                PropIdType propId = GetPropId(propertyName);
                if (!_ourStoreAccessor.TryRemove(this, propId, out IPropData foundValue))
                {
                    System.Diagnostics.Debug.WriteLine($"The prop was found, but could not be removed. Property: {propertyName}.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Could not remove property: {propertyName}.");
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="doWhenChanged"></param>
        ///// <param name="doAfterNotify"></param>
        ///// <param name="propertyName"></param>
        ///// <returns>True, if there was an existing Action in place for this property.</returns>
        //protected bool RegisterDoWhenChanged<T>(Action<T, T> doWhenChanged, bool doAfterNotify, string propertyName)
        //{
        //    IPropPrivate<T> prop = GetTypedPropPrivate<T>(propertyName, mustBeRegistered: true);
        //    return prop.UpdateDoWhenChangedAction(doWhenChanged, doAfterNotify);
        //}

        // TODO: Make instances re-usable.
        // TODO: Make this implement a 'Reset All Data' type opertions.
        protected void ClearAllProps()
        {
            _ourStoreAccessor.Dispose();
        }

        //protected void ClearEventSubscribers()
        //{
        //    IEnumerable<IPropData> propDataObjects = _ourStoreAccessor.GetValues(this);

        //    foreach (IPropData propData in propDataObjects)
        //    {
        //        propData.CleanUp(doTypedCleanup: true);
        //    }
        //}

        #endregion

        #region Private Methods

        private bool DoSet<T>(PropIdType propId, string propertyName, IPropPrivate<T> typedProp, ref T curValue, T newValue)
        {
            if (typedProp.ValueIsDefined)
            {
                bool theSame = typedProp.CompareTo(newValue);
                if (!theSame)
                {
                    T oldValue = typedProp.TypedValue;

                    // TODO: Consider adding a typed version of OnPropertyChanging.
                    OnPropertyChanging(propertyName);

                    // Make the update.
                    if (typedProp.HasStore)
                    {
                        typedProp.TypedValue = newValue;
                    }
                    curValue = newValue;

                    // Raise notify events.
                    DoNotifyWork(propId, propertyName, typedProp, oldValue, newValue);
                }
                return !theSame;
            }
            else
            {
                //if (_notifyWhenOldIsUndefined)
                //{

                // TODO: Consider adding a typed version of OnPropertyChanging.
                OnPropertyChanging(propertyName);

                // Make the update.
                if (typedProp.HasStore)
                {
                    typedProp.TypedValue = newValue;
                }
                curValue = newValue;

                // Raise notify events.
                DoNotifyWork(propId, propertyName, typedProp, newValue);

                //}

                // The current value is undefined and the new value is defined, therefore: their has been a real update -- return true.
                return true;
            }
        }

        private void DoNotifyWork<T>(PropIdType propId, PropNameType propertyName, IPropPrivate<T> typedProp, T oldVal, T newValue)
        {
            List<ISubscriptionGen> reff = GetSubscriptions(propId).ToList();
            foreach (ISubscriptionGen sub in GetSubscriptions(propId))
            {
                if (sub is ISubscription<T> typedSub)
                {
                    if (typedSub.TypedHandler == null)
                    {
                        throw new InvalidOperationException("The Typed handler is null.");
                    }

                    try
                    {
                        typedSub.TypedHandler(this, new PCTypedEventArgs<T>(propertyName, oldVal, newValue));
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"A TypedHandler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
                    }
                }
                else if (sub.SubscriptionKind == SubscriptionKind.GenHandler)
                {
                    if (sub.GenHandler == null)
                    {
                        throw new InvalidOperationException("The Gen handler is null.");
                    }

                    try
                    {
                        sub.GenHandler(this, new PCGenEventArgs(propertyName, typeof(T), oldVal, newValue));
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"A GenHandler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
                    }
                }
                else if (sub.SubscriptionKind == SubscriptionKind.ObjHandler)
                {
                    if (sub.ObjHandler == null)
                    {
                        throw new InvalidOperationException("The Obj handler is null.");
                    }

                    try
                    {
                        sub.ObjHandler(this, new PCObjectEventArgs(propertyName, oldVal, newValue));
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"An ObjHandler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Handlers of kind: {sub.SubscriptionKind} are not supported.");
                }
            }

            // Raise the standard PropertyChanged event
            OnPropertyChanged(propertyName);

            // Raise the standard PropertyChanged event for those subscribers who address us via the indexer.
            OnPropertyChanged(_propFactory.IndexerName);

            // The shared, EventHandler<PCObjectEventArgs>
            OnPropertyChangedWithObjVals(new PCObjectEventArgs(propertyName, oldVal, newValue));

            // The shared, EventHandler<PCGenEventArgs>
            _eventHolder.OnPropertyChangedWithGenVals(new PCGenEventArgs(propertyName, typeof(T), oldVal, newValue));
        }

        // For when the current value is undefined.
        private void DoNotifyWork<T>(PropIdType propId, PropNameType propertyName, IPropPrivate<T> typedProp, T newValue)
        {
            // PROCESS BINDINGS

            foreach (ISubscriptionGen sub in GetSubscriptions(propId))
            {
                if (sub is ISubscription<T> typedSub)
                {
                    if (typedSub.TypedHandler == null)
                    {
                        throw new InvalidOperationException("The Typed handler is null.");
                    }

                    try
                    {
                        typedSub.TypedHandler(this, new PCTypedEventArgs<T>(propertyName, newValue));
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"A TypedHandler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
                    }
                }
                else if (sub.SubscriptionKind == SubscriptionKind.GenHandler)
                {
                    if (sub.GenHandler == null)
                    {
                        throw new InvalidOperationException("The Gen handler is null.");
                    }

                    try
                    {
                        sub.GenHandler(this, new PCGenEventArgs(propertyName, typeof(T), newValue));
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"A GenHandler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
                    }
                }
                else if (sub.SubscriptionKind == SubscriptionKind.ObjHandler)
                {
                    if (sub.ObjHandler == null)
                    {
                        throw new InvalidOperationException("The Obj handler is null.");
                    }

                    try
                    {
                        sub.ObjHandler(this, new PCObjectEventArgs(propertyName, newValue));
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"An ObjHandler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Handlers of kind: {sub.SubscriptionKind} are not supported.");
                }
            }

            // Raise the standard PropertyChanged event
            OnPropertyChanged(propertyName);

            // The shared, EventHandler<PCObjectEventArgs>
            OnPropertyChangedWithObjVals(new PCObjectEventArgs(propertyName, newValue));

            // The shared, EventHandler<PCGenEventArgs>
            _eventHolder.OnPropertyChangedWithGenVals(new PCGenEventArgs(propertyName, typeof(T), newValue));
        }

        private IEnumerable<ISubscriptionGen> GetSubscriptions(PropIdType propId)
        {
            IEnumerable<ISubscriptionGen> sc = _ourStoreAccessor.GetSubscriptions(this, propId);
            return sc;
        }

        //private IEnumerable<ISubscription<T>> GetTypedSubscriptions<T>(SubscriberCollection sc)
        //{
        //    IEnumerable<ISubscription<T>> typedSubs = sc.Where(x => x.SubscriptionKind == SubscriptionKind.TypedHandler)
        //        .Select(y => (ISubscription<T>)y);

        //    return typedSubs;
        //}

        //private IEnumerable<ISubscriptionGen> GetPCGenSubscriptions(SubscriberCollection sc)
        //{
        //    IEnumerable<ISubscriptionGen> genSubs = sc.Where(x => x.SubscriptionKind == SubscriptionKind.GenHandler)
        //        .Select(y => (ISubscriptionGen)y);

        //    return genSubs;
        //}

        //private IEnumerable<ISubscriptionGen> GetPCObjectSubscriptions(SubscriberCollection sc)
        //{
        //    IEnumerable<ISubscriptionGen> objSubs = sc.Where(x => x.SubscriptionKind == SubscriptionKind.ObjHandler)
        //        .Select(y => (ISubscriptionGen)y);

        //    return objSubs;
        //}

        protected IPropData GetPropGen(PropNameType propertyName, Type propertyType,
            out bool wasRegistered, out PropIdType propId,
            bool haveValue, object value,
            bool alwaysRegister, bool mustBeRegistered, bool neverCreate, bool? desiredHasStoreValue)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName", "PropertyName is null on call to GetValue.");

            propId = GetPropId(propertyName);

            Debug.Assert(!(alwaysRegister && mustBeRegistered), "AlwaysRegister and MustBeRegistered cannot both be true.");
            Debug.Assert(!(alwaysRegister && neverCreate), "AlwaysRegister and NeverCreate cannot both be true.");

            IPropData PropData;

            if (_ourStoreAccessor.TryGetValue(this, propId, out PropData))
            {
                wasRegistered = false;
            }
            else
            {
                PropData = this.HandleMissingProp(propId, propertyName, propertyType, out wasRegistered, haveValue, value, alwaysRegister, mustBeRegistered, neverCreate);
            }

            if (!PropData.IsEmpty && desiredHasStoreValue.HasValue && desiredHasStoreValue.Value != PropData.TypedProp.HasStore)
            {
                if (desiredHasStoreValue.Value)
                    //Caller needs property to have a backing store.
                    throw new InvalidOperationException(string.Format("Property: {0} has no backing store held by this instance of PropBag. This operation can only be performed on properties for which a backing store is held by this instance.", propertyName));
                else
                    throw new InvalidOperationException(string.Format("Property: {0} has a backing store held by this instance of PropBag. This operation can only be performed on properties for which no backing store is kept by this instance.", propertyName));
            }

            return (IPropData) PropData;
        }

        protected IPropData GetPropGen<T>(PropNameType propertyName, out PropIdType propId, bool? desiredHasStoreValue)
        {
            propId = GetPropId(propertyName);
            if (!_ourStoreAccessor.TryGetValue(this, propId, out IPropData PropData))
            {
                throw new KeyNotFoundException("That cKey was not found.");
            }

            if (!PropData.IsEmpty && desiredHasStoreValue.HasValue && desiredHasStoreValue.Value != PropData.TypedProp.HasStore)
            {
                if (desiredHasStoreValue.Value)
                    //Caller needs property to have a backing store.
                    throw new InvalidOperationException(string.Format("Property: {0} has no backing store held by this instance of PropBag. This operation can only be performed on properties for which a backing store is held by this instance.", propertyName));
                else
                    throw new InvalidOperationException(string.Format("Property: {0} has a backing store held by this instance of PropBag. This operation can only be performed on properties for which no backing store is kept by this instance.", propertyName));
            }

            return (IPropData)PropData;
        }

        private IPropPrivate<T> CheckTypeInfo<T>(PropIdType propId, PropNameType propertyName, IPropData PropData, PSAccessServiceType storeAccess)
        {
            if (!PropData.TypedProp.TypeIsSolid)
            {
                try
                {
                    Type newType = typeof(T);
                    MakeTypeSolid(propId, propertyName, PropData, newType);
                }
                catch (InvalidCastException ice)
                {
                    throw new ApplicationException($"The property: {propertyName} was originally set to null. " +
                        "Now its being set to a value whose type is a value type. Value types don't allow setting to null.", ice);
                }
            }
            else
            {
                if (!AreTypesSame(typeof(T), PropData.TypedProp.Type))
                {
                    throw new ApplicationException($"Attempting to set property: {propertyName} whose type is {PropData.TypedProp.Type}, " +
                        $"with a call whose type parameter is {typeof(T).ToString()} is invalid.");
                }
            }

            return (IPropPrivate<T>)PropData.TypedProp;
        }

        /// <summary>
        /// Returns true if and only if the type was updated.
        /// </summary>
        /// <param name="PropData"></param>
        /// <param name="newType"></param>
        /// <returns>True if the type was updated, otherwise false.</returns>
        private bool MakeTypeSolid(PropIdType propId, PropNameType propertyName, IPropData PropData, Type newType)
        {
            //Type currentType = PropData.Type;

            Debug.Assert(PropData.TypedProp.TypedValueAsObject == null, "The current value of the property should be null when MakeTypeSolid is called.");
            Debug.Assert(!PropData.IsEmpty, "PropData is empty on call to MakeTypeSolid.");

            System.Type underlyingtype = Nullable.GetUnderlyingType(newType);

            // Check to see if the new type is a non-nullable, value type.
            // If it is, then it was invalid to set this property to null in the first place.
            // Consider setting value to the type's default value instead of throwing this exception.
            if (underlyingtype == null && newType.IsValueType)
                throw new InvalidCastException("The new type is a non-nullable value type.");

            // We are using strict equality here, since we have the oportunity to update the type to anything
            // that is assignable from a value of type object (which is everything.)
            if (newType != PropData.TypedProp.Type)
            {
                object curValue = PropData.TypedProp.TypedValueAsObject;
                PropKindEnum propKind = PropData.TypedProp.PropKind;

                IProp genericTypedProp = _propFactory.CreateGenFromObject(newType, curValue, propertyName, null, true, true, propKind, null, false, null, false);

                bool result = _ourStoreAccessor.SetTypedProp(this, propId, propertyName, genericTypedProp);
                return result;
            }
            else
            {
                return false;
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

                return aUnder == bUnder || aUnder.UnderlyingSystemType == bUnder.UnderlyingSystemType;
            }
            else if (newType.IsGenericType)
            {
                return curType.IsAssignableFrom(newType);
            }
            else
            {
                return newType.UnderlyingSystemType == curType.UnderlyingSystemType;
            }
        }

        /// <summary>
        /// Given a string in the form "{0}Changed", where {0} is the underlying property name, parse out and return the value of {0}.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        protected string GetPropNameFromEventProp(string x)
        {
            //PropStringChanged
            if(x.Length > 7 && x.EndsWith("Changed", StringComparison.InvariantCultureIgnoreCase))
            {
                return x.Substring(0, x.Length - 7);
            }
            else
            {
                return x;
            }
        }

        //public object GetValueGen(object host, string propertyName, Type propertyType)
        //{
        //    return ((IPropBag)host).GetValWithType(propertyName, propertyType);
        //}

        //public void SetValueGen(object host, string propertyName, Type propertyType, object value)
        //{
        //    ((IPropBag)host).SetValWithType(propertyName, propertyType, value);
        //}

        #endregion

        #region Level2 Key Management

        //private PropNameType GetPropName(PropIdType propId)
        //{
        //    PropNameType propertyName = _level2KeyManager.FromCooked(propId);
        //    return propertyName;
        //}

        private PropIdType GetPropId(PropNameType propertyName)
        {
            // Register new propertyName and get an exploded key.
            PropIdType propId = _level2KeyManager.GetOrAdd(propertyName);

            return propId;
        }

        #endregion

        #region Methods to Raise Events

        // Raise Standard Events
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChanged, null, null);

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void OnPropertyChanging(string propertyName)
        {
            PropertyChangingEventHandler handler = Interlocked.CompareExchange(ref PropertyChanging, null, null);

            if (handler != null)
            {
                handler(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        // INotifyPCGen
        // Moved to the PB_EventHolder


        // INotifyPCObject
        protected void OnPropertyChangedWithObjVals(PCObjectEventArgs eArgs)
        {
            EventHandler<PCObjectEventArgs> handler = Interlocked.CompareExchange(ref PropertyChangedWithObjectVals, null, null);

            if (handler != null)
                handler(this, eArgs);
        }

        #endregion

        #region IListSource Support

        public bool TryGetPropType(string propertyName, out PropKindEnum propType)
        {
            PropIdType propId = GetPropId(propertyName);
            if (_ourStoreAccessor.TryGetValue(this, propId, out IPropData value))
            {
                propType = value.TypedProp.PropKind;
                return true;
            }
            else
            {
                propType = PropKindEnum.Prop;
                return false;
            }
        }

        public bool TryGetListSource(string propertyName, Type itemType, out IListSource listSource)
        {
            PropIdType l2Key = GetPropId(propertyName);
            if (_ourStoreAccessor.TryGetValue(this, l2Key, out IPropData value))
            {
                listSource = value.TypedProp.ListSource;
                return true;
            }
            else
            {
                listSource = null;
                return false;
            }
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //_propFactory.PropStoreAccessServiceProvider.TearDown(this, _ourStoreAccessor);
                    _ourStoreAccessor.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

        #region Generic Method Support

        // TODO: Consider creating a new interface: IPropBagInternal and making this method be a member of that interface.
        private bool DoSetBridge<T>(IPropBag target, PropIdType propId, string propertyName, IProp prop, object value)
        {
            T typedValue = (T)value;

            IPropPrivate<T> typeProp = (IPropPrivate<T>)prop;
            bool result = ((PropBag)target).DoSet<T>(propId, propertyName, typeProp, ref typedValue, (T)value);

            typeProp.TypedValue = typedValue;

            return result;
        }

        #endregion

        
    }

    internal class PB_EventHolder : INotifyPropertyChanged,
        INotifyPropertyChanging,
        INotifyPCGen,
        INotifyPCObject
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        public event EventHandler<PCGenEventArgs> PropertyChangedWithGenVals;
        public event EventHandler<PCObjectEventArgs> PropertyChangedWithObjectVals;

        public void OnPropertyChangedWithGenVals(PCGenEventArgs eArgs)
        {
            EventHandler<PCGenEventArgs> handler = Interlocked.CompareExchange(ref PropertyChangedWithGenVals, null, null);

            if (handler != null)
                handler(this, eArgs);
        }
    }
}
