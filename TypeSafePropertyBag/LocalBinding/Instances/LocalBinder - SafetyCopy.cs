using System;
using DRM.TypeSafePropertyBag.LocalBinding.Engine;

namespace DRM.TypeSafePropertyBag.LocalBinding
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceInternalType = IPropStoreAccessServiceInternal<UInt32, String>;

    // TODO: Separate the two functions being peformed in this class:
    // 1. Create a copy of this class named: LocalWatcher<T> and have it only support 
    //      The use case where a IReceivePropStoreNodeUpdates receiver is provided.
    // 2. Change this class so that it encapsulates a LocalWatcher<T>.

    public class LocalBinder<T> : IDisposable
    {
        const string BINDER_NAME = "PB_LocalBinder";

        #region Private Properties

        readonly WeakReference<PSAccessServiceType> _propStoreAccessService_wr;

        readonly StoreNodeBag _ourNode;

        readonly IReceivePropStoreNodeUpdates _storeNodeUpdateReceiver;
        readonly ExKeyT _bindingTarget;
        readonly WeakReference<IPropBagInternal> _targetObject;
        readonly PropNameType _propertyName;

        readonly LocalBindingInfo _bindingInfo;

        PropStorageStrategyEnum _targetHasStore;

        ObservableSource<T> _rootListener;
        readonly OSCollection<T> _pathListeners;
        string[] _pathElements;

        bool _isPathAbsolute;
        bool _isComplete;
        int _firstNamedStepIndex;

        #endregion

        #region Public Properties

        public bool NoTarget_NotifiesReceiverInstead => _bindingTarget.IsEmpty;

        public ExKeyT BindingTarget => _bindingTarget;

        public LocalBindingInfo BindingInfo => _bindingInfo;
        public bool PathIsAbsolute => _isPathAbsolute;
        public bool Complete => _isComplete;

        public PropIdType PropId => _bindingTarget.Level2Key;
        public PropNameType PropertyName => _propertyName;

        #endregion

        #region Converter Properties

        //Lazy<IValueConverter> _defaultConverter;
        //public virtual Lazy<IValueConverter> DefaultConverter
        //{
        //    get
        //    {
        //        if(_defaultConverter == null)
        //        {
        //            return new Lazy<IValueConverter>(() => new PropValueConverter());
        //        }
        //        return _defaultConverter;
        //    }
        //    set
        //    {
        //        _defaultConverter = value;
        //    }
        //}

        //Func<BindingTarget, MyBindingInfo, Type, string, object> _defConvParamBuilder;
        //public virtual Func<BindingTarget, MyBindingInfo, Type, string, object> DefaultConverterParameterBuilder
        //{
        //    get
        //    {
        //        if (_defConvParamBuilder == null)
        //        {
        //            return OurDefaultConverterParameterBuilder;
        //        }
        //        return _defConvParamBuilder;
        //    }
        //    set
        //    {
        //        _defConvParamBuilder = value;
        //    }
        //}
        #endregion

        #region Constructor

        internal LocalBinder(PSAccessServiceType propStoreAccessService, LocalBindingInfo bindingInfo, IReceivePropStoreNodeUpdates storeNodeUpdateReceiver)
        {
            _propStoreAccessService_wr = new WeakReference<PSAccessServiceType>(propStoreAccessService);
            _bindingInfo = bindingInfo;
            _storeNodeUpdateReceiver = storeNodeUpdateReceiver;

            _bindingTarget = new SimpleExKey();

            // Get the PropStore Node for the IPropBag object hosting the property that is the target of the binding.
            // TODO: Instead of doing this now, create a property that allows us to access upon first access.
            _ourNode = GetPropBagNode(propStoreAccessService);

            _targetObject = null;

            _propertyName = null;
            _targetHasStore = PropStorageStrategyEnum.Virtual;

            _pathElements = GetPathElements(_bindingInfo, out _isPathAbsolute, out _firstNamedStepIndex);

            if (_isPathAbsolute)
            {
                _rootListener = CreateAndListen(_ourNode, "root", SourceKindEnum.AbsRoot);
            }
            else
            {
                _rootListener = null;
            }

            _pathListeners = new OSCollection<T>();

            _isComplete = StartBinding(_targetObject, _pathElements, _pathListeners, _isPathAbsolute);
        }


        public LocalBinder(PSAccessServiceType propStoreAccessService, ExKeyT ownerPropId, LocalBindingInfo bindingInfo)
        {
            _propStoreAccessService_wr = new WeakReference<PSAccessServiceType>(propStoreAccessService);
            _bindingTarget = ownerPropId;
            _bindingInfo = bindingInfo;
            _storeNodeUpdateReceiver = null;


            // Get the PropStore Node for the IPropBag object hosting the property that is the target of the binding.
            _ourNode = GetPropBagNode(propStoreAccessService);

            PropIdType propId = _bindingTarget.Level2Key;

            _targetObject = _ourNode.PropBagProxy;

            if (_targetObject.TryGetTarget(out IPropBagInternal propBag))
            {
                _propertyName = GetPropertyName(propStoreAccessService, propBag, propId, out PropStorageStrategyEnum storageStrategy);
                _targetHasStore = storageStrategy;
            }

            _pathElements = GetPathElements(_bindingInfo, out _isPathAbsolute, out _firstNamedStepIndex);

            if (_isPathAbsolute)
            {
                _rootListener = CreateAndListen(_ourNode, "root", SourceKindEnum.AbsRoot);
            }
            else
            {
                _rootListener = null;
            }

            _pathListeners = new OSCollection<T>();

            _isComplete = StartBinding(_targetObject, _pathElements, _pathListeners, _isPathAbsolute);
        }

        private StoreNodeBag GetPropBagNode(PSAccessServiceType propStoreAccessService)
        {
            if (propStoreAccessService is IHaveTheStoreNode storeNodeProvider)
            {
                StoreNodeBag propStoreNode = storeNodeProvider.PropStoreNode;
                return propStoreNode;
            }
            else
            {
                throw new InvalidOperationException($"The {nameof(propStoreAccessService)} does not implement the {nameof(IHaveTheStoreNode)} interface.");
            }
        }

        // TODO: should be able to have IPropStoreAccessServiceInternal provide all of this with a single call.
        private PropNameType GetPropertyName(PSAccessServiceType propStoreAccessService, IPropBagInternal propBag, PropIdType propId, out PropStorageStrategyEnum storageStrategy)
        {
            PropNameType result;

            if (propStoreAccessService.TryGetPropName(propId, out PropNameType propertyName))
            {
                result = propertyName;
            }
            else
            {
                throw new InvalidOperationException("Cannot retrieve the Target's property name from the TargetPropId.");
            }

            if (propStoreAccessService.TryGetValue((IPropBag) propBag, propId, out IPropData genProp))
            {
                storageStrategy = genProp.TypedProp.StorageStrategy;
            }
            else
            {
                throw new InvalidOperationException("Cannot retrieve the Target's property name from the TargetPropId.");
            }

            return result;
        }

        #endregion

        #region Path Processing

        private string[] GetPathElements(LocalBindingInfo bInfo, out bool pathIsAbsolute, out int firstNamedStepIndex)
        {
            string[] pathElements = bInfo.PropertyPath.Path.Split('/');
            int compCount = pathElements.Length;

            if (compCount == 0)
            {
                throw new InvalidOperationException("The path has no components.");
            }

            if (pathElements[compCount - 1] == "..")
            {
                throw new InvalidOperationException("The last component of the path cannot be '..'");
            }

            if (compCount == 1 && pathElements[0] == ".")
            {
                // Can't bind to yourself.
            }

            // Remove initial "this" component, if present.
            if (pathElements[0] == ".")
            {
                pathElements = RemoveFirstItem(pathElements);
                compCount--;
                if (pathElements[0] == ".") throw new InvalidOperationException("A path that starts with '././' is not supported.");
            }

            if (pathElements[0] == string.Empty)
            {
                pathIsAbsolute = true;

                // remove the initial (empty) path component.
                pathElements = RemoveFirstItem(pathElements);
                compCount--;

                if (pathElements[0] == "..") throw new InvalidOperationException("Absolute Paths cannot refer to a parent. (Path begins with '/../'.");
                firstNamedStepIndex = 0;

                // TODO: Listen to changes in the value of our node's root.
            }
            else
            {
                pathIsAbsolute = false;
                firstNamedStepIndex = GetFirstPathElementWithName(0, pathElements);

            }

            CheckForBadParRefs(firstNamedStepIndex + 1, pathElements);
            return pathElements;
        }

        private int GetFirstPathElementWithName(int nPtr, string[] pathElements)
        {
            for (; nPtr < pathElements.Length; nPtr++)
            {
                if (pathElements[nPtr] != "..") break;
            }
            return nPtr;
        }

        private void CheckForBadParRefs(int nPtr, string[] pathElements)
        {
            for (; nPtr < pathElements.Length - 1; nPtr++)
            {
                if (pathElements[nPtr] == "..")
                {
                    throw new InvalidOperationException("A path cannot refer to a parent once a path element references a property by name.");
                }
            }
        }

        private bool StartBinding(WeakReference<IPropBagInternal> bindingTarget, string[] pathElements, OSCollection<T> pathListeners, bool pathIsAbsolute)
        {
            StoreNodeBag next;

            if (pathIsAbsolute)
            {
                next = _ourNode.Root;
                if (next == null)
                {
                    System.Diagnostics.Debug.WriteLine("OurNode's root is null when starting the local binding.");
                    return false;
                }
            }
            else
            {
                next = _ourNode;
            }

            int nPtr = 0;
            bool complete = HandleNodeUpdate(bindingTarget, next, pathElements, pathListeners, nPtr);
            return complete;
        }

        private bool HandleNodeUpdate(WeakReference<IPropBagInternal> bindingTarget, StoreNodeBag next,
            string[] pathElements, OSCollection<T> pathListeners, int nPtr)
        {
            bool complete = false;

            // Process each step, except for the last.
            for (; next != null && nPtr < pathElements.Length - 1; nPtr++)
            {
                string pathComp = pathElements[nPtr];
                if (pathComp == "..")
                {
                    bool listenerWasAdded = AddOrUpdateListener(next, pathComp, SourceKindEnum.Up, pathListeners, nPtr);
                    string mg = listenerWasAdded ? "added" : "updated";
                    System.Diagnostics.Debug.WriteLine($"The Listener for step: {pathComp} was {mg}.");

                    next = next.Parent?.Parent;
                }
                else
                {
                    if (TryGetPropBag(next, out IPropBagInternal propBag))
                    {
                        bool listenerWasAdded = AddOrUpdateListener(propBag, next.CompKey, pathComp, SourceKindEnum.Down, pathListeners, nPtr);
                        string mg = listenerWasAdded ? "added" : "updated";
                        System.Diagnostics.Debug.WriteLine($"The Listener for step: {pathComp} was {mg}.");

                        if (TryGetChildProp(next, propBag, pathComp, out StoreNodeProp child))
                        {
                            next = child.Child;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Could not get reference to the PropItem's PropStoreNode during binding update.");
                            next = null;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("The weak reference to the PropBag refers to a ProBag which 'is no longer with us.'");
                        next = null;
                    }
                }
            }

            // Add the terminal node.
            if (next != null)
            {
                System.Diagnostics.Debug.Assert(nPtr == pathElements.Length - 1, $"The counter variable: nPtr should be {pathElements.Length - 1}, but is {nPtr} instead.");

                if (TryGetPropBag(next, out IPropBagInternal propBag))
                {
                    string pathComp = pathElements[nPtr];

                    bool listenerWasAdded = AddOrUpdateListener(propBag, next.CompKey, pathComp, SourceKindEnum.TerminalNode, pathListeners, nPtr);
                    string mg = listenerWasAdded ? "added" : "updated";
                    System.Diagnostics.Debug.WriteLine($"The Listener for terminal step: {pathComp} was {mg}.");

                    // We have created or updated the listener for this step, advance the pointer.
                    nPtr++;

                    // We have subscribed to the property that is the source of the binding.
                    complete = true;

                    // Let's try to get the value of the property for which we just started listening to changes.
                    if (TryGetChildProp(next, propBag, pathComp, out StoreNodeProp child))
                    {
                        if (UpdateTargetWithStartingValue(bindingTarget, child))
                        {
                            System.Diagnostics.Debug.WriteLine($"The target has been updated during refresh. " +
                                $"Target: {((IPropBag)propBag).GetClassName()}, {pathComp}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("The binding source has been reached, but the target was not updated during refresh. " +
                                $"Target: {((IPropBag)propBag).GetClassName()}, {pathComp}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Could not get reference to the PropItem's PropStoreNode during binding update.");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("The weak reference to the PropBag refers to a ProBag which 'is no longer with us.'");
                }
            }

            for (; nPtr < pathListeners.Count; nPtr++)
            {
                ObservableSource<T> listener = pathListeners[nPtr];
                listener.Dispose();
                pathListeners.RemoveAt(nPtr);
            }

            return complete;
        }

        private bool HandleTerminalNodeUpdate(WeakReference<IPropBagInternal> bindingTarget, StoreNodeBag next, string[] pathElements, int nPtr)
        {
            System.Diagnostics.Debug.Assert(nPtr == pathElements.Length - 1, $"The counter variable: nPtr should be {pathElements.Length - 1}, but is {nPtr} instead.");

            if (TryGetPropBag(next, out IPropBagInternal propBag))
            {
                string pathComp = pathElements[nPtr];

                if (TryGetChildProp(next, propBag, pathComp, out StoreNodeProp child))
                {
                    if (UpdateTargetWithStartingValue(bindingTarget, child))
                    {
                        System.Diagnostics.Debug.WriteLine($"The target has been updated during refresh. " +
                            $"Target: {((IPropBag)propBag).GetClassName()}, {pathComp}");
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("The binding source has been reached, but the target was not updated during refresh. " +
                            $"Target: {((IPropBag)propBag).GetClassName()}, {pathComp}");
                        return false;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Could not get reference to the PropItem's PropStoreNode during binding update.");
                    return false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("The weak reference to the PropBag refers to a ProBag which 'is no longer with us.'");
                return false;
            }
        }

        private bool GetChangedNode(StoreNodeBag ourNode, ObservableSource<T> signalingNode, 
            bool pathIsAbsolute, string[] pathElements, out StoreNodeBag next, out int nPtr)
        {
            int startIndex = _pathListeners.IndexOf(signalingNode);
            if (startIndex == -1) throw new InvalidOperationException($"Could not get pointer to path element while processing DataSourceChanged event for {BINDER_NAME}.");

            if (pathIsAbsolute)
            {
                SourceKindEnum sourceKind = signalingNode.SourceKind;
                System.Diagnostics.Debug.Assert(sourceKind == SourceKindEnum.Down || sourceKind == SourceKindEnum.TerminalNode, $"When the source path is absolute, GetChangedNode can only be called for a node whose kind is {nameof(SourceKindEnum.Down)} or {nameof(SourceKindEnum.TerminalNode)}.");
                next = ourNode.Root;
                if (next == null)
                {
                    if (sourceKind == SourceKindEnum.Down)
                    {
                        System.Diagnostics.Debug.WriteLine("OurNode's root is null when processing DataSource Changed for 'intervening' node.");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("OurNode's root is null when processing Terminal node.");
                    }
                    nPtr = 0;
                    return false;
                }
            }
            else
            {
                next = ourNode;
            }

            // For terminal nodes:
            //      Initially startIndex should point to the last path element.
            //      On exit, next be the PropBag that owns the binding source propItem
            //      and nPtr should point to the last listener.

            // For intervening nodes:
            //      Initially startIndex should point to the element that was changed.
            //      On exit, next should be the PropBag that owns the propItem of the property that was changed
            //      and nPtr should point to the listener that changed.

            for (nPtr = 0; next != null && nPtr < startIndex; nPtr++)
            {
                string pathComp = pathElements[nPtr];
                if (pathComp == "..")
                {
                    next = next.Parent?.Parent;
                }
                else
                {
                    if (TryGetPropBag(next, out IPropBagInternal propBag))
                    {
                        if (TryGetChildProp(next, propBag, pathComp, out StoreNodeProp child))
                        {
                            if (nPtr + 1 == pathElements.Length - 1)
                            {
                                // next is set to the node we wan't and we can access the child.
                            }
                            else
                            {
                                // Get the PropBag being hosted by this current object's property.
                                next = child.Child;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Could not get reference to the PropItem's PropStoreNode during binding update.");
                            next = null;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("The weak reference to the PropBag refers to a ProBag which 'is no longer with us.'");
                        next = null;
                    }
                }
            }

            return next != null;
        }

        private string[] RemoveFirstItem(string[] x)
        {
            string[] newElements = new string[x.Length - 1];
            Array.Copy(x, 1, newElements, 0, x.Length - 1);
            return newElements;
        }

        private bool AddOrUpdateListener(IPropBagInternal propBag, ExKeyT compKey, string pathComp, SourceKindEnum sourceKind, OSCollection<T> pathListeners, int nPtr)
        {
            bool result;

            if (pathListeners.Count > nPtr)
            {
                ObservableSource<T> listener = pathListeners[nPtr];

                if (compKey != listener.CompKey || sourceKind != listener.SourceKind)
                {
                    listener.Dispose();
                    ObservableSource<T> newListener = CreateAndListen(propBag, compKey, pathComp, sourceKind);
                    pathListeners[nPtr] = newListener;
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                ObservableSource<T> newListener = CreateAndListen(propBag, compKey, pathComp, sourceKind);
                pathListeners.Add(newListener);
                result = true;
            }
            return result;
        }

        private ObservableSource<T> CreateAndListen(IPropBagInternal propBag, ExKeyT compKey, string pathComp, SourceKindEnum sourceKind)
        {
            ObservableSource<T> result;
            if (sourceKind == SourceKindEnum.Down)
            {
                result = new ObservableSource<T>((IPropBag)propBag, compKey, pathComp, sourceKind, BINDER_NAME);
                result.PropertyChangedWithVals += PropertyChangedWithVals_Handler;
            }
            else if (sourceKind == SourceKindEnum.TerminalNode)
            {
                result = new ObservableSource<T>((IPropBag)propBag, compKey, pathComp, BINDER_NAME);
                result.PropertyChangedWithTVals += PropertyChangedWithTVals_Handler;
            }
            else
            {
                throw new InvalidOperationException($"CreateAndListen when supplied a propBag can only process nodes of source kind = {nameof(SourceKindEnum.Down)} and {nameof(SourceKindEnum.TerminalNode)}.");
            }

            return result;
        }

        private bool AddOrUpdateListener(StoreNodeBag propStoreNode, string pathComp, SourceKindEnum sourceKind, OSCollection<T> pathListeners, int nPtr)
        {
            bool result;

            if (pathListeners.Count > nPtr)
            {
                ObservableSource<T> listener = pathListeners[nPtr];

                if (propStoreNode.CompKey != listener.CompKey || sourceKind != listener.SourceKind)
                {
                    listener.Dispose();
                    ObservableSource<T> newListener = CreateAndListen(propStoreNode, pathComp, sourceKind);
                    pathListeners[nPtr] = newListener;
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                ObservableSource<T> newListener = CreateAndListen(propStoreNode, pathComp, sourceKind);
                pathListeners.Add(newListener);
                result = true;
            }

            return result;
        }

        private ObservableSource<T> CreateAndListen(StoreNodeBag propStoreNode, string pathComp, SourceKindEnum sourceKind)
        {
            ObservableSource<T> result = new ObservableSource<T>(propStoreNode, propStoreNode.CompKey, pathComp, sourceKind, BINDER_NAME);
            result.ParentHasChanged += ParentHasChanged_Handler;
            return result;
        }

        private bool TryGetPropBag(StoreNodeBag objectNode, out IPropBagInternal propBag)
        {
            // Unwrap the weak reference held by the objectNode in it's PropBagProxy.PropBagRef.
            //bool result = objectNode.PropBagProxy.PropBagRef.TryGetTarget(out propBag);
            bool result = objectNode.TryGetPropBag(out propBag);

            return result;
        }

        private bool TryGetChildProp(StoreNodeBag objectNode, IPropBagInternal propBag, string propertyName, out StoreNodeProp child)
        {
            PropIdType propId = ((PSAccessServiceInternalType)propBag.ItsStoreAccessor).Level2KeyManager.FromRaw(propertyName);
            bool result = objectNode.TryGetChild(propId, out child);
            return result;
        }

        #endregion

        #region Value Converter Support

        private void SetDefaultConverter()
        {
            //if (DefaultConverter == null)
            //{
            //    if (DefaultConverterParameterBuilder != null)
            //    {
            //        throw new InvalidOperationException("The DefaultParameterBuilder has been given a value, but the DefaultConverter is unassigned.");
            //    }
            //    DefaultConverter = new Lazy<IValueConverter>(() => new PropValueConverter());
            //    DefaultConverterParameterBuilder = OurDefaultConverterParameterBuilder;
            //}
        }

        private IConvertValues GetConverter(WeakReference<IPropBagInternal> bindingTarget, LocalBindingInfo bInfo, Type sourceType,
            string pathElement, bool isPropBagBased, out object converterParameter)
        {

            //if (isPropBagBased)
            //{
            //    if (bInfo.Converter != null)
            //    {
            //        // Use the one specified in the MarkUp
            //        converterParameter = bInfo.ConverterParameterBuilder(bindingTarget, bInfo, sourceType, pathElement);
            //        return bInfo.Converter;
            //    }
            //    else
            //    {
            //        // Use the default for ItemsSource
            //        if(bindingTarget.DependencyProperty.Name == "SelectedItem")
            //        {
            //            // If this is being used to supply a SelectedItem property.
            //            converterParameter = null;
            //            return new SelectedItemConverter();
            //        }
            //        else
            //        {
            //            // Use the default converter provided by the caller or if no default provide our standard PropValueConverter.
            //            converterParameter = DefaultConverterParameterBuilder(bindingTarget, bInfo, sourceType, pathElement);
            //            return DefaultConverter.Value;
            //        }
            //    }
            //}
            //else
            //{
            //    // If no converter specified and the target is SelectedItem,
            //    // use the SelectedItemConverter.
            //    if (bInfo.Converter == null && bindingTarget.DependencyProperty?.Name == "SelectedItem")
            //    {
            //        converterParameter = null;
            //        return new SelectedItemConverter();
            //    }
            //    else
            //    {
            //        // TODO: Check this. I beleive that the Microsoft Binder supplies a default converter.

            //        // If no converter specified in the markup, no converter will be used.
            //        converterParameter = bInfo.ConverterParameterBuilder(bindingTarget, bInfo, sourceType, pathElement);
            //        return bInfo.Converter;
            //    }
            //}
            converterParameter = null;
            return null;
        }

        private object OurDefaultConverterParameterBuilder(WeakReference<IPropBagInternal> bindingTarget, LocalBindingInfo bInfo, Type sourceType,
            string pathElement)
        {
            //return new TwoTypes(sourceType, bindingTarget.PropertyType);
            return null;
        }

        #endregion

        #region PropertyChanged and ParentHasChanged Handlers

        private void PropertyChangedWithVals_Handler(object sender, PcGenEventArgs e)
        {
            DataSourceHasChanged_Handler((ObservableSource<T>)sender, DataSourceChangeTypeEnum.PropertyChanged);
        }

        private void ParentHasChanged_Handler(object sender, EventArgs e)
        {
            DataSourceHasChanged_Handler((ObservableSource<T>)sender, DataSourceChangeTypeEnum.ParentHasChanged);
        }

        private void DataSourceHasChanged_Handler(ObservableSource<T> signalingNode, DataSourceChangeTypeEnum changeType)
        {
            StoreNodeBag next;

            switch (signalingNode.SourceKind)
            {
                case SourceKindEnum.AbsRoot:
                    {
                        System.Diagnostics.Debug.Assert(changeType == DataSourceChangeTypeEnum.ParentHasChanged,
                            $"Received a DataSourceChanged event from on a node of kind = AbsRoot. " +
                            $"The DataSourceChangeType should be {nameof(DataSourceChangeTypeEnum.ParentHasChanged)}, but is {changeType}.");

                        System.Diagnostics.Debug.Assert(ReferenceEquals(signalingNode, _rootListener),
                            "Received a ParentHasChanged on a node of kind = AbsRoot and the signaling node is not the _rootListener.");

                        System.Diagnostics.Debug.Assert(_isPathAbsolute,
                            "Received a ParentHasChanged on a node of kind = AbsRoot, but our 'PathIsAbsolute' property is set to false.");

                        // We have a new root, start at the beginning.
                        next = _ourNode.Root;
                        if (next == null)
                        {
                            System.Diagnostics.Debug.WriteLine("OurNode's root is null when processing update to AbsRoot.");
                            return;
                        }

                        int nPtr = 0;
                        HandleNodeUpdate(_targetObject, next, _pathElements, _pathListeners, nPtr);
                        break;
                    }
                case SourceKindEnum.Up:
                    {
                        System.Diagnostics.Debug.Assert(changeType == DataSourceChangeTypeEnum.ParentHasChanged, $"DataSourceHasChanged is processing a observable source of kind = {signalingNode.SourceKind}, but the ChangeType does not equal {nameof(DataSourceChangeTypeEnum.ParentHasChanged)}.");

                        if (GetChangedNode(_ourNode, signalingNode, _isPathAbsolute, _pathElements, out next, out int nPtr))
                        {
                            System.Diagnostics.Debug.Assert(nPtr < _pathElements.Length - 1, "GetChangedNode for 'up' PropertyChanged event returned with nPtr beyond next to last node.");
                            HandleNodeUpdate(_targetObject, next, _pathElements, _pathListeners, nPtr);
                        }

                        break;
                    }
                case SourceKindEnum.Down:
                    {
                        System.Diagnostics.Debug.Assert(changeType == DataSourceChangeTypeEnum.PropertyChanged, $"DataSourceHasChanged is processing a observable source of kind = {signalingNode.SourceKind}, but the ChangeType does not equal {nameof(DataSourceChangeTypeEnum.PropertyChanged)}.");

                        if (GetChangedNode(_ourNode, signalingNode, _isPathAbsolute, _pathElements, out next, out int nPtr))
                        {
                            System.Diagnostics.Debug.Assert(nPtr < _pathElements.Length - 1, "GetChangedNode for 'down' PropertyChanged event returned with nPtr beyond next to last node.");
                            HandleNodeUpdate(_targetObject, next, _pathElements, _pathListeners, nPtr);
                        }

                        break;
                    }
                case SourceKindEnum.TerminalNode:
                    {
                        System.Diagnostics.Debug.Assert(Complete, "The Complete flag should be set when responding to Terminal node updates.");
                        System.Diagnostics.Debug.WriteLine("Beginning to proccess property changed event raised from the Terminal node of the source path.");

                        if (GetChangedNode(_ourNode, signalingNode, _isPathAbsolute, _pathElements, out next, out int nPtr))
                        {
                            HandleTerminalNodeUpdate(_targetObject, next, _pathElements, nPtr);
                        }

                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException($"The SourceKind: {signalingNode.SourceKind} is not recognized or is not supported.");
                    }
            }
        }

        private void PropertyChangedWithTVals_Handler(object sender, PcTypedEventArgs<T> e)
        {
            System.Diagnostics.Debug.Assert(e.PropertyName == this.PropertyName, "PropertyName from PCTypedEventArgs does not match the PropertyName registered with the Binding.");
            System.Diagnostics.Debug.WriteLine("The terminal node's property value has been updated.");

            if (NoTarget_NotifiesReceiverInstead)
            {
                CheckSenderAndOriginalSender(sender);
                WeakReference<IPropBag> propItemParent = (sender as ObservableSource<T>).LastEventSender as WeakReference<IPropBag>;
                NotifyReceiver(_storeNodeUpdateReceiver, propItemParent);
            }
            else
            {
                UpdateTarget(_targetObject, e.NewValue);
            }
        }
        
        #endregion

        #region Update Target

        private bool UpdateTargetWithStartingValue(WeakReference<IPropBagInternal> bindingTarget, StoreNodeProp sourcePropNode)
        {
            bool result;

            if (NoTarget_NotifiesReceiverInstead)
            {
                //WeakReference<IPropBagInternal> propItemParentPropBag_internal = sourcePropNode?.Parent?.PropBagProxy;
                //WeakReference<IPropBag> propItemParentPropBag = GetPublicVersion(propItemParentPropBag_internal);

                WeakReference<IPropBag> propItemParentPropBag_wr = GetPropItemParent(_propStoreAccessService_wr, sourcePropNode);
                result = NotifyReceiver(_storeNodeUpdateReceiver, propItemParentPropBag_wr);
            }
            else
            {
                IProp typedProp = sourcePropNode.Int_PropData.TypedProp;

                if (typedProp.StorageStrategy == PropStorageStrategyEnum.Internal)
                {
                    T newValue = (T)typedProp.TypedValueAsObject;
                    result = UpdateTarget(_targetObject, newValue);
                }
                else
                {
                    // This property has no backing store, there is no concept of a starting value. (Propably used to send messages.)
                    result = false;
                }
            }

            return result;
        }

        private bool UpdateTarget(WeakReference<IPropBagInternal> bindingTarget, T newValue)
        {
            if (bindingTarget.TryGetTarget(out IPropBagInternal propBag))
            {
                string status;
                if(_targetHasStore == PropStorageStrategyEnum.Internal)
                {
                    bool wasSet = ((IPropBag)propBag).SetIt(newValue, this.PropertyName);
                    status = wasSet ? "has been updated" : "already had the new value";
                }
                else
                {
                    T dummy = default(T);
                    bool wasSet = ((IPropBag)propBag).SetIt(newValue, ref dummy, this.PropertyName);
                    status = wasSet ? "has been updated" : "already had the new value";
                }

                System.Diagnostics.Debug.WriteLine($"The binding target {status}.");
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Target IPropBag was found to be 'not with us' on call to Update Target.");
                return false;
            }
        }

        private bool NotifyReceiver(IReceivePropStoreNodeUpdates propStoreUpdateReceiver, WeakReference<IPropBag> propItemParent, PcTypedEventArgs<T> e)
        {
            propStoreUpdateReceiver.OnPropStoreNodeUpdated(propItemParent, e.NewValueIsUndefined, e.NewValue);
            return true;
        }

        private bool NotifyReceiver(IReceivePropStoreNodeUpdates propStoreUpdateReceiver, WeakReference<IPropBag> propItemParent)
        {
            propStoreUpdateReceiver.OnPropStoreNodeUpdated(propItemParent, true, null);
            return true;
        }

        #endregion

        #region Support Methods

        private WeakReference<IPropBag> GetPropItemParent(WeakReference<PSAccessServiceType> propStoreAccessService_wr, StoreNodeProp sourcePropNode)
        {
            if (propStoreAccessService_wr.TryGetTarget(out PSAccessServiceType storeAccessor))
            {
                if (storeAccessor is PSAccessServiceInternalType storeAcessor_internal)
                {
                    WeakReference<IPropBagInternal> propItemParentPropBag_internal_wr = storeAcessor_internal.GetPropBagProxy(sourcePropNode);
                    WeakReference<IPropBag> propItemParentPropBag_wr = storeAcessor_internal.GetPublicInterface(propItemParentPropBag_internal_wr);
                    return propItemParentPropBag_wr;
                }
                else
                {
                    return new WeakReference<IPropBag>(null);
                }
            }
            else
            {
                return new WeakReference<IPropBag>(null);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckSenderAndOriginalSender(object sender)
        {
            if ((sender is ObservableSource<T> listener))
            {
                if (!(listener.LastEventSender is WeakReference<IPropBag>))
                {
                    throw new InvalidOperationException("The PropertyChangedWithTVals event raised from an instance of Observable<T> has a value of original sender that is not of type: WeakReference<IPropBag>.");
                }
            }
            else
            {
                throw new InvalidOperationException("The PropertyChangedWithTVals event raised from an instance of Observable<T> is supplying a sender object that has a type different from ObservableSource<T>.");
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
                    // TODO: dispose managed state (managed objects).
                    if(_rootListener != null) _rootListener.Dispose();
                    _pathListeners.Clear();
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

        #region OLD_CODE

        //private WeakReference<IPropBag> GetPublicVersion(WeakReference<IPropBagInternal> x)
        //{
        //    WeakReference<IPropBag> result;

        //    if (x.TryGetTarget(out IPropBagInternal propBagInternal))
        //    {
        //        System.Diagnostics.Debug.Assert(propBagInternal == null || propBagInternal is IPropBag, "This instance of IPropBagInternal does not also implement: IPropBag.");
        //        result = new WeakReference<IPropBag>(propBagInternal as IPropBag);
        //    }
        //    else
        //    {
        //        result = new WeakReference<IPropBag>(null);
        //    }

        //    return result;
        //}

        //private StoreNodeBag GetBagNode(WeakReference<PSAccessServiceType> propStoreAccessService_wr)
        //{
        //    if (propStoreAccessService_wr.TryGetTarget(out PSAccessServiceType accessService))
        //    {
        //        if (accessService is IHaveTheStoreNode storeNodeHolder)
        //        {
        //            StoreNodeBag result = storeNodeHolder.PropStoreNode;
        //            return result;
        //        }
        //    }

        //    return null;
        //}

        #endregion
    }
}
