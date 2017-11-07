using DRM.PropBag.Caches;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals.ObjectIdDictionary;
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

    public partial class PropBag : IPropBag
    {
        //delegate IPropGen MissingPropStategy(string propertyName, Type propertyType, out bool wasRegistered,
        //    bool haveValue, object value, bool alwaysRegister, bool readMissingDoesRegister);

        #region Member Declarations

        // TO-DOC: Explain that the life time of any sources that listen to the events provided by this class,
        // including the events provided by the IProp instances
        // is determined by the lifetime of the instances of classes that derive from this PropBag class.

        // TO-DOC: Explain since we may subscribe to our own events we would like to have these initialized.
        // confirm that on post construction events are initialized for us if we don't
        // alternative could be that they are initialized on first assignment.
        public event PropertyChangedEventHandler PropertyChanged; // = delegate { };
        public event PropertyChangingEventHandler PropertyChanging; // = delegate { };
        public event EventHandler<PCGenEventArgs> PropertyChangedWithGenVals = delegate { };
        public event PropertyChangedEventHandler PropertyChangedIndividual;


        // DRM: Changed to protected and added set accessor on 10/29/17; DRM: removed set accessor on 11/2/17.
        protected IPropFactory PropFactory { get; /*set; */}

        public string FullClassName => OurMetaData.FullClassName;

        private PropBagTypeSafetyMode TypeSafetyMode { get; }
        protected virtual TypeSafePropBagMetaData OurMetaData { get; }

        private readonly Dictionary<string, PropGen> tVals;

        private uint _ourObjectId;
        private IL2KeyMan<uint, string> _level2KeyManager;
        private ICKeyMan<ulong, uint, uint, string> _compKeyManager;
        private readonly IObjectIdDictionary<ulong, uint, uint, string, PropGen> _theStore;

        #endregion

        #region Constructor

        /// <summary>
        /// This is constructor creates an instance with minimal resources, and is not operational.
        /// It can be called in those cases where an instance is required as a target to use when calling
        /// a method from the System.Reflection namespace.
        /// </summary>
        /// <param name="dummy"></param>
        protected PropBag(byte dummy)
        {
            TypeSafetyMode = PropBagTypeSafetyMode.None;
            PropFactory = null;
            tVals = null;
            _theStore = null;
            OurMetaData = BuildMetaData(this.TypeSafetyMode, classFullName: null, propFactory: null);
        }

        protected PropBag()
            : this(PropBagTypeSafetyMode.None, null, null) { }

        protected PropBag(PropBagTypeSafetyMode typeSafetyMode)
            : this(typeSafetyMode, null, null) { }

        // TODO: remove this constructor.
        protected PropBag(PropBagTypeSafetyMode typeSafetyMode, IPropFactory propFactory)
            : this(typeSafetyMode, null, propFactory) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pm"></param>
        /// <param name="propFactory">The PropFactory to use instead of the one specified by the PropModel.</param>
        public PropBag(ControlModel.PropModel pm, string fullClassName = null, IPropFactory propFactory = null)
            : this(pm.TypeSafetyMode, fullClassName, propFactory ?? pm.PropFactory)
        {
            Hydrate(pm);
        }

        protected PropBag(PropBagTypeSafetyMode typeSafetyMode, string fullClassName = null, IPropFactory propFactory = null)
        {
            this.TypeSafetyMode = typeSafetyMode;

            bool returnDefaultForUndefined = TypeSafePropBagMetaData.Helper.GetReturnDefaultForUndefined(typeSafetyMode);

            if (propFactory != null)
            {
                //if (propFactory.ReturnDefaultForUndefined != returnDefaultForUndefined)
                //{
                //    throw new ApplicationException("The 'ReturnDefaultForUndefined' setting on the specified property factory conflicts with the TypeSafetyMode specified.");
                //}
                PropFactory = propFactory;
            }
            else
            {
                // Use the "built-in" property factory, if the caller did not supply one.
                PropFactory = new PropFactory(typeResolver: null, valueConverter: null);
            }

            OurMetaData = BuildMetaData(TypeSafetyMode, fullClassName, PropFactory);

            _ourObjectId = (uint) ObjectIdIssuer.NextObjectId;
            _theStore = ProvisonTheStore(out _level2KeyManager, out _compKeyManager);

            tVals = new Dictionary<string, PropGen>();
        }

        private IObjectIdDictionary<ulong, uint, uint, string, PropGen> ProvisonTheStore(
            out IL2KeyMan<uint, string> level2KeyMan, out ICKeyMan<ulong, uint, uint, string> compKeyManager) 
        {
            level2KeyMan = new SimpleLevel2KeyMan();
            compKeyManager = new SimpleCompKeyMan(level2KeyMan, 48);


            IObjectIdDictionary<ulong, uint, uint, string, PropGen> result 
                = new SimpleObjectIdDictionary<PropGen>(compKeyManager: compKeyManager, level2KeyManager: level2KeyMan);

            return result;
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

                if(doWhenChangedAction == null && (pi?.DoWhenChangedField?.DoWhenActionGetter != null))
                {
                    Func<object, EventHandler<PCGenEventArgs>> rr = pi.DoWhenChangedField.DoWhenActionGetter;

                    doWhenChangedAction = rr(this);
                }

                IPropGen pg;

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

                    pg = PropFactory.CreateGenFromString(pi.PropertyType, value, useDefault, pi.PropertyName, ei, pi.HasStore, pi.TypeIsSolid,
                        pi.PropKind, doWhenChangedAction, pi.DoWhenChangedField.DoAfterNotify, comparer, useRefEquality, pi.ItemType);
                }
                else
                {
                    pg = PropFactory.CreateGenWithNoValue(pi.PropertyType, pi.PropertyName, ei, pi.HasStore, pi.TypeIsSolid,
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
        private PropGen HandleMissingProp(string propertyName, Type propertyType, out bool wasRegistered,
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
                return new PropGen(null, null);
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
                            return new PropGen(null, null);
                        }
                    }
                case ReadMissingPropPolicyEnum.Register:
                    {
                        // TODO: Must determine the PropKind for this new property.
                        IPropGen typedPropWrapper;

                        if (haveValue)
                        {
                            bool typeIsSolid = PropFactory.IsTypeSolid(value, propertyType);

                            typedPropWrapper = PropFactory.CreateGenFromObject(propertyType, value,
                                propertyName, null, PropFactory.ProvidesStorage, typeIsSolid, PropKindEnum.Prop, null, false, null);
                        }
                        else
                        {
                            // TODO: This must set the value of the property to the default for its type,
                            // in order to satisfy TypeSafety modes: 'RegisterOnGetLoose' and 'RegisterOnGetSafe.'

                            //object newValue = ThePropFactory.GetDefaultValue(propertyType, propertyName);
                            bool typeIsSolid = true;

                            // On 10/8/17: Changed to use NoValue, instead of trying to generate a default value.
                            typedPropWrapper = PropFactory.CreateGenWithNoValue(propertyType, propertyName,
                                null, PropFactory.ProvidesStorage, typeIsSolid, PropKindEnum.Prop, null, false, null);
                        }

                        //PropGen propGen = new PropGen(typedPropWrapper);

                        PropGen propGen = AddProp(propertyName, typedPropWrapper);

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

        //public IPropGen this[int index] => throw new NotImplementedException();

        public object this[string typeName, string propertyName]
        {
            get
            {
                Type propertyType = PropFactory.TypeResolver(typeName);
                return GetValWithType(propertyName, propertyType);
            }
            set
            {
                Type propertyType = PropFactory.TypeResolver(typeName);
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

        public bool TryGetPropGen(string propertyName, Type propertyType, out IPropGen propGen)
        {
            bool mustBeRegistered = TypeSafetyMode == PropBagTypeSafetyMode.Locked;

            propGen = GetPropGen(propertyName, propertyType, out bool wasRegistered,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: true,
                desiredHasStoreValue: PropFactory.ProvidesStorage);

            if(!propGen.IsEmpty)
            {
                return true;
            }
            else if(TypeSafetyMode == PropBagTypeSafetyMode.Tight)
            {
                return ReportAccessToMissing(propertyName, nameof(TryGetPropGen));
            }
            else
            {
                return false;
            }
        }

        // Public wrapper aroud GetPropGen
        //
        //CHK: PropertyType may be null.
        public IPropGen GetPropGen(string propertyName, Type propertyType = null)
        {
            if (propertyType == null && OurMetaData.OnlyTypedAccess)
            {
                ReportNonTypedAccess(propertyName, nameof(GetPropGen));
            }

            PropGen genProp = GetPropGen(propertyName, propertyType, out bool wasRegistered,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: true,
                neverCreate: false,
                desiredHasStoreValue: PropFactory.ProvidesStorage);

            if (!wasRegistered)
            {
                if (propertyType != null)
                {
                    if (!genProp.TypeIsSolid)
                    {
                        MakeTypeSolid(ref genProp, propertyType, propertyName);

                        //ulong cKey = _compKeyManager.Join(_ourObjectId, propertyName);
                        _theStore[genProp.PropId] = genProp;

                        //tVals[propertyName] = genProp;
                    }
                    else
                    {
                        if (propertyType != genProp.Type)
                        {
                            throw new InvalidOperationException(string.Format("Attempting to get property: {0} whose type is {1}, with a call whose type parameter is {2} is invalid.", propertyName, genProp.Type.ToString(), propertyType.ToString()));
                        }
                    }
                }
            }

            return genProp;
        }

        public object GetValWithType(string propertyName, Type propertyType)
        {
            PropGen pg = (PropGen)GetPropGen(propertyName, propertyType);
            return pg.Value;
        }

        public ValPlusType GetValPlusType(string propertyName, Type propertyType)
        {
            IPropGen pg = GetPropGen(propertyName, propertyType);
            return pg.ValuePlusType();
        }

        public T GetIt<T>(string propertyName)
        {
            return GetTypedPropPrivate<T>(propertyName, mustBeRegistered: true).TypedValue;
        }

        public IProp<T> GetTypedProp<T>(string propertyName)
        {
            return (IProp<T>)GetTypedPropPrivate<T>(propertyName, mustBeRegistered: true);
        }

        private IPropPrivate<T> GetTypedPropPrivate<T>(string propertyName, bool mustBeRegistered, bool neverCreate = false)
        {
            PropGen genProp = GetGenPropPrivate<T>(propertyName, mustBeRegistered, neverCreate);

            if (!genProp.IsEmpty)
            {
                return (IPropPrivate<T>)genProp.TypedProp;
            }
            else
            {
                return null;
            }
        }

        private PropGen GetGenPropPrivate<T>(string propertyName, bool mustBeRegistered, bool neverCreate = false)
        {
            bool hasStore = PropFactory.ProvidesStorage;

            // TODO: Make this use a different version of GetPropGen: one that takes advantage of the 
            // compile-time type knowlege -- especially if we have to register the property in HandleMissing.

            PropGen genProp = GetPropGen(propertyName, typeof(T), out bool wasRegistered,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: neverCreate,
                desiredHasStoreValue: PropFactory.ProvidesStorage);

            if (wasRegistered)
                return genProp;
            else
            {
                if (!genProp.IsEmpty)
                {
                    CheckTypeInfo<T>(genProp, propertyName, _theStore);
                }
                return genProp;
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

            PropGen genProp = GetPropGen(propertyName, propertyType, out bool wasRegistered,
                    haveValue: true,
                    value: value,
                    alwaysRegister: alwaysRegister,
                    mustBeRegistered: mustBeRegistered,
                    neverCreate: false,
                    desiredHasStoreValue: PropFactory.ProvidesStorage);

            // No point in calling DoSet, it would find that the value is the same and do nothing.
            if (wasRegistered) return true;

            if (value != null)
            {
                if (!genProp.TypeIsSolid)
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
                            newType = PropFactory.GetTypeFromValue(value);
                        }

                        if (MakeTypeSolid(ref genProp, newType, propertyName))
                        {
                            // TODO: implement try update.
                            //ulong cKey = _compKeyManager.Join(_ourObjectId, propertyName);
                            _theStore[genProp.PropId] = genProp;

                            //tVals[propertyName] = genProp;
                        }

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
                        newType = PropFactory.GetTypeFromValue(value);
                    }

                    if (!AreTypesSame(newType, genProp.Type))
                    {
                        throw new ApplicationException(string.Format("Attempting to set property: {0} whose type is {1}, with a call whose type parameter is {2} is invalid.", propertyName, genProp.Type.ToString(), newType.ToString()));
                    }
                }
            }
            else
            {
                // Check to make sure that we are not attempting to set the value of a ValueType to null.
                if (genProp.TypeIsSolid && genProp.Type.IsValueType)
                {
                    throw new InvalidOperationException(string.Format("Cannot set property: {0} to null, it is a value type.", propertyName));
                }
            }

            // TODO: Make the PropFactory supply this service.
            DoSetDelegate setPropDel = DelegateCacheProvider.DoSetDelegateCache.GetOrAdd(genProp.Type);
            return setPropDel(value, this, propertyName, genProp);
        }

        public bool SetIt<T>(T value, string propertyName)
        {
            // For Set operations where a type is given, 
            // Register the property if it does not exist, unless the TypeSafetyMode
            // setting is AllPropsMustBe (explictly) registered.
            bool alwaysRegister = !OurMetaData.AllPropsMustBeRegistered;
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            PropGen genProp = GetPropGen(propertyName, typeof(T), out bool wasRegistered,
                haveValue: true,
                value: value,
                alwaysRegister: alwaysRegister,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: PropFactory.ProvidesStorage);

            // No point in calling DoSet, it would find that the value is the same and do nothing.
            if (wasRegistered) return true;

            //IPropPrivate<T> prop = CheckTypeInfo<T>(genProp, propertyName, tVals);

            IPropPrivate<T> prop = CheckTypeInfo<T>(genProp, propertyName, _theStore);


            return DoSet(value, propertyName, prop);
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

            PropGen genProp = GetPropGen(propertyName, typeof(T), out bool wasRegistered,
                    haveValue: true,
                    value: newValue,
                    alwaysRegister: alwaysRegister,
                    mustBeRegistered: mustBeRegistered,
                    neverCreate: false,
                    desiredHasStoreValue: false);

            // No point in calling DoSet, it would find that the value is the same and do nothing.
            if (wasRegistered) return true;

            IPropPrivate<T> prop = CheckTypeInfo<T>(genProp, propertyName, _theStore);

            bool theSame = prop.Compare(newValue, curValue);

            if (!theSame)
            {
                // Save the value before the update.
                T oldValue = curValue;

                OnPropertyChanging(PropFactory.IndexerName);

                // Make the update.
                curValue = newValue;

                // Raise notify events.
                DoNotifyWork<T>(oldValue, newValue, propertyName, prop);
            }

            // Return true, if the new value was found to be different than the current value.
            return !theSame;
        }

        #endregion

        #region Subscribe to Property Changed

        public bool SubscribeToPropChanged(EventHandler<PropertyChangedEventArgs> handler,
            string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;
            IPropGen genProp = GetPropGen(propertyName, propertyType, out bool wasRegistered,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: PropFactory.ProvidesStorage);

            if (!genProp.IsEmpty)
            {
                WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.
                    AddHandler(this, "PropertyChangedIndividual", handler);
                return true;
            }
            return false;
        }

        public bool UnsubscribeToPropChanged(EventHandler<PropertyChangedEventArgs> handler,
            string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;
            IPropGen genProp = GetPropGen(propertyName, propertyType, out bool wasRegistered,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: PropFactory.ProvidesStorage);

            if (!genProp.IsEmpty)
            {
                WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.
                    RemoveHandler(this, "PropertyChangedIndividual", handler);
                return true;
            }
            return false;
        }

        #endregion

        #region Subscribe to Typed PropertyChanged

        // TODO: Implement our own WeakEventManager for Delegates that have a single Type Parameter.
        public bool SubscribeToPropChanged<T>(EventHandler<PCTypedEventArgs<T>> eventHandler, string propertyName)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IProp<T> prop = GetTypedPropPrivate<T>(propertyName, mustBeRegistered: mustBeRegistered, neverCreate: false);

            if (prop != null)
            {
                //prop.GetTheEventManger().AddHandler()

                PropertyChangedEventManager p;

                //WeakEventManager<IProp<T>, PropertyChangedWithTValsEventArgs<T>>.
                //    AddHandler(prop, "PropertyChangedWithTVals", eventHandler);
                prop.PropertyChangedWithTVals += eventHandler;
                return true;
            }


            //        WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.
            //AddHandler(this, "PropertyChangedIndividual", handler);
            //        return true;
            return false;
        }

        public bool UnSubscribeToPropChanged<T>(EventHandler<PCTypedEventArgs<T>> eventHandler, string propertyName)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IProp<T> prop = GetTypedPropPrivate<T>(propertyName, mustBeRegistered: mustBeRegistered, neverCreate: true);

            if (prop != null)
            {
                prop.PropertyChangedWithTVals -= eventHandler;
                return true;
            }
            return false;
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

        //// Allow callers to easily subscribe to PropertyChangedWithTVals.
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
            throw new NotImplementedException();

            //bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            //IPropGen genProp = GetPropGen(propertyName, null, out bool wasRegistered,
            //        haveValue: false,
            //        value: null,
            //        alwaysRegister: false,
            //        mustBeRegistered: mustBeRegistered,
            //        neverCreate: false,
            //        desiredHasStoreValue: PropFactory.ProvidesStorage);

            //if (!genProp.IsEmpty)
            //{
            //    // TODO: Help the caller subscribe using a WeakReference
            //    genProp.SubscribeToPropChanged(doOnChange);
            //    return true;
            //}
            //return false;
        }

        public bool UnSubscribeToPropChanged(EventHandler<PCGenEventArgs> eventHandler, string propertyName, Type propertyType)
        {
            throw new NotImplementedException();
        }

        //// This is used to allow the caller to get notified only when a particular property is changed with values.
        //// It can be used in any for any TypeSafetyMode, but is especially handy when the TypeSafetyMode = 'none.'
        //public bool SubscribeToPropChanged(Action<object, object> doOnChange, string propertyName)
        //{
        //    bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

        //    IPropGen genProp = GetPropGen(propertyName, null, out bool wasRegistered,
        //            haveValue: false,
        //            value: null,
        //            alwaysRegister: false,
        //            mustBeRegistered: mustBeRegistered,
        //            neverCreate: false,
        //            desiredHasStoreValue: PropFactory.ProvidesStorage);

        //    if (!genProp.IsEmpty)
        //    {
        //        // TODO: Help the caller subscribe using a WeakReference
        //        genProp.SubscribeToPropChanged(doOnChange);
        //        return true;
        //    }
        //    return false;
        //}

        //public bool UnSubscribeToPropChanged(Action<object, object> doOnChange, string propertyName)
        //{
        //    bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

        //    IPropGen genProp = GetPropGen(propertyName, null, out bool wasRegistered,
        //            haveValue: false,
        //            value: null,
        //            alwaysRegister: false,
        //            mustBeRegistered: mustBeRegistered,
        //            neverCreate: false,
        //            desiredHasStoreValue: PropFactory.ProvidesStorage);

        //    if (!genProp.IsEmpty)
        //        return genProp.UnSubscribeToPropChanged(doOnChange);
        //    else
        //        return false;
        //}

        #endregion

        #region Subscribe to Object PropertyChanges

        public bool SubscribeToPropChanged(EventHandler<PCObjectEventArgs> eventHandler, string propertyName)
        {
            throw new NotImplementedException();
        }

        public bool UnSubscribeToPropChanged(EventHandler<PCObjectEventArgs> eventHandler, string propertyName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public Methods

        public TypeSafePropBagMetaData GetMetaData()
        {
            return OurMetaData;
        }

        public bool PropertyExists(string propertyName)
        {
            if (TypeSafetyMode == PropBagTypeSafetyMode.Locked)
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

            //bool result = tVals.ContainsKey(propertyName);

            bool result = _theStore.ContainsKey(_ourObjectId, propertyName); 
            return result;
        }

        // TODO: PropBagTypeSafetyMode.Locked is not honored here.
        public bool TryGetTypeOfProperty(string propertyName, out Type type)
        {
            if(_theStore.TryGetValue(_ourObjectId, propertyName, out PropGen value))
            //if (tVals.TryGetValue(propertyName, out PropGen value))
            {
                type = value.Type;
                return true;
            }
            else if (TypeSafetyMode == PropBagTypeSafetyMode.Locked)
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
            IPropGen pGen = GetPropGen(propertyName, null, out bool wasRegistered,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: false,
                neverCreate: true,
                desiredHasStoreValue: PropFactory.ProvidesStorage);

            if (!pGen.IsEmpty)
            {
                return pGen.TypedProp.Type;
            }
            else if(TypeSafetyMode == PropBagTypeSafetyMode.Locked)
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
            IProp<T> pg = PropFactory.Create<T>(initialValue, propertyName, extraInfo, hasStorage, typeIsSolid, doWhenChangedX, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropObjComp<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            object extraInfo = null, T initialValue = default(T))
        {
            bool hasStorage = true;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = PropFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = PropFactory.Create<T>(initialValue, propertyName, extraInfo, hasStorage, typeIsSolid, doWhenChangedX, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropNoValue<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            bool hasStorage = true;
            bool typeIsSolid = true;
            IProp<T> pg = PropFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doWhenChangedX, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropObjCompNoValue<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            object extraInfo = null)
        {
            bool hasStorage = true;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = PropFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = PropFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doWhenChangedX, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropNoStore<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            bool hasStorage = false;
            bool typeIsSolid = true;
            IProp<T> pg = PropFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doWhenChangedX, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropObjCompNoStore<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            object extraInfo = null)
        {
            bool hasStorage = false;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = PropFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = PropFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doWhenChangedX, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected void AddProp<T>(string propertyName, IProp<T> prop)
        {
            ulong cKey = GetNewCKey(propertyName);
            PropGen propGen = new PropGen(prop, cKey);

            //tVals.Add(propertyName, propGen);
            if (!_theStore.TryAdd(cKey, propGen))
            {
                throw new ApplicationException("Could not add the new propGen to the store.");
            }
            //prop.DoWhenChanged = 
        }

        protected PropGen AddProp(string propertyName, IPropGen typedPropWrapper)
        {
            ulong cKey = GetNewCKey(propertyName);

            PropGen propGen = new PropGen(typedPropWrapper, cKey);

            //tVals.Add(propertyName, propGen);
            if (!_theStore.TryAdd(cKey, propGen))
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

            bool mustBeRegistered = TypeSafetyMode == PropBagTypeSafetyMode.Locked;

            IPropGen pGen = GetPropGen(propertyName, null, out bool wasRegistered,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: false,
                neverCreate: true,
                desiredHasStoreValue: PropFactory.ProvidesStorage);

            if (!pGen.IsEmpty)
            {
                pGen.CleanUp(doTypedCleanup: false);

                if(!_theStore.TryRemove(_ourObjectId, propertyName, out PropGen foundValue) )
                {
                    System.Diagnostics.Debug.WriteLine($"The prop was found, but could not be removed. Property: {propertyName}.");

                }
                //tVals.Remove(propertyName);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Could not remove property: {propertyName}.");
            }

        }
        
        protected void RemoveProp<T>(string propertyName)
        {
            bool mustBeRegistered = TypeSafetyMode == PropBagTypeSafetyMode.Locked;

            PropGen genProp = GetGenPropPrivate<T>(propertyName, mustBeRegistered: mustBeRegistered, neverCreate: true);

            if(!genProp.IsEmpty)
            {
                genProp.CleanUp(doTypedCleanup: true);

                if (!_theStore.TryRemove(_ourObjectId, propertyName, out PropGen foundValue))
                {
                    System.Diagnostics.Debug.WriteLine($"The prop was found, but could not be removed. Property: {propertyName}.");

                }

                //tVals.Remove(propertyName);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Could not remove property: {propertyName}.");
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
        protected bool RegisterDoWhenChanged<T>(Action<T, T> doWhenChanged, bool doAfterNotify, string propertyName)
        {
            IPropPrivate<T> prop = GetTypedPropPrivate<T>(propertyName, mustBeRegistered: true);
            return prop.UpdateDoWhenChangedAction(doWhenChanged, doAfterNotify);
        }

        protected void ClearAllProps()
        {
            // TODO: Fix Me.
            //DelegateCacheProvider.DoSetDelegateCache.Clear();
            //doSetDelegateDict.Clear();
            ClearEventSubscribers();


            //tVals.Clear();
            (_theStore as IDictionary<ulong, PropGen>).Clear();
        }

        protected void ClearEventSubscribers()
        {
            var theStoreAsCollection = _theStore as ICollection<KeyValuePair<ulong, PropGen>>;

            foreach (var x in theStoreAsCollection)
            {
                x.Value.CleanUp(doTypedCleanup: true);
            }
            //foreach (var x in tVals.Values)
            //{
            //    x.CleanUp(doTypedCleanup: true);
            //}
        }

        /// <summary>
        /// Makes a copy of the core list.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, ValPlusType> GetAllPropNamesAndTypes()
        {
            var theStoreAsCollection = _theStore as ICollection<KeyValuePair<ulong, PropGen>>;

            IEnumerable<KeyValuePair<string, ValPlusType>> list = theStoreAsCollection.Select(x =>
            new KeyValuePair<string, ValPlusType>(GetPropNameFromCKey(x.Key), x.Value.ValuePlusType())).ToList();

            //IEnumerable<KeyValuePair<string, ValPlusType>> list =
            //    tVals.Select(x => new KeyValuePair<string, ValPlusType>(x.Key, x.Value.ValuePlusType())).ToList();

            IDictionary <string, ValPlusType> result = list.ToDictionary(pair => pair.Key, pair => pair.Value);
            return result; 
        }

        /// <summary>
        /// Returns all of the values in dictionary of objects, keyed by PropertyName.
        /// </summary>
        public IDictionary<string, object> GetAllPropertyValues()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            var theStoreAsCollection = _theStore as ICollection<KeyValuePair<ulong, PropGen>>;

            //foreach (KeyValuePair<string, PropGen> kvp in tVals)
            //{
            //    result.Add(kvp.Key, kvp.Value.Value);
            //}

            foreach(KeyValuePair<ulong, PropGen> kvp in theStoreAsCollection)
            {
                result.Add(GetPropNameFromCKey(kvp.Key), kvp.Value.Value);
            }
            return result;
        }

        public IList<string> GetAllPropertyNames()
        {
            //List<string> result = new List<string>();

            //foreach (var y in tVals.Keys)
            //{
            //    result.Add(y);
            //}

            var theStoreAsDict = _theStore as IDictionary<ulong, PropGen>;
            List<string> result = theStoreAsDict.Select(x => GetPropNameFromCKey(x.Key)).ToList();
            return result;
        }

        #endregion

        #region Private Methods and Properties

        private bool DoSet<T>(T newValue, string propertyName, IPropPrivate<T> prop)
        {
            if (!prop.ValueIsDefined)
            {
                // Update and only raise the standard OnPropertyChanged
                // Since there's no way to pass an undefined value to the other OnPropertyChanged event subscribers.
                prop.TypedValue = newValue;

                // Raise the standard PropertyChanged event
                OnPropertyChanged(PropFactory.IndexerName);
                OnPropertyChangedIndividual(propertyName);
                return true; // If it was originally unasigned, then it will always be updated.
            }
            else
            {
                bool theSame = prop.CompareTo(newValue);

                if (!theSame)
                {
                    // Save the value before the update.
                    T oldValue = prop.TypedValue;

                    OnPropertyChanging(PropFactory.IndexerName);

                    // Make the update.
                    prop.TypedValue = newValue;

                    // Raise notify events.
                    DoNotifyWork(oldValue, newValue, propertyName, prop);
                }
                return !theSame;
            }
        }

        private void DoNotifyWork<T>(T oldVal, T newValue, string propertyName, IPropPrivate<T> prop)
        {
            if (prop.DoAfterNotify)
            {
                // Raise the standard PropertyChanged event
                OnPropertyChanged(PropFactory.IndexerName);

                // Raise the individual PropertyChanged event
                OnPropertyChangedIndividual(propertyName);

                // The typed, PropertyChanged event defined on the individual property.
                prop.OnPropertyChangedWithTVals(propertyName, oldVal, newValue);

                // The un-typed, PropertyChanged event defined on the individual property.
                prop.OnPropertyChangedWithVals(propertyName, oldVal, newValue);

                //// The un-typed, PropertyChanged shared event.
                OnPropertyChangedWithVals(propertyName, typeof(T), oldVal, newValue);

                // then perform the call back.
                prop.DoWhenChanged(oldVal, newValue);
            }
            else
            {
                // Peform the call back,
                prop.DoWhenChanged(oldVal, newValue);

                // Raise the standard PropertyChanged event
                OnPropertyChanged(PropFactory.IndexerName);

                // Raise the individual PropertyChanged event
                OnPropertyChangedIndividual(propertyName);

                // The typed, PropertyChanged event defined on the individual property.
                prop.OnPropertyChangedWithTVals(propertyName, oldVal, newValue);

                // The un-typed, PropertyChanged event defined on the individual property.
                prop.OnPropertyChangedWithVals(propertyName, oldVal, newValue);

                // The un-typed, PropertyChanged shared event.
                OnPropertyChangedWithVals(propertyName, typeof(T),  oldVal, newValue);
            }
        }

        protected PropGen GetPropGen(string propertyName, Type propertyType,
            out bool wasRegistered, bool haveValue, object value,
            bool alwaysRegister, bool mustBeRegistered, bool neverCreate, bool? desiredHasStoreValue)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName", "PropertyName is null on call to GetValue.");

            Debug.Assert(!(alwaysRegister && mustBeRegistered), "AlwaysRegister and MustBeRegistered cannot both be true.");
            Debug.Assert(!(alwaysRegister && neverCreate), "AlwaysRegister and NeverCreate cannot both be true.");

            PropGen genProp;

            //try
            //{
            //genProp = tVals[propertyName];
            //    wasRegistered = false;
            //}
            //catch (KeyNotFoundException)
            //{
            //    genProp = this.HandleMissingProp(propertyName, propertyType, out wasRegistered, haveValue, value, alwaysRegister, mustBeRegistered, neverCreate);
            //}

            if (_theStore.TryGetValue(_ourObjectId, propertyName, out genProp))
            {
                wasRegistered = false;
            }
            else
            {
                genProp = this.HandleMissingProp(propertyName, propertyType, out wasRegistered, haveValue, value, alwaysRegister, mustBeRegistered, neverCreate);
            }

            if (!genProp.IsEmpty && desiredHasStoreValue.HasValue && desiredHasStoreValue.Value != genProp.HasStore)
            {
                if (desiredHasStoreValue.Value)
                    //Caller needs property to have a backing store.
                    throw new InvalidOperationException(string.Format("Property: {0} has no backing store held by this instance of PropBag. This operation can only be performed on properties for which a backing store is held by this instance.", propertyName));
                else
                    throw new InvalidOperationException(string.Format("Property: {0} has a backing store held by this instance of PropBag. This operation can only be performed on properties for which no backing store is kept by this instance.", propertyName));
            }

            return genProp;
        }

        //private IPropPrivate<T> CheckTypeInfo<T>(PropGen genProp, string propertyName, IDictionary<string, PropGen> dict)

        private IPropPrivate<T> CheckTypeInfo<T>(PropGen genProp, string propertyName, IDictionary<ulong, PropGen> dict)
        {
            if (!genProp.TypeIsSolid)
            {
                try
                {
                    if (MakeTypeSolid(ref genProp, typeof(T), propertyName))
                    {
                        dict[genProp.PropId] = genProp;
                    }
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

            return (IPropPrivate<T>)genProp.TypedProp;
        }

        /// <summary>
        /// Returns Null if no update was required,
        /// otherwise it returns the new PropGen object.
        /// </summary>
        /// <param name="genProp"></param>
        /// <param name="newType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private bool MakeTypeSolid(ref PropGen genProp, Type newType, string propertyName)
        {
            //Type currentType = genProp.Type;

            Debug.Assert(genProp.Value == null, "The current value of the property should be null when MakeTypeSolid is called.");

            System.Type underlyingtype = Nullable.GetUnderlyingType(newType);

            // Check to see if the new type is a non-nullable, value type.
            // If it is, then it was invalid to set this property to null in the first place.
            // Consider setting value to the type's default value instead of throwing this exception.
            if (underlyingtype == null && newType.IsValueType)
                throw new InvalidCastException("The new type is a non-nullable value type.");

            // We are using strict equality here, since we have the oportunity to update the type to anything
            // that is assignable from a value of type object (which is everything.)
            if (newType != genProp.Type)
            {
                // Next statement uses reflection.
                object curValue = genProp.Value;
                PropKindEnum propKind = genProp.TypedProp.PropKind;

                IPropGen typedPropWrapper = PropFactory.CreateGenFromObject(newType, curValue, propertyName, null, true, true, propKind, null, false, null, false);

                //genProp.UpdateWithSolidType(newType, curValue);
                genProp = new PropGen(typedPropWrapper, genProp.PropId);
                return true;
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

        #region Composite Key Support

        private string GetPropNameFromCKey(ulong cKey)
        {
            ulong objectId = _compKeyManager.Split(cKey, out string propertyName);
            return propertyName;
        }

        private ulong GetNewCKey(string propertyName)
        {
            // Register new propertyName and get a Level2 key.
            uint l2Key = _level2KeyManager.Add(propertyName);

            // Create a combined (L1) Key
            ulong cKey = _compKeyManager.Join(_ourObjectId, l2Key);

            return cKey;
        }

        #endregion

        #region Methods to Raise Events

        protected void OnPropertyChangedIndividual(string propertyName)
        {
            PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChangedIndividual, null, null);

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

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

        protected void OnPropertyChangedWithVals(string propertyName, Type propertyType, object oldVal, object newVal)
        {
            EventHandler<PCGenEventArgs> handler = Interlocked.CompareExchange(ref PropertyChangedWithGenVals, null, null);

            if (handler != null)
                handler(this, new PCGenEventArgs(propertyName, propertyType, oldVal, newVal));
        }

        #endregion

        #region IListSource Support

        public bool TryGetPropType(string propertyName, out PropKindEnum propType)
        {
            //if (tVals.TryGetValue(propertyName, out PropGen value))
            //{
            //    propType = value.TypedProp.PropKind;
            //    return true;
            //}
            //else
            //{
            //    propType = PropKindEnum.Prop;
            //    return false;
            //}

            if (_theStore.TryGetValue(_ourObjectId, propertyName, out PropGen value))
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
            //if (tVals.TryGetValue(propertyName, out PropGen value))
            //{
            //    listSource = value.TypedProp.ListSource;
            //    return true;
            //}
            //else
            //{
            //    listSource = null;
            //    return false;
            //}

            if (_theStore.TryGetValue(_ourObjectId, propertyName, out PropGen value))
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

        #region Generic Method Support

        /// <summary>
        /// Used to create Delegates when the type of the value is not known at run time.
        /// </summary>
        static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        // Method Templates for Property Bag
        internal static class GenericMethodTemplates
        {
            static Lazy<MethodInfo> theSingleGenericDoSetBridgeMethodInfo;
            static MethodInfo GenericDoSetBridgeMethodInfo { get { return theSingleGenericDoSetBridgeMethodInfo.Value; } }

            static Lazy<MethodInfo> theSingleGenGetTypedCollMethodInfo;
            static MethodInfo GenGetTypedCollMethodInfo { get { return theSingleGenGetTypedCollMethodInfo.Value; } }

            static GenericMethodTemplates()
            {
                theSingleGenericDoSetBridgeMethodInfo = new Lazy<MethodInfo>(() =>
                    GMT_TYPE.GetMethod("DoSetBridge", BindingFlags.Static | BindingFlags.NonPublic),
                    LazyThreadSafetyMode.PublicationOnly);

                theSingleGenGetTypedCollMethodInfo = new Lazy<MethodInfo>(() =>
                    GMT_TYPE.GetMethod("GetTypedCollection", BindingFlags.Static | BindingFlags.NonPublic),
                    LazyThreadSafetyMode.PublicationOnly);
            }

            static bool DoSetBridge<T>(object value, PropBag target, string propertyName, IPropGen prop)
            {
                return target.DoSet<T>((T)value, propertyName, (IPropPrivate<T>)prop.TypedProp);
            }

            public static DoSetDelegate GetDoSetDelegate(Type typeOfThisValue)
            {
                MethodInfo methInfoSetProp = GenericMethodTemplates.GenericDoSetBridgeMethodInfo.MakeGenericMethod(typeOfThisValue);
                DoSetDelegate result = (DoSetDelegate)Delegate.CreateDelegate(typeof(DoSetDelegate), methInfoSetProp);

                return result;
            }

            static IList GetTypedCollection<T>(PropBag source, string propertyName)
            {
                ObservableCollection<T> result = source.GetIt<ObservableCollection<T>>(propertyName);
                return result;
            }

            public static GetTypedCollectionDelegate GetTheGetTypedCollectionDelegate(Type typeOfThisValue)
            {
                MethodInfo methInfoSetProp = GenericMethodTemplates.GenGetTypedCollMethodInfo.MakeGenericMethod(typeOfThisValue);
                GetTypedCollectionDelegate result = (GetTypedCollectionDelegate)Delegate.CreateDelegate(typeof(GetTypedCollectionDelegate), methInfoSetProp);

                return result;
            }

        }

        #endregion

    }
}
