using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.Caches;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.DataAccessSupport;
using DRM.TypeSafePropertyBag.Fundamentals;
using ObjectSizeDiagnostics;
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
    using PropIdType = UInt32;
    using PropNameType = String;
    using IRegisterBindingsFowarderType = IRegisterBindingsForwarder<UInt32>;

    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    using PropItemSetInterface = IPropItemSet<String>;

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

    public partial class PropBag : IPropBag, IRegisterBindingsFowarderType
    {
        #region Member Declarations

        // These items are provided to us.
        protected internal IPropFactory _propFactory { get; set; }
        protected internal PSAccessServiceInterface _ourStoreAccessor { get; set; }
        protected internal IProvideAutoMappers _autoMapperService { get; set; }

        // We are responsible for these
        private ITypeSafePropBagMetaData _ourMetaData { get; set; }
        protected virtual ITypeSafePropBagMetaData OurMetaData { get { return _ourMetaData; } set { _ourMetaData = value; } }
        //protected internal object _propItemSet_Handle { get; private set; }

        private IDictionary<string, IViewManagerProviderKey> _foreignViewManagers; // TODO: Consider creating a lazy property accessor for this field.

        private object _sync = new object();

        // Diagnostics
        MemConsumptionTracker _memConsumptionTracker = new MemConsumptionTracker(enabled:false);

        #endregion

        #region Public Events and Properties

        public string FullClassName => OurMetaData.FullClassName;

        //public IPropFactory PropFactory => _propFactory;
        public PropBagTypeSafetyMode TypeSafetyMode => _ourMetaData.TypeSafetyMode;

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

        public PropBag(IPropModel propModel, PSAccessServiceCreatorInterface storeAcessorCreator)
            : this(propModel, storeAcessorCreator, autoMapperService: null, propFactory: null, fullClassName: null)
        {
        }

        public PropBag(IPropModel propModel, PSAccessServiceCreatorInterface storeAcessorCreator, IPropFactory propFactory)
            : this(propModel, storeAcessorCreator, autoMapperService: null, propFactory: propFactory, fullClassName: null)
        {
        }

        public PropBag(IPropModel propModel, PSAccessServiceCreatorInterface storeAcessorCreator, IPropFactory propFactory, string fullClassName)
            : this(propModel, storeAcessorCreator, autoMapperService: null, propFactory: propFactory, fullClassName: fullClassName)
        {
        }

        /// <summary>
        /// Creates a new PropBag using the specified PropModel and Property Store Access Creator.
        /// </summary>
        /// <param name="propModel">An instance of the class: DRM.PropBag.PropModel that defines the PropItems that this
        /// PropBag will have.</param>
        /// <param name="storeAcessorCreator"></param>
        /// <param name="propFactory">The PropFactory to use instead of the one specified by the PropModel.</param>
        /// <param name="fullClassName">The namespace and class name to use instead of the one specified by the PropMode.</param>
        public PropBag(IPropModel propModel, PSAccessServiceCreatorInterface storeAcessorCreator, IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName)
        {
            long memUsedSoFar = _memConsumptionTracker.UsedSoFar;

            if (storeAcessorCreator == null) throw new ArgumentNullException(nameof(storeAcessorCreator));

            IPropFactory propFactoryToUse = propFactory ?? propModel.PropFactory ?? throw new InvalidOperationException("The propModel has no PropFactory and one was not provided in the constructor.");
            string fullClassNameToUse = fullClassName ?? propModel.FullClassName;

            BasicConstruction(propModel.TypeSafetyMode, autoMapperService, propFactoryToUse, fullClassNameToUse);

            _ourStoreAccessor = storeAcessorCreator.CreatePropStoreService(this);
                _memConsumptionTracker.MeasureAndReport("CreatePropStoreService from Scratch.", null);

            if (propModel.PropItemSet == null) propModel.PropItemSet = new PropItemSet();

            BuildPropItems(propModel, propModel.PropItemSet, storeAcessorCreator);
                _memConsumptionTracker.Report(memUsedSoFar, "---- After BuildPropItems.");

            //int testc = _ourStoreAccessor.PropertyCount;
        }

        // TODO: This assumes that all IPropBag implementations use PropBag.
        public PropBag(IPropBag copySource)
        {
            IProvideAutoMappers autoMapperService = ((PropBag)copySource)._autoMapperService;
            IPropFactory propFactory = ((PropBag)copySource)._propFactory;

            BasicConstruction(copySource.TypeSafetyMode, autoMapperService, propFactory, copySource.FullClassName);

            PSAccessServiceInterface storeAccessor = ((PropBag)copySource)._ourStoreAccessor;
            _ourStoreAccessor = CloneProps(copySource, storeAccessor);
        }

        protected PropBag(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAcessorCreator, IPropFactory propFactory, string fullClassName)
        {
            _memConsumptionTracker.MeasureAndReport("Testing", "Top of PropBag Constructor.");
            _memConsumptionTracker.Measure($"Top of PropBag Constructor.");

            if (storeAcessorCreator == null) throw new ArgumentNullException(nameof(storeAcessorCreator));
            if (propFactory == null) throw new ArgumentNullException(nameof(propFactory));

            IProvideAutoMappers autoMapperService = null;

            BasicConstruction(typeSafetyMode, autoMapperService, propFactory, fullClassName);

            _ourStoreAccessor = storeAcessorCreator.CreatePropStoreService(this);
            _memConsumptionTracker.MeasureAndReport("CreatePropStoreService", null);
        }

        private void BasicConstruction(PropBagTypeSafetyMode typeSafetyMode, IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName)
        {
            if(autoMapperService == null)
            {
                System.Diagnostics.Debug.WriteLine($"Note: IPropBag with FullClassName: {fullClassName} does not have a AutoMapperService.");
            }
            _autoMapperService = autoMapperService;
            _propFactory = propFactory ?? throw new ArgumentNullException(nameof(propFactory));

            _ourMetaData = BuildMetaData(typeSafetyMode, fullClassName, _propFactory);
            _memConsumptionTracker.MeasureAndReport("BuildMetaData", $"Full Class Name: {fullClassName ?? "NULL"}");

            _foreignViewManagers = null;
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

        #endregion

        #region PropModel Processing

        const string UN_SET_COOKED_INIT_VAL = "XxYy001122334455";

        protected void BuildPropItems(IPropModel pm, PropItemSetInterface propItemSet, PSAccessServiceCreatorInterface storeAccessCreator)
        {
            CheckClassNames(pm);

            bool propItemSetHadValues = propItemSet.Count > 0;

            foreach (IPropModelItem pi in pm.Props)
            {
                long amountUsedBeforeThisPropItem = _memConsumptionTracker.Measure($"Building Prop Item: {pi.PropertyName}");

                IProp typedProp;
                IPropTemplate propTemplate;

                if (propItemSetHadValues && propItemSet.TryGetPropTemplate(pi.PropertyName, out propTemplate))
                {
                    // BuildPropFromCooked
                    try
                    {
                        typedProp = BuildPropFromCooked(pi, propTemplate, pm.PropModelProvider, storeAccessCreator);
                    }
                    catch
                    {
                        throw;
                    }
                }
                else
                {
                    // BuildPropFromRaw
                    propTemplate = null;
                    typedProp = BuildPropFromRaw(pi, pm.PropModelProvider, storeAccessCreator);

                    // Save the PropTemplate in our PropItemSet
                    propTemplate = typedProp.PropTemplate;

                    pi.PropCreator = propTemplate.PropCreator;

                    if (!propTemplate.IsPropBag && !pi.InitialValueField.CreateNew && typedProp.ValueIsDefined)
                    {
                        pi.InitialValueCooked = typedProp.TypedValueAsObject;
                    }
                    else
                    {
                        pi.InitialValueCooked = UN_SET_COOKED_INIT_VAL;
                    }

                    propItemSet.Add(pi.PropertyName, propTemplate);
                }

                // Make sure the propTemplate doesn't hold a referene to the class implementing the IProp interface.
                propTemplate.PropCreator = null;

                IPropData newPropItem;
                PropIdType propId;

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

                    newPropItem = AddProp(pi.PropertyName, typedProp,
                        target,
                        pi.DoWhenChangedField.Method,
                        pi.DoWhenChangedField.SubscriptionKind,
                        pi.DoWhenChangedField.PriorityGroup, out propId);
                }
                else
                {
                    newPropItem = AddProp(pi.PropertyName, typedProp, out propId);
                }

                _memConsumptionTracker.MeasureAndReport("Add the TypedProp to the property store", $"for {pi.PropertyName}");

                if (pi.BinderField?.Path != null && pi.PropKind != PropKindEnum.CollectionView && pi.PropKind != PropKindEnum.CollectionViewSource_RO)
                {
                    _memConsumptionTracker.Measure();
                    processBinderField(pi, propId, newPropItem);
                    _memConsumptionTracker.MeasureAndReport("Process Binder Field", $"PropBag: {_ourStoreAccessor.ToString()} : {pi.PropertyName}");
                }

                _memConsumptionTracker.Report(amountUsedBeforeThisPropItem, $"--Completed BuildPropFromRaw for { pi.PropertyName}");
            }

            if(!propItemSetHadValues)
            {
                pm.PropertyDescriptorCollection = this.GetProperties();
                _ourStoreAccessor.FixPropItemSet();
            }
            else
            {
                if(_ourStoreAccessor.IsPropItemSetFixed)
                {
                    System.Diagnostics.Debug.WriteLine($"Notice: We just created a new PropBag using an open PropNodeCollection for {_ourStoreAccessor.ToString()}.");
                }
                this._properties = pm.PropertyDescriptorCollection;
            }
        }

        protected IProp BuildPropFromRaw(IPropModelItem pi, IProvidePropModels propModelProvider, PSAccessServiceCreatorInterface storeAccessCreator)
        {
            IProp typedProp;

            if (pi.PropKind == PropKindEnum.CollectionViewSource)
            {
                // Get the name of the Collection-Type PropItem that provides the data for this CollectionViewSource.
                string srcPropName = pi.BinderField.Path;

                // Get the ViewManager for this source collection from the Property Store.
                if (TryGetViewManager(srcPropName, pi.PropertyType, out IManageCViews cViewManager))
                {
                    IProvideAView viewProvider = cViewManager.GetDefaultViewProvider();

                    // Use our PropFactory to create a CollectionView PropItem using the ViewProvider.
                    typedProp = _propFactory.CreateCVSProp(pi.PropertyName, viewProvider);
                }
                else
                {
                    throw new InvalidOperationException($"Could not retrieve the (generic) CViewManager for source PropItem: {srcPropName}.");
                }
            }
            else if (pi.PropKind == PropKindEnum.CollectionView)
            {
                IProvideAView viewProvider;

                // Get the name of the Collection-Type PropItem that provides the data for this CollectionViewSource.
                string binderPath = pi.BinderField?.Path;

                if (binderPath != null)
                {
                    //_memConsumptionTracker.Measure();
                    IViewManagerProviderKey viewManagerProviderKey = BuildTheViewManagerProviderKey(pi, propModelProvider);
                    _memConsumptionTracker.MeasureAndReport("BuildTheViewManagerProviderKey", $"PropBag: {_ourStoreAccessor.ToString()}: {pi.PropertyName}");

                    IProvideACViewManager cViewManagerProvider = GetOrAddCViewManagerProviderGen(pi.PropertyType, viewManagerProviderKey);
                    _memConsumptionTracker.MeasureAndReport("GetOrAddCViewManagerProviderGen", $"PropBag: {_ourStoreAccessor.ToString()}: {pi.PropertyName}");

                    if (_foreignViewManagers == null)
                    {
                        _foreignViewManagers = new Dictionary<PropNameType, IViewManagerProviderKey>();
                        _foreignViewManagers.Add(pi.PropertyName, viewManagerProviderKey);
                    }
                    else
                    {
                        if (_foreignViewManagers.ContainsKey(pi.PropertyName))
                        {
                            System.Diagnostics.Debug.WriteLine($"Warning: We already have a reference to a ViewManager on a 'foreign' IPropBag. We are updating this reference while processing Property: {pi.PropertyName} with BinderPath = {binderPath}.");
                            _foreignViewManagers[pi.PropertyName] = viewManagerProviderKey;
                        }
                        else
                        {
                            _foreignViewManagers.Add(pi.PropertyName, viewManagerProviderKey);
                        }
                    }

                    //curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After PropBag::Hydrate -- update _foreignViewManagers");

                    cViewManagerProvider.ViewManagerChanged += CViewManagerProvider_ViewManagerChanged;
                    //curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After PropBag::Hydrate -- Add handler to ViewManagerChanged");

                    IManageCViews cViewManager = cViewManagerProvider.CViewManager;

                    if (cViewManager != null)
                    {
                        viewProvider = cViewManager.GetDefaultViewProvider();
                        _memConsumptionTracker.MeasureAndReport("GetDefaultViewProvider", $"PropBag: {_ourStoreAccessor.ToString()}: {pi.PropertyName}");
                    }
                    else
                    {
                        viewProvider = null;
                    }
                }
                else
                {
                    viewProvider = null;
                }

                // Use our PropFactory to create a CollectionView PropItem using the ViewProvider.
                typedProp = _propFactory.CreateCVProp(pi.PropertyName, viewProvider);
                _memConsumptionTracker.MeasureAndReport("CreateCVProp", $"PropBag: {_ourStoreAccessor.ToString()} : {pi.PropertyName}");

                //newPropItem = AddProp(pi.PropertyName, typedProp, out propId);
                //_memConsumptionTracker.MeasureAndReport("AddProp", $"PropBag: {_ourStoreAccessor.ToString()} : {pi.PropertyName}");


            }
            else
            {
                typedProp = BuildStandardPropFromRaw(pi, propModelProvider, storeAccessCreator);
            }

            return typedProp;
        }


        protected IProp BuildPropFromCooked(IPropModelItem pi, IPropTemplate propTemplate, IProvidePropModels propModelProvider, PSAccessServiceCreatorInterface storeAccessCreator)
        {
            IProp typedProp;

            if (pi.PropKind == PropKindEnum.CollectionViewSource)
            {
                // Get the name of the Collection-Type PropItem that provides the data for this CollectionViewSource.
                string srcPropName = pi.BinderField.Path;

                // Get the ViewManager for this source collection from the Property Store.
                if (TryGetViewManager(srcPropName, pi.PropertyType, out IManageCViews cViewManager))
                {
                    IProvideAView viewProvider = cViewManager.GetDefaultViewProvider();

                    // Use our PropFactory to create a CollectionView PropItem using the ViewProvider.
                    typedProp = _propFactory.CreateCVSProp(pi.PropertyName, viewProvider);
                }
                else
                {
                    throw new InvalidOperationException($"Could not retrieve the (generic) CViewManager for source PropItem: {srcPropName}.");
                }
            }
            else if (pi.PropKind == PropKindEnum.CollectionView)
            {
                IProvideAView viewProvider;

                // Get the name of the Collection-Type PropItem that provides the data for this CollectionViewSource.
                string binderPath = pi.BinderField?.Path;

                if (binderPath != null)
                {
                    //_memConsumptionTracker.Measure();
                    IViewManagerProviderKey viewManagerProviderKey = BuildTheViewManagerProviderKey(pi, propModelProvider);
                    _memConsumptionTracker.MeasureAndReport("BuildTheViewManagerProviderKey", $"PropBag: {_ourStoreAccessor.ToString()}: {pi.PropertyName}");

                    IProvideACViewManager cViewManagerProvider = GetOrAddCViewManagerProviderGen(pi.PropertyType, viewManagerProviderKey);
                    _memConsumptionTracker.MeasureAndReport("GetOrAddCViewManagerProviderGen", $"PropBag: {_ourStoreAccessor.ToString()}: {pi.PropertyName}");

                    if (_foreignViewManagers == null)
                    {
                        _foreignViewManagers = new Dictionary<PropNameType, IViewManagerProviderKey>();
                        _foreignViewManagers.Add(pi.PropertyName, viewManagerProviderKey);
                    }
                    else
                    {
                        if (_foreignViewManagers.ContainsKey(pi.PropertyName))
                        {
                            System.Diagnostics.Debug.WriteLine($"Warning: We already have a reference to a ViewManager on a 'foreign' IPropBag. We are updating this reference while processing Property: {pi.PropertyName} with BinderPath = {binderPath}.");
                            _foreignViewManagers[pi.PropertyName] = viewManagerProviderKey;
                        }
                        else
                        {
                            _foreignViewManagers.Add(pi.PropertyName, viewManagerProviderKey);
                        }
                    }

                    cViewManagerProvider.ViewManagerChanged += CViewManagerProvider_ViewManagerChanged;

                    IManageCViews cViewManager = cViewManagerProvider.CViewManager;

                    if (cViewManager != null)
                    {
                        viewProvider = cViewManager.GetDefaultViewProvider();
                        _memConsumptionTracker.MeasureAndReport("GetDefaultViewProvider", $"PropBag: {_ourStoreAccessor.ToString()}: {pi.PropertyName}");
                    }
                    else
                    {
                        viewProvider = null;
                    }
                }
                else
                {
                    viewProvider = null;
                }

                // Use our PropFactory to create a CollectionView PropItem using the ViewProvider.
                typedProp = _propFactory.CreateCVProp(pi.PropertyName, viewProvider);
                _memConsumptionTracker.MeasureAndReport("CreateCVProp", $"PropBag: {_ourStoreAccessor.ToString()} : {pi.PropertyName}");
            }
            else
            {
                typedProp = BuildStandardPropFromCooked(pi, propTemplate, propModelProvider, storeAccessCreator);
            }

            return typedProp;
        }


        private void CViewManagerProvider_ViewManagerChanged(object sender, EventArgs e)
        {
            if(sender is IProvideACViewManager cViewManagerProvider)
            {
                // Get the Id for this ViewManager Provider
                IViewManagerProviderKey viewManagerProviderKey = cViewManagerProvider.ViewManagerProviderKey;

                // Get the name and type of property for which this ViewManager Provider was created.
                PropNameType propertyName = GetTargetPropNameForView(_foreignViewManagers, viewManagerProviderKey, out Type propertyType);

                // Retreive the management Item from the PropStore for this property.
                IPropData propData;
                try
                {
                    propData = GetPropGen(propertyName, propertyType,
                        haveValue: false, value: null, alwaysRegister: false, mustBeRegistered: true,
                        neverCreate: true, desiredHasStoreValue: null, wasRegistered: out bool wasRegistered, propId: out PropIdType propId);
                }
                catch (InvalidOperationException ioe)
                {
                    throw new InvalidOperationException
                        ($"The {nameof(IProvideACViewManager)} raised the ViewManagerChanged and the PropertyName associated with this ViewManger:" +
                        $" {propertyName} could not be found in this PropBag's list of registered PropItems.");
                }

                // Retrieve the ViewProvider from the Property's value.
                if (propData.TypedProp is IUseAViewProvider viewProviderHolder)
                {
                    IProvideAView viewProvider;
                    try
                    {
                        // Get the Default ViewProvider from the ViewProvider Manager.
                        viewProvider = cViewManagerProvider.CViewManager.GetDefaultViewProvider();
                    }
                    catch 
                    {
                        // TODO: Raise new exception with more detail.
                        throw;
                    }

                    try
                    {
                        // Update the ViewProvider that is used by the Property's value.
                        viewProviderHolder.ViewProvider = viewProvider;
                    }
                    catch
                    {
                        // TODO: Raise new exception with more detail.
                        throw;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"The property for which the ViewManagerProvider was created, does not implement " +
                        $"the {nameof(IUseAViewProvider)} interface. (PropBag::ViewManagerChanged event handler");
                }
            }
            else
            {
                throw new InvalidOperationException($"The {nameof(IProvideACViewManager)} raised the ViewManagerChanged event but the sender was not a {nameof(IProvideACViewManager)}.");
            }
        }

        private IViewManagerProviderKey BuildTheViewManagerProviderKey(IPropModelItem pi, IProvidePropModels propModelProvider)
        {
            if (pi == null) throw new ArgumentNullException(nameof(pi));

            if(pi.BinderField == null || pi.BinderField.Path == null)
            {
                throw new InvalidOperationException("The BinderField Path is null.");
            }

            LocalBindingInfo localBindingInfo = new LocalBindingInfo(new LocalPropertyPath(pi.BinderField.Path));

            if(pi.MapperRequest == null && pi.MapperRequestResourceKey != null)
            {
                pi.MapperRequest = propModelProvider.GetMapperRequest(pi.MapperRequestResourceKey);
            }

            if(pi.MapperRequest != null)
            {
                if (pi.MapperRequest.PropModel == null && pi.MapperRequest.PropModelResourceKey != null)
                {
                    pi.MapperRequest.PropModel = propModelProvider.GetPropModel(pi.MapperRequest.PropModelResourceKey);
                }
            }

            IViewManagerProviderKey result = new ViewManagerProviderKey(localBindingInfo, pi.MapperRequest);
            return result;
        }

        private PropNameType GetTargetPropNameForView(IDictionary<PropNameType, IViewManagerProviderKey> viewManagerProviders, IViewManagerProviderKey viewManagerProviderKey, out Type propertyType)
        {
            KeyValuePair<string, IViewManagerProviderKey> kvp = viewManagerProviders.FirstOrDefault(x => x.Value == viewManagerProviderKey);
            PropNameType propertyName = kvp.Key;
            propertyType = viewManagerProviderKey.MapperRequest.PropModel.TypeToCreate;
            return propertyName;
        }

        private PropBagMapperCreator GetPropBagMapperFactory()
        {
            return GetPropBagMapper;
        }

        private IPropBagMapperGen GetPropBagMapper(IMapperRequest mr)
        {
            return GetPropBagMapper_Internal(mr, out IPropBagMapperKeyGen dummy);
        }

        private IPropBagMapperGen GetPropBagMapper_Internal(IMapperRequest mr, out IPropBagMapperKeyGen mapperRequest)
        {
            IProvideAutoMappers autoMapperProvider = this._autoMapperService ?? throw new InvalidOperationException
                ($"This PropBag instance cannot create IProvideDataSourceProvider instances: No AutoMapperSupport was supplied upon construction.");

            mapperRequest = autoMapperProvider.RegisterMapperRequest(mr.PropModel, mr.SourceType, mr.ConfigPackageName);
            IPropBagMapperGen genMapper = autoMapperProvider.GetMapper(mapperRequest);
            return genMapper;
        }

        private IProp BuildStandardPropFromRaw(IPropModelItem pi, IProvidePropModels propModelProvider, PSAccessServiceCreatorInterface storeAccessCreator)
        {
                _memConsumptionTracker.Measure($"Begin BuildStandardPropFromRaw: {pi.PropertyName}.");

            IProp result;

            if (pi.ComparerField == null) pi.ComparerField = new PropComparerField();

            if (pi.InitialValueField == null) pi.InitialValueField = PropInitialValueField.UseUndefined;

                _memConsumptionTracker.MeasureAndReport("After field prepartion", $"for {pi.PropertyName}");

            string creationMethodDescription;
            if (pi.StorageStrategy == PropStorageStrategyEnum.Internal && !pi.InitialValueField.SetToUndefined)
            {
                // Create New PropBag-based Object
                if (pi.InitialValueField.PropBagResourceKey != null)
                {
                    System.Diagnostics.Debug.Assert(pi.TypeIsSolid == true,
                        "Creating new PropItem of type IPropBag and the Type is not Solid!");

                    IPropBag newObject = GetNewViewModel(pi, propModelProvider, storeAccessCreator);

                    creationMethodDescription = "CreateGenFromObject";
                    result = _propFactory.CreateGenFromObject(pi.PropertyType, newObject, pi.PropertyName, pi.ExtraInfo, pi.StorageStrategy, pi.TypeIsSolid,
                        pi.PropKind, pi.ComparerField.Comparer, pi.ComparerField.UseRefEquality, pi.ItemType);
                }

                // Create New CLR Type
                else if (pi.InitialValueField.CreateNew)
                {
                    object newValue = Activator.CreateInstance(pi.PropertyType);

                    pi.TypeIsSolid = _propFactory.IsTypeSolid(newValue, pi.PropertyType);

                    creationMethodDescription = "CreateGenFromObject";
                    result = _propFactory.CreateGenFromObject(pi.PropertyType, newValue, pi.PropertyName, pi.ExtraInfo, pi.StorageStrategy, pi.TypeIsSolid,
                        pi.PropKind, pi.ComparerField.Comparer, pi.ComparerField.UseRefEquality, pi.ItemType);
                }

                // Using the initial value specified by the PropModelItem.
                else
                {
                    bool useDefault = pi.InitialValueField.SetToDefault;
                    string value = GetValue(pi);

                    pi.TypeIsSolid = _propFactory.IsTypeSolid(value, pi.PropertyType);

                    creationMethodDescription = "CreateGenFromString";
                    result = _propFactory.CreateGenFromString(pi.PropertyType, value, useDefault, pi.PropertyName, pi.ExtraInfo, pi.StorageStrategy, pi.TypeIsSolid,
                        pi.PropKind, pi.ComparerField.Comparer, pi.ComparerField.UseRefEquality, pi.ItemType);
                }
            }
            else
            {
                creationMethodDescription = "CreateGenWithNoValue";

                pi.TypeIsSolid = pi.PropertyType != typeof(object);

                result = _propFactory.CreateGenWithNoValue(pi.PropertyType, pi.PropertyName, pi.ExtraInfo, pi.StorageStrategy, pi.TypeIsSolid,
                    pi.PropKind, pi.ComparerField.Comparer, pi.ComparerField.UseRefEquality, pi.ItemType);
            }
                _memConsumptionTracker.MeasureAndReport($"Built Standard Typed Prop using {creationMethodDescription}", $" for {pi.PropertyName} (Raw.)");

            return result;
        }

        private IProp BuildStandardPropFromCooked(IPropModelItem pi, IPropTemplate propTemplate, IProvidePropModels propModelProvider, PSAccessServiceCreatorInterface storeAccessCreator)
        {
            _memConsumptionTracker.Measure($"Begin BuildStandardPropFromCooked: {pi.PropertyName}.");

            IProp result;

            if (pi.ComparerField == null) pi.ComparerField = new PropComparerField();

            if (pi.InitialValueField == null) pi.InitialValueField = PropInitialValueField.UseUndefined;

            _memConsumptionTracker.MeasureAndReport("After field prepartion", $"for {pi.PropertyName}");

            string creationMethodDescription;
            if (pi.StorageStrategy == PropStorageStrategyEnum.Internal && !pi.InitialValueField.SetToUndefined)
            {
                // Create New PropBag-based Object
                if (pi.InitialValueField.PropBagResourceKey != null)
                {
                    IPropBag newObject = GetNewViewModel(pi, propModelProvider, storeAccessCreator);

                    if (pi.PropCreator != null)
                    {
                        creationMethodDescription = "CreateGenFromObject - Using PropCreator.";
                        result = pi.PropCreator(pi.PropertyName, newObject, pi.TypeIsSolid, propTemplate);
                    }
                    else
                    {
                        creationMethodDescription = "CreateGenFromObject - PropCreator was not set.";
                        result = _propFactory.CreateGenFromObject(pi.PropertyType, newObject, pi.PropertyName, pi.ExtraInfo, pi.StorageStrategy, pi.TypeIsSolid,
                            pi.PropKind, pi.ComparerField.Comparer, pi.ComparerField.UseRefEquality, pi.ItemType);
                    }
                }

                // Create New CLR Type
                else if (pi.InitialValueField.CreateNew)
                {
                    object newValue = Activator.CreateInstance(pi.PropertyType);
                    pi.TypeIsSolid = _propFactory.IsTypeSolid(newValue, pi.PropertyType);

                    if (pi.PropCreator != null)
                    {
                        creationMethodDescription = "CreateGenFromObject - Using PropCreator.";
                        result = pi.PropCreator(pi.PropertyName, newValue, pi.TypeIsSolid, propTemplate);
                    }
                    else
                    {
                        creationMethodDescription = "CreateGenFromObject - PropCreator was not set.";
                        result = _propFactory.CreateGenFromObject(pi.PropertyType, newValue, pi.PropertyName, pi.ExtraInfo, pi.StorageStrategy, pi.TypeIsSolid,
                            pi.PropKind, pi.ComparerField.Comparer, pi.ComparerField.UseRefEquality, pi.ItemType);
                    }
                }

                // Using the initial value specified by the PropModelItem.
                else
                {
                    if (pi.PropCreator != null)
                    {
                        creationMethodDescription = "CreateGenFromString - Using PropCreator.";
                        CheckCookedInitialValue(pi.InitialValueCooked);
                        result = pi.PropCreator(pi.PropertyName, pi.InitialValueCooked, pi.TypeIsSolid, propTemplate);
                    }
                    else
                    {
                        bool useDefault = pi.InitialValueField.SetToDefault;
                        string value = GetValue(pi);

                        pi.TypeIsSolid = _propFactory.IsTypeSolid(value, pi.PropertyType);

                        creationMethodDescription = "CreateGenFromString - PropCreator was not set.";
                        result = _propFactory.CreateGenFromString(pi.PropertyType, value, useDefault, pi.PropertyName, pi.ExtraInfo, pi.StorageStrategy, pi.TypeIsSolid,
                            pi.PropKind, pi.ComparerField.Comparer, pi.ComparerField.UseRefEquality, pi.ItemType);
                    }
                }
            }
            else
            {
                if(pi.PropCreator != null)
                {
                    creationMethodDescription = "CreateGenWithNoValue - Using PropCreator.";
                    result = pi.PropCreator(pi.PropertyName, null, pi.TypeIsSolid, propTemplate);
                }
                else
                {
                    creationMethodDescription = "CreateGenWithNoValue - PropCreator was not set.";
                    result = _propFactory.CreateGenWithNoValue(pi.PropertyType, pi.PropertyName, pi.ExtraInfo, pi.StorageStrategy, pi.TypeIsSolid,
                        pi.PropKind, pi.ComparerField.Comparer, pi.ComparerField.UseRefEquality, pi.ItemType);
                }
            }
            _memConsumptionTracker.MeasureAndReport($"Built Standard Typed Prop using {creationMethodDescription}", $" for {pi.PropertyName} (Cooked.");

            return result;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckCookedInitialValue(object val)
        {
            if(val is string s && s == UN_SET_COOKED_INIT_VAL)
            {
                throw new InvalidOperationException("The InitialValueCooked was not set.");
            }
        }

        private string GetValue(IPropModelItem pi)
        {
            if (pi.InitialValueField.SetToEmptyString && pi.PropertyType == typeof(Guid))
            {
                const string EMPTY_GUID = "00000000-0000-0000-0000-000000000000";
                return EMPTY_GUID;
            }
            else
            {
                return pi.InitialValueField.GetStringValue();
            }
        }

        private IPropBag GetNewViewModel(IPropModelItem pi, IProvidePropModels propModelProvider, PSAccessServiceCreatorInterface storeAccessCreator)
        {
            IPropModel propModel = propModelProvider.GetPropModel(pi.InitialValueField.PropBagResourceKey);
            IPropBag newObject = (IPropBag)VmActivator.GetNewViewModel(pi.PropertyType, propModel, storeAccessCreator, _autoMapperService, propFactory: null, fullClassName: null);
            return newObject;
        }

        private IViewModelActivator _vmActivator;
        private IViewModelActivator VmActivator
        {
            get
            {
                if(_vmActivator == null)
                {
                    _vmActivator = new SimpleViewModelActivator();
                }
                return _vmActivator;
            }
        }

        private void processBinderField(IPropModelItem pi, PropIdType propId, IPropData propItem)
        {
            switch (pi.PropKind)
            {
                case PropKindEnum.Prop:
                    {
                        LocalBindingInfo bindingInfo = new LocalBindingInfo(new LocalPropertyPath(pi.BinderField.Path));
                        propItem.TypedProp.RegisterBinding((IRegisterBindingsFowarderType)this, propId, bindingInfo);
                        break;
                    }
                //case PropKindEnum.Enumerable:
                //    break;
                //case PropKindEnum.Enumerable_RO:
                //    break;
                //case PropKindEnum.EnumerableTyped:
                //    break;
                //case PropKindEnum.EnumerableTyped_RO:
                //    break;
                //case PropKindEnum.ObservableCollection:
                //    break;
                //case PropKindEnum.ObservableCollection_RO:
                //    break;

                case PropKindEnum.CollectionViewSource:
                    throw new InvalidOperationException($"LocalBinding for {pi.PropKind} is not handled by the {nameof(processBinderField)} method.");
                case PropKindEnum.CollectionViewSource_RO:
                    goto case PropKindEnum.CollectionViewSource;

                //case PropKindEnum.CollectionView:
                //    break;
                //case PropKindEnum.DataTable:
                //    break;
                //case PropKindEnum.DataTable_RO:
                //    break;
                default:
                    throw new InvalidOperationException($"The PropKind of {pi.PropKind} is not recognized, or LocalBinding is not supported for PropItems of this kind.");
            }
        }

        [Conditional("DEBUG")]
        private void CheckClassNames(IPropModel pm)
        {
            string cName = GetClassNameOfThisInstance();
            string pCName = pm.ClassName;

            if (cName != pCName)
            {
                System.Diagnostics.Debug.WriteLine($"CLR class name: {cName} does not match PropModel class name: {pCName}.");
            }
        }

        //private ActivationInfo GetActivationInfo(string propertyName, IPropTemplate propTemplate)
        //{
        //    ActivationInfo activationInfo;

        //    List<Argument> arguments = new List<Argument>();

        //    string typeName = propTemplate.Type.Name;

        //    if (typeName == "CViewProp" || typeName == "CViewSourceProp")
        //    {
        //        //public CViewProp(PropNameType propertyName, IProvideAView viewProvider, IPropTemplate<ListCollectionView> template)
        //        //public CViewSourceProp(PropNameType propertyName, IProvideAView viewProvider, IPropTemplate<CollectionViewSource> template)

        //        arguments.Add(new Argument(typeof(string), 0, propertyName));
        //        arguments.Add(new Argument(typeof(IProvideAView), 1, null));
        //        arguments.Add(new Argument(typeof(IPropTemplate), 2, propTemplate));
        //        activationInfo = new ActivationInfo(propTemplate.Type, arguments);
        //    }
        //    else if(typeName == "Prop" || typeName == "CProp" || typeName == "NoStore")
        //    {
        //        //public Prop(PropNameType propertyName, bool typeIsSolid, IPropTemplate<T> template)
        //        //public Prop(PropNameType propertyName, T initalValue, bool typeIsSolid, IPropTemplate<T> template)

        //        //public CProp(PropNameType propertyName, bool typeIsSolid, IPropTemplate<CT> template)
        //        //public CProp(PropNameType propertyName, CT initalValue, bool typeIsSolid, IPropTemplate<CT> template)

        //        //public PropNoStore(PropNameType propertyName, bool typeIsSolid, IPropTemplate<T> template)

        //        arguments.Add(new Argument(typeof(string), 0, propertyName));
        //        arguments.Add(new Argument(typeof(bool), 1, true));
        //        arguments.Add(new Argument(typeof(IPropTemplate), 2, propTemplate));
        //        activationInfo = new ActivationInfo(propTemplate.Type, arguments);
        //    }
        //    else if(typeName == "PropExternStore")
        //    {
        //        //public PropExternStore(string propertyName, object extraInfo, bool typeIsSolid, IPropTemplate<T> template)

        //        arguments.Add(new Argument(typeof(string), 0, propertyName));
        //        arguments.Add(new Argument(typeof(object), 1, null));
        //        arguments.Add(new Argument(typeof(bool), 2, true));
        //        arguments.Add(new Argument(typeof(IPropTemplate), 3, propTemplate));
        //        activationInfo = new ActivationInfo(propTemplate.Type, arguments);
        //    }
        //    else
        //    {
        //        activationInfo = null;
        //    }

        //    return activationInfo;
        //}

        // WE WILL USE LATER FOR TESTING
        public void FetchData_Test(string pName, Type pType)
        {
            IProvideAView viewProvider = null;
            ICollectionView lcv = null;

            IProvideAView viewProvider2 = null;
            ICollectionView lcv2 = null;

            if (TryGetViewManager(pName, pType, out IManageCViews cViewManager))
            {
                viewProvider = cViewManager.GetDefaultViewProvider();
                lcv = viewProvider.View;
            }

            if (TryGetViewManager(pName, pType, out IManageCViews cViewManager2))
            {
                viewProvider2 = cViewManager2.GetDefaultViewProvider();
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
                        }
                        else
                        {
                            throw new InvalidOperationException($"Property: {propertyName} does not exist in this PropBag."); 
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
                        throw new InvalidOperationException($"{nameof(OurMetaData.ReadMissingPropPolicy)} is not recognized.");
                    }
            }
        }

        #endregion

        #region Property Access Methods

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
            bool mustBeRegistered = _ourMetaData.TypeSafetyMode == PropBagTypeSafetyMode.Locked;

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
            //else if(_ourMetaData.TypeSafetyMode == PropBagTypeSafetyMode.Tight)
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
                        try
                        {
                            MakeTypeSolid(propId, propertyName, PropData, propertyType);
                        }
                        catch (InvalidCastException ice)
                        {
                            throw new ApplicationException($"Invalid opertion: the value of property: {propertyName}, " +
                                $"however the type specfied for retreival is {propertyType} which is a value type. Value types cannot be set to null.", ice);
                        }
                        MakeTypeSolid(propId, propertyName, PropData, propertyType);
                    }
                    else
                    {
                        if (propertyType != PropData.TypedProp.PropTemplate.Type)
                        {
                            throw new InvalidOperationException($"Invalid opertion: attempting to get property: {propertyName} whose type is {PropData.TypedProp.PropTemplate.Type}, with a call whose type parameter is {propertyType}.");
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
            if(typedProp == null)
            {
                throw new InvalidOperationException($"Could not retrieve value for PropItem: {propertyName}.");
            }
            else
            {
                return typedProp.TypedValue;
            }
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
            {
                return PropData;
            }
            else
            {
                if (!PropData.IsEmpty)
                {
                    CheckTypeInfo<T>(propId, propertyName, PropData);
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
            return SetValWithType_Private(propertyName, null, value);
        }

        public bool SetValWithType(string propertyName, Type propertyType, object value)
        {
            //CHK: PropertyType may be null.
            if (propertyType == null && OurMetaData.OnlyTypedAccess)
            {
                ReportNonTypedAccess(propertyName, nameof(SetValWithType));
            }
            return SetValWithType_Private(propertyName, propertyType, value);
        }

        /// <summary>
        /// Set's the value of the property with optional type information.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyType">If unknown, set this parameter to null.</param>
        /// <param name="value"></param>
        private bool SetValWithType_Private(string propertyName, Type propertyType, object value)
        {
            // For Set operations where a type is given, 
            // Register the property if it does not exist, unless the TypeSafetyMode
            // setting is AllPropsMustBe (explictly) registered.
            bool alwaysRegister = !OurMetaData.AllPropsMustBeRegistered;
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            IPropData propData = GetPropGen(propertyName, propertyType, haveValue: true, value: value,
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
                Type newType = propertyType ?? _propFactory.GetTypeFromValue(value);

                if (!propData.TypedProp.TypeIsSolid)
                {
                    try
                    {
                        MakeTypeSolid(propId, propertyName, propData, newType);
                    }
                    catch (InvalidCastException ice)
                    {
                        throw new InvalidOperationException($"Invalid Operation: The property: {propertyName} was originally set to null, now its being set with type: {propertyType}. {propertyType} is a value type and value types cannot bet set to null.", ice);
                    }
                }
                else
                {
                    if (!AreTypesSame(newType, propData.TypedProp.PropTemplate.Type))
                    {
                        throw new InvalidOperationException($"Invalid opertion: attempting to get property: {propertyName} whose type is {propData.TypedProp.PropTemplate.Type}, with a call whose type parameter is {propertyType}.");
                    }
                }
            }
            else
            {
                // Check to make sure that we are not attempting to set the value of a ValueType to null.
                if (propData.TypedProp.TypeIsSolid && propData.TypedProp.PropTemplate.Type.IsValueType)
                {
                    throw new InvalidOperationException($" Invalid Operation: cannot set property: {propertyName} to null, it is of type: {propertyType} which is a value type.");
                }
            }

            //ICacheDelegates<DoSetDelegate> dc = _propFactory.DelegateCacheProvider.DoSetDelegateCache;
            //DoSetDelegate setPropDel = dc.GetOrAdd(propData.TypedProp.Type);

            DoSetDelegate setPropDel = GetTheDoSetDelegate(propData);
            return setPropDel(this, propId, propertyName, propData.TypedProp, value);
        }


        private DoSetDelegate GetTheDoSetDelegate(IPropData propData)
        {
            if(propData.DoSetDelegate == null)
            {
                ICacheDelegates<DoSetDelegate> dc = _propFactory.DelegateCacheProvider.DoSetDelegateCache;
                DoSetDelegate setPropDel = dc.GetOrAdd(propData.TypedProp.PropTemplate.Type);
                propData.TypedProp.PropTemplate.DoSetDelegate = setPropDel;
                return setPropDel;
            }
            else
            {
                return propData.DoSetDelegate;
            }
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
            // Register the property if it does not exist,
            // unless the TypeSafetyMode setting is AllPropsMustBe (explictly) registered.
            bool alwaysRegister = !OurMetaData.AllPropsMustBeRegistered;
            bool mustBeRegistered = OurMetaData.AllPropsMustBeRegistered;

            // TODO: Create a GetPropGen<T> that uses the _theStore.
            IPropData PropData = GetPropGen(propertyName, typeof(T), haveValue: true, value: value,
                alwaysRegister: alwaysRegister,
                mustBeRegistered: mustBeRegistered,
                neverCreate: false,
                desiredHasStoreValue: null, // _propFactory.ProvidesStorage,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            // No point in calling DoSet, it would find that the value is the same and do nothing.
            if (wasRegistered) return true;

            IProp<T> prop = CheckTypeInfo<T>(propId, propertyName, PropData);
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

            IProp<T> typedProp = CheckTypeInfo<T>(propId, propertyName, PropData);

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

                bool wasAdded = ((IRegisterBindingsFowarderType)this).RegisterBinding<T>(propId, bindingInfo);
                return wasAdded;
            }
            else
            {
                return false;
            }
        }

        bool IRegisterBindingsFowarderType.RegisterBinding<T>(PropIdType propId, LocalBindingInfo bindingInfo)
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

                bool wasRemoved = ((IRegisterBindingsFowarderType)this).UnregisterBinding<T>(propId, bindingInfo);
                return wasRemoved;
            }
            else
            {
                return false;
            }
        }

        bool IRegisterBindingsFowarderType.UnregisterBinding<T>(PropIdType propId, LocalBindingInfo bindingInfo)
        {
            bool wasRemoved = _ourStoreAccessor.UnregisterBinding<T>(this, propId, bindingInfo);
            return wasRemoved;
        }

        #endregion

        #region Public Methods

        public virtual object Clone()
        {
            return new PropBag(this);
        }

        private PSAccessServiceInterface CloneProps(IPropBag copySource, PSAccessServiceInterface storeAccessor)
        {
            PSAccessServiceInterface result = storeAccessor.CloneProps(this, copySource);

            if(copySource is ICustomTypeDescriptor ictd)
            {
                _properties = ictd.GetProperties();
            }

            return result;
        }

        public virtual ITypeSafePropBagMetaData GetMetaData()
        {
            return OurMetaData;
        }

        /// <summary>
        /// Makes a copy of the core list.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, ValPlusType> GetAllPropNamesAndTypes()
        {
            IEnumerable<KeyValuePair<PropNameType, IPropData>> theStoreAsCollection = _ourStoreAccessor.GetCollection(this);

            IEnumerable<KeyValuePair<string, ValPlusType>> list = theStoreAsCollection.Select(x =>
            new KeyValuePair<string, ValPlusType>(x.Key, x.Value.TypedProp.GetValuePlusType())).ToList();

            IDictionary<string, ValPlusType> dict = list.ToDictionary(pair => pair.Key, pair => pair.Value);

            IReadOnlyDictionary<string, ValPlusType> result2 = new ReadOnlyDictionary<string, ValPlusType>(dict);

            return result2;
        }

        /// <summary>
        /// Returns all of the values in dictionary of objects, keyed by PropertyName.
        /// </summary>
        public IReadOnlyDictionary<PropNameType, IPropData> GetAllPropertyValues()
        {
            //IEnumerable<KeyValuePair<PropNameType, IPropData>> theStoreAsCollection = _ourStoreAccessor.GetCollection(this);
            //IReadOnlyDictionary<PropNameType, IPropData> result = theStoreAsCollection.ToDictionary(pair => pair.Key, pair => pair.Value);

            IReadOnlyDictionary<PropNameType, IPropData> result = _ourStoreAccessor.GetCollection(this);

            return result;
        }

        public IList<PropNameType> GetAllPropertyNames()
        {
            var result = _ourStoreAccessor.GetKeys(this).ToList();
            return result;
        }

        public bool PropertyExists(string propertyName)
        {
            if (_ourMetaData.TypeSafetyMode == PropBagTypeSafetyMode.Locked)
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
                    type = value.TypedProp.PropTemplate.Type;
                    return true;
                }
                else if (_ourMetaData.TypeSafetyMode == PropBagTypeSafetyMode.Locked)
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
                return pGen.TypedProp.PropTemplate.Type;
            }
            else if(_ourMetaData.TypeSafetyMode == PropBagTypeSafetyMode.Locked)
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
                    propType = value.TypedProp.PropTemplate.PropKind;
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
            IReadOnlyDictionary<string, ValPlusType> x = GetAllPropNamesAndTypes();

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

        #region Add ObservableCollection<T> Props

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

        #region Add Collection and CollectionViewSource Props

        public IProp AddCollectionViewSourceProp(string propertyName, IProvideAView viewProvider)
        {
            IProp cvsProp = _propFactory.CreateCVSProp(propertyName, viewProvider);

            AddProp(propertyName, cvsProp, out PropIdType propId);
            return cvsProp;
        }

        public IProp CreateCollectionViewProp(string propertyName, IProvideAView viewProvider) 
        {
            IProp cvsProp = _propFactory.CreateCVProp(propertyName, viewProvider);
            return cvsProp;
        }

        // MAY WANT TO KEEP THIS -- in case we decide to support this.
        //protected IProp CreateCollectionViewPropDSGen(PropNameType propertyName, Type typeOfThisProperty, PropNameType srcPropName, IMapperRequest mr)
        //{
        //    ICacheDelegatesForTypePair<CVPropFromDsDelegate> dc = _propFactory.DelegateCacheProvider.CreateCViewPropCache;
        //    CVPropFromDsDelegate cvPropCreator = dc.GetOrAdd(new TypePair(mr.SourceType, typeOfThisProperty));
        //    IProp result = cvPropCreator(this, propertyName, srcPropName, mr);

        //    return result;
        //}

        protected IManageCViews GetOrAddCViewManagerGen(Type typeOfThisProperty, PropNameType srcPropName, IMapperRequest mr)
        {
            // Check the delegate cache to see if a delegate for this already exists, and if not create a new delegate.
            ICacheDelegatesForTypePair<CViewManagerFromDsDelegate> dc = _propFactory.DelegateCacheProvider.GetOrAddCViewManagerCache;
            CViewManagerFromDsDelegate cViewManagerCreator = dc.GetOrAdd(new TypePair(mr.SourceType, typeOfThisProperty));

            // Use the delegate to create or fetch the existing Collection View Manager for this propperty.
            // This eventually calls: this.GetOrAddCViewManager
            IManageCViews result = cViewManagerCreator(this, srcPropName, mr);

            return result;
        }

        //protected IProvideACViewManager GetOrAddCViewManagerProviderGen(Type typeOfThisProperty, PropNameType srcPropName, IMapperRequest mr)
        protected IProvideACViewManager GetOrAddCViewManagerProviderGen(Type typeOfThisProperty, IViewManagerProviderKey viewManagerProviderKey)
        {
            // Check the delegate cache to see if a delegate for this already exists, and if not create a new delegate.
            ICacheDelegatesForTypePair<CViewManagerProviderFromDsDelegate> dc = _propFactory.DelegateCacheProvider.GetOrAddCViewManagerProviderCache;
            CViewManagerProviderFromDsDelegate cViewManagerProviderCreator = dc.GetOrAdd(new TypePair(viewManagerProviderKey.MapperRequest.SourceType, typeOfThisProperty));

            //LocalBindingInfo bindingInfo = new LocalBindingInfo(new LocalPropertyPath(srcPropName), LocalBindingMode.OneWay);

            // Use the delegate to create or fetch the existing Collection View Manager for this propperty.
            // This eventually calls: this.GetOrAddCViewManagerProvider
            IProvideACViewManager result = cViewManagerProviderCreator(this, viewManagerProviderKey);

            return result;
        }

        // TODO: Allow mapper to be null on construction, and then omit mapping functionality, but instead
        // use the IDoCRUD<T> straight, without any mapping

        // Using a IMapperRequest and Factory
        public IProp CreateCollectionViewPropDS<TDal, TSource, TDestination>
        (
            PropNameType propertyName, // The name for the new property.
            PropNameType srcPropName,   // The name of the property that holds the data (of type IDoCRUD<TSource>.)
            IMapperRequest mr   // The (non-generic) information necessary to create a AutoMapper Mapper request.
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            // Get the PropItem for the property that holds the DataSource (IDoCRUD<TSource>)
            IPropData propGen = GetPropGen(srcPropName, null, haveValue: false, value: null, alwaysRegister: false, mustBeRegistered: true,
            neverCreate: true, desiredHasStoreValue: true, wasRegistered: out bool wasRegistered, propId: out PropIdType dalPropId);

            if (propGen.IsEmpty)
            {
                throw new InvalidOperationException($"The {nameof(GetOrAddCViewManager)} cannot find the source PropItem with name = {srcPropName}.");
            }

            IManageCViews cViewManager = _ourStoreAccessor.GetOrAddViewManager<TDal, TSource, TDestination>
                (
                this, dalPropId, propGen, mr, GetPropBagMapperFactory(), _propFactory.GetCViewProviderFactory()
                );

            IProvideAView viewProvider = cViewManager.GetDefaultViewProvider();
            IProp result = CreateCollectionViewProp(propertyName, viewProvider);
            return result;
        }

        // new version: returns a IManageCViews Using a IMapperRequest and Factory
        public IManageCViews GetOrAddCViewManager<TDal, TSource, TDestination>
        (
            PropNameType srcPropName,   // The name of the property that holds the data (of type IDoCRUD<TSource>.)
            IMapperRequest mr   // The (non-generic) information necessary to create a AutoMapper Mapper request.
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            // Get the PropItem for the property that holds the DataSource (IDoCRUD<TSource>)
            IPropData propGen = GetPropGen(srcPropName, null, haveValue: false, value: null, alwaysRegister: false, mustBeRegistered: true,
            neverCreate: true, desiredHasStoreValue: true, wasRegistered: out bool wasRegistered, propId: out PropIdType dalPropId);

            if(propGen.IsEmpty)
            {
                throw new InvalidOperationException($"The {nameof(GetOrAddCViewManager)} cannot find the source PropItem with name = {srcPropName}.");
            }

            IManageCViews cViewManager = _ourStoreAccessor.GetOrAddViewManager<TDal, TSource, TDestination>
                (
                this, dalPropId, propGen, mr, GetPropBagMapperFactory(), _propFactory.GetCViewProviderFactory()
                );
            return cViewManager;
        }

        public bool TryGetCViewManagerProvider(PropNameType propertyName, out IProvideACViewManager cViewManagerProvider)
        {
            if(_foreignViewManagers.TryGetValue(propertyName, out IViewManagerProviderKey viewManagerProviderKey))
            {
                if(_ourStoreAccessor.TryGetViewManagerProvider(this, viewManagerProviderKey, out cViewManagerProvider))
                {
                    return true;
                }
                else
                {
                    cViewManagerProvider = null;
                    return false;
                }
            }
            throw new KeyNotFoundException($"There is no CViewManagerProvider allocated for PropItem with name = {propertyName}.");
        }

        // new version: returns a IManageCViews Using a IMapperRequest and Factory
        public IProvideACViewManager GetOrAddCViewManagerProvider<TDal, TSource, TDestination>
        (
            IViewManagerProviderKey viewManagerProviderKey
            //LocalBindingInfo bindingInfo,   // The name of the property that holds the data (of type IDoCRUD<TSource>.)
            //IMapperRequest mr   // The (non-generic) information necessary to create a AutoMapper Mapper request.
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            IProvideACViewManager cViewManagerProvider = _ourStoreAccessor.GetOrAddViewManagerProvider<TDal, TSource, TDestination>
                (
                this, viewManagerProviderKey, GetPropBagMapperFactory(), _propFactory.GetCViewProviderFactory()
                );
            return cViewManagerProvider;
        }

        // Using a IPropBagMapper directly.
        public IProp CreateCollectionViewPropDS<TDal, TSource, TDestination>
        (
            PropNameType propertyName, // The name for the new property.
            PropNameType srcPropName,   // The name of the property that holds the data (of type IDoCRUD<TSource>.)
            IPropBagMapper<TSource, TDestination> mapper    // Optional. The AutoMapper to use to map data from the source to data in the view.
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            IPropData propGen = GetPropGen(srcPropName, null, haveValue: false, value: null, alwaysRegister: false, mustBeRegistered: true,
            neverCreate: true, desiredHasStoreValue: true, wasRegistered: out bool wasRegistered, propId: out PropIdType dalPropId);

            if (propGen.IsEmpty)
            {
                throw new InvalidOperationException($"The {nameof(GetOrAddCViewManager)} cannot find the source PropItem with name = {srcPropName}.");
            }

            IManageCViews cViewManager = _ourStoreAccessor.GetOrAddViewManager<TDal, TSource, TDestination>(this, dalPropId, propGen, mapper, _propFactory.GetCViewProviderFactory());

            IProvideAView viewProvider = cViewManager.GetDefaultViewProvider();
            IProp result = CreateCollectionViewProp(propertyName, viewProvider);
            return result;
        }

        // Typed version, using a IPropBagMapper directly. 
        public ICViewProp<CVT> AddCollectionViewPropDS_Typed<CVT, TDal, TSource, TDestination>
        (
            PropNameType propertyName,
            PropNameType srcPropName,
            IPropBagMapper<TSource, TDestination> mapper
        )
            where CVT : ICollectionView
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            IPropData propGen = GetPropGen(srcPropName, null, haveValue: false, value: null, alwaysRegister: false, mustBeRegistered: true,
            neverCreate: true, desiredHasStoreValue: true, wasRegistered: out bool wasRegistered, propId: out PropIdType dalPropId);

            if (propGen.IsEmpty)
            {
                throw new InvalidOperationException($"The {nameof(GetOrAddCViewManager)} cannot find the source PropItem with name = {srcPropName}.");
            }

            IManageCViews cViewManager = _ourStoreAccessor.GetOrAddViewManager<TDal, TSource, TDestination>(this, dalPropId, propGen, mapper, _propFactory.GetCViewProviderFactory());

            IProvideAView viewProvider = cViewManager.GetDefaultViewProvider();
            ICViewProp<CVT> result = (ICViewProp<CVT>)CreateCollectionViewProp(propertyName, viewProvider);
            return result;
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
            IProp<T> pg = _propFactory.Create<T>(initialValue, propertyName, extraInfo, storageStrategy, typeIsSolid, comparer, null);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropObjComp<T>(string propertyName, object extraInfo = null, T initialValue = default(T))
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = _propFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = _propFactory.Create<T>(initialValue, propertyName, extraInfo, storageStrategy, typeIsSolid, comparer, null);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropNoValue<T>(string propertyName, Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal;
            bool typeIsSolid = true;
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, typeIsSolid, comparer, null);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropObjCompNoValue<T>(string propertyName, object extraInfo = null)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = _propFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, typeIsSolid, comparer, null);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropNoStore<T>(string propertyName, Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.External;
            bool typeIsSolid = true;
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, typeIsSolid, comparer, null);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropObjCompNoStore<T>(string propertyName, object extraInfo = null)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.External;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = _propFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, typeIsSolid, comparer, null);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        #endregion

        #region Property Management

        protected IPropData AddProp(string propertyName, IProp genericTypedProp, out PropIdType propId)
        {
            if (!_ourStoreAccessor.TryAdd(this, propertyName, genericTypedProp, out IPropData propGen, out propId))
            {
                throw new ApplicationException("Could not add the new propGen to the store.");
            }
            return propGen;
        }

        protected IPropData AddProp(string propertyName, IProp genericTypedProp, object target, MethodInfo method, 
            SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup, out PropIdType propId)
        {
            if (!_ourStoreAccessor.TryAdd(this, propertyName, genericTypedProp, target, method,
                subscriptionKind, priorityGroup, out IPropData propGen, out propId))
            {
                throw new ApplicationException("Could not add the new propGen to the store.");
            }
            return propGen;
        }

        protected IPropData AddProp<T>(string propertyName, IProp<T> genericTypedProp, EventHandler<PcTypedEventArgs<T>> doWhenChanged,
            SubscriptionPriorityGroup priorityGroup, out PropIdType propId)
        {
            if (!_ourStoreAccessor.TryAdd(this, propertyName, genericTypedProp, doWhenChanged,
                priorityGroup, out IPropData propGen, out propId))
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

            bool mustBeRegistered = _ourMetaData.TypeSafetyMode == PropBagTypeSafetyMode.Locked;

            IPropData propData = GetPropGen(propertyName, null, haveValue: false, value: null,
                alwaysRegister: false,
                mustBeRegistered: false,
                neverCreate: true,
                desiredHasStoreValue: _propFactory.ProvidesStorage,
                wasRegistered: out bool wasRegistered,
                propId: out PropIdType propId);

            if (!propData.IsEmpty)
            {
                //PropData.CleanUp(doTypedCleanup: true);

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
            bool mustBeRegistered = _ourMetaData.TypeSafetyMode == PropBagTypeSafetyMode.Locked;

            IPropData propData = GetGenPropPrivate<T>(propertyName, mustBeRegistered: mustBeRegistered, neverCreate: true, propId: out PropIdType propId);

            if(!propData.IsEmpty)
            {
                //PropData.CleanUp(doTypedCleanup: true);

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

        // TODO: Create a second version of this method that does not include the "ref T curVal" parameter.
        // This new version can be used when the IProp uses internal storage.
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
                    T oldValue = curValue; // typedProp.TypedValue;

                    if(subscriptions != null) CallChangingSubscribers(subscriptions, propertyName);
                    if(globalSubs != null) CallChangingSubscribers(globalSubs, propertyName);

                    // Make the update.
                    if (typedProp.TypedPropTemplate.StorageStrategy == PropStorageStrategyEnum.Internal)
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
                if (typedProp.TypedPropTemplate.StorageStrategy == PropStorageStrategyEnum.Internal)
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
                            PcGenEventArgs e2 = new PcGenEventArgs(propertyName, typedProp.TypedPropTemplate.Type, oldVal, newValue);
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

        public void RaiseStandardPropertyChanged(PropNameType propertyName)
        {
            if(TryGetPropId(propertyName, out PropIdType propId))
            {
                RaiseStandardPropertyChanged(propId, propertyName);
            }
            else
            {
                throw new InvalidOperationException($"Could not get the PropId for property name = {propertyName}.");
            }
        }

        private void RaiseStandardPropertyChanged(PropIdType propId, PropNameType propertyName)
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
                            PcGenEventArgs e2 = new PcGenEventArgs(propertyName, typedProp.TypedPropTemplate.Type, newValue);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyType">This is only required if there's a chance the PropItem will be registered during this call.</param>
        /// <param name="haveValue"></param>
        /// <param name="value"></param>
        /// <param name="alwaysRegister"></param>
        /// <param name="mustBeRegistered"></param>
        /// <param name="neverCreate"></param>
        /// <param name="desiredHasStoreValue"></param>
        /// <param name="wasRegistered"></param>
        /// <param name="propId"></param>
        /// <returns>The existing or newly created PropItem.</returns>
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

            if(propertyName.StartsWith("Business"))
            {
                System.Diagnostics.Debug.WriteLine("Acessing PropItem with name starting: 'Business'.");
            }

            if (TryGetPropId(propertyName, out propId))
            {
                if (_ourStoreAccessor.TryGetValue(this, propId, out PropData))
                {
                    wasRegistered = false;
                }
                else
                {
                    throw new InvalidOperationException($"Critical Error: The property has been registered! However the store accessor did not find the property: {propertyName}.");
                }
            }
            else
            {
                PropData = this.HandleMissingProp(propertyName, propertyType, out wasRegistered, haveValue, value, alwaysRegister, mustBeRegistered, neverCreate);
            }

            CheckStorageStrategy(propertyName, PropData, desiredHasStoreValue);

            return (IPropData) PropData;
        }

        protected IPropData GetPropGen<T>(PropNameType propertyName,
            bool haveValue, object value,
            bool alwaysRegister, bool mustBeRegistered, bool neverCreate, bool? desiredHasStoreValue, out bool wasRegistered, out PropIdType propId)
        {
            if (TryGetPropId(propertyName, out propId))
            {
                if (!_ourStoreAccessor.TryGetValue(this, propId, out IPropData propData))
                {
                    throw new KeyNotFoundException($"The property named: {propertyName} could not be fetched from the property store.");
                }

                CheckStorageStrategy(propertyName, propData, desiredHasStoreValue);

                wasRegistered = false;
                return (IPropData)propData;
            }
            else
            {
                throw new KeyNotFoundException($"The property named: {propertyName} could not be found in the PropCollection for this PropBag.");
            }
        }

        private void CheckStorageStrategy(PropNameType propertyName, IPropData propData, bool? desiredHasStoreValue)
        {
            if (!propData.IsEmpty && desiredHasStoreValue.HasValue)
            {
                if (desiredHasStoreValue.Value && propData.TypedProp.PropTemplate.StorageStrategy != PropStorageStrategyEnum.Internal)
                {
                    //Caller needs property to have a backing store.
                    throw new InvalidOperationException($"Property: {propertyName} has no backing store held by this instance of PropBag. " +
                        $"This operation can only be performed on properties for which a backing store is held by this instance.");
                }
                else if (!desiredHasStoreValue.Value && propData.TypedProp.PropTemplate.StorageStrategy == PropStorageStrategyEnum.Internal)
                {
                    throw new InvalidOperationException($"Property: {propertyName} has a backing store held by this instance of PropBag. " +
                        $"This operation can only be performed on properties for which no backing store is kept by this instance.");
                }
            }
        }

        private IProp<T> CheckTypeInfo<T>(PropIdType propId, PropNameType propertyName, IPropData PropData, bool isGetOp = true)
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
                if (!AreTypesSame(typeof(T), PropData.TypedProp.PropTemplate.Type))
                {
                    throw new ApplicationException($"Attempting to {(isGetOp ? "get" : "set")} property: {propertyName} whose type is {PropData.TypedProp.PropTemplate.Type}, " +
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
            Type currentType = PropData.TypedProp.PropTemplate.Type;

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
            if (newType != currentType)
            {
                object curValue = PropData.TypedProp.TypedValueAsObject;
                PropKindEnum propKind = PropData.TypedProp.PropTemplate.PropKind;

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

        //private PropIdType AddPropId(PropNameType propertyName)
        //{
        //    // Register new propertyName and get an exploded key.
        //    PropIdType propId = _ourStoreAccessor.Add(propertyName);
        //    return propId;
        //}

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

        public IManageCViews GetOrAddViewManager<TDal, TSource, TDestination>
        (
            PropNameType propertyName,
            Type propertyType,
            IPropBagMapper<TSource, TDestination> mapper
        )
            where TDal : IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            IManageCViews result = null;
            return result;
        }

        public IManageCViews GetOrAddViewManager<TDal, TSource, TDestination>
        (
            PropNameType propertyName,
            Type propertyType,
            IMapperRequest mr
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
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
                IManageCViews result = _ourStoreAccessor.GetOrAddViewManager<TDal, TSource, TDestination>
                    (this, propId, propData, mr, GetPropBagMapperFactory(), _propFactory.GetCViewProviderFactory());
                return result;
            }
            else
            {
                return null;
            }
        }

        public bool TryGetDataSourceProvider(PropNameType propertyName, Type propertyType,
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
                dataSourceProvider = _ourStoreAccessor.GetOrAddDataSourceProvider(this, propId, propData, _propFactory.GetCViewProviderFactory());
                return true;
            }
            else
            {
                dataSourceProvider = null;
                return false;
            }
        }

        public IManageCViews GetOrAddViewManager(PropNameType propertyName, Type propertyType)
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
                IManageCViews result = _ourStoreAccessor.GetOrAddViewManager(this, propId, propData, _propFactory.GetCViewProviderFactory());
                return result;
            }
            else
            {
                return null;
            }
        }

        public bool TryGetViewManager(PropNameType propertyName, Type propertyType, out IManageCViews cViewManager)
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
                cViewManager = _ourStoreAccessor.GetViewManager(this, propId);
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
                        //_ourStoreAccessor = null;

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
        // Consider creating a new interface: IPropBagInternal and making this method a member of that interface.
        static private bool DoSetBridge<T>(IPropBag target, PropIdType propId, PropNameType propertyName, IProp prop, object value)
        {
            T newValue = (T)value;

            IProp<T> typedProp = (IProp<T>)prop;
            T curVal = typedProp.ValueIsDefined ? (T)typedProp.TypedValueAsObject : default(T);

            PropBag pb = (PropBag)target;
            bool result = pb.DoSet<T>(propId, propertyName, typedProp, ref curVal, newValue);
            return result;
        }

        // Create a CollectionView Manager using an optional MapperRequest.
        private IManageCViews CViewManagerFromDsBridge<TSource, TDestination>(IPropBag target, PropNameType srcPropName, IMapperRequest mr)
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            PropBag pb = (PropBag)target;
            IManageCViews result = pb.GetOrAddCViewManager<IDoCRUD<TSource>, TSource, TDestination>(srcPropName, mr);
            return result;
        }

        // Create a CollectionView Manager Provider from a viewManagerProviderKey. Key consists of an optional MapperRequest and a Binding Path.)
        private IProvideACViewManager CViewManagerProviderFromDsBridge<TSource, TDestination>(IPropBag target, IViewManagerProviderKey viewManagerProviderKey)
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            PropBag pb = (PropBag)target;
            IProvideACViewManager result = pb.GetOrAddCViewManagerProvider<IDoCRUD<TSource>, TSource, TDestination>(viewManagerProviderKey);
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

        //public int CreatePropWithNoValCacheCount
        //{
        //    get
        //    {
        //        int result = _propFactory.CreatePropWithNoValCacheCount;
        //        return result;
        //    }
        //}

        #endregion
    }
}
