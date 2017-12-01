using System;
using DRM.TypeSafePropertyBag.LocalBinding.Engine;

namespace DRM.TypeSafePropertyBag.LocalBinding
{
    #region Type Aliases 
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using L2KeyManType = IL2KeyMan<UInt32, String>;
    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;

    #endregion

    public class LocalBinder<T> : IDisposable
    {
        const int ROOT_INDEX = 0;
        const string BINDER_NAME = "PB_LocalBinder";

        #region Private Properties

        StoreNodeBag _ourNode { get; }

        WeakReference<IPropBagInternal> _targetObject;
        PropIdType _propId;

        ObservableSource<T> _rootListener;

        OSCollection<T> _listeners { get; set; }

        string[] _pathElements { get; set; }

        #endregion

        #region Public Properties

        public ExKeyT BindingTarget { get; }

        public LocalBindingInfo BindingInfo { get; private set; }

        public bool PathIsAbsolute { get; }
        public bool Complete { get; private set; }

        public PropNameType PropertyName { get; }

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

        public LocalBinder(PSAccessServiceType propStoreAccessService, PropIdType propId, LocalBindingInfo bindingInfo)
        {
            _propId = propId;
            BindingInfo = bindingInfo;

            // Get the PropStore (Tree) Node for the IPropBag object hosting the property that is the target of the binding.
            _ourNode = GetPropStore(propStoreAccessService);

            _targetObject = _ourNode.PropBagProxy.PropBagRef;
            if (propStoreAccessService.Level2KeyManager.TryGetFromCooked(propId, out string propertyName))
            {
                PropertyName = propertyName;
            }
            else
            {
                throw new InvalidOperationException("Cannot retrieve the Target's property name from the TargetPropId.");
            }

            _pathElements = GetPathElements(BindingInfo, out bool pathIsAbsolute);
            PathIsAbsolute = pathIsAbsolute;

            _listeners = StartBinding(_targetObject, BindingInfo, _pathElements, pathIsAbsolute, out bool complete);
            Complete = complete;
        }

        private StoreNodeBag GetPropStore(PSAccessServiceType propStoreAccessService)
        {
            if (propStoreAccessService is IHaveTheStoreNode storeKeyProvider)
            {
                StoreNodeBag propStoreNode = storeKeyProvider.PropStoreNode;
                return propStoreNode;
            }
            else
            {
                throw new InvalidOperationException($"The {nameof(propStoreAccessService)} does not implement the {nameof(IHaveTheStoreNode)} interface.");
            }
        }

        #endregion

        #region Path Processing

        private string[] GetPathElements(LocalBindingInfo bInfo, out bool pathIsAbsolute)
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
                CheckForBadParRefs(1, pathElements);

                // TODO: Listen to changes in the value of our node's root.
            }
            else
            {
                pathIsAbsolute = false;

                int nPtr = GetFirstPathElementWithName(0, pathElements);
                CheckForBadParRefs(nPtr + 1, pathElements);
            }

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

        private OSCollection<T> StartBinding(WeakReference<IPropBagInternal> bindingTarget, LocalBindingInfo bInfo,
            string[] pathElements, bool pathIsAbsolute, out bool complete)
        {
            complete = false;

            OSCollection<T> result = new OSCollection<T>();
            StoreNodeBag next;

            if(pathIsAbsolute)
            {
                // Find our current root.
                next = _ourNode.Root;
                if (next == null)
                {
                    System.Diagnostics.Debug.WriteLine("OurNode's root is null when starting the local binding.");
                    return result;
                }
            }
            else
            {
                next = _ourNode;
            }

            int nPtr = 0;
            complete = HandleNodeUpdate(bindingTarget, next, pathElements, nPtr);

            return result;
        }

        private bool HandleNodeUpdate(WeakReference<IPropBagInternal> bindingTarget, StoreNodeBag next, string[] pathElements, int nPtr)
        {
            bool complete = false;

            // Process each step, except for the last.
            for (; next != null && nPtr < pathElements.Length - 1; nPtr++)
            {
                string pathComp = pathElements[nPtr];
                if (pathComp == "..")
                {
                    ObservableSource<T> newListener = CreateAndListen(next, pathComp, SourceKindEnum.Up);

                    if (_listeners.Count > nPtr)
                    {
                        ObservableSource<T> listener = _listeners[nPtr];
                        listener.Dispose();
                        _listeners[nPtr] = newListener;
                    }
                    else
                    {
                        _listeners.Add(newListener);
                    }

                    next = next.Parent?.Parent;
                }
                else
                {
                    if (GetPropBag(next, out IPropBagInternal propBag))
                    {
                        ObservableSource<T> newListener = CreateAndListen(propBag, next.CompKey, pathComp, SourceKindEnum.Down);

                        if (_listeners.Count > nPtr)
                        {
                            ObservableSource<T> listener = _listeners[nPtr];
                            listener.Dispose();
                            _listeners[nPtr] = newListener;
                        }
                        else
                        {
                            _listeners.Add(newListener);
                        }

                        if (GetChildProp(next, propBag, pathComp, out StoreNodeProp child))
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

                if (GetPropBag(next, out IPropBagInternal propBag))
                {
                    string pathComp = pathElements[nPtr];

                    ObservableSource<T> newListener = CreateAndListen(propBag, next.CompKey, pathComp, SourceKindEnum.TerminalNode);

                    if (_listeners.Count > nPtr)
                    {
                        ObservableSource<T> listener = _listeners[nPtr];
                        listener.Dispose();
                        _listeners[nPtr] = newListener;
                    }
                    else
                    {
                        _listeners.Add(newListener);
                    }

                    // We have created or updated the listener for this step, advance the pointer.
                    nPtr++;

                    // We have subscribed to the property that is the source of the binding.
                    complete = true;

                    // Let's try to get the value of the property for which we just started listening to changes.
                    if (GetChildProp(next, propBag, pathComp, out StoreNodeProp child))
                    {
                        if (UpdateTarget(bindingTarget, child))
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

            for (; nPtr < _listeners.Count; nPtr++)
            {
                ObservableSource<T> listener = _listeners[nPtr];
                listener.Dispose();
                _listeners.RemoveAt(nPtr);
            }

            return complete;
        }

        private bool HandleTerminalNodeUpdate(WeakReference<IPropBagInternal> bindingTarget, StoreNodeBag next, string[] pathElements, int nPtr)
        {
            System.Diagnostics.Debug.Assert(nPtr == pathElements.Length - 1, $"The counter variable: nPtr should be {pathElements.Length - 1}, but is {nPtr} instead.");

            if (GetPropBag(next, out IPropBagInternal propBag))
            {
                string pathComp = pathElements[nPtr];

                if (GetChildProp(next, propBag, pathComp, out StoreNodeProp child))
                {
                    System.Diagnostics.Debug.WriteLine("The binding source has been reached during binding update.");
                    bool result = UpdateTarget(bindingTarget, child);
                    return result;
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

        private bool GetChangedNode(StoreNodeBag ourNode, SourceKindEnum sourceKind, bool pathIsAbsolute, string[] pathElements, int startIndex, out StoreNodeBag next)
        {
            if (pathIsAbsolute)
            {
                System.Diagnostics.Debug.Assert(sourceKind == SourceKindEnum.Down || sourceKind == SourceKindEnum.TerminalNode, $"When the source path is absolute, GetChangedNode can only be called for a node whose kind is {nameof(SourceKindEnum.Down)} or {nameof(SourceKindEnum.TerminalNode)}.");
                next = ourNode.Root;
                if (next == null)
                {
                    if(sourceKind == SourceKindEnum.Down)
                    {
                        System.Diagnostics.Debug.WriteLine("OurNode's root is null when processing DataSource Changed for 'intervening' node.");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("OurNode's root is null when processing Terminal node.");
                    }
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

            for (int nPtr = 0; next != null && nPtr < startIndex; nPtr++)
            {
                string pathComp = pathElements[nPtr];
                if (pathComp == "..")
                {
                    next = next.Parent?.Parent;
                }
                else
                {
                    if (GetPropBag(next, out IPropBagInternal propBag))
                    {
                        if (GetChildProp(next, propBag, pathComp, out StoreNodeProp child))
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

            return next != null;
        }

        private string[] RemoveFirstItem(string[] x)
        {
            string[] newElements = new string[x.Length - 1];
            Array.Copy(x, 1, newElements, 0, x.Length - 1);
            return newElements;
        }

        private ObservableSource<T> CreateAndListen(IPropBagInternal propBag, ExKeyT compKey, string pathComp, SourceKindEnum sourceKind)
        {
            ObservableSource<T> result;
            if (sourceKind == SourceKindEnum.Down)
            {
                result = new ObservableSource<T>((IPropBag)propBag, compKey, pathComp, sourceKind, BINDER_NAME);
                result.DataSourceChanged += DataSourceHasChanged_Handler;
            }
            else if(sourceKind == SourceKindEnum.TerminalNode)
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

        private ObservableSource<T> CreateAndListen(StoreNodeBag propStoreNode, string pathComp, SourceKindEnum sourceKind)
        {
            ObjectIdType objectId = propStoreNode.ObjectId;
            ObservableSource<T> result = new ObservableSource<T>(propStoreNode, propStoreNode.CompKey, pathComp, sourceKind, BINDER_NAME);
            result.DataSourceChanged += DataSourceHasChanged_Handler;
            return result;
        }

        private bool GetPropBag(StoreNodeBag objectNode, out IPropBagInternal propBag)
        {
            // Unwrap the weak reference held by the objectNode in it's PropBagProxy.PropBagRef.
            bool result = objectNode.PropBagProxy.PropBagRef.TryGetTarget(out propBag);
            return result;
        }

        private bool GetChildProp(StoreNodeBag objectNode, IPropBagInternal propBag, string propertyName, out StoreNodeProp child)
        {
            PropIdType propId = propBag.Level2KeyManager.FromRaw(propertyName);
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

        private IConvertValues GetConverter(IPropBagProxy bindingTarget, LocalBindingInfo bInfo, Type sourceType,
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

        private object OurDefaultConverterParameterBuilder(IPropBagProxy bindingTarget, LocalBindingInfo bInfo, Type sourceType,
            string pathElement)
        {
            //return new TwoTypes(sourceType, bindingTarget.PropertyType);
            return null;
        }

        #endregion

        #region Data Source Change Handling

        private void DataSourceHasChanged_Handler(object sender, DataSourceChangedEventArgs e)
        {
            // Check for unsupported Change Type.
            if (!(e.ChangeType == DataSourceChangeTypeEnum.ParentHasChanged || e.ChangeType == DataSourceChangeTypeEnum.PropertyChanged))
            {
                throw new ApplicationException($"The DataSourceChangedType: {e.ChangeType} is not recognized.");
            }

            ObservableSource<T> signalingNode = (ObservableSource<T>)sender;
            int nodeIndex = _listeners.IndexOf(signalingNode);
            if (nodeIndex == -1) throw new InvalidOperationException($"Could not get pointer to path element while processing DataSourceChanged event for {BINDER_NAME}.");

            StoreNodeBag next;

            switch (signalingNode.SourceKind)
            {
                case SourceKindEnum.AbsRoot:
                    {
                        System.Diagnostics.Debug.Assert(e.ChangeType == DataSourceChangeTypeEnum.ParentHasChanged,
                            $"Received a DataSourceChanged event from on a node of kind = AbsRoot. " +
                            $"The DataSourceChangeType should be {nameof(DataSourceChangeTypeEnum.ParentHasChanged)}, but is {e.ChangeType}.");

                        System.Diagnostics.Debug.Assert(nodeIndex == 0,
                            "Received a ParentHasChanged on a node of kind = AbsRoot and the node index is not zero.");

                        System.Diagnostics.Debug.Assert(this.PathIsAbsolute,
                            "Received a ParentHasChanged on a node of kind = AbsRoot, but our 'PathIsAbsolute' property is set to false.");

                        // We have a new root, start at the beginning.
                        next = _ourNode.Root;
                        if (next == null)
                        {
                            System.Diagnostics.Debug.WriteLine("OurNode's root is null when processing update to AbsRoot.");
                            return;
                        }



                        int nPtr = 0;
                        HandleNodeUpdate(_targetObject, next, _pathElements, nPtr);
                        break;
                    }
                case SourceKindEnum.Up:
                    {
                        goto case SourceKindEnum.Down;
                    }
                case SourceKindEnum.Down:
                    {
                        int nPtr = nodeIndex;
                        if(GetChangedNode(_ourNode, signalingNode.SourceKind, PathIsAbsolute, _pathElements, nPtr, out next))
                        {
                            HandleNodeUpdate(_targetObject, next, _pathElements, nPtr);
                        }

                        break;
                    }
                case SourceKindEnum.TerminalNode:
                    {
                        System.Diagnostics.Debug.Assert(Complete, "The Complete flag should be set when responding to Terminal node updates.");
                        System.Diagnostics.Debug.Assert(nodeIndex == _pathElements.Length - 1, "The nodeIndex should point to the last path element when processing Terminal node updates.");

                        System.Diagnostics.Debug.WriteLine("Beginning to proccess property changed event raised from the Terminal node of the source path.");

                        int nPtr = nodeIndex;
                        if (GetChangedNode(_ourNode, signalingNode.SourceKind, PathIsAbsolute, _pathElements, nPtr, out next))
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

        private void PropertyChangedWithTVals_Handler(object sender, PCTypedEventArgs<T> e)
        {
            System.Diagnostics.Debug.Assert(e.PropertyName == this.PropertyName, "PropertyName from PCTypedEventArgs does not match the PropertyName registered with the Binding.");
            System.Diagnostics.Debug.WriteLine("The terminal node's property value has been updated.");

            UpdateTarget(_targetObject, e.NewValue);
        }

        #endregion

        #region Update Target

        private bool UpdateTarget(WeakReference<IPropBagInternal> bindingTarget, StoreNodeProp sourcePropNode)
        {
            bool result;
            if (sourcePropNode.Parent.PropBagProxy.PropBagRef.TryGetTarget(out IPropBagInternal propBag))
            {
                if (propBag.Level2KeyManager.TryGetFromRaw(PropertyName, out PropIdType propId))
                {
                    StoreNodeBag propBagNode = sourcePropNode.Parent;

                    if (propBagNode.TryGetChild(propId, out StoreNodeProp child))
                    {
                        IPropDataInternal propData = child.Int_PropData;
                        T newValue = (T)propData.TypedProp.TypedValueAsObject;

                        result = UpdateTarget(bindingTarget, newValue);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"PropBagNode.TryGetChild failed to access the child node with property name:{PropertyName}.");
                        result = false;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Update Target could not find that property by name: {PropertyName}.");
                    result = false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Our weak reference to the binding target holds a reference to an object 'no longer with us.'");
                result = false;
            }
            return result;
        }

        private bool UpdateTarget(WeakReference<IPropBagInternal> bindingTarget, T newValue)
        {
            if(bindingTarget.TryGetTarget(out IPropBagInternal propBag))
            {
                bool wasSet = ((IPropBag)propBag).SetIt(newValue, this.PropertyName);
                string status = wasSet ? "has been updated" : "could not be updated";
                System.Diagnostics.Debug.WriteLine($"The binding target {status}.");
                return wasSet;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Target IPropBag was found to be 'not with us' on call to Update Target.");
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
                    // TODO: dispose managed state (managed objects).
                    _listeners.Clear();
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

        private void RefreshPathListeners(WeakReference<IPropBagInternal> bindingTarget, LocalBindingInfo bInfo,
            string[] pathElements, bool pathIsAbsolute,
            OSCollection<T> pathListeners, ObservableSource<T> signalingOs, DataSourceChangedEventArgs changeInfo)
        {
            //    Complete = false;
            //    int nodeIndex = _listeners.IndexOf(signalingOs);
            //    if (nodeIndex == -1) throw new InvalidOperationException($"Could not get pointer to path element while processing DataSourceChanged event for {BINDER_NAME}.");

            //    StoreNodeBag next;

            //    if (signalingOs.SourceKind == SourceKindEnum.AbsRoot)
            //    {
            //        System.Diagnostics.Debug.Assert(changeInfo.ChangeType == DataSourceChangeTypeEnum.ParentHasChanged,
            //            $"Received a DataSourceChanged event from on a node of kind = AbsRoot. " +
            //            $"The DataSourceChangeType should be {nameof(DataSourceChangeTypeEnum.ParentHasChanged)}, but is {changeInfo.ChangeType}.");

            //        System.Diagnostics.Debug.Assert(nodeIndex == 0,
            //            "Received a ParentHasChanged on a node of kind = AbsRoot and the node index is not zero.");

            //        System.Diagnostics.Debug.Assert(pathIsAbsolute,
            //            "Received a ParentHasChanged on a node of kind = AbsRoot, but our 'PathIsAbsolute' property is set to false.");

            //        // We have a new root, start at the beginning.
            //        next = GetRootPropBag(_ourNode);
            //    }
            //    else if(signalingOs.SourceKind == SourceKindEnum.TerminalNode)
            //    {
            //        System.Diagnostics.Debug.WriteLine("The source data value of the binding has been updated.");

            //        if (pathIsAbsolute)
            //        {
            //            next = GetRootPropBag(_ourNode);
            //        }
            //        else
            //        {
            //            next = _ourNode;
            //        }

            //        next = GetChangedNode(next, pathElements, nodeIndex - 1);

            //        if(next != null)
            //        {
            //            if (GetPropBag(next, out IPropBagInternal propBag))
            //            {
            //                string pathComp = pathElements[pathElements.Length - 1];

            //                if (GetChildProp(next, propBag, pathComp, out StoreNodeProp child))
            //                {
            //                    System.Diagnostics.Debug.WriteLine("The binding source has been reached during binding update.");
            //                    if (UpdateTarget(bindingTarget, child))
            //                    {
            //                        System.Diagnostics.Debug.WriteLine("The target has been updated.");
            //                    }
            //                }
            //                else
            //                {
            //                    System.Diagnostics.Debug.WriteLine("Could not get reference to the PropItem's PropStoreNode during binding update.");
            //                }
            //            }
            //            else
            //            {
            //                System.Diagnostics.Debug.WriteLine("Could not get reference to ChildPropBag during binding update.");
            //            }
            //        }
            //        else
            //        {
            //            System.Diagnostics.Debug.WriteLine("Get ChangedNode returned null when processing Terminal Node.");
            //        }

            //        return;
            //    }
            //    else
            //    {
            //        if (pathIsAbsolute)
            //        {
            //            next = GetRootPropBag(_ourNode);
            //        }
            //        else
            //        {
            //            next = _ourNode;
            //        }

            //        next = GetChangedNode(next, pathElements, nodeIndex - 1);
            //    }

            //    int nPtr = nodeIndex + 1;

            //    // Process each step, except for the last.
            //    for (; next != null && nPtr < pathElements.Length - 1; nPtr++)
            //    {
            //        string pathComp = pathElements[nPtr];
            //        if (pathComp == "..")
            //        {
            //            ObservableSource<T> newListener = CreateAndListen(next, pathComp, SourceKindEnum.Up);

            //            if (_listeners.Count > nPtr)
            //            {
            //                ObservableSource<T> listener = _listeners[nPtr];
            //                listener.Dispose();
            //                _listeners[nPtr] = newListener;
            //            }
            //            else
            //            {
            //                _listeners.Add(newListener);
            //            }

            //            next = next.Parent?.Parent;
            //        }
            //        else
            //        {
            //            if (GetPropBag(next, out IPropBagInternal propBag))
            //            {
            //                ObservableSource<T> newListener = CreateAndListen(propBag, next.CompKey, pathComp, SourceKindEnum.Down);

            //                if (_listeners.Count > nPtr)
            //                {
            //                    ObservableSource<T> listener = _listeners[nPtr];
            //                    listener.Dispose();
            //                    _listeners[nPtr] = newListener;
            //                }
            //                else
            //                {
            //                    _listeners.Add(newListener);
            //                }

            //                if (GetChildProp(next, propBag, pathComp, out StoreNodeProp child))
            //                {
            //                    next = child.Child;
            //                }
            //                else
            //                {
            //                    // Can't get PropBag that is a guest of the child PropItem.
            //                    next = null;
            //                }
            //            }
            //            else
            //            {
            //                // TODO: Check this -- can't we get the PropBag from the previous path step.
            //                // Can't get the PropBag for the child.
            //                next = null;
            //            }
            //        }
            //    }

            //    // Add the terminal node.
            //    if (next != null)
            //    {
            //        System.Diagnostics.Debug.Assert(nPtr == pathElements.Length - 1, $"The counter variable: nPtr should be {pathElements.Length - 1}, but is {nPtr} instead.");

            //        if(GetPropBag(next, out IPropBagInternal propBag))
            //        {
            //            string pathComp = pathElements[nPtr];

            //            ObservableSource<T> newListener = CreateAndListen(propBag, next.CompKey, pathComp, SourceKindEnum.TerminalNode);

            //            if (_listeners.Count > nPtr)
            //            {
            //                ObservableSource<T> listener = _listeners[nPtr];
            //                listener.Dispose();
            //                _listeners[nPtr] = newListener;
            //            }
            //            else
            //            {
            //                _listeners.Add(newListener);
            //            }

            //            // We have created or updated the listener for this step, advance the pointer.
            //            nPtr++;

            //            // We have subscribed to the property that is the source of the binding.
            //            Complete = true;

            //            if (GetChildProp(next, propBag, pathComp, out StoreNodeProp child))
            //            {
            //                if (UpdateTarget(bindingTarget, child))
            //                {
            //                    System.Diagnostics.Debug.WriteLine($"The target has been updated during refresh. " +
            //                        $"Target: {((IPropBag)propBag).GetClassName()}, {pathComp}");
            //                }
            //                else
            //                {
            //                    System.Diagnostics.Debug.WriteLine("The binding source has been reached, but the target was not updated during refresh. " +
            //                        $"Target: {((IPropBag)propBag).GetClassName()}, {pathComp}");
            //                }
            //            }
            //        }
            //    }

            //    for(; nPtr < _listeners.Count; nPtr ++)
            //    {
            //        ObservableSource<T> listener = _listeners[nPtr];
            //        listener.Dispose();
            //        _listeners.RemoveAt(nPtr);
            //    }
        }

            #region Old StartBinding Code
            // Old StartBinding Code
            //// As each step is processed, next is set to the Object Node that is the "home" for the next path (either a ".." or a property name.)
            //// If all objects have not been instantiated, processing may stop before the source property is reached.

            //// Process each step, except for the last.
            //for (int nPtr = 0; next != null && nPtr < pathElements.Length - 1; nPtr++)
            //{
            //    string pathComp = pathElements[nPtr];
            //    if (pathComp == "..")
            //    {
            //        if(nPtr > 0)
            //        { 
            //            SourceKindEnum parentSourceKind = result[nPtr - 1].SourceKind;

            //            if(parentSourceKind != SourceKindEnum.Up)
            //            {
            //                throw new InvalidOperationException("A path cannot have '..' components that folllow a component that indentifies a property by name.");
            //            }
            //        }

            //        result.Add(CreateAndListen(next, pathComp, SourceKindEnum.Up));
            //        //next = GetHostingPropBag(next);
            //        next = next.Parent?.Parent;
            //    }
            //    else
            //    {
            //        if (GetPropBag(next, out IPropBagInternal propBag))
            //        {
            //            result.Add(CreateAndListen(propBag, next.CompKey, pathComp, SourceKindEnum.Down));

            //            if (GetChildProp(next, propBag, pathComp, out StoreNodeProp child))
            //            {
            //                next = child.Child;
            //            }
            //            else
            //            {
            //                // Can't get PropBag that is a guest of the child PropItem.
            //                next = null;
            //            }
            //        }
            //        else
            //        {
            //            // TODO: Check this -- can't we get the PropBag from the previous path step.
            //            // Can't get the PropBag for the child.
            //            next = null;
            //        }
            //    }
            //}

            //// Add the terminal node.
            //if(next != null)
            //{
            //    string pathComp = pathElements[pathElements.Length - 1];

            //    if (GetPropBag(next, out IPropBagInternal propBag))
            //    {
            //        result.Add(CreateAndListen(propBag, next.CompKey, pathComp, SourceKindEnum.TerminalNode));
            //        // We have subscribed to the property that is the source of the binding.
            //        complete = true;

            //        if (GetChildProp(next, propBag, pathComp, out StoreNodeProp child))
            //        {
            //            if (UpdateTarget(bindingTarget, child))
            //            {
            //                System.Diagnostics.Debug.WriteLine($"The target has been updated on construction. " +
            //                    $"Target: {((IPropBag)propBag).GetClassName()}, {pathComp}");
            //            }
            //            else
            //            {
            //                System.Diagnostics.Debug.WriteLine("The binding source has been reached, but the target was not updated on construction. " +
            //                    $"Target: {((IPropBag)propBag).GetClassName()}, {pathComp}");
            //            }
            //        }
            //    }
            //}

            //return result;

            #endregion
        }
}
