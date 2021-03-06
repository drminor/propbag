﻿
using DRM.PropBag.Caches;
using DRM.PropBag.ViewModelTools;

using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.DataAccessSupport;
using DRM.TypeSafePropertyBag.DelegateCaches;
using DRM.TypeSafePropertyBag.TypeDescriptors;
using ObjectSizeDiagnostics;
using Swhp.Tspb.PropBagAutoMapperService;
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
    //using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PropNameType = String;

    using PropItemSetKeyType = PropItemSetKey<String>;

    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;
    using PSFastAccessServiceInterface = IPropStoreFastAccess<UInt32, String>;

    using PropModelType = IPropModel<String>;

    using PropModelCacheInterface = ICachePropModels<String>;
    using ViewModelActivatorInterface = IViewModelActivator<UInt32, String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    using IRegisterBindingsFowarderType = IRegisterBindingsForwarder<UInt32>;


    #region Summary and Remarks

    /// <remarks>
    /// The contents of this code file were designed and created by David R. Minor, Pittsboro, NC. (Swamp Hill Productions)
    /// I have chosen to provide others free access to this intellectual product using the terms set forth
    /// by the well known Code Project Open License.
    /// Please refer to the file in this same folder named CPOP.htm for the exact set of terms that govern this release.
    /// Although not included as a condition of use, I would prefer that this text, 
    /// or a similar text which covers all of the points made here, be included along with a copy of cpol.htm
    /// in the set of artifacts deployed with any product
    /// wherein this source code, or a derivative thereof, is used.
    /// </remarks>

    /// <remarks>
    /// While writing this code, I learned much and was guided by the material found at the following locations.
    /// http://northhorizon.net/2011/the-right-way-to-do-inotifypropertychanged/ (Daniel Moore)
    /// https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/ (Jon Skeet)
    /// </remarks>

    #endregion

    public partial class PropBag : IPropBagInternal, IRegisterBindingsFowarderType, IDisposable
    {
        #region Member Declarations

        private ITypeSafePropBagMetaData _ourMetaData { get; set; }
        private IDictionary<string, IViewManagerProviderKey> _foreignViewManagers; // TODO: Consider creating a lazy property accessor for this field.
        private object _sync = new object();
        private MemConsumptionTracker _memConsumptionTracker = new MemConsumptionTracker(enabled: false);

        // These items are provided to us and are only set during construction.
        protected internal PSAccessServiceInterface _ourStoreAccessor { get; private set; }
        protected internal ViewModelFactoryInterface _viewModelFactory { get; private set; }
        protected internal IPropBagMapperService _propBagMapperService { get; private set; }

        protected internal PropModelType _propModel { get; private set; }
        protected internal IPropFactory _propFactory { get; private set; }

        // Inheritors may override this Property
        protected internal virtual ITypeSafePropBagMetaData OurMetaData { get { return _ourMetaData; } private set { _ourMetaData = value; } }
        protected internal PropBagTypeSafetyMode TypeSafetyMode => _ourMetaData.TypeSafetyMode;

        #endregion

        #region Public Events and Properties

        public string FullClassName => OurMetaData.FullClassName;
        public PSAccessServiceInterface ItsStoreAccessor => _ourStoreAccessor;
        public ObjectIdType ObjectId => _ourStoreAccessor.ObjectId;

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

        public PropBag(PropModelType propModel, ViewModelFactoryInterface viewModelFactory, IPropBagMapperService propBagMapperService)
            : this(propModel, viewModelFactory, propBagMapperService, propFactory: null, fullClassName: null)
        {
        }

        public PropBag(PropModelType propModel, ViewModelFactoryInterface viewModelFactory, IPropBagMapperService propBagMapperService, IPropFactory propFactory)
            : this(propModel, viewModelFactory, propBagMapperService, propFactory: propFactory, fullClassName: null)
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
        public PropBag(PropModelType propModel, ViewModelFactoryInterface viewModelFactory, IPropBagMapperService propBagMapperService, IPropFactory propFactory, string fullClassName)
        {
            long memUsedSoFar = _memConsumptionTracker.UsedSoFar;

            if(propModel == null)
            {
                throw new ArgumentNullException(nameof(propModel));
            }

            IPropFactory propFactoryToUse = propFactory ?? propModel.PropFactory ?? throw new InvalidOperationException("The propModel has no PropFactory and one was not provided in the constructor.");
            string fullClassNameToUse = fullClassName ?? propModel.FullClassName;

            BasicConstruction(propModel.TypeSafetyMode, propModel, viewModelFactory, propBagMapperService, propFactoryToUse, fullClassNameToUse);

            PSAccessServiceCreatorInterface psStoreCreator = viewModelFactory.PropStoreAccessServiceCreator;

            _ourStoreAccessor = psStoreCreator.CreatePropStoreService(this);
                _memConsumptionTracker.MeasureAndReport("CreatePropStoreService from Scratch.", null);

            bool propModelWasRaw = IsPropModelRaw(propModel);

            BuildPropItems(propModel, viewModelFactory, propBagMapperService);
                _memConsumptionTracker.Report(memUsedSoFar, "---- After BuildPropItems.");

            // Cache or Set the PropertyDescriptors (for our ICustomTypeDescriptor implementation.)
            // If not already fixed, fix the PropModel.

            if (propModel.Count == 0)
                return;

            if (propModelWasRaw)
            {
                // This must be the first time this PropModel has been applied.

                // If we were given a PropModelCache, let the cache know that we are finished addding PropItems.
                _viewModelFactory.PropModelCache?.TryFix(propModel);

                // Also, to improve performance, let the PropertyStore know that the set of PropItems can be fixed as well.
                PropItemSetKeyType propItemSetKey = new PropItemSetKeyType(propModel);
                _ourStoreAccessor.TryFixPropItemSet(propItemSetKey);
            }
            else
            {
                // Diagnostic Checks
                CheckForOpenPropSet();
                CheckForOpenPropNodeCollection();
            }
        }

        [Conditional("DEBUG")]
        private void CheckForOpenPropSet()
        {
            if (!IsPropSetFixed)
            {
                Debug.WriteLine($"Notice: We just created a new PropBag using a 'cooked', but open PropModel with FullClassName: {_propModel} for {_ourStoreAccessor}.");
            }
        }

        [Conditional("DEBUG")]
        private void CheckForOpenPropNodeCollection()
        {
            if (!_ourStoreAccessor.IsPropItemSetFixed)
            {
                Debug.WriteLine($"Notice: We just created a new PropBag using an open PropNodeCollection for {_ourStoreAccessor}.");
            }
        }

        private bool IsPropModelRaw(PropModelType propModel)
        {
            foreach(IPropItemModel pi in propModel.GetPropItems())
            {
                if(pi.PropTemplate != null)
                {
                    return false;
                }
            }
            return true;
        }

        public PropBag(IPropBag copySource)
        {
            // TODO: This assumes that all IPropBag implementations use PropBag.

            PropBagTypeSafetyMode typeSafetyMode = ((PropBag)copySource).TypeSafetyMode;

            IPropBagMapperService propBagMapperService = ((PropBag)copySource)._propBagMapperService;
            //ICreateWrapperTypes wrapperTypeCreator = ((PropBag)copySource)._wrapperTypeCreator;
            //PropModelCacheInterface propModelCache = ((PropBag)copySource)._propModelCache;

            ViewModelFactoryInterface viewModelFactory = ((PropBag)copySource)._viewModelFactory;

            PropModelType propModel = ((PropBag)copySource)._propModel;
            IPropFactory propFactory = ((PropBag)copySource)._propFactory;

            BasicConstruction(typeSafetyMode, propModel, viewModelFactory, propBagMapperService, propFactory, copySource.FullClassName);

            PSAccessServiceInterface storeAccessor = ((PropBag)copySource)._ourStoreAccessor;
            _ourStoreAccessor = CloneProps(copySource, storeAccessor);
        }

        protected PropBag(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAcessorCreator, IPropFactory propFactory, string fullClassName)
        {
            _memConsumptionTracker.MeasureAndReport("Testing", "Top of PropBag Constructor.");
            _memConsumptionTracker.Measure($"Top of PropBag Constructor.");

            if (storeAcessorCreator == null) throw new ArgumentNullException(nameof(storeAcessorCreator));

            ViewModelActivatorInterface viewModelActivator = VmActivator;
            ViewModelFactoryInterface viewModelFactory = new SimpleViewModelFactory(viewModelActivator, storeAcessorCreator);

            IPropBagMapperService propBagMapperService = null;

            PropModelType propModel = null;
            BasicConstruction(typeSafetyMode, propModel, viewModelFactory, propBagMapperService, propFactory, fullClassName);

            _ourStoreAccessor = storeAcessorCreator.CreatePropStoreService(this);
            _memConsumptionTracker.MeasureAndReport("CreatePropStoreService", null);
        }

        protected PropBag(PropBagTypeSafetyMode typeSafetyMode, ViewModelFactoryInterface viewModelFactory, IPropFactory propFactory, string fullClassName)
        {
            PropModelType propModel = null;
            IPropBagMapperService propBagMapperService = null;
            BasicConstruction(typeSafetyMode, propModel, viewModelFactory, propBagMapperService, propFactory, fullClassName);

            _ourStoreAccessor = viewModelFactory.PropStoreAccessServiceCreator.CreatePropStoreService(this);
        }

        private void BasicConstruction(PropBagTypeSafetyMode typeSafetyMode, PropModelType propModel,
            ViewModelFactoryInterface viewModelFactory, IPropBagMapperService propBagMapperService,
            IPropFactory propFactory, string fullClassName)
        {
            if (propBagMapperService == null)
            {
                Debug.WriteLine($"Note: IPropBag with FullClassName: {fullClassName} does not have a (PropBag) AutoMapperService.");
            }

            _viewModelFactory = viewModelFactory;
            _propBagMapperService = propBagMapperService;

            _propFactory = propFactory ?? throw new ArgumentNullException(nameof(propFactory));
            if (fullClassName == null) throw new ArgumentNullException(nameof(fullClassName));

            _ourMetaData = BuildMetaData(typeSafetyMode, fullClassName, _propFactory);
            _memConsumptionTracker.MeasureAndReport("BuildMetaData", $"Full Class Name: {fullClassName ?? "NULL"}");

            _propModel = propModel;

            _foreignViewManagers = null;
            _propertyDescriptors = null;
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

        // TODO: The PropModel given to the PropBag is not immutable: The PropBag updates the PropModel in place as it goes from 'raw' to 'cooked'.
        // Consider providing a separate class (and associated caches) for 'cooked' PropModels.
        // In this way, we can make PropModels fetched from XAML, XML, or other sources immutable.

        protected void BuildPropItems(PropModelType pm, ViewModelFactoryInterface viewModelFactory, IPropBagMapperService propBagMapperService)
        {
            CheckClassNames(pm);

            foreach (IPropItemModel pi in pm.GetPropItems())
            {
                long amountUsedBeforeThisPropItem = _memConsumptionTracker.Measure($"Building Prop Item: {pi.PropertyName}");

                bool weHadAPropTemplate = pi.PropTemplate != null;
                IProp typedProp = BuildProp(pi, viewModelFactory, propBagMapperService);

                if(!weHadAPropTemplate)
                {
                    IPropTemplate propTemplate = typedProp.PropTemplate;

                    // Remove the PropCreator function from the PropTemplate and store it in the PropModel
                    // A single PropTemplate may serve many PropItems and this value will be overwritten if left in the PropTemplate.
                    pi.PropCreator = propTemplate.PropCreator;

                    // Save the initial value of the Prop into the PropItemModel.
                    if (!propTemplate.IsPropBag && !pi.InitialValueField.CreateNew && typedProp.ValueIsDefined)
                    {
                        pi.InitialValueCooked = typedProp.TypedValueAsObject;
                    }
                    else
                    {
                        pi.InitialValueCooked = UN_SET_COOKED_INIT_VAL;
                    }
                }

                // Make sure the propTemplate doesn't hold a reference to the class implementing the IProp interface.
                if (pi.PropTemplate?.PropCreator != null)
                {
                    pi.PropTemplate.PropCreator = null;
                }

                // Add the new Prop to the PropertyStore and retreive the new PropItem that is created in the process.
                IPropData newPropItem = RegisterPropWithStore(pi, typedProp, out PropIdType propId);

                _memConsumptionTracker.MeasureAndReport("Add the TypedProp to the property store", $"for {pi.PropertyName}");

                // Add Bindings
                if (pi.BinderField?.Path != null && pi.PropKind != PropKindEnum.CollectionView && pi.PropKind != PropKindEnum.CollectionViewSource_RO)
                {
                    _memConsumptionTracker.Measure();
                    ProcessBinderField(pi, propId, newPropItem);
                    _memConsumptionTracker.MeasureAndReport("Process Binder Field", $"PropBag: {_ourStoreAccessor.ToString()} : {pi.PropertyName}");
                }

                _memConsumptionTracker.Report(amountUsedBeforeThisPropItem, $"--Completed BuildPropFromRaw for { pi.PropertyName}");
            }
        }

        protected IProp BuildProp(IPropItemModel pi, ViewModelFactoryInterface viewModelFactory, IPropBagMapperService propBagMapperService)
        {
            IProp typedProp;

            if (pi.PropKind == PropKindEnum.CollectionViewSource)
            {
                typedProp = BuildCollectionViewSourceProp(pi);
            }
            else if (pi.PropKind == PropKindEnum.CollectionView)
            {
                typedProp = BuildCollectionViewProp(pi, viewModelFactory.PropModelCache, viewModelFactory.WrapperTypeCreator);
            }
            else
            {
                typedProp = BuildStandardProp(pi, viewModelFactory, propBagMapperService);
            }

            return typedProp;
        }

        private IProp BuildCollectionViewSourceProp(IPropItemModel pi)
        {
            // Get the name of the Collection-Type PropItem that provides the data for this CollectionViewSource.
            string srcPropName = pi.BinderField.Path;

            // Get the ViewManager for this source collection from the Property Store.
            if (TryGetViewManager(srcPropName, pi.PropertyType, out IManageCViews cViewManager))
            {
                IProvideAView viewProvider = cViewManager.GetDefaultViewProvider();

                // Use our PropFactory to create a CollectionView PropItem using the ViewProvider.
                IProp typedProp = _propFactory.CreateCVSProp(pi.PropertyName, viewProvider, pi.PropTemplate);
                return typedProp;
            }
            else
            {
                throw new InvalidOperationException($"Could not retrieve the (generic) CViewManager for source PropItem: {srcPropName}.");
            }
        }

        private IProp BuildCollectionViewProp(IPropItemModel pi, PropModelCacheInterface propModelBuilder, ICreateWrapperTypes wrapperTypeCreator)
        {
            IProvideAView viewProvider;

            // Get the name of the Collection-Type PropItem that provides the data for this CollectionViewSource.
            string binderPath = pi.BinderField?.Path;

            if (binderPath != null)
            {
                //_memConsumptionTracker.Measure();
                IViewManagerProviderKey viewManagerProviderKey = BuildTheViewManagerProviderKey(pi, propModelBuilder);
                _memConsumptionTracker.MeasureAndReport("After BuildTheViewManagerProviderKey", $"PropBag: {_ourStoreAccessor.ToString()}: {pi.PropertyName}");

                IMapperRequest mr = viewManagerProviderKey.MapperRequest;

                // TODO: Make IProvideAutoMapppers return 'requiresWrappperTypeEmitServices' (of type bool)
                // when given a ConfigPackageName (of type string)
                if (mr.ConfigPackageName.ToLower().Contains("emit"))
                {
                    PropModelType propModel = mr.PropModel;

                    if (propModel.NewEmittedType == null)
                    {
                        if(propModel.IsFixed)
                        {
                            throw new InvalidOperationException("This PropModel has been fixed and has a null value for the NewEmittedType. We were expecting an open PropModel, or a fixed PropModel with a non-null value for NewEmittedType.");
                        }

                        Type et = wrapperTypeCreator.GetWrapperType(propModel, propModel.TypeToWrap);
                        propModel.NewEmittedType = et;
                        _memConsumptionTracker.MeasureAndReport("After GetWrapperType", $"PropBag: {_ourStoreAccessor.ToString()}: {pi.PropertyName}, Created Type: {propModel.TypeToWrap}");
                    }
                }
                else
                {
                    if(mr.PropModel.NewEmittedType != null)
                    {
                        throw new InvalidOperationException("This PropModel has a non-null value for the NewEmittedType. We were expecting a PropModel that has a null value for the NewEmitedType.");
                    }
                }

                Type destinationType = mr.PropModel.NewEmittedType ?? pi.ItemType;

                IProvideACViewManager cViewManagerProvider = GetOrAddCViewManagerProviderGen(destinationType, viewManagerProviderKey);
                _memConsumptionTracker.MeasureAndReport("After GetOrAddCViewManagerProviderGen", $"PropBag: {_ourStoreAccessor.ToString()}: {pi.PropertyName}");

                if (_foreignViewManagers == null)
                {
                    _foreignViewManagers = new Dictionary<PropNameType, IViewManagerProviderKey>
                    {
                        { pi.PropertyName, viewManagerProviderKey }
                    };
                }
                else
                {
                    if (_foreignViewManagers.ContainsKey(pi.PropertyName))
                    {
                        Debug.WriteLine($"Warning: We already have a reference to a ViewManager on a 'foreign' IPropBag. We are updating this reference while processing Property: {pi.PropertyName} with BinderPath = {binderPath}.");
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
            IProp typedProp = _propFactory.CreateCVProp(pi.PropertyName, viewProvider, pi.PropTemplate);
            _memConsumptionTracker.MeasureAndReport("CreateCVProp", $"PropBag: {_ourStoreAccessor.ToString()} : {pi.PropertyName}");

            return typedProp;
        }

        private void CViewManagerProvider_ViewManagerChanged(object sender, EventArgs e)
        {
            if (sender is IProvideACViewManager cViewManagerProvider)
            {
                // Get the Id for this ViewManager Provider
                IViewManagerProviderKey viewManagerProviderKey = cViewManagerProvider.ViewManagerProviderKey;

                // Get the name and type of property for which this ViewManager Provider was created.
                if(!TryGetTargetPropNameForView(_foreignViewManagers, viewManagerProviderKey, out PropNameType propertyName, out Type propertyType))
                {
                    throw new InvalidOperationException($"The {nameof(IProvideACViewManager)} raised the ViewManagerChanged event but could not get the Target PropName from our list of foreign view managers.");
                }

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

        private IViewManagerProviderKey BuildTheViewManagerProviderKey(IPropItemModel pi, PropModelCacheInterface propModelCache)
        {
            if (pi == null) throw new ArgumentNullException(nameof(pi));

            string bindingPath = pi.BinderField?.Path ?? throw new InvalidOperationException("The BinderField Path is null on call to BuildTheViewManagerProviderKey.");

            LocalBindingInfo localBindingInfo = new LocalBindingInfo(new LocalPropertyPath(bindingPath));

            if (pi.MapperRequest == null)
            {
                if (pi.MapperRequestResourceKey != null)
                {
                    // Get the MapperRequestTemplate specified by this PropModelItem.
                    pi.MapperRequest = propModelCache.GetMapperRequest(pi.MapperRequestResourceKey);
                }
                else
                {
                    throw new InvalidOperationException("There is no MapperRequest defined for this PropModelItem.");
                }
            }

            // Get the PropModel specified by this MapperRequestTemplate.
            if (pi.MapperRequest.PropModel == null)
            {
                if (pi.MapperRequest.PropModelFullClassName != null)
                {
                    //pi.MapperRequest.PropModel = propModelBuilder.GetPropModel(pi.MapperRequest.PropModelResourceKey);

                    if (propModelCache.TryGetPropModel(pi.MapperRequest.PropModelFullClassName, out PropModelType propModel))
                    {
                        pi.MapperRequest.PropModel = propModel;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Could not load a PropModel for the MapperRequest for this PropModelItem.");
                }
            }

            //CheckMapperRequestPropModelType(pi);

            IViewManagerProviderKey result = new ViewManagerProviderKey(localBindingInfo, pi.MapperRequest);
            return result;
        }

        //[Conditional("DEBUG")]
        //private void CheckMapperRequestPropModelType(IPropModelItem pi)
        //{
        //    PropModelType propModel = pi.MapperRequest.PropModel;

        //    Debug.Assert(propModel.TargetType == pi.PropertyType,
        //        $"The TargetType {propModel.TargetType} does not match the PropertyType: {pi.PropertyType}" +
        //        $" when creating a CollectionViewProp.");
        //}

        private bool TryGetTargetPropNameForView(IDictionary<PropNameType, IViewManagerProviderKey> viewManagerProviders, IViewManagerProviderKey viewManagerProviderKey, out PropNameType propertyName, out Type propertyType)
        {
            if(viewManagerProviders == null)
            {
                propertyName = null;
                propertyType = typeof(object);
                return false;
            }

            KeyValuePair<string, IViewManagerProviderKey> kvp = viewManagerProviders.FirstOrDefault(x => x.Value == viewManagerProviderKey);
            if(kvp.Equals(default(KeyValuePair<string, IViewManagerProviderKey>)))
            {
                propertyName = null;
                propertyType = typeof(object);
                return false;
            }

            propertyName = kvp.Key;
            propertyType = viewManagerProviderKey.MapperRequest.PropModel.TypeToWrap;
            return true;
        }

        private IProp BuildStandardProp(IPropItemModel pi, ViewModelFactoryInterface viewModelFactory,
            IPropBagMapperService propBagMapperService)
        {
            _memConsumptionTracker.Measure($"Begin BuildStandardPropFromCooked: {pi.PropertyName}.");

            IProp result;

            if (pi.ComparerField == null) pi.ComparerField = new PropComparerField();

            if (pi.InitialValueField == null) pi.InitialValueField = PropInitialValueField.UseUndefined;

            _memConsumptionTracker.MeasureAndReport("After field preparation", $"for {pi.PropertyName}");

            string creationMethodDescription;
            if (pi.StorageStrategy == PropStorageStrategyEnum.Internal && !pi.InitialValueField.SetToUndefined)
            {
                // Create New PropBag-based Object
                if (pi.InitialValueField.PropBagFCN != null)
                {
                    string fcn = pi.InitialValueField.PropBagFCN;
                    //IPropBag newObject = BuildNewViewModel(fcn, pi.PropertyType, propModelBuilder, storeAccessCreator, autoMapperService, wrapperTypeCreator);
                    IPropBag newObject = BuildNewViewModel(fcn, pi.PropertyType, viewModelFactory, propBagMapperService);


                    if (pi.PropTemplate != null && pi.PropCreator != null)
                    {
                        creationMethodDescription = "CreateGenFromObject - Using PropCreator.";
                        result = pi.PropCreator(pi.PropertyName, newObject, pi.TypeIsSolid, pi.PropTemplate);
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

                    if (pi.PropTemplate != null && pi.PropCreator != null)
                    {
                        creationMethodDescription = "CreateGenFromObject - Using PropCreator.";
                        result = pi.PropCreator(pi.PropertyName, newValue, pi.TypeIsSolid, pi.PropTemplate);
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
                    if (pi.PropTemplate != null && pi.PropCreator != null)
                    {
                        creationMethodDescription = "CreateGenFromString - Using PropCreator.";
                        CheckCookedInitialValue(pi.InitialValueCooked);
                        result = pi.PropCreator(pi.PropertyName, pi.InitialValueCooked, pi.TypeIsSolid, pi.PropTemplate);
                    }
                    else
                    {
                        object value;
                        bool useDefault = pi.InitialValueField.SetToDefault;
                        if(useDefault)
                        {
                            value = _propFactory.GetDefaultValue(pi.PropertyType, pi.PropertyName);
                        }
                        else
                        {
                            value = pi.InitialValueField.InitialValue;
                        }

                        pi.TypeIsSolid = _propFactory.IsTypeSolid(value, pi.PropertyType);

                        creationMethodDescription = "CreateGenFromString - PropCreator was not set.";
                        result = _propFactory.CreateGenFromObject(pi.PropertyType, value, pi.PropertyName, pi.ExtraInfo, pi.StorageStrategy, pi.TypeIsSolid,
                            pi.PropKind, pi.ComparerField.Comparer, pi.ComparerField.UseRefEquality, pi.ItemType);
                    }
                }
            }

            // Intitial Value is Undefined or the Storage Strategy is External or Virtual
            else
            {
                if (pi.PropTemplate != null && pi.PropCreator != null)
                {
                    creationMethodDescription = "CreateGenWithNoValue - Using PropCreator.";
                    result = pi.PropCreator(pi.PropertyName, null, pi.TypeIsSolid, pi.PropTemplate);
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

        private IPropData RegisterPropWithStore(IPropItemModel pi, IProp typedProp, out PropIdType propId)
        {
            object target;
            MethodInfo method;
            SubscriptionKind subscriptionKind;
            SubscriptionPriorityGroup subscriptionPriorityGroup;

            if (pi.DoWhenChangedField != null)
            {
                if (pi.DoWhenChangedField.MethodIsLocal)
                {
                    target = this;
                }
                else
                {
                    throw new NotSupportedException("Only local methods are supported.");
                }

                method = pi.DoWhenChangedField.Method;
                subscriptionKind = pi.DoWhenChangedField.SubscriptionKind;
                subscriptionPriorityGroup = pi.DoWhenChangedField.PriorityGroup;
            }
            else
            {
                target = null;
                method = null;
                subscriptionKind = SubscriptionKind.GenHandler;
                subscriptionPriorityGroup = SubscriptionPriorityGroup.Standard;
            }

            IPropData newPropItem = AddProp_Internal(pi.PropertyName, typedProp, target, method, subscriptionKind, subscriptionPriorityGroup, addToModel: false, propId: out propId);

            return newPropItem;
        }
        
        const string UN_SET_COOKED_INIT_VAL = "&&&&_UN_SET_COOKED_INIT_VAL_&&&&";

        [Conditional("DEBUG")]
        private void CheckCookedInitialValue(object val)
        {
            if(val is string s && s == UN_SET_COOKED_INIT_VAL)
            {
                throw new InvalidOperationException("The InitialValueCooked was not set.");
            }
        }

        private string GetValue(IPropItemModel pi)
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

        //private IPropBag BuildNewViewModel(string fullClassName, Type propertyType, PropModelCacheInterface propModelBuilder,
        //    PSAccessServiceCreatorInterface storeAccessCreator, IProvideAutoMappers autoMapperService, ICreateWrapperTypes wrapperTypeCreator)
        //{
        //    if (propModelBuilder.TryGetPropModel(fullClassName, out PropModelType propModel))
        //    {
        //        IPropBag newObject = (IPropBag)VmActivator.GetNewViewModel(propertyType, propModel, storeAccessCreator, autoMapperService, wrapperTypeCreator, pfOverride: null, fcnOverride: null);
        //        return newObject;
        //    }
        //    else
        //    {
        //        throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fullClassName}.");
        //    }
        //}

        private IPropBag BuildNewViewModel(string fullClassName, Type propertyType, ViewModelFactoryInterface viewModelFactory, IPropBagMapperService propBagMapperService)
        {
            PropModelCacheInterface propModelCache = viewModelFactory.PropModelCache;

            if (propModelCache.TryGetPropModel(fullClassName, out PropModelType propModel))
            {
                IPropBag newObject = (IPropBag)VmActivator.GetNewViewModel(propertyType, propModel, viewModelFactory, propBagMapperService, pfOverride: null, fcnOverride: null);
                return newObject;
            }
            else
            {
                throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fullClassName}.");
            }
        }

        private ViewModelActivatorInterface _vmActivator;
        private ViewModelActivatorInterface VmActivator
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

        private void ProcessBinderField(IPropItemModel pi, PropIdType propId, IPropData propItem)
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
                    throw new InvalidOperationException($"LocalBinding for {pi.PropKind} is not handled by the {nameof(ProcessBinderField)} method.");
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
        private void CheckClassNames(PropModelType pm)
        {
            string cName = GetClassNameOfThisInstance();
            string pCName = pm.ClassName;

            if (cName != pCName)
            {
                Debug.WriteLine($"CLR class name: {cName} does not match PropModel class name: {pCName}.");
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

            Debug.WriteLine("You may want to set a break point here.");

            //ICollectionView icv = cvs?.View;

            //bool testC = ReferenceEquals(cvsPrior, cvs);
            //bool testI = ReferenceEquals(icvPrior, icv);

            //if (cvs.View is ListCollectionView lcv)
            //{
            //    SetIt<ListCollectionView>(lcv, "PersonListView");
            //}
            //else
            //{
            //    Debug.WriteLine("The default view of the CollectionViewSource: CVS does not implement ListCollectionView.");
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
            IProp<T> typedProp = GetTypedProp<T>(propertyName, mustBeRegistered: true, neverCreate: false);
            if(typedProp == null)
            {
                throw new InvalidOperationException($"Could not retrieve value for PropItem: {propertyName}.");
            }
            else
            {
                return typedProp.TypedValue;
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

        #region FAST Property Access Methods

        public object GetValueFast(IPropBag component, PropItemSetKeyType propItemSetKey, PropIdType propId)
        {
            object result;
            if (component is IPropBagInternal ipbi)
            {
                ExKeyT compKey = new SimpleExKey(ipbi.ObjectId, propId);
                result = _ourStoreAccessor.GetValueFast(compKey, propItemSetKey);
            }
            else
            {
                result = _ourStoreAccessor.GetValueFast(component, propId, propItemSetKey);
            }
            return result;
        }

        public bool SetValueFast(IPropBag component, PropItemSetKeyType propItemSetKey, PropIdType propId, object value)
        {
            bool result;

            if (component is IPropBagInternal ipbi)
            {
                ExKeyT compKey = new SimpleExKey(ipbi.ObjectId, propId);
                result = _ourStoreAccessor.SetValueFast(compKey, propItemSetKey, value);
            }
            else
            {
                result = _ourStoreAccessor.SetValueFast(component, propId, propItemSetKey, value);
            }

            return result;
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

        // Global Property Changed Events notifies the subscriber whenever any property has a value changed.
        // Since this event is typed, it only makes sense when a property of the correct type has been changed.
        // Since it very difficult to determine when a property has a type that matches a specified type,
        // SUBSCRIBING to GLOBAL PropertyChangedTyped events is NOT SUPPORTED.
        //public bool SubscribeToGlobalPropChanged<T>(EventHandler<PcTypedEventArgs<T>> eventHandler, bool unregister)
        //{
        //    uint propId = 0;

        //    bool result;
        //    if (unregister)
        //    {
        //        result = _ourStoreAccessor.UnregisterHandler(this, propId, eventHandler/*, SubscriptionPriorityGroup.Standard, keepRef: false*/);
        //    }
        //    else
        //    {
        //        IDisposable disable = _ourStoreAccessor.RegisterHandler(this, propId, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
        //        result = disable != null;
        //    }
        //    return result;
        //}

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

            IPropData PropData = GetPropGen(propertyName, propertyType: null, haveValue: false, value: null,
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

        public ICustomTypeDescriptor GetCustomTypeDescriptor() 
        {
            // Get the type of this instance.
            Type t = GetType();

            // Get the Default TypeDescriptor for the type of this instance.
            ICustomTypeDescriptor parent = TypeDescriptor.GetProvider(t).GetTypeDescriptor(t);

            // Get our Custom TypeDescriptor using the default TypeDescriptor as the base.
            ICustomTypeDescriptor result = GetCustomTypeDescriptor(parent);
            return result;
        }

        ICustomTypeDescriptor _ctd;
        public ICustomTypeDescriptor GetCustomTypeDescriptor(ICustomTypeDescriptor parent)
        {
            if (_propModel == null || !_propModel.IsFixed)
            {
                if (_ctd == null)
                {
                    _ctd = new PropBagCustomTypeDescriptor(parent, this.CustomPropsGetter);
                }
                return _ctd;
            }
            else
            {
                if (_propModel.CustomTypeDescriptor == null)
                {
                    // Create a new PropertyDescriptorListProvider to hold the fixed list of PropertyDescriptors 
                    // for this fixed PropModel.
                    // This class, instead of this instance the PropBag, will be held by the new CustomTypeDescriptor,
                    // allowing this instance of the PropBag to be Garbage Collected, without interferring with 
                    // the opertion of the other users of this PropModel.
                    IList<PropertyDescriptor> propertyDescriptors = BuildCustomFixedProps(_propModel);
                    PropertyDescriptorStaticListProvider pdList = new PropertyDescriptorStaticListProvider(propertyDescriptors);

                    // Store the CustomTypeDescriptor in the PropModel for all to use.
                    _propModel.CustomTypeDescriptor = new PropBagCustomTypeDescriptor(parent, pdList.CustomPropsGetter);
                }
                return _propModel.CustomTypeDescriptor;
            }
        }

        //protected internal virtual ICustomTypeDescriptor GetCustomTypeDescriptor_Int()
        //{
        //    CustomTypeDescriptor x = null;
        //    return x;
        //}

        public virtual object Clone()
        {
            return new PropBag(this);
        }

        private PSAccessServiceInterface CloneProps(IPropBag copySource, PSAccessServiceInterface storeAccessor)
        {
            PSAccessServiceInterface result = storeAccessor.CloneProps(this, copySource);

            // TODO: FixMe -- PD
            //if (copySource is ICustomTypeDescriptor ictd)
            //{
            //    _properties = ictd.GetProperties();
            //}

            return result;
        }

        public virtual ITypeSafePropBagMetaData GetMetaData()
        {
            return OurMetaData;
        }

        public IEnumerable<KeyValuePair<PropNameType, ValPlusType>> GetAllPropNamesValuesAndTypes()
        {
            IEnumerable<KeyValuePair<PropNameType, IPropData>> theStoreAsCollection = _ourStoreAccessor.GetPropDataItemsWithNames(this);

            IEnumerable<KeyValuePair<string, ValPlusType>> list = theStoreAsCollection.Select(x =>
                new KeyValuePair<string, ValPlusType>(x.Key, x.Value.TypedProp.GetValuePlusType()));

            return list;
        }

        /// <summary>
        /// Returns all of the values in dictionary of objects, keyed by PropertyName.
        /// </summary>
        public IReadOnlyDictionary<PropNameType, IPropData> GetAllPropertyValues()
        {
            IReadOnlyDictionary<PropNameType, IPropData> result = _ourStoreAccessor.GetCollection(this);

            return result;
        }

        public IList<PropNameType> GetAllPropertyNames()
        {
            var result = _ourStoreAccessor.GetKeys(this).ToList();
            return result;
        }

        public bool HasPropModel => _propModel != null;

        //public IEnumerable<IPropItemModel> GetPropItemModels()
        //{
        //    IEnumerable<IPropItemModel> result = _propModel?.GetPropItems() ?? Enumerable.Empty<IPropItemModel>();
        //    return result;
        //}

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
            IEnumerable<KeyValuePair<PropNameType, IPropData>> namesTypesAndValues = GetAllPropertyValues();

            StringBuilder result = new StringBuilder();
            int cnt = 0;

            foreach (KeyValuePair<string, IPropData> kvp in namesTypesAndValues)
            {
                if (cnt++ == 0) result.Append("\n\r");

                result.Append($" -- {kvp.Key}: {kvp.Value.TypedProp.TypedValueAsObject}");
            }

            string sResult = result.ToString();
            return sResult;

            //return "PersonVM --";

        }

        #endregion

        #region Register Enumerable-Type Props
        #endregion

        #region Register ObservableCollection<T> Props

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

        #region Register Collection and CollectionViewSource Props

        public IProp AddCollectionViewSourceProp(string propertyName, IProvideAView viewProvider, IPropTemplate propTemplate)
        {
            IProp cvsProp = _propFactory.CreateCVSProp(propertyName, viewProvider, propTemplate);

            AddProp(propertyName, cvsProp, out PropIdType propId);
            return cvsProp;
        }

        public IProp CreateCollectionViewProp(string propertyName, IProvideAView viewProvider, IPropTemplate propTemplate)
        {
            IProp cvsProp = _propFactory.CreateCVProp(propertyName, viewProvider, propTemplate);
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

        // Eventually calls GetOrAddCViewManager<IDoCRUD<TSource>, TSource, TDestination>(srcPropName, mr);
        // This is currenlty not used (nor is there any test coverage.)
        protected IManageCViews GetOrAddCViewManagerGen(Type typeOfThisProperty, PropNameType srcPropName, IMapperRequest mr)
        {
            // Check the delegate cache to see if a delegate for this already exists, and if not create a new delegate.
            ICacheDelegatesForTypePair<CViewManagerFromDsDelegate> dc = _propFactory.DelegateCacheProvider.GetOrAddCViewManagerCache;
            CViewManagerFromDsDelegate cViewManagerCreator = dc.GetOrAdd(new TypeSafePropertyBag.DelegateCaches.TypePair(mr.SourceType, typeOfThisProperty));

            // Use the delegate to create or fetch the existing Collection View Manager for this propperty.
            // This eventually calls: this.GetOrAddCViewManager
            IManageCViews result = cViewManagerCreator(this, srcPropName, mr);

            return result;
        }

        // Eventually calls GetOrAddCViewManagerProvider<IDoCRUD<TSource>, TSource, TDestination>(viewManagerProviderKey);
        // Used by PropBag::Hydrate::BuildCollectionViewProp
        protected IProvideACViewManager GetOrAddCViewManagerProviderGen(Type typeOfThisProperty, IViewManagerProviderKey viewManagerProviderKey)
        {
            // Check the delegate cache to see if a delegate for this already exists, and if not create a new delegate.
            ICacheDelegatesForTypePair<CViewManagerProviderFromDsDelegate> dc = _propFactory.DelegateCacheProvider.GetOrAddCViewManagerProviderCache;

            Type sourceType = viewManagerProviderKey.MapperRequest.SourceType;
            Type destinationType = typeOfThisProperty;

            TypeSafePropertyBag.DelegateCaches.TypePair tp = new TypeSafePropertyBag.DelegateCaches.TypePair(sourceType, destinationType);

            // Get the CollectionView Manager Provider Creator Delegate from the Delegate Cache.
            CViewManagerProviderFromDsDelegate cViewManagerProviderCreator = dc.GetOrAdd(tp);

            // Use the delegate to create or fetch the existing Collection View Manager for this propperty.
            // This eventually calls: this.GetOrAddCViewManagerProvider
            IProvideACViewManager result = cViewManagerProviderCreator(this, viewManagerProviderKey);

            return result;
        }

        //// TODO: Update to use the CrudWithMappingCreator delegate (has a gen counterpart.)
        //// Currently this is only used by the Gen counterpart -- and its not being used.
        // TODO: This is being replaced by the next method.
        //public IManageCViews GetOrAddCViewManager<TDal, TSource, TDestination>
        //(
        //    PropNameType srcPropName,   // The name of the property that holds the data (of type IDoCRUD<TSource>.)
        //    IMapperRequest mr   // The (non-generic) information necessary to create a AutoMapper Mapper request.
        //)
        //    where TDal : class, IDoCRUD<TSource>
        //    where TSource : class
        //                where TDestination : class, INotifyItemEndEdit, IPropBag
        //{
        //    // Get the PropItem for the property that holds the DataSource (IDoCRUD<TSource>)
        //    IPropData propGen = GetPropGen(srcPropName, propertyType: null, haveValue: false, value: null,
        //        alwaysRegister: false, mustBeRegistered: true, neverCreate: true, 
        //        desiredHasStoreValue: true, wasRegistered: out bool wasRegistered, propId: out PropIdType dalPropId);

        //    if(propGen.IsEmpty)
        //    {
        //        throw new InvalidOperationException($"The {nameof(GetOrAddCViewManager)} cannot find the source PropItem with name = {srcPropName}.");
        //    }

        //    if (!(propGen.TypedProp.PropTemplate.Type is IDoCRUD<TSource>))
        //    {
        //        throw new InvalidOperationException($"The PropItem used as a data source for the CollectionView Manager does not implement IDoCRUD<({typeof(TSource)}.");
        //    }

        //    IManageCViews cViewManager = _ourStoreAccessor.GetOrAddViewManager<TDal, TSource, TDestination>
        //        (
        //        this,
        //        dalPropId,
        //        propGen,
        //        mr,
        //        GetPropBagMapperFactory(),
        //        _propFactory.GetCViewProviderFactory(),
        //        _propFactory.ListCollectionViewCreator<TDestination>()
        //        );

        //    return cViewManager;
        //}

        // TODO: Update to use the CrudWithMappingCreator delegate (has a gen counterpart.)
        // Currently this is only used by the Gen counterpart -- and its not being used.
        public IManageCViews GetOrAddCViewManager_New<TDal, TSource, TDestination>
        (
            PropNameType srcPropName,   // The name of the property that holds the data (of type IDoCRUD<TSource>.)
            IMapperRequest mr   // The (non-generic) information necessary to create a AutoMapper Mapper request.
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
                        where TDestination : class, INotifyItemEndEdit, IPropBag
        {
            // Get the PropItem for the property that holds the DataSource (IDoCRUD<TSource>)
            IPropData propGen = GetPropGen(srcPropName, propertyType: null, haveValue: false, value: null,
                alwaysRegister: false, mustBeRegistered: true, neverCreate: true,
                desiredHasStoreValue: true, wasRegistered: out bool wasRegistered, propId: out PropIdType dalPropId);

            if (propGen.IsEmpty)
            {
                throw new InvalidOperationException($"The {nameof(GetOrAddCViewManager_New)} cannot find the source PropItem with name = {srcPropName}.");
            }

            if (!(propGen.TypedProp.PropTemplate.Type is IDoCRUD<TSource>))
            {
                throw new InvalidOperationException($"The PropItem used as a data source for the CollectionView Manager does not implement IDoCRUD<({typeof(TSource)}.");
            }

            IManageCViews cViewManager = _ourStoreAccessor.GetOrAddViewManager_New<TDal, TSource, TDestination>
                (
                this,
                dalPropId,
                propGen,
                mr,
                BuildCrudWithMapping,
                _propFactory.GetCViewProviderFactory(),
                _propFactory.ListCollectionViewCreator<TDestination>()
                );

            return cViewManager;

            // CrudWithMapping Factory
            IDoCrudWithMapping<TDestination> BuildCrudWithMapping(IWatchAPropItem<TDal> propItemWatcher)
            {
                // TODO: ---------------- 
                IDoCrudWithMapping<TDestination> result = null;
                return result;
            }
        }


        public bool TryGetCViewManagerProvider(PropNameType propertyName, out IProvideACViewManager cViewManagerProvider)
        {
            if(_foreignViewManagers == null || !_foreignViewManagers.TryGetValue(propertyName, out IViewManagerProviderKey viewManagerProviderKey))
            {
                cViewManagerProvider = null;
                return false;
                //throw new KeyNotFoundException($"There is no CViewManagerProvider allocated for PropItem with name = {propertyName}.");
            }

            if (_ourStoreAccessor.TryGetViewManagerProvider(this, viewManagerProviderKey, out cViewManagerProvider))
            {
                return true;
            }
            else
            {
                cViewManagerProvider = null;
                return false;
            }
        }

        //// TODO: Update to use the CrudWithMappingCreator delegate (has a gen counterpart.)
        //// The Gen counterpart is being used.
        //public IProvideACViewManager GetOrAddCViewManagerProvider_OLD<TDal, TSource, TDestination>
        //(
        //    IViewManagerProviderKey viewManagerProviderKey
        //)
        //    where TDal : class, IDoCRUD<TSource>
        //    where TSource : class
        //                where TDestination : class, INotifyItemEndEdit, IPropBag
        //{
        //    IProvideACViewManager cViewManagerProvider = _ourStoreAccessor.GetOrAddViewManagerProvider<TDal, TSource, TDestination>
        //        (
        //        this,
        //        viewManagerProviderKey,
        //        GetPropBagMapperFactory(),
        //        _propFactory.GetCViewProviderFactory()
        //        );

        //    return cViewManagerProvider;
        //}

        // Working this to submit a CrudWithMappingCreator delegate
        public IProvideACViewManager GetOrAddCViewManagerProvider<TDal, TSource, TDestination>
        (
            IViewManagerProviderKey viewManagerProviderKey
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : class, INotifyItemEndEdit, IPropBag
        {
            IProvideACViewManager cViewManagerProvider = _ourStoreAccessor.GetOrAddViewManagerProvider_New<TDal, TSource, TDestination>
                (
                this,
                viewManagerProviderKey,
                BuildCrudWithMapping,
                _propFactory.GetCViewProviderFactory()
                );

            return cViewManagerProvider;

            // CrudWithMapping Factory
            IDoCrudWithMapping<TDestination> BuildCrudWithMapping(IWatchAPropItem<TDal> propItemWatcher)
            {
                // TODO: ---------------- 
                IDoCrudWithMapping<TDestination> result = null;
                return result;
            }
        }

        // TODO: Allow mapper to be null on construction, and then omit mapping functionality, but instead
        // use the IDoCRUD<T> straight, without any mapping

        //// TODO: Update to use the CrudWithMappingCreator delegate.
        //// This is currenlty not used (nor is there any test coverage.)
        //public IProp CreateCollectionViewPropDS<TDal, TSource, TDestination>
        //(
        //    PropNameType propertyName, // The name for the new property.
        //    PropNameType srcPropName,   // The name of the property that holds the data (of type IDoCRUD<TSource>.)
        //    IMapperRequest mr,   // The (non-generic) information necessary to create a AutoMapper Mapper request.
        //    IPropTemplate propTemplate
        //)
        //    where TDal : class, IDoCRUD<TSource>
        //    where TSource : class
        //                where TDestination : class, INotifyItemEndEdit, IPropBag
        //{
        //    // Get the PropItem for the property that holds the DataSource (IDoCRUD<TSource>)
        //    IPropData propGen = GetPropGen(srcPropName, propertyType: null, haveValue: false, value: null,
        //        alwaysRegister: false, mustBeRegistered: true, neverCreate: true,
        //        desiredHasStoreValue: true, wasRegistered: out bool wasRegistered, propId: out PropIdType dalPropId);

        //    if (propGen.IsEmpty)
        //    {
        //        throw new InvalidOperationException($"The {nameof(GetOrAddCViewManager)} cannot find the source PropItem with name = {srcPropName}.");
        //    }

        //    if (!(propGen.TypedProp.PropTemplate.Type is IDoCRUD<TSource>))
        //    {
        //        throw new InvalidOperationException($"The PropItem used as a data source for the CollectionView Manager does not implement IDoCRUD<({typeof(TSource)}.");
        //    }

        //    IManageCViews cViewManager = _ourStoreAccessor.GetOrAddViewManager<TDal, TSource, TDestination>
        //        (
        //        this,
        //        dalPropId,
        //        propGen,
        //        mr,
        //        GetPropBagMapperFactory(),
        //        _propFactory.GetCViewProviderFactory(),
        //        _propFactory.ListCollectionViewCreator<TDestination>()
        //        );

        //    IProvideAView viewProvider = cViewManager.GetDefaultViewProvider();
        //    IProp result = CreateCollectionViewProp(propertyName, viewProvider, propTemplate);
        //    return result;
        //}

        // TODO: Update to use the CrudWithMappingCreator delegate.
        // This is currenlty not used (nor is there any test coverage.)
        public IProp CreateCollectionViewPropDS<TDal, TSource, TDestination>
        (
            PropNameType propertyName, // The name for the new property.
            PropNameType srcPropName,   // The name of the property that holds the data (of type IDoCRUD<TSource>.)
            IMapperRequest mr,   // The (non-generic) information necessary to create a AutoMapper Mapper request.
            IPropTemplate propTemplate
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
                        where TDestination : class, INotifyItemEndEdit, IPropBag
        {
            // Get the PropItem for the property that holds the DataSource (IDoCRUD<TSource>)
            IPropData propGen = GetPropGen(srcPropName, propertyType: null, haveValue: false, value: null,
                alwaysRegister: false, mustBeRegistered: true, neverCreate: true,
                desiredHasStoreValue: true, wasRegistered: out bool wasRegistered, propId: out PropIdType dalPropId);

            if (propGen.IsEmpty)
            {
                throw new InvalidOperationException($"The {nameof(CreateCollectionViewPropDS)} cannot find the source PropItem with name = {srcPropName}.");
            }

            if (!(propGen.TypedProp.PropTemplate.Type is IDoCRUD<TSource>))
            {
                throw new InvalidOperationException($"The PropItem used as a data source for the CollectionView Manager does not implement IDoCRUD<({typeof(TSource)}.");
            }

            IManageCViews cViewManager = _ourStoreAccessor.GetOrAddViewManager_New<TDal, TSource, TDestination>
                (
                this,
                dalPropId,
                propGen,
                mr,
                BuildCrudWithMapping,
                _propFactory.GetCViewProviderFactory(),
                _propFactory.ListCollectionViewCreator<TDestination>()
                );

            IProvideAView viewProvider = cViewManager.GetDefaultViewProvider();
            IProp result = CreateCollectionViewProp(propertyName, viewProvider, propTemplate);
            return result;

            // CrudWithMapping Factory
            IDoCrudWithMapping<TDestination> BuildCrudWithMapping(IWatchAPropItem<TDal> propItemWatcher)
            {
                // TODO: ---------------- 
                IDoCrudWithMapping<TDestination> result2 = null;
                return result2;
            }
        }

        //// Using a IPropBagMapper directly.
        //public IProp CreateCollectionViewPropDS<TDal, TSource, TDestination>
        //(
        //    PropNameType propertyName, // The name for the new property.
        //    PropNameType srcPropName,   // The name of the property that holds the data (of type IDoCRUD<TSource>.)
        //    IPropBagMapper<TSource, TDestination> mapper,    // Optional. The AutoMapper to use to map data from the source to data in the view.
        //    IPropTemplate propTemplate
        //)
        //    where TDal : class, IDoCRUD<TSource>
        //    where TSource : class
        //                where TDestination : class, INotifyItemEndEdit, IPropBag
        //{
        //    IPropData propGen = GetPropGen(srcPropName, propertyType: null, haveValue: false, value: null,
        //        alwaysRegister: false, mustBeRegistered: true, neverCreate: true, 
        //        desiredHasStoreValue: true, wasRegistered: out bool wasRegistered, propId: out PropIdType dalPropId);

        //    if (propGen.IsEmpty)
        //    {
        //        throw new InvalidOperationException($"The {nameof(GetOrAddCViewManager)} cannot find the source PropItem with name = {srcPropName}.");
        //    }

        //    if(!(propGen.TypedProp.PropTemplate.Type is IDoCRUD<TSource>))
        //    {
        //        throw new InvalidOperationException($"The PropItem used as a data source for the CollectionView DataSource does not implement IDoCRUD<({typeof(TSource)}.");
        //    }

        //    IManageCViews cViewManager = _ourStoreAccessor.GetOrAddViewManager<TDal, TSource, TDestination>
        //        (
        //        this,
        //        dalPropId,
        //        propGen,
        //        mapper,
        //        _propFactory.GetCViewProviderFactory(),
        //        _propFactory.ListCollectionViewCreator<TDestination>()
        //        );

        //    IProvideAView viewProvider = cViewManager.GetDefaultViewProvider();
        //    IProp result = CreateCollectionViewProp(propertyName, viewProvider, propTemplate);
        //    return result;
        //}

        //// Typed version, using a IPropBagMapper directly. 
        //public ICViewProp<CVT> AddCollectionViewPropDS_Typed<CVT, TDal, TSource, TDestination>
        //(
        //    PropNameType propertyName,
        //    PropNameType srcPropName,
        //    IPropBagMapper<TSource, TDestination> mapper,
        //    IPropTemplate propTemplate
        //)
        //    where CVT : ICollectionView
        //    where TDal : class, IDoCRUD<TSource>
        //    where TSource : class
        //                where TDestination : class, INotifyItemEndEdit, IPropBag
        //{
        //    IPropData propGen = GetPropGen(srcPropName, propertyType: null, haveValue: false, value: null,
        //        alwaysRegister: false, mustBeRegistered: true, neverCreate: true, 
        //        desiredHasStoreValue: true, wasRegistered: out bool wasRegistered, propId: out PropIdType dalPropId);

        //    if (propGen.IsEmpty)
        //    {
        //        throw new InvalidOperationException($"The {nameof(GetOrAddCViewManager)} cannot find the source PropItem with name = {srcPropName}.");
        //    }

        //    IManageCViews cViewManager = _ourStoreAccessor.GetOrAddViewManager<TDal, TSource, TDestination>
        //        (
        //        this,
        //        dalPropId,
        //        propGen,
        //        mapper,
        //        _propFactory.GetCViewProviderFactory(),
        //        _propFactory.ListCollectionViewCreator<TDestination>()
        //        );

        //    IProvideAView viewProvider = cViewManager.GetDefaultViewProvider();
        //    ICViewProp<CVT> result = (ICViewProp<CVT>)CreateCollectionViewProp(propertyName, viewProvider, propTemplate);
        //    return result;
        //}

        #endregion

        #region Register Property-Type Props

        protected IProp<T> AddProp<T>(string propertyName)
        {
            IProp<T> result = AddProp<T>(propertyName, comparer: null, extraInfo: null, initialValue: default(T));
            return result;
        }

        protected IProp<T> AddProp<T>(string propertyName, T initialValue)
        {
            IProp<T> result = AddProp<T>(propertyName, comparer: null, extraInfo: null, initialValue: initialValue);
            return result;
        }

        /// <summary>
        /// Register a new Prop Item for this PropBag.
        /// </summary>
        /// <typeparam name="T">The type of this property's value.</typeparam>
        /// <param name="propertyName"></param>
        /// <param name="comparer">An instance of a class that implements IEqualityComparer and thus an Equals method.</param>
        /// <param name="extraInfo"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        protected IProp<T> AddProp<T>(string propertyName, Func<T, T, bool> comparer, object extraInfo, T initialValue)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal;
            bool typeIsSolid = true;
            Func<PropNameType, T> getDefaultValueFunc = null;

            // TODO: Get a real value for comparerIsRefEquality
            bool comparerIsRefEquality = false;

            IProp<T> pg = _propFactory.Create<T>(initialValue, propertyName, extraInfo, storageStrategy, typeIsSolid, comparer, comparerIsRefEquality, getDefaultValueFunc);

            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropObjComp<T>(string propertyName, object extraInfo = null, T initialValue = default(T))
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = _propFactory.GetRefEqualityComparer<T>();

            IProp<T> pg = _propFactory.Create<T>(initialValue, propertyName, extraInfo, storageStrategy, typeIsSolid, comparer, true, null);

            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropNoValue<T>(string propertyName, Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal;
            bool typeIsSolid = true;

            // TODO: Get a real value for comparerIsRefEquality
            bool comparerIsRefEquality = false;

            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, typeIsSolid, comparer, comparerIsRefEquality, null);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropObjCompNoValue<T>(string propertyName, object extraInfo = null)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = _propFactory.GetRefEqualityComparer<T>();

            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, typeIsSolid, comparer, true, null);

            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropNoStore<T>(string propertyName, Func<T, T, bool> comparer = null, object extraInfo = null)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.External;
            bool typeIsSolid = true;

            // TODO: Get a real value for comparerIsRefEquality
            bool comparerIsRefEquality = false;

            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, typeIsSolid, comparer, comparerIsRefEquality, null);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        protected IProp<T> AddPropObjCompNoStore<T>(string propertyName, object extraInfo = null)
        {
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.External;
            bool typeIsSolid = true;
            Func<T, T, bool> comparer = _propFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = _propFactory.CreateWithNoValue<T>(propertyName, extraInfo, storageStrategy, typeIsSolid, comparer, true, null);
            AddProp<T>(propertyName, pg, null, SubscriptionPriorityGroup.Standard, out PropIdType propId);
            return pg;
        }

        #endregion

        #region Property Management

        protected IPropData AddProp<T>(string propertyName, IProp<T> genericTypedProp, EventHandler<PcTypedEventArgs<T>> doWhenChanged,
            SubscriptionPriorityGroup priorityGroup, out PropIdType propId)
        {
            //if (!_ourStoreAccessor.TryAdd(this, propertyName, genericTypedProp, doWhenChanged,
            //    priorityGroup, out IPropData propGen, out propId))
            //{
            //    throw new ApplicationException("Could not add the new propGen to the store.");
            //}

            //return propGen;

            bool addToModel = true;

            object target = doWhenChanged?.Target;
            MethodInfo method = doWhenChanged?.Method;
            SubscriptionKind subscriptionKind = SubscriptionKind.GenHandler;

            IPropData result = AddProp_Internal(propertyName, genericTypedProp,
                target: target, method: method, subscriptionKind: subscriptionKind, priorityGroup: priorityGroup,
                addToModel: addToModel, propId: out propId);

            return result;
        }

        protected IPropData AddProp(string propertyName, IProp genericTypedProp, out PropIdType propId)
        {
            //if (!_ourStoreAccessor.TryAdd(this, propertyName, genericTypedProp, out IPropData propGen, out propId))
            //{
            //    throw new ApplicationException("Could not add the new propGen to the store.");
            //}
            //return propGen;

            bool addToModel = true;

            IPropData result = AddProp_Internal(propertyName, genericTypedProp, 
                target: null, method: null, subscriptionKind: SubscriptionKind.GenHandler, priorityGroup: SubscriptionPriorityGroup.Standard,
                addToModel: addToModel, propId: out propId);

            return result;
        }

        protected IPropData AddProp(string propertyName, IProp genericTypedProp, object target, MethodInfo method, 
            SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup, out PropIdType propId)
        {
            //if (!_ourStoreAccessor.TryAdd(this, propertyName, genericTypedProp, target, method,
            //    subscriptionKind, priorityGroup, out IPropData propGen, out propId))
            //{
            //    throw new ApplicationException("Could not add the new propGen to the store.");
            //}

            //return propGen;

            bool addToModel = true;

            IPropData result = AddProp_Internal(propertyName, genericTypedProp,
                target, method, subscriptionKind, priorityGroup,
                addToModel, out propId);

            return result;
        }

        private IPropData AddProp_Internal(string propertyName, IProp genericTypedProp, object target, MethodInfo method,
            SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup, bool addToModel, out PropIdType propId)
        {
            if (!_ourStoreAccessor.TryAdd(this, propertyName, genericTypedProp, target, method, subscriptionKind, priorityGroup,
                out IPropData propGen, out propId))
            {
                throw new ApplicationException("Could not add the new propGen to the store.");
            }

            if (addToModel && _propModel != null)
            {
                IPropItemModel propItem = GetPropItemRecord(propertyName, propGen);
                _propModel.Add(propertyName, propItem);
            }

            return propGen;
        }

        protected bool IsPropSetFixed => _propModel?.IsFixed ?? false;

        protected bool TryFixPropSet()
        {
            if (_propModel == null) return true;

            if (_viewModelFactory.PropModelCache == null)
            {
                _propModel.Fix();
                return true;
            }
            else
            {
                bool result = _viewModelFactory.PropModelCache.TryFix(_propModel);
                return result;
            }
        }

        protected bool TryOpenPropSet()
        {
            if (_propModel == null) return true;

            if(_viewModelFactory.PropModelCache == null)
            {
                _propModel.Open();
                _propertyDescriptors = null;
                return true;
            }
            else
            {
                _propModel = _viewModelFactory.PropModelCache.Open(_propModel, out long generationId);
                _propertyDescriptors = null;
                return true;
            }
        }

        private PropItemModel GetPropItemRecord(PropNameType propertyName, IPropData propGen)
        {
            IPropTemplate propTemplate = propGen.TypedProp?.PropTemplate ?? throw new InvalidOperationException("The PropTemplate is null during call to GetPropItemRecord.");

            PropItemModel propItemModel = new PropItemModel
                (
                type: propTemplate.Type,
                name: propertyName,
                storageStrategy: propTemplate.StorageStrategy,
                typeIsSolid: propGen.TypedProp.TypeIsSolid,
                propKind: propTemplate.PropKind,
                propTypeInfoField: null,
                initialValueField: null,
                extraInfo: null,
                comparer: new PropComparerField(propTemplate.ComparerProxy, false),
                itemType: null,
                binderField: null,
                mapperRequest: null,
                propCreator: propTemplate.PropCreator
                )
            {
                InitialValueCooked = propGen.TypedProp.TypedValueAsObject
            };
            PropItemModel propItem = propItemModel;
            return propItem;
        }

        // TODO: Also remove the Prop from the PropModel.
        protected bool TryRemoveProp(string propertyName, Type propertyType)
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
                if (_ourStoreAccessor.TryRemove(this, propId, out IPropData foundValue))
                {
                    return true;
                }
                else
                {
                    Debug.WriteLine($"The prop was found, but could not be removed. Property: {propertyName}.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"Could not remove property: {propertyName}.");
                return false;
            }
        }
        
        protected bool TryRemoveProp<T>(string propertyName)
        {
            bool mustBeRegistered = _ourMetaData.TypeSafetyMode == PropBagTypeSafetyMode.Locked;

            IPropData propData = GetPropGenPrivate<T>(propertyName, mustBeRegistered: mustBeRegistered, neverCreate: true, propId: out PropIdType propId);

            if (!propData.IsEmpty)
            {
                if (_ourStoreAccessor.TryRemove(this, propId, out IPropData foundValue))
                {
                    return true;
                }
                else
                {
                    Debug.WriteLine($"The prop was found, but could not be removed. Property: {propertyName}.");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"Could not remove property: {propertyName}.");
                return false;
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
                    if(subscriptions != null)
                        DoNotifyWork(propId, propertyName, typedProp, oldValue, newValue, subscriptions);

                    if (globalSubs != null && globalSubs.Count() > 0)
                        DoNotifyWork(propId, propertyName, typedProp, oldValue, newValue, globalSubs);
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
                if (subscriptions != null)
                    DoNotifyWork(propId, propertyName, typedProp, newValue, subscriptions);

                if (globalSubs != null && globalSubs.Count() > 0)
                    DoNotifyWork(propId, propertyName, typedProp, newValue, globalSubs);

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
                    object target = sub.Target;
                    if (target == null)
                    {
                        Debug.WriteLine($"The object listening to this event is 'no longer with us.'");
                    }
                    else
                    {
                        PropertyChangingEventArgs e = new PropertyChangingEventArgs(propertyName);
                        sub.PChangingHandlerDispatcher(target, this, e, sub.HandlerProxy);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"A Changing (PropertyChangingEventHandler) handler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
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

                object target = sub.Target;
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
                    Debug.WriteLine($"A {sub.SubscriptionKind} handler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
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

            if (sub.Target == null)
            {
                Debug.WriteLine($"The object listening to this event is 'no longer with us.'");
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

                object target = sub.Target;
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
                    Debug.WriteLine($"A {sub.SubscriptionKind} handler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
                }
            }
        }

        private IEnumerable<ISubscription> GetSubscriptions(PropIdType propId)
        {
            IEnumerable<ISubscription> sc = _ourStoreAccessor.GetSubscriptions(this, propId);
            return sc;
        }

        //protected IProp<T> GetTypedProp<T>(string propertyName)
        //{
        //    return (IProp<T>)GetTypedPropPrivate<T>(propertyName, mustBeRegistered: true, neverCreate: false);
        //}

        protected IProp<T> GetTypedProp<T>(string propertyName, bool mustBeRegistered, bool neverCreate)
        {
            IPropData PropData = GetPropGenPrivate<T>(propertyName, mustBeRegistered, neverCreate, out PropIdType notUsed);

            if (!PropData.IsEmpty)
            {
                return (IProp<T>)PropData.TypedProp;
            }
            else
            {
                return null;
            }
        }

        //protected IReadOnlyCProp<CT, T> GetCProp<CT, T>(string propertyName) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
        //{
        //    return (IReadOnlyCProp<CT, T>)GetTypedCPropPrivate<CT, T>(propertyName, mustBeRegistered: true);
        //}

        protected ICProp<CT, T> GetTypedProp<CT, T>(string propertyName, bool mustBeRegistered, bool neverCreate = false) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
        {
            IPropData PropData = GetPropGenPrivate<T>(propertyName, mustBeRegistered, neverCreate, out PropIdType notUsed);

            if (!PropData.IsEmpty)
            {
                return (ICProp<CT, T>)PropData.TypedProp;
            }
            else
            {
                return null;
            }
        }

        // Used when the value (for type checking) is not available.
        private IPropData GetPropGenPrivate<T>(string propertyName,
            bool mustBeRegistered, bool neverCreate, out PropIdType propId)
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

        protected IPropData GetPropGen<T>(PropNameType propertyName,
            bool haveValue, object value, bool alwaysRegister,
            bool mustBeRegistered, bool neverCreate, bool? desiredHasStoreValue, out bool wasRegistered, out PropIdType propId)
        {
            // Commented this out, in favor of calling the non-generic GetPropGen
            // because this does not check the type of the registered property to see if matches typeof(T).
            //if (TryGetPropId(propertyName, out propId))
            //{
            //    if (!_ourStoreAccessor.TryGetValue(this, propId, out IPropData propData))
            //    {
            //        throw new KeyNotFoundException($"The property named: {propertyName} could not be fetched from the property store.");
            //    }

            //    CheckStorageStrategy(propertyName, propData, desiredHasStoreValue);

            //    wasRegistered = false;
            //    return (IPropData)propData;
            //}
            //else
            //{
            //    throw new KeyNotFoundException($"The property named: {propertyName} could not be found in the PropCollection for this PropBag.");
            //}

            IPropData result = GetPropGen(propertyName, typeof(T), haveValue, value, alwaysRegister, mustBeRegistered,
                neverCreate, desiredHasStoreValue, out wasRegistered, out propId);

            return result;
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
                Debug.WriteLine("Acessing PropItem with name starting: 'Business'.");
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

        public void RaiseStandardPropertyChanged(PropNameType propertyName)
        {
            if (TryGetPropId(propertyName, out PropIdType propId))
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
                    object target = sub.Target;
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
                        Debug.WriteLine($"A {sub.SubscriptionKind} handler raised an exception: {e.Message} with inner: {e.InnerException?.Message} ");
                    }
                }
            }
        }

        // OLD CODE
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
            Debug.WriteLine($"BeginEdit was called on PropBag: {FullClassName}.");
        }

        public void CancelEdit()
        {
        }

        public void EndEdit()
        {
            ItemEndEdit?.Invoke(this, EventArgs.Empty);
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

        #endregion

        #region View Manager Support

        //public IManageCViews GetOrAddViewManager<TDal, TSource, TDestination>
        //(
        //    PropNameType propertyName,
        //    Type propertyType,
        //    IPropBagMapper<TSource, TDestination> mapper
        //)
        //    where TDal : IDoCRUD<TSource>
        //    where TSource : class
        //                where TDestination : class, INotifyItemEndEdit, IPropBag
        //{
        //    IManageCViews result = null;
        //    return result;
        //}

        //// TODO: Being Replaced with New Version -- see next method.
        //public IManageCViews GetOrAddViewManager<TDal, TSource, TDestination>
        //(
        //    PropNameType propertyName,
        //    Type propertyType,
        //    IMapperRequest mr
        //)
        //    where TDal : class, IDoCRUD<TSource>
        //    where TSource : class
        //                where TDestination : class, INotifyItemEndEdit, IPropBag
        //{
        //    bool mustBeRegistered = true; // TryGetViewManager is called in the constructor, we cannot reference the virtual property: OurMetaData.AllPropsMustBeRegistered; 

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
        //        IManageCViews result = _ourStoreAccessor.GetOrAddViewManager<TDal, TSource, TDestination>
        //            (
        //            this,
        //            propId,
        //            propData,
        //            mr,
        //            GetPropBagMapperFactory(),
        //            _propFactory.GetCViewProviderFactory(),
        //            _propFactory.ListCollectionViewCreator<TDestination>()
        //            );

        //        // Used to test the ability to create a new Prop that has a PropertyType = to some Emitted Type.
        //        //result.DataSourceProvider.DataChanged += DataSourceProvider_DataChanged;

        //        return result;
        //    }
        //    else
        //    {
        //        return null;
        //    }

        //    //// Just for Diagnostics
        //    //void DataSourceProvider_DataChanged(object sender, EventArgs e)
        //    //{
        //    //    if(sender is DataSourceProvider dsp)
        //    //    {
        //    //        Type rtt = GetCollectionItemRunTimeType<TDal, TSource, TDestination>(dsp);

        //    //        if (rtt != null)
        //    //        {
        //    //            _propFactory.CreateGenWithNoValue(rtt, "test", null, PropStorageStrategyEnum.Internal, true, PropKindEnum.Prop, null, false, null);
        //    //        }
        //    //    }
        //    //}
        //}

        // TODO: Working on this to use the CrudWithMappingCreator delegate (has a gen counterpart.)
        public IManageCViews GetOrAddViewManager_New<TDal, TSource, TDestination>
        (
            PropNameType propertyName,
            Type propertyType,
            IMapperRequest mr
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : class, INotifyItemEndEdit, IPropBag
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
                CrudWithMappingCreator<TDal, TSource, TDestination> cwmc = BuildCrudWithMapping<TDal, TSource, TDestination>;

                IManageCViews result = _ourStoreAccessor.GetOrAddViewManager_New<TDal, TSource, TDestination>
                    (
                    this,
                    propId,
                    propData,
                    mr,
                    cwmc, //BuildCrudWithMapping,
                    _propFactory.GetCViewProviderFactory(),
                    _propFactory.ListCollectionViewCreator<TDestination>()
                    );

                // Used to test the ability to create a new Prop that has a PropertyType = to some Emitted Type.
                //result.DataSourceProvider.DataChanged += DataSourceProvider_DataChanged;

                return result;
            }
            else
            {
                return null;
            }

            // CrudWithMapping Factory
            IDoCrudWithMapping<TDestination> BuildCrudWithMapping<TDal, TSource, TDestination>(IWatchAPropItem<TDal> propItemWatcher)
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : class, INotifyItemEndEdit, IPropBag
            {
                IPropBagMapper<TSource, TDestination> propBagMapper = GetPropBagMapper<TSource, TDestination>
                    (
                    mr,
                    _propBagMapperService,
                    out IPropBagMapperRequestKey<TSource, TDestination> propBagMapperRequestKey
                    );

                // Create a IDoCRUD<TSource> using the watcher and mapper
                IDoCrudWithMapping<TDestination> result =
                    new CrudWithMapping<TDal, TSource, TDestination>(propItemWatcher, propBagMapper);

                return result;
            }

            //// Just for Diagnostics
            //void DataSourceProvider_DataChanged(object sender, EventArgs e)
            //{
            //    if(sender is DataSourceProvider dsp)
            //    {
            //        Type rtt = GetCollectionItemRunTimeType<TDal, TSource, TDestination>(dsp);

            //        if (rtt != null)
            //        {
            //            _propFactory.CreateGenWithNoValue(rtt, "test", null, PropStorageStrategyEnum.Internal, true, PropKindEnum.Prop, null, false, null);
            //        }
            //    }
            //}
        }

        private IPropBagMapper<TSource, TDestination> GetPropBagMapper<TSource, TDestination>
            (
            IMapperRequest mapperRequest,
            IPropBagMapperService propBagMapperService,
            out IPropBagMapperRequestKey<TSource, TDestination> propBagMapperRequestKey
            )
            where TDestination : class, IPropBag
        {
            // TODO: See if we can submit the request earlier; perhaps when the mapper request is created.

            // Submit the Mapper Request.
            propBagMapperRequestKey = propBagMapperService.SubmitPropBagMapperRequest<TSource, TDestination>
                (mapperRequest.PropModel, mapperRequest.ConfigPackageName);

            // Get the AutoMapper mapping function associated with the mapper request just submitted.
            IPropBagMapper<TSource, TDestination> propBagMapper
                = propBagMapperService.GetPropBagMapper<TSource, TDestination>(propBagMapperRequestKey);

            return propBagMapper;
        }

        // This is used by this.BuildPropItems.
        private IPropBagMapperGen GetPropBagMapperGen
        (
            IMapperRequest mapperRequest,
            IPropBagMapperService propBagMapperService,
            out IPropBagMapperRequestKeyGen propBagMapperRequestKeyGen
        )
        {
            // Submit the Mapper Request. TODO: See if we can submit the request earlier; perhaps when the mapper request is created.
            propBagMapperRequestKeyGen = propBagMapperService.SubmitPropBagMapperRequest
                (mapperRequest.PropModel, mapperRequest.SourceType, mapperRequest.ConfigPackageName);

            // Get the AutoMapper mapping function associated with the mapper request just submitted.
            IPropBagMapperGen propBagMapperGen = propBagMapperService.GetPropBagMapper(propBagMapperRequestKeyGen);

            return propBagMapperGen;
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

        /// <summary>
        /// In those cases where the View's data is retreived from a 'foreign' PropItem, this should be called on 
        /// the foreign View Model (i.e., PropBag.)
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyType"></param>
        /// <param name="cViewManager"></param>
        /// <returns></returns>
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
                // TODO: Consider fixing up this commented-out code to handle cases 
                // when the property specified is the target of a CollectionViewManagerBinding.

                //if(propData.TypedProp.PropTemplate.PropKind == PropKindEnum.CollectionView)
                //{
                //    if(propData.TypedProp is IProvideAView viewProvider)
                //    {
                //        if(viewProvider.DataSourceProvider is IProvideADataSourceProvider dsProviderProvider)
                //        {
                //            Type collectionItemRunTimeType = dsProviderProvider.CollectionItemRunTimeType;
                //            // Next try to get the CLR_Mapped_DSP<DestinationT>
                //        }
                //    }
                //}

                //if(_foreignViewManagers.TryGetValue(propertyName, out IViewManagerProviderKey vmpKey))
                //{

                //}

                cViewManager = _ourStoreAccessor.GetViewManager(this, propId);
                return true;
            }
            else
            {
                cViewManager = null;
                return false;
            }
        }

        protected internal Type GetCollectionItemRunTimeType(IManageCViews cViewManager)
        {
            DataSourceProvider dsp = cViewManager.DataSourceProvider;

            try
            {
                Type result = GetCollectionItemRunTimeType(dsp);

                return result;
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException($"The CollectionViewManager's DataSourceProvider does not implement the IProvideADataSourceProvider interface.");
            }
        }

        protected internal Type GetCollectionItemRunTimeType(DataSourceProvider dsp)
        {
            if(dsp is IProvideADataSourceProvider dsProviderProvider)
            {
                Type result = dsProviderProvider.CollectionItemRunTimeType;
                return result;
            }
            else
            {
                throw new InvalidOperationException($"The DataSourceProvider does not implement the IProvideADataSourceProvider interface.");
            }
        }

        protected internal Type GetCollectionItemRunTimeType<TDal, TSource, TDestination>(IManageCViews cViewManager)
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
                        where TDestination : class, INotifyItemEndEdit, IPropBag
        {
            DataSourceProvider dsp = cViewManager.DataSourceProvider;
            Type result = GetCollectionItemRunTimeType<TDal, TSource, TDestination>(dsp);
            return result;
        }

        protected internal Type GetCollectionItemRunTimeType<TDal, TSource, TDestination>(DataSourceProvider dsp)
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
                        where TDestination : class, INotifyItemEndEdit, IPropBag
        {
            //  CrudWithMapping<TDal, TSource, TDestination>;

            if (dsp is ClrMappedDSP<TDestination> mappedDsp)
            {
                //var z = y.CrudWithMapping;

                //var a = z as CrudWithMapping<TDal, TSource, TDestination>;
                //return a.Mapper.TargetRunTimeType;

                Type result = (mappedDsp as IProvideADataSourceProvider)?.CollectionItemRunTimeType;
                return result;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Custom Type Descriptor Support

        // TOOD: Consider building the PropertyDescriptors directly from our list of PropData Items.
        //_ourStoreAccessor.GetPropDataItemsWithNames(this);

        protected internal Func<IList<PropertyDescriptor>> CustomPropsGetter => GetPropertyDescriptors;

        private IList<PropertyDescriptor> _propertyDescriptors;

        private IList<PropertyDescriptor> GetPropertyDescriptors()
        {
            IList<PropertyDescriptor> result;

            if (HasPropModel)
            {
                // Use the List of PropItems from the PropModel
                if (_propModel.IsFixed)
                {
                    // Build and Cache.
                    if (_propertyDescriptors == null)
                    {
                        //if(_propModel.ClassName == "MainWindowViewModel")
                        //{
                        //    Debug.WriteLine("Getting Custom Props for the MainWindowViewModel.");
                        //}

                        IEnumerable<IPropItemModel> propItemModels = _propModel.GetPropItems();
                        _propertyDescriptors = BuildPropertyDescriptors(propItemModels);
                    }
                    result = _propertyDescriptors;
                }
                else
                {
                    // Build a new set every time.
                    IEnumerable<IPropItemModel> propItemModels = _propModel.GetPropItems();
                    result = BuildPropertyDescriptors(propItemModels);
                }
            }
            else
            {
                // Use the list of registered IProps.
                // TODO: See if we can cache this list of PropertyDescriptors.
                result = new List<PropertyDescriptor>();

                foreach (KeyValuePair<string, ValPlusType> kvp in GetAllPropNamesValuesAndTypes())
                {
                    PropertyDescriptor propItemTypeDesc =
                        new PropItemPropertyDescriptor<PropBag>(kvp.Key, kvp.Value.Type, new Attribute[] { });

                    result.Add(propItemTypeDesc);
                }
            }

            return result;
        }

        private IList<PropertyDescriptor> BuildCustomFixedProps(PropModelType propModel)
        {
            if(!propModel.IsFixed)
            {
                throw new InvalidOperationException("This can only be called using a fixed propModel.");
            }

            IEnumerable<IPropItemModel> propItemModels = _propModel.GetPropItems();

            IList<PropertyDescriptor> result = new List<PropertyDescriptor>();


            PSFastAccessServiceInterface propStoreFastAccess = _ourStoreAccessor.GetFastAccessService();

            foreach (IPropItemModel pi in propItemModels)
            {
                PropItemFixedPropertyDescriptor<PropBag> propItemTypeDesc;

                if(!TryGetPropId(pi.PropertyName, out PropIdType propertyId))
                {
                    throw new InvalidOperationException("Could not get the property id.");
                }

                if (pi.PropKind == PropKindEnum.CollectionView || pi.PropKind == PropKindEnum.ObservableCollection || pi.PropKind == PropKindEnum.Prop)
                {
                    propItemTypeDesc = new PropItemFixedPropertyDescriptor<PropBag>(propStoreFastAccess, propModel, propertyId, pi.PropertyName, pi.PropertyType, new Attribute[] { });
                }
                else
                {
                    throw new InvalidOperationException($"The {nameof(PropBagTypeDescriptionProvider<PropBag>)} does not recognized or does not support Props of Kind = {pi.PropKind}.");
                }

                result.Add(propItemTypeDesc);
            }

            return result;
        }

        private IList<PropertyDescriptor> BuildPropertyDescriptors(IEnumerable<IPropItemModel> propItemModels)
        {
            IList<PropertyDescriptor> result = new List<PropertyDescriptor>();

            foreach (IPropItemModel pi in propItemModels)
            {
                PropItemPropertyDescriptor<PropBag> propItemTypeDesc;

                if (pi.PropKind == PropKindEnum.CollectionView || pi.PropKind == PropKindEnum.ObservableCollection || pi.PropKind == PropKindEnum.Prop)
                {
                    propItemTypeDesc = new PropItemPropertyDescriptor<PropBag>(pi.PropertyName, pi.PropertyType, new Attribute[] { });
                }
                else
                {
                    throw new InvalidOperationException($"The {nameof(PropBagTypeDescriptionProvider<PropBag>)} does not recognized or does not support Props of Kind = {pi.PropKind}.");
                }

                result.Add(propItemTypeDesc);
            }

            return result;
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
        static private IManageCViews CViewManagerFromDsBridge<TSource, TDestination>(IPropBag target, PropNameType srcPropName, IMapperRequest mr)
            where TSource : class
                        where TDestination : class, INotifyItemEndEdit, IPropBag
        {
            PropBag pb = (PropBag)target;
            IManageCViews result = pb.GetOrAddCViewManager_New<IDoCRUD<TSource>, TSource, TDestination>(srcPropName, mr);
            return result;
        }

        // Create a CollectionView Manager Provider from a viewManagerProviderKey. Key consists of an optional MapperRequest and a Binding Path.)
        static private IProvideACViewManager CViewManagerProviderFromDsBridge<TSource, TDestination>(IPropBag target, IViewManagerProviderKey viewManagerProviderKey)
            where TSource : class
                        where TDestination : class, INotifyItemEndEdit, IPropBag
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
                int result = _propFactory.CreateScalarPropCacheCount;
                return result;
            }
        }

        #endregion
    }
}
