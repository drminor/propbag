using DRM.PropBag.Caches;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;

namespace DRM.PropBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PropIdType = UInt32;
    using PropNameType = String;
    using IRegisterBindingsFowarderType = IRegisterBindingsForwarder<UInt32>;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;

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

    public partial class PropBag : IPropBag, IPropBagInternal, IRegisterBindingsFowarderType
    {
        #region Member Declarations

        // These items are provided to us.
        private IPropFactory _propFactory { get; set; }
        private PSAccessServiceType _ourStoreAccessor { get; set; }

        // We are responsible for these
        private ITypeSafePropBagMetaData _ourMetaData { get; set; }
        protected virtual ITypeSafePropBagMetaData OurMetaData { get { return _ourMetaData; } set { _ourMetaData = value; } }
        private PropBagTypeSafetyMode _typeSafetyMode { get; }

        private object _sync = new object();

        // These fulfill the IPropBagInternal contract
        public PSAccessServiceType /*IPropBagInternal.*/ItsStoreAccessor => _ourStoreAccessor;

        #endregion

        #region Public Events and Properties

        public string FullClassName => OurMetaData.FullClassName;
        public IPropFactory PropFactory => _propFactory;
        public PropBagTypeSafetyMode TypeSafetyMode => _typeSafetyMode;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (_sync)
                {
                    SubscribeToGlobalPropChanged(value, unregister: false);
                }
            }
            remove
            {
                if (disposedValue) return;
                lock (_sync)
                {
                    SubscribeToGlobalPropChanged(value, unregister: true);
                }
            }
        }

        public event PropertyChangingEventHandler PropertyChanging
        {
            add
            {
                lock (_sync)
                {
                    SubscribeToGlobalPropChanging(value, unregister: false);
                }
            }
            remove
            {
                if (disposedValue) return;
                lock (_sync)
                {
                    SubscribeToGlobalPropChanging(value, unregister: true);
                }
            }
        }

        public event EventHandler<PcGenEventArgs> PropertyChangedWithGenVals
        {
            add
            {
                lock (_sync)
                {
                    SubscribeToGlobalPropChanged(value, unregister: false);
                    //WeakEventManager<PB_EventHolder, PcGenEventArgs>.AddHandler(_eventHolder, "PropertyChangedWithGenVals", value);
                }
            }
            remove
            {
                if (disposedValue) return;
                lock (_sync)
                {
                    SubscribeToGlobalPropChanged(value, unregister: true);
                    //WeakEventManager<PB_EventHolder, PcGenEventArgs>.RemoveHandler(_eventHolder, "PropertyChangedWithGenVals", value);
                }
            }
        }

        public event EventHandler<PcObjectEventArgs> PropertyChangedWithObjectVals
        {
            add
            {
                lock (_sync)
                {
                    SubscribeToGlobalPropChanged(value, unregister: false);
                }
            }
            remove
            {
                if (disposedValue) return;
                lock (_sync)
                {
                    SubscribeToGlobalPropChanged(value, unregister: true);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pm"></param>
        /// <param name="propFactory">The PropFactory to use instead of the one specified by the PropModel.</param>
        public PropBag(ControlModel.PropModel pm, string fullClassName = null, IPropFactory propFactory = null)
            : this(pm.TypeSafetyMode, propFactory ?? pm.PropFactory, fullClassName)
        {
            Hydrate(pm);
            int testc = _ourStoreAccessor.PropertyCount;
        }

        protected PropBag(IPropBag copySource)
            : this(copySource.TypeSafetyMode, copySource.PropFactory, copySource.FullClassName)
        {
            CloneProps(copySource);
        }

        protected PropBag(PropBagTypeSafetyMode typeSafetyMode, IPropFactory propFactory, string fullClassName = null)
        {
            _typeSafetyMode = typeSafetyMode;
            _propFactory = propFactory ?? throw new ArgumentNullException(nameof(propFactory));

            _ourMetaData = BuildMetaData(_typeSafetyMode, fullClassName, _propFactory);

            _ourStoreAccessor = _propFactory.CreatePropStoreService(this);
        }

        protected ITypeSafePropBagMetaData BuildMetaData(PropBagTypeSafetyMode typeSafetyMode, string classFullName, IPropFactory propFactory)
        {
            classFullName = classFullName ?? GetFullTypeNameOfThisInstance();
            ITypeSafePropBagMetaData result = new TypeSafePropBagMetaData(classFullName, typeSafetyMode, propFactory);
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
                    System.Diagnostics.Debug.WriteLine("We are creating prop item: PersonList.");
                }

                //if (pi.PropKind == PropKindEnum.ObservableCollectionFB)
                //{
                //    System.Diagnostics.Debug.WriteLine("Processing the PersonList Prop Item.");
                //}

                PropIdType propId;
                IPropData propItem;
                IProp typedProp;

                if (pi.PropKind == PropKindEnum.CollectionViewSource)
                {
                    // Get the name of the Collection-Type PropItem that provides the data for this CollectionViewSource.
                    string srcPropName = pi.BinderField.Path;

                    //FetchData_Test(srcPropName, pi.PropertyType);

                    // TODO: Consider creating a local cache of CViewManagers, keyed by srcPropName.
                    // This cache may be handy since several properties may reference the same CViewManager for a 
                    // given instance of a PropBag.
                    if (TryGetViewManager(this, srcPropName, pi.PropertyType, out IManageCViews cViewManager))
                    {
                        IProvideAView viewProvider = cViewManager.GetViewProvider();

                        typedProp = _propFactory.CreateCVSProp(pi.PropertyName, viewProvider);
                        propItem = AddProp(pi.PropertyName, typedProp, out propId);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Could not retrieve the (generic) CViewManager for source PropItem: {srcPropName}.");
                    }
                }
                else if(pi.PropKind == PropKindEnum.CollectionView)
                {
                    // Get the name of the Collection-Type PropItem that provides the data for this CollectionViewSource.
                    string srcPropName = pi.BinderField?.Path;

                    //FetchData_Test(srcPropName, pi.PropertyType);

                    IProvideAView viewProvider;
                    if (srcPropName != null)
                    {
                        if (TryGetViewManager(this, srcPropName, pi.PropertyType, out IManageCViews cViewManager))
                        {
                            viewProvider = cViewManager.GetViewProvider();
                        }
                        else
                        {
                            throw new InvalidOperationException($"Could not retrieve the (generic) CViewManager for source PropItem: {srcPropName}.");
                        }
                    }
                    else
                    {
                        viewProvider = null;
                    }

                    typedProp = _propFactory.CreateCVProp(pi.PropertyName, viewProvider);
                    propItem = AddProp(pi.PropertyName, typedProp, out propId);
                }
                else
                {
                    propItem = processProp(pi, out propId);
                }

                if (pi.BinderField?.Path != null)
                {
                    if (pi.PropKind == PropKindEnum.CollectionView)
                    {
                        //throw new InvalidOperationException($"PropItems of kind = {pi.PropKind} cannot have a local binding.");
                    }
                    else if (pi.PropKind == PropKindEnum.CollectionViewSource || pi.PropKind == PropKindEnum.CollectionViewSource_RO)
                    {
                        //throw new InvalidOperationException($"PropItems of kind = {pi.PropKind} cannot have a local binding.");
                    }
                    else
                    {
                        LocalBindingInfo bindingInfo = new LocalBindingInfo(new LocalPropertyPath(pi.BinderField.Path));
                        propItem.TypedProp.RegisterBinding((IRegisterBindingsFowarderType)this, propId, bindingInfo);
                    }
                }


            }
        }

        public void FetchData_Test(string pName, Type pType)
        {
            IProvideAView viewProvider = null;
            ICollectionView lcv = null;

            IProvideAView viewProvider2 = null;
            ICollectionView lcv2 = null;

            if (TryGetViewManager(this, pName, pType, out IManageCViews cViewManager))
            {
                viewProvider = cViewManager.GetViewProvider();
                lcv = viewProvider.View;
            }

            if (TryGetViewManager(this, pName, pType, out IManageCViews cViewManager2))
            {
                viewProvider2 = cViewManager2.GetViewProvider();
                lcv2 = viewProvider2.View;
            }

            bool test1 = ReferenceEquals(cViewManager, cViewManager2);
            bool test2 = ReferenceEquals(cViewManager.DataSourceProvider, cViewManager2.DataSourceProvider);
            bool test3 = ReferenceEquals(viewProvider, viewProvider2);
            bool test4 = ReferenceEquals(lcv, lcv2);

            //var x = cvs.View;

            //ListCollectionView ff = GetIt<ListCollectionView>("PersonListView");

            //IEnumerable gg = ff?.SourceCollection;

            //if(gg != null)
            //{
            //    List<object> hh = new List<object>(gg.Cast<object>());

            //    int cnt = hh.Count;
            //}



            System.Diagnostics.Debug.WriteLine("You may want to set a break point here.");

            //ICollectionView icv = cvs?.View;

            //bool testC = ReferenceEquals(cvsPrior, cvs);
            //bool testI = ReferenceEquals(icvPrior, icv);

            //if (cvs.View is ListCollectionView lcv)
            //{
            //    SetIt<ListCollectionView>(lcv, "PersonListView");
            //}
            //else
            //{
            //    System.Diagnostics.Debug.WriteLine("The default view of the CollectionViewSource: CVS does not implement ListCollectionView.");
            //    SetIt<ListCollectionView>(null, "PersonListView");
            //}

        }

        private IPropData processProp(PropItem pi, out PropIdType propId)
        {
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

            //EventHandler<PcGenEventArgs> doWhenChangedAction = pi.DoWhenChangedField?.DoWhenChangedAction;

            //if(doWhenChangedAction == null && (pi.DoWhenChangedField?.DoWhenGenHandlerGetter != null))
            //{
            //    Func<object, EventHandler<PcGenEventArgs>> rr = pi.DoWhenChangedField.DoWhenGenHandlerGetter;

            //    doWhenChangedAction = rr(this);
            //}

            IProp propGen;

            if (pi.StorageStrategy == PropStorageStrategyEnum.Internal && !pi.InitialValueField.SetToUndefined)
            {
                if (pi.InitialValueField.ValueCreator != null)
                {
                    object newValue = pi.InitialValueField.ValueCreator();
                    propGen = _propFactory.CreateGenFromObject(pi.PropertyType, newValue, pi.PropertyName, ei, pi.StorageStrategy, pi.TypeIsSolid,
                        pi.PropKind, comparer, useRefEquality, pi.ItemType);
                }
                else
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

                    propGen = _propFactory.CreateGenFromString(pi.PropertyType, value, useDefault, pi.PropertyName, ei, pi.StorageStrategy, pi.TypeIsSolid,
                        pi.PropKind, comparer, useRefEquality, pi.ItemType);
                }
            }
            else
            {
                propGen = _propFactory.CreateGenWithNoValue(pi.PropertyType, pi.PropertyName, ei, pi.StorageStrategy, pi.TypeIsSolid,
                    pi.PropKind, comparer, useRefEquality, pi.ItemType);
            }

            // If DoAfterNotify is true, use the 'Last' group, otherwise use the 'standard' group.
            //SubscriptionPriorityGroup priorityGroup = pi.DoWhenChangedField?.DoAfterNotify ?? false ? SubscriptionPriorityGroup.Last : SubscriptionPriorityGroup.Standard;

            IPropData propData;
            if (pi.DoWhenChangedField != null)
            {
                object target;
                if (pi.DoWhenChangedField.MethodIsLocal)
                {
                    target = this;
                }
                else
                {
                    throw new NotSupportedException("Only local methods are supported.");
                }
                propData = AddProp(pi.PropertyName, propGen,
                    target,
                    pi.DoWhenChangedField.Method,
                    pi.DoWhenChangedField.SubscriptionKind,
                    pi.DoWhenChangedField.PriorityGroup, out propId);
            }
            else
            {
                propData = AddProp(pi.PropertyName, propGen, out propId);
            }

            return propData;

        }

        #endregion

        #region Missing Prop Handler

        // This will always return a valid value -- or throw an exception,
        // unless neverCreate is set, in which case it will
        // never throw an exception and always return an empty PropGen.
        private IPropData HandleMissingProp(PropNameType propertyName, Type propertyType, out bool wasRegistered,
            bool haveValue, object value, bool alwaysRegister, bool mustBeRegistered, bool neverCreate, [CallerMemberName] string nameOfCallingMethod = null)
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
                case ReadMissingPropPolicyEnum.NotAllowed:
                    {
                        if (OurMetaData.AllPropsMustBeRegistered)
                        {
                            ReportAccessToMissing(propertyName, nameOfCallingMethod ?? nameof(HandleMissingProp));
                            throw new InvalidOperationException("ReportAccessToMissing did not raise an exception.");
                            //throw new InvalidOperationException(string.Format("Property: {0} has not been declared by calling AddProp. Cannot use this method in this case. Declare by calling AddProp.", propertyName));
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format("No property: {0} exists in this PropBag.", propertyName));
                        }
                    }
                case ReadMissingPropPolicyEnum.Register:
                    {
                        // TODO: Must determine the PropKind for this new property.
                        IProp genericTypedProp;

                        if (haveValue)
                        {
                            bool typeIsSolid = _propFactory.IsTypeSolid(value, propertyType);

                            PropStorageStrategyEnum storageStrategy = _propFactory.ProvidesStorage ? PropStorageStrategyEnum.Internal : PropStorageStrategyEnum.External;

                            // TODO Create a Typed version of HandleMissingProp.
                            genericTypedProp = _propFactory.CreateGenFromObject(propertyType, value,
                                propertyName, null, storageStrategy, typeIsSolid, PropKindEnum.Prop, null, false, null);
                        }
                        else
                        {
                            // TODO: This must set the value of the property to the default for its type,
                            // in order to satisfy TypeSafety modes: 'RegisterOnGetLoose' and 'RegisterOnGetSafe.'

                            //object newValue = ThePropFactory.GetDefaultValue(propertyType, propertyName);
                            bool typeIsSolid = true;

                            PropStorageStrategyEnum storageStrategy = _propFactory.ProvidesStorage ? PropStorageStrategyEnum.Internal : PropStorageStrategyEnum.External;

                            // On 10/8/17: Changed to use NoValue, instead of trying to generate a default value.
                            genericTypedProp = _propFactory.CreateGenWithNoValue(propertyType, propertyName,
                                null, storageStrategy, typeIsSolid, PropKindEnum.Prop, null, false, null);
                        }

                        IPropData propGen = AddProp(propertyName, genericTypedProp, out PropIdType propId2);

                        wasRegistered = true;
                        return propGen;
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

            propGen = GetPropGen(propertyName, propertyType, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: true,
                desiredHasStoreValue: _propFactory.ProvidesStorage,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if(!propGen.IsEmpty)
            {
                return true;
            }
            //else if(_typeSafetyMode == PropBagTypeSafetyMode.Tight)
            //{
            //    return ReportAccessToMissing(propertyName, nameof(TryGetPropGen));
            //}
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

            IPropData PropData = GetPropGen(propertyName, propertyType, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: true,
                neverCreate: false,
                desiredHasStoreValue: null, // _propFactory.ProvidesStorage,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

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
            return pg.TypedProp.GetValuePlusType(); //pg.GetValuePlusType();
        } 

        public T GetIt<T>(string propertyName)
        {
            //OurStoreAccessor.IncAccess();
            IProp<T> typedProp = GetTypedPropPrivate<T>(propertyName, mustBeRegistered: true, neverCreate: false);
            return typedProp.TypedValue;
        }

        protected IProp<T> GetTypedProp<T>(string propertyName)
        {
            return (IProp<T>)GetTypedPropPrivate<T>(propertyName, mustBeRegistered: true, neverCreate: false);
        }

        protected IProp<T> GetTypedPropPrivate<T>(string propertyName, bool mustBeRegistered, bool neverCreate/* = false*/)
        {
            IPropData PropData = GetGenPropPrivate<T>(propertyName, mustBeRegistered, neverCreate, out PropIdType notUsed);

            if (!PropData.IsEmpty)
            {
                return (IProp<T>)PropData.TypedProp;
            }
            else
            {
                return null;
            }
        }

        protected IReadOnlyCProp<CT,T> GetCProp<CT, T>(string propertyName) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
        {
            return (IReadOnlyCProp<CT,T>)GetTypedCPropPrivate<CT,T>(propertyName, mustBeRegistered: true);
        }

        protected ICProp<CT,T> GetTypedCPropPrivate<CT,T>(string propertyName, bool mustBeRegistered, bool neverCreate = false) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
        {
            IPropData PropData = GetGenPropPrivate<T>(propertyName, mustBeRegistered, neverCreate, out PropIdType notUsed);

            if (!PropData.IsEmpty)
            {
                return (ICProp<CT,T>)  PropData.TypedProp;
            }
            else
            {
                return null;
            }
        }

        private IPropData GetGenPropPrivate<T>(string propertyName, bool mustBeRegistered, bool neverCreate, out PropIdType propId)
        {
            PropStorageStrategyEnum storageStrategy = _propFactory.ProvidesStorage ? PropStorageStrategyEnum.Internal : PropStorageStrategyEnum.External;

            // TODO: Make this use a different version of GetPropGen: one that takes advantage of the 
            // compile-time type knowlege -- especially if we have to register the property in HandleMissing.

            IPropData PropData = GetPropGen(propertyName, typeof(T), haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: neverCreate,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out propId);

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

            IPropData PropData = GetPropGen(propertyName, propertyType, haveValue: true, value: value,
                alwaysRegister: alwaysRegister,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: _propFactory.ProvidesStorage,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

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

            ICacheDelegates<DoSetDelegate> dc = _propFactory.DelegateCacheProvider.DoSetDelegateCache;
            DoSetDelegate setPropDel = dc.GetOrAdd(PropData.TypedProp.Type);
            return setPropDel(this, propId, propertyName, PropData.TypedProp, value);
        }

        //public bool SetIt<T>(T value, PropIdType propId)
        //{
        //    PropNameType propertyName = GetPropName(propId); 
        //    IPropData PropData = GetPropGen<T>(propertyName, out PropIdType dummy, desiredHasStoreValue: PropFactory.ProvidesStorage);

        //    IProp<T> prop = CheckTypeInfo<T>(propId, propertyName, PropData, OurStoreAccessor);

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
            IPropData PropData = GetPropGen(propertyName, typeof(T), haveValue: true, value: value,
                alwaysRegister: alwaysRegister,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: _propFactory.ProvidesStorage,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            // No point in calling DoSet, it would find that the value is the same and do nothing.
            if (wasRegistered) return true;

            IProp<T> prop = CheckTypeInfo<T>(propId, propertyName, PropData, _ourStoreAccessor);
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

            IPropData PropData = GetPropGen(propertyName, typeof(T), haveValue: true, value: newValue,
                alwaysRegister: alwaysRegister,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: false,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            // No point in calling DoSet, it would find that the value is the same and do nothing.
            if (wasRegistered) return true;

            IProp<T> typedProp = CheckTypeInfo<T>(propId, propertyName, PropData, _ourStoreAccessor);

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

        #region Subscribe to Property Changing

        public bool SubscribeToGlobalPropChanging(PropertyChangingEventHandler eventHandler, bool unregister)
        {
            uint propId = 0;

            bool result;
            if (unregister)
            {
                result = _ourStoreAccessor.UnregisterHandler(this, propId, eventHandler/*, SubscriptionPriorityGroup.Standard, keepRef: false*/);
            }
            else
            {
                result = _ourStoreAccessor.RegisterHandler(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
            }
            return result;
        }

        public bool SubscribeToPropChanging(PropertyChangingEventHandler handler,
            string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;
            IPropData PropData = GetPropGen(propertyName, propertyType, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (!PropData.IsEmpty)
            {
                bool wasAdded = _ourStoreAccessor.RegisterHandler(this, propId, handler, priorityGroup: SubscriptionPriorityGroup.Standard, keepRef: false);
                //WeakEventManager<INotifyPropertyChanging, PropertyChangingEventArgs>.AddHandler(this, "PropertyChanging", handler);
                return wasAdded;
            }
            return false;
        }

        public bool UnsubscribeToPropChanging(PropertyChangingEventHandler handler,
            string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;
            IPropData PropData = GetPropGen(propertyName, propertyType, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (!PropData.IsEmpty)
            {
                bool wasRemoved = _ourStoreAccessor.UnregisterHandler(this, propId, handler);
                return wasRemoved;
            }
            return false;
        }

        #endregion

        #region Subscribe to Property Changed

        public bool SubscribeToGlobalPropChanged(PropertyChangedEventHandler eventHandler, bool unregister)
        {
            uint propId = 0;

            bool result;
            if (unregister)
            {
                result = _ourStoreAccessor.UnregisterHandler(this, propId, eventHandler/*, SubscriptionPriorityGroup.Standard, keepRef: false*/);
            }
            else
            {
                result = _ourStoreAccessor.RegisterHandler(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
            }
            return result;
        }

        public bool SubscribeToPropChanged(PropertyChangedEventHandler handler,
            string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;
            IPropData PropData = GetPropGen(propertyName, propertyType, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (!PropData.IsEmpty)
            {
                _ourStoreAccessor.RegisterHandler(this, propId, handler, SubscriptionPriorityGroup.Standard, keepRef: false);
                //PropertyChangedEventManager.AddHandler(this, handler, propertyName);
                return true;
            }
            return false;
        }

        public bool UnsubscribeToPropChanged(PropertyChangedEventHandler handler,
            string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;
            IPropData PropData = GetPropGen(propertyName, propertyType, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (!PropData.IsEmpty)
            {
                _ourStoreAccessor.RegisterHandler(this, propId, handler, SubscriptionPriorityGroup.Standard, keepRef: false);
                //PropertyChangedEventManager.RemoveHandler(this, handler, propertyName);
                return true;
            }
            return false;
        }

        #endregion

        #region Subscribe to Typed PropertyChanged

        public bool SubscribeToGlobalPropChanged<T>(EventHandler<PcTypedEventArgs<T>> eventHandler, bool unregister)
        {
            uint propId = 0;

            bool result;
            if (unregister)
            {
                result = _ourStoreAccessor.UnregisterHandler(this, propId, eventHandler/*, SubscriptionPriorityGroup.Standard, keepRef: false*/);
            }
            else
            {
                IDisposable disable = _ourStoreAccessor.RegisterHandler(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
                result = disable != null;
            }
            return result;
        }

        public IDisposable SubscribeToPropChanged<T>(EventHandler<PcTypedEventArgs<T>> eventHandler, string propertyName)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData propData = GetPropGen<T>(propertyName,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (propData != null)
            {
                IDisposable disable = _ourStoreAccessor.RegisterHandler<T>(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
                return disable;
            }
            else
            {
                return null;
            }
        }

        public bool UnSubscribeToPropChanged<T>(EventHandler<PcTypedEventArgs<T>> eventHandler, string propertyName)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData propData = GetPropGen<T>(propertyName,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (propData != null && _ourStoreAccessor != null)
            {
                bool result = _ourStoreAccessor.UnregisterHandler<T>(this, propId, eventHandler /*, SubscriptionPriorityGroup.Standard, keepRef: false*/);
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
        protected bool AddToPropChanged<T>(EventHandler<PcTypedEventArgs<T>> eventHandler, string eventPropertyName)
        {
            string propertyName = GetPropNameFromEventProp(eventPropertyName);
            IDisposable disable = SubscribeToPropChanged<T>(eventHandler, propertyName);

            return disable != null;
        }

        /// <summary>
        /// Uses the name of the property or event accessor of the calling method to indentify the property,
        /// if the propertyName argument is not specifed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="eventPropertyName"></param>
        protected bool RemoveFromPropChanged<T>(EventHandler<PcTypedEventArgs<T>> eventHandler, string eventPropertyName)
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

        public bool SubscribeToGlobalPropChanged(EventHandler<PcGenEventArgs> eventHandler, bool unregister)
        {
            uint propId = 0;

            bool result;
            if(unregister)
            {
                result = _ourStoreAccessor.UnregisterHandler(this, propId, eventHandler/*, SubscriptionPriorityGroup.Standard, keepRef: false*/);
            }
            else
            {
                IDisposable disable = _ourStoreAccessor.RegisterHandler(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
                return disable != null;
                //result = _ourStoreAccessor.RegisterHandler(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
            }
            return result;
        }

        public IDisposable SubscribeToPropChanged(EventHandler<PcGenEventArgs> eventHandler, string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData PropData = GetPropGen(propertyName, null, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (!PropData.IsEmpty)
            {
                IDisposable disable = _ourStoreAccessor.RegisterHandler(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
                return disable;
            }
            else
            {
                return null;
            }
        }

        public bool UnSubscribeToPropChanged(EventHandler<PcGenEventArgs> eventHandler, string propertyName, Type propertyType)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData PropData = GetPropGen(propertyName, null, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (!PropData.IsEmpty && _ourStoreAccessor != null)
            {
                bool result = _ourStoreAccessor.UnregisterHandler(this, propId, eventHandler);
                return result;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Subscribe to Object PropertyChanges

        public bool SubscribeToGlobalPropChanged(EventHandler<PcObjectEventArgs> eventHandler, bool unregister)
        {
            uint propId = 0;

            bool result;
            if (unregister)
            {
                result = _ourStoreAccessor.UnregisterHandler(this, propId, eventHandler/*, SubscriptionPriorityGroup.Standard, keepRef: false*/);
            }
            else
            {
                result = _ourStoreAccessor.RegisterHandler(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
            }
            return result;
        }

        public bool SubscribeToPropChanged(EventHandler<PcObjectEventArgs> eventHandler, string propertyName)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData PropData = GetPropGen(propertyName, null, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (!PropData.IsEmpty && _ourStoreAccessor != null)
            {
                bool result = _ourStoreAccessor.RegisterHandler(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
                return result;
            }
            else
            {
                return false;
            }
        }

        public bool UnSubscribeToPropChanged(EventHandler<PcObjectEventArgs> eventHandler, string propertyName)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData PropData = GetPropGen(propertyName, null, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (!PropData.IsEmpty && _ourStoreAccessor != null)
            {
                bool result = _ourStoreAccessor.UnregisterHandler(this, propId, eventHandler);
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
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData propData = GetPropGen<T>(nameOfPropertyToUpdate,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (propData != null)
            {
                LocalPropertyPath lpp = new LocalPropertyPath(pathToSource);
                LocalBindingInfo bindingInfo = new LocalBindingInfo(lpp, LocalBindingMode.OneWay);

                bool wasAdded = RegisterBinding<T>(propId, bindingInfo);
                return wasAdded;
            }
            else
            {
                return false;
            }
        }

        public bool RegisterBinding<T>(PropIdType propId, LocalBindingInfo bindingInfo)
        {
            bool wasAdded = _ourStoreAccessor.RegisterBinding<T>(this, propId, bindingInfo);
            return wasAdded;
        }

        public bool UnregisterBinding<T>(string nameOfPropertyToUpdate, string pathToSource)
        {
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData propData = GetPropGen<T>(nameOfPropertyToUpdate,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (propData != null && _ourStoreAccessor != null)
            {
                LocalPropertyPath lpp = new LocalPropertyPath(pathToSource);
                LocalBindingInfo bindingInfo = new LocalBindingInfo(lpp, LocalBindingMode.OneWay);

                bool wasRemoved = UnregisterBinding<T>(propId, bindingInfo);
                return wasRemoved;
            }
            else
            {
                return false;
            }
        }

        public bool UnregisterBinding<T>(PropIdType propId, LocalBindingInfo bindingInfo)
        {
            bool wasRemoved = _ourStoreAccessor.UnregisterBinding<T>(this, propId, bindingInfo);
            return wasRemoved;
        }

        #endregion

        #region Public Methods

        public object Clone()
        {
            return new PropBag(this);
        }

        public void CloneProps(IPropBag copySource)
        {
            _ourStoreAccessor = _ourStoreAccessor.CloneProps(this, copySource);

            _properties = ((ICustomTypeDescriptor)copySource).GetProperties();
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
            //new KeyValuePair<string, ValPlusType>(x.Key, x.Value.GetValuePlusType())).ToList();
            new KeyValuePair<string, ValPlusType>(x.Key, x.Value.TypedProp.GetValuePlusType())).ToList();

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

            if(TryGetPropId(propertyName, out PropIdType propId))
            {
                bool result = _ourStoreAccessor.ContainsKey(this, propId);
                return result;
            }
            else
            {
                return false;
            }
        }

        // TODO: PropBagTypeSafetyMode.Locked is not honored here.
        public bool TryGetTypeOfProperty(string propertyName, out Type type)
        {
            //PropIdType propId = GetPropId(propertyName);

            if (TryGetPropId(propertyName, out PropIdType propId))
            {
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
            else
            {
                type = null;
                return false;
            }
        }

        public Type GetTypeOfProperty(string propertyName)
        {
            IPropData pGen = GetPropGen(propertyName, null, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: false,
                neverCreate: true,
                desiredHasStoreValue: _propFactory.ProvidesStorage,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

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

        public bool TryGetPropType(string propertyName, out PropKindEnum propType)
        {
            if (TryGetPropId(propertyName, out PropIdType propId))
            {
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
            else
            {
                propType = PropKindEnum.Prop;
                return false;
            }
        }

        public override string ToString()
        {
            IDictionary<string, ValPlusType> x = GetAllPropNamesAndTypes();

            StringBuilder result = new StringBuilder();
            int cnt = 0;
            foreach (KeyValuePair<string, ValPlusType> kvp in x)
            {
                if (cnt++ == 0) result.Append("\n\r");

                result.Append($" -- {kvp.Key}: {kvp.Value.Value}");
            }
            return result.ToString();
        }

        #endregion

        #region Add Enumerable-Type Props
        #endregion

        #region Add ObservableCollection<T> and IObsCollection<T> Props

        public ICProp<CT, T> AddCollectionProp<CT, T>
            (
                string propertyName,
                Func<CT, CT, bool> comparer = null,
                object extraInfo = null,
                CT initialValue = default(CT)
            ) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal;
            bool typeIsSolid = true;

            Func<CT, CT, bool> comparerToUse = comparer ?? _propFactory.GetRefEqualityComparer<CT>();

            ICProp<CT, T> typedCollectionProp = _propFactory.Create<CT,T>(initialValue, propertyName, extraInfo, storageStrategy, typeIsSolid, comparerToUse);

            AddProp(propertyName, typedCollectionProp, out PropIdType propId);
            return typedCollectionProp;
        }

        #endregion

        #region Add CollectionViewSource-type Props

        public IProp AddCollectionViewSourceProp(string propertyName, IProvideAView viewProvider)
        {
            IProp cvsProp = _propFactory.CreateCVSProp(propertyName, viewProvider);

            AddProp(propertyName, cvsProp, out PropIdType propId);
            return cvsProp;
        }

        public IProp AddCollectionViewProp(string propertyName, IProvideAView viewProvider) 
        {
            IProp cvsProp = _propFactory.CreateCVProp(propertyName, viewProvider);

            AddProp(propertyName, cvsProp, out PropIdType propId);
            return cvsProp;
        }

        #endregion

        #region Add Property-Type Props

        /// <summary>
        /// Register a new Prop Item for this PropBag.
        /// </summary>
        /// <typeparam name="T">The type of this property's value.</typeparam>
        /// <param name="propertyName"></param>
        /// <param name="comparer">An instance of a class that implements IEqualityComparer and thus an Equals method.</param>
        /// <param name="extraInfo"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        protected IProp<T> AddProp<T>(string propertyName, Func<T, T, bool> comparer = null, object extraInfo = null, T initialValue = default(T))
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal;
            bool typeIsSolid = true;
            IProp<T> pg = _propFactory.Create<T>(initialValue, propertyName, extraInfo, storageStrategy, typeIsSolid, comparer);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropObjComp<T>(string propertyName, object extraInfo = null, T initialValue = default(T))
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = _propFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = _propFactory.Create<T>(initialValue, propertyName, extraInfo, storageStrategy, typeIsSolid, comparer);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropNoValue<T>(string propertyName, Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal;
            bool typeIsSolid = true;
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, typeIsSolid, comparer);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropObjCompNoValue<T>(string propertyName, object extraInfo = null)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = _propFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, typeIsSolid, comparer);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropNoStore<T>(string propertyName, Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.External;
            bool typeIsSolid = true;
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, typeIsSolid, comparer);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropObjCompNoStore<T>(string propertyName, object extraInfo = null)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.External;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = _propFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, typeIsSolid, comparer);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        #endregion

        #region Property Management

        protected IPropData AddProp<T>(string propertyName, IProp<T> genericTypedProp,
            EventHandler<PcTypedEventArgs<T>> doWhenChanged,
            SubscriptionPriorityGroup priorityGroup,
            out PropIdType propId)
        {
            propId = AddPropId(propertyName);

            if (!_ourStoreAccessor.TryAdd(this, propId, genericTypedProp, doWhenChanged, priorityGroup, out IPropData propGen))
            {
                throw new ApplicationException("Could not add the new propGen to the store.");
            }

            return propGen;
        }

        protected IPropData AddProp(string propertyName, IProp genericTypedProp, out PropIdType propId)
        {
            propId = AddPropId(propertyName);

            if (!_ourStoreAccessor.TryAdd(this, propId, genericTypedProp, out IPropData propGen))
            {
                throw new ApplicationException("Could not add the new propGen to the store.");
            }
            return propGen;
        }

        protected IPropData AddProp(string propertyName, IProp genericTypedProp, object target, MethodInfo method, 
            SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup, out PropIdType propId)
        {
            propId = AddPropId(propertyName);

            if (!_ourStoreAccessor.TryAdd(this, propId, genericTypedProp, target, method, subscriptionKind, priorityGroup, out IPropData propGen))
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

            IPropData pGen = GetPropGen(propertyName, null, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: false,
                neverCreate: true,
                desiredHasStoreValue: _propFactory.ProvidesStorage,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

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

            IPropData PropData = GetGenPropPrivate<T>(propertyName, mustBeRegistered: mustBeRegistered, neverCreate: true, propId: out PropIdType propId);

            if(!PropData.IsEmpty)
            {
                PropData.CleanUp(doTypedCleanup: true);

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
        //    IProp<T> prop = GetTypedPropPrivate<T>(propertyName, mustBeRegistered: true);
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

        private bool DoSet<T>(PropIdType propId, string propertyName, IProp<T> typedProp, ref T curValue, T newValue)
        {
            IEnumerable<ISubscription> subscriptions = _ourStoreAccessor.GetSubscriptions(this, propId);

            IEnumerable<ISubscription> globalSubs = _ourStoreAccessor.GetSubscriptions(this, 0);

            //if(propertyName == "SelectedPerson")
            //{
            //    int cnt = subscriptions.Count();
            //}

            if (typedProp.ValueIsDefined)
            {
                bool theSame = typedProp.CompareTo(newValue);
                if (!theSame)
                {
                    T oldValue = typedProp.TypedValue;

                    if(subscriptions != null) CallChangingSubscribers(subscriptions, propertyName);
                    if(globalSubs != null) CallChangingSubscribers(globalSubs, propertyName);

                    // Make the update.
                    if (typedProp.StorageStrategy == PropStorageStrategyEnum.Internal)
                    {
                        typedProp.TypedValue = newValue;
                    }
                    curValue = newValue;

                    // Raise notify events.
                    if(subscriptions != null) DoNotifyWork(propId, propertyName, typedProp, oldValue, newValue, subscriptions);
                    if (globalSubs != null) DoNotifyWork(propId, propertyName, typedProp, oldValue, newValue, globalSubs);
                }
                return !theSame;
            }
            else
            {
                if (subscriptions != null) CallChangingSubscribers(subscriptions, propertyName);
                if (globalSubs != null) CallChangingSubscribers(globalSubs, propertyName);

                // Make the update.
                if (typedProp.StorageStrategy == PropStorageStrategyEnum.Internal)
                {
                    typedProp.TypedValue = newValue;
                }
                curValue = newValue;

                // Raise notify events.
                if (subscriptions != null) DoNotifyWork(propId, propertyName, typedProp, newValue, subscriptions);
                if (globalSubs != null) DoNotifyWork(propId, propertyName, typedProp, newValue, globalSubs);

                // The current value is undefined and the new value is defined, therefore: their has been a real update -- return true.
                return true;
            }
        }

        private void CallChangingSubscribers(IEnumerable<ISubscription> subscriptions, string propertyName)
        {
            if (subscriptions == null) return;

            // TODO: Consider adding a typed version of OnPropertyChanging.
            IEnumerable<ISubscription> propChangingSubs = subscriptions.Where(x => x.SubscriptionKind == SubscriptionKind.ChangingHandler);

            if (propChangingSubs == null) return;

            foreach (ISubscription sub in propChangingSubs)
            {
                if (sub.HandlerProxy == null)
                {
                    throw new InvalidOperationException("The Changing Handler (HandlerProxy) is null.");
                }

                try
                {
                    object target = sub.Target.Target;
                    if (target == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"The object listening to this event is 'no longer with us.'");
                    }
                    else
                    {
                        PropertyChangingEventArgs e = new PropertyChangingEventArgs(propertyName);
                        sub.PChangingHandlerDispatcher(target, this, e, sub.HandlerProxy);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"A Changing (PropertyChangingEventHandler) handler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
                }
            }
        }

        private void DoNotifyWork<T>(PropIdType propId, PropNameType propertyName, IProp<T> typedProp, T oldVal, T newValue, IEnumerable<ISubscription> subscriptions)
        {
            //List<ISubscription> diag_ListCheck = GetSubscriptions(propId).ToList();

            if (subscriptions == null)
                return;

            foreach (ISubscription sub in subscriptions)
            {
                CheckSubScriptionObject(sub);

                object target = sub.Target.Target;
                if (target == null)
                    return; // The subscriber has been collected by the GC.

                try
                {
                    switch (sub.SubscriptionKind)
                    {
                        case SubscriptionKind.TypedHandler:
                            PcTypedEventArgs<T> e = new PcTypedEventArgs<T>(propertyName, oldVal, newValue);
                            sub.PcTypedHandlerDispatcher(target, this, e, sub.HandlerProxy);
                            break;
                        case SubscriptionKind.GenHandler:
                            PcGenEventArgs e2 = new PcGenEventArgs(propertyName, typedProp.Type, oldVal, newValue);
                            sub.PcGenHandlerDispatcher(target, this, e2, sub.HandlerProxy);
                            break;
                        case SubscriptionKind.ObjHandler:
                            PcObjectEventArgs e3 = new PcObjectEventArgs(propertyName, oldVal, newValue);
                            sub.PcObjHandlerDispatcher(target, this, e3, sub.HandlerProxy);
                            break;
                        case SubscriptionKind.StandardHandler:
                            PropertyChangedEventArgs e4 = new PropertyChangedEventArgs(propertyName);
                            sub.PcStandardHandlerDispatcher(target, this, e4, sub.HandlerProxy);
                            break;
                        case SubscriptionKind.ChangingHandler:
                            // These are handled separately.
                            break;
                        //case SubscriptionKind.TypedAction:
                        //    break;
                        //case SubscriptionKind.ObjectAction:
                        //    break;
                        //case SubscriptionKind.ActionNoParams:
                        //    break;
                        //case SubscriptionKind.LocalBinding:
                        //    break;
                        default:
                            throw new InvalidOperationException($"Handlers of kind: {sub.SubscriptionKind} are not supported.");
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"A {sub.SubscriptionKind} handler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
                }
            }
        }

        public void /*IPropBagInternal.*/RaiseStandardPropertyChanged(PropIdType propId, PropNameType propertyName)
        {
            IEnumerable<ISubscription> subscriptions = _ourStoreAccessor.GetSubscriptions(this, propId);

            IEnumerable<ISubscription> globalSubs = _ourStoreAccessor.GetSubscriptions(this, 0);

            if (subscriptions != null) RaiseStandardPropertyChangedWorker(propertyName, subscriptions);

            if (globalSubs != null) RaiseStandardPropertyChangedWorker(propertyName, globalSubs);
        }

        private void RaiseStandardPropertyChangedWorker(PropNameType propertyName, IEnumerable<ISubscription> subscriptions)
        {
            if (subscriptions == null)
                return;

            foreach (ISubscription sub in subscriptions)
            {
                CheckSubScriptionObject(sub);

                if (sub.SubscriptionKind == SubscriptionKind.StandardHandler)
                {
                    object target = sub.Target.Target;
                    if (target == null)
                    {
                        return; // The subscriber has been collected by the GC.
                    }

                    try
                    {
                        PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                        sub.PcStandardHandlerDispatcher(target, this, e, sub.HandlerProxy);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"A {sub.SubscriptionKind} handler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
                    }
                }

            }
        }

        [Conditional("DEBUG")]
        private void CheckSubScriptionObject(ISubscription sub)
        {
            if (sub.HandlerProxy == null)
            {
                throw new InvalidOperationException($"The {sub.SubscriptionKind} (HandlerProxy) is null.");
            }

            switch (sub.SubscriptionKind)
            {
                case SubscriptionKind.TypedHandler:
                    if (sub.PcTypedHandlerDispatcher == null)
                    {
                        throw new InvalidOperationException("The Typed handler is null.");
                    }
                    break;

                case SubscriptionKind.GenHandler:
                    if (sub.PcGenHandlerDispatcher == null)
                    {
                        throw new InvalidOperationException("The GenHandler dispatcher is null.");
                    }
                    break;

                case SubscriptionKind.ObjHandler:
                    if (sub.PcObjHandlerDispatcher == null)
                    {
                        throw new InvalidOperationException("The ObjHandler dispatcher is null.");
                    }
                    break;

                case SubscriptionKind.StandardHandler:
                    if (sub.PcStandardHandlerDispatcher == null)
                    {
                        throw new InvalidOperationException("The Standard dispatcher is null.");
                    }
                    break;

                case SubscriptionKind.ChangingHandler:
                    if (sub.PChangingHandlerDispatcher == null)
                    {
                        throw new InvalidOperationException("The Changing dispatcher is null.");
                    }

                    break;
                //case SubscriptionKind.TypedAction:
                //    break;
                //case SubscriptionKind.ObjectAction:
                //    break;
                //case SubscriptionKind.ActionNoParams:
                //    break;
                //case SubscriptionKind.LocalBinding:
                //    break;
                default:
                    throw new InvalidOperationException($"Handlers of kind: {sub.SubscriptionKind} are not supported.");
            }

            if (sub.Target.Target == null)
            {
                System.Diagnostics.Debug.WriteLine($"The object listening to this event is 'no longer with us.'");
            }

        }

        // For when the current value is undefined.
        private void DoNotifyWork<T>(PropIdType propId, PropNameType propertyName, IProp<T> typedProp, T newValue, IEnumerable<ISubscription> subscriptions)
        {
            //List<ISubscription> diag_ListCheck = GetSubscriptions(propId).ToList();

            if (subscriptions == null)
                return;

            foreach (ISubscription sub in subscriptions)
            {
                CheckSubScriptionObject(sub);

                object target = sub.Target.Target;
                if (target == null)
                    return; // The subscriber has been collected by the GC.

                try
                {
                    switch (sub.SubscriptionKind)
                    {
                        case SubscriptionKind.TypedHandler:
                            PcTypedEventArgs<T> e = new PcTypedEventArgs<T>(propertyName, newValue);
                            sub.PcTypedHandlerDispatcher(target, this, e, sub.HandlerProxy);
                            break;
                        case SubscriptionKind.GenHandler:
                            PcGenEventArgs e2 = new PcGenEventArgs(propertyName, typedProp.Type, newValue);
                            sub.PcGenHandlerDispatcher(target, this, e2, sub.HandlerProxy);
                            break;
                        case SubscriptionKind.ObjHandler:
                            PcObjectEventArgs e3 = new PcObjectEventArgs(propertyName, newValue);
                            sub.PcObjHandlerDispatcher(target, this, e3, sub.HandlerProxy);
                            break;
                        case SubscriptionKind.StandardHandler:
                            PropertyChangedEventArgs e4 = new PropertyChangedEventArgs(propertyName);
                            sub.PcStandardHandlerDispatcher(target, this, e4, sub.HandlerProxy);
                            break;
                        case SubscriptionKind.ChangingHandler:
                            PropertyChangingEventArgs e5 = new PropertyChangingEventArgs(propertyName);
                            sub.PChangingHandlerDispatcher(target, this, e5, sub.HandlerProxy);
                            break;
                        //case SubscriptionKind.TypedAction:
                        //    break;
                        //case SubscriptionKind.ObjectAction:
                        //    break;
                        //case SubscriptionKind.ActionNoParams:
                        //    break;
                        //case SubscriptionKind.LocalBinding:
                        //    break;
                        default:
                            throw new InvalidOperationException($"Handlers of kind: {sub.SubscriptionKind} are not supported.");
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"A {sub.SubscriptionKind} handler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
                }
            }
        }

        private IEnumerable<ISubscription> GetSubscriptions(PropIdType propId)
        {
            IEnumerable<ISubscription> sc = _ourStoreAccessor.GetSubscriptions(this, propId);
            return sc;
        }

        protected IPropData GetPropGen(PropNameType propertyName, Type propertyType,
            bool haveValue, object value,
            bool alwaysRegister, bool mustBeRegistered,
            bool neverCreate, bool? desiredHasStoreValue,
            out bool wasRegistered, out PropIdType propId)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName", "PropertyName is null on call to GetValue.");

            Debug.Assert(!(alwaysRegister && mustBeRegistered), "AlwaysRegister and MustBeRegistered cannot both be true.");
            Debug.Assert(!(alwaysRegister && neverCreate), "AlwaysRegister and NeverCreate cannot both be true.");

            IPropData PropData;

            if (TryGetPropId(propertyName, out propId))
            {
                if (_ourStoreAccessor.TryGetValue(this, propId, out PropData))
                {
                    wasRegistered = false;
                }
                else
                {
                    throw new InvalidOperationException($"The store accessor did not find the property: {propertyName}.");
                    //PropData = this.HandleMissingProp(propId, propertyName, propertyType, out wasRegistered, haveValue, value, alwaysRegister, mustBeRegistered, neverCreate);
                }
            }
            else
            {
                PropData = this.HandleMissingProp(propertyName, propertyType, out wasRegistered, haveValue, value, alwaysRegister, mustBeRegistered, neverCreate);
            }

            if (!PropData.IsEmpty && desiredHasStoreValue.HasValue)
            {
                if (desiredHasStoreValue.Value && PropData.TypedProp.StorageStrategy != PropStorageStrategyEnum.Internal)
                {
                    //Caller needs property to have a backing store.
                    throw new InvalidOperationException(string.Format("Property: {0} has no backing store held by this instance of PropBag. This operation can only be performed on properties for which a backing store is held by this instance.", propertyName));
                }
                else if (!desiredHasStoreValue.Value && PropData.TypedProp.StorageStrategy == PropStorageStrategyEnum.Internal)
                {
                    throw new InvalidOperationException(string.Format("Property: {0} has a backing store held by this instance of PropBag. This operation can only be performed on properties for which no backing store is kept by this instance.", propertyName));
                }
            }

            return (IPropData) PropData;
        }

        protected IPropData GetPropGen<T>(PropNameType propertyName,
            bool haveValue, object value,
            bool alwaysRegister, bool mustBeRegistered, bool neverCreate, bool? desiredHasStoreValue, out bool wasRegistered, out PropIdType propId)
        {
            if (TryGetPropId(propertyName, out propId))
            {
                if (!_ourStoreAccessor.TryGetValue(this, propId, out IPropData PropData))
                {
                    throw new KeyNotFoundException($"The property named: {propertyName} could not be found in the property store.");
                }

                if (!PropData.IsEmpty && desiredHasStoreValue.HasValue)
                {
                    if (desiredHasStoreValue.Value && PropData.TypedProp.StorageStrategy != PropStorageStrategyEnum.Internal)
                    {
                        //Caller needs property to have a backing store.
                        throw new InvalidOperationException(string.Format("Property: {0} has no backing store held by this instance of PropBag. This operation can only be performed on properties for which a backing store is held by this instance.", propertyName));
                    }
                    else if (!desiredHasStoreValue.Value && PropData.TypedProp.StorageStrategy == PropStorageStrategyEnum.Internal)
                    {
                        throw new InvalidOperationException(string.Format("Property: {0} has a backing store held by this instance of PropBag. This operation can only be performed on properties for which no backing store is kept by this instance.", propertyName));
                    }
                }

                wasRegistered = false;
                return (IPropData)PropData;
            }
            else
            {
                throw new KeyNotFoundException($"The property named: {propertyName} could not be found in the Level2Key man.");
            }
        }

        private IProp<T> CheckTypeInfo<T>(PropIdType propId, PropNameType propertyName, IPropData PropData, PSAccessServiceType storeAccess, bool isGetOp = true)
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
                    throw new ApplicationException($"Attempting to {(isGetOp ? "get" : "set")} property: {propertyName} whose type is {PropData.TypedProp.Type}, " +
                        $"with a call whose type parameter is {typeof(T).ToString()} is invalid.");
                }
            }

            return (IProp<T>)PropData.TypedProp;
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

                IProp genericTypedProp = _propFactory.CreateGenFromObject(newType, curValue, propertyName, null, PropStorageStrategyEnum.Internal, true, propKind, null, false, null);

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
            else if(newType.UnderlyingSystemType == curType.UnderlyingSystemType)
            {
                return true;
            }
            else
            {
                return false;
                //bool result = curType.IsAssignableFrom(newType);
                //if(!result)
                //{
                //    result = newType.IsAssignableFrom(curType);
                //}
                //return result;
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

        private bool TryGetPropId(PropNameType propertyName, out PropIdType propId)
        {
            bool result = _ourStoreAccessor.TryGetPropId(propertyName, out propId);
            return result;
        }

        private PropIdType AddPropId(PropNameType propertyName)
        {
            // Register new propertyName and get an exploded key.
            PropIdType propId = _ourStoreAccessor.Add(propertyName);
            return propId;
        }

        #endregion

        #region Methods to Raise Events

        // Raise Standard Events
        //protected void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChanged, null, null);

        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}

        //protected void OnPropertyChanging(string propertyName)
        //{
        //    PropertyChangingEventHandler handler = Interlocked.CompareExchange(ref PropertyChanging, null, null);

        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangingEventArgs(propertyName));
        //    }
        //}

        // INotifyPCGen
        // Moved to the PB_EventHolder

        //public void OnPropertyChangedWithGenVals(PcGenEventArgs eArgs)
        //{
        //    EventHandler<PcGenEventArgs> handler = Interlocked.CompareExchange(ref PropertyChangedWithGenVals, null, null);

        //    if (handler != null)
        //        handler(this, eArgs);
        //}


        //// INotifyPCObject
        //protected void OnPropertyChangedWithObjVals(PcObjectEventArgs eArgs)
        //{
        //    EventHandler<PcObjectEventArgs> handler = Interlocked.CompareExchange(ref PropertyChangedWithObjectVals, null, null);

        //    if (handler != null)
        //        handler(this, eArgs);
        //}

        #endregion

        #region IEditableObject Members

        public event EventHandler<EventArgs> ItemEndEdit;


        public void BeginEdit()
        {
            System.Diagnostics.Debug.WriteLine($"BeginEdit was called on PropBag: {FullClassName}.");
        }

        public void CancelEdit()
        {
        }

        public void EndEdit()
        {
            if (ItemEndEdit != null)
            {
                ItemEndEdit(this, EventArgs.Empty);
            }
        }

        #endregion

        #region IListSource Support

        //public bool TryGetListSource(string propertyName, Type itemType, out IListSource listSource)
        //{
        //    if (TryGetPropId(propertyName, out PropIdType propId))
        //    {
        //        if (_ourStoreAccessor.TryGetValue(this, propId, out IPropData value))
        //        {
        //            listSource = value.TypedProp.ListSource;
        //            return true;
        //        }
        //        else
        //        {
        //            listSource = null;
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        listSource = null;
        //        return false;
        //    }
        //}

        #endregion

        #region DataSourceProvider Support

        public bool TryGetDataSourceProvider(IPropBag propBag, PropNameType propertyName, Type propertyType,
            out DataSourceProvider dataSourceProvider)
        {
            bool mustBeRegistered = true; // TryGetViewManager is called in the constructor, we cannot reference the virtual property: OurMetaData.AllPropsMustBeRegistered; 

            IPropData propData = GetPropGen(propertyName, propertyType,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (propData != null)
            {
                dataSourceProvider = _ourStoreAccessor.GetDataSourceProvider(this, propId, propData, _propFactory.GetCViewProviderFactory());
                return true;
            }
            else
            {
                dataSourceProvider = null;
                return false;
            }

            //if (TryGetDataSourceProviderProvider(propBag, propertyName, propertyType, out IProvideADataSourceProvider dataSourceProviderProvider))
            //{
            //    dataSourceProvider = dataSourceProviderProvider.DataSourceProvider;
            //    return true;
            //}
            //else
            //{
            //    dataSourceProvider = null;
            //    return false;
            //}
        }

        //public bool TryGetDataSourceProviderProvider(IPropBag propBag, PropNameType propertyName, Type propertyType,
        //    out IProvideADataSourceProvider dataSourceProviderProvider)
        //{
        //    bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

        //    IPropData propData = GetPropGen(propertyName, propertyType,
        //        haveValue: false,
        //        value: null,
        //        alwaysRegister: false,
        //        mustBeRegistered: mustBeRegistered,
        //        neverCreate: false,
        //        desiredHasStoreValue: null,
        //        wasRegistered: out bool wasRegistered,
        //        propId: out PropIdType propId);

        //    if (propData != null)
        //    {
        //        dataSourceProviderProvider = _ourStoreAccessor.GetDataSourceProviderProvider(this, propId, propData, _propFactory.GetCViewProviderFactory());
        //        return true;
        //    }
        //    else
        //    {
        //        dataSourceProviderProvider = null;
        //        return false;
        //    }
        //}

        public bool TryGetViewManager(IPropBag propBag, PropNameType propertyName, Type propertyType, out IManageCViews cViewManager)
        {
            bool mustBeRegistered = true; // TryGetViewManager is called in the constructor, we cannot reference the virtual property: OurMetaData.AllPropsMustBeRegistered; 

            IPropData propData = GetPropGen(propertyName, propertyType,
                haveValue: false,
                value: null,
                alwaysRegister: false,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (propData != null)
            {
                cViewManager = _ourStoreAccessor.GetViewManager(this, propId, propData, _propFactory.GetCViewProviderFactory());
                return true;
            }
            else
            {
                cViewManager = null;
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
                    lock(_sync)
                    {
                        _ourStoreAccessor.Dispose(); // This disposes each of our PropItems.
                        _ourStoreAccessor = null;

                        _ourMetaData = null;
                        _propFactory = null;

                        // TODO: Provide WeakReference to subscribers for this event.
                        // instead of relying on callers to call Dispose.
                        ItemEndEdit = null;

                        disposedValue = true;
                    }

                    //_sync = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
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

        // TODO: This method is here because DoSet<T> is private.
        // Consider creating a new interface: IPropBagInternal and making this method be a member of that interface.
        private bool DoSetBridge<T>(IPropBag target, PropIdType propId, string propertyName, IProp prop, object value)
        {
            T typedValue = (T)value;

            IProp<T> typedProp = (IProp<T>)prop;
            bool result = ((PropBag)target).DoSet<T>(propId, propertyName, typedProp, ref typedValue, (T)value);

            typedProp.TypedValue = typedValue;

            return result;
        }

        #endregion

        #region Diagnostics

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

        #endregion
    }
}
