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

    using IHaveTheKeyIT = IHaveTheKey<UInt64, UInt64, UInt32>;

    #endregion

    public class LocalBinder<T> 
    {
        const int ROOT_INDEX = 0;
        const string BINDER_NAME = "PB_LocalBinder";

        #region Private Properties

        PropStoreNode _ourNode { get; }

        WeakReference<IPropBagInternal> _targetObject;
        PropIdType _propId;

        OSCollection<T> _listeners { get; set; }

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

        public LocalBinder(PSAccessServiceType propStoreAccessService, CompositeKeyType cKey, PropIdType propId, LocalBindingInfo bindingInfo)
        {
            _propId = propId;
            BindingInfo = bindingInfo;

            // Get the PropStore (Tree) Node for the IPropBag object hosting the property that is the target of the binding.
            _ourNode = GetPropStore(propStoreAccessService);

            _targetObject = _ourNode.PropBagProxy.PropBagRef;

            if (_ourNode.PropBagProxy.Level2KeyManager.TryGetFromCooked(propId, out string propertyName))
            {
                PropertyName = propertyName;
            }
            else
            {
                throw new InvalidOperationException("Cannot retrieve the Target's property name from the TargetPropId.");
            }


            _listeners = StartBinding(BindingInfo, out bool pathIsAbsolute, out bool complete);
            PathIsAbsolute = pathIsAbsolute;
            Complete = complete;

            if (complete)
            {
                System.Diagnostics.Debug.WriteLine("The binding source has been reached on construction.");
            }
        }

        private PropStoreNode GetPropStore(PSAccessServiceType propStoreAccessService)
        {
            if (propStoreAccessService is IHaveTheKeyIT skp)
            {
                IHaveTheKeyIT storeKeyProvider = skp;
                PropStoreNode propStoreNode = storeKeyProvider.PropStoreNode;
                return propStoreNode;
            }
            else
            {
                throw new InvalidOperationException($"The {nameof(propStoreAccessService)} does not implement the {nameof(IHaveTheKeyIT)} interface.");
            }
        }

        #endregion

        #region Path Analysis

        private OSCollection<T> StartBinding(LocalBindingInfo bInfo, out bool pathIsAbsolute, out bool complete)
        {
            complete = false;
            string[] pathElements = GetPathComponents(bInfo.PropertyPath.Path, out int compCount);

            if (compCount == 0)
            {
                throw new InvalidOperationException("The path has no components.");
            }

            if(pathElements[compCount - 1] == "..")
            {
                throw new InvalidOperationException("The last component of the path cannot be '..'");
            }

            if (compCount == 1 && pathElements[0] == ".")
            {
                // Can't bind to yourself.
            }

            // Remove initial "this" component, if present.
            if(pathElements[0] == ".")
            {
                pathElements.CopyTo(pathElements, 1);
            }

            OSCollection<T> result = new OSCollection<T>();
            PropStoreNode next;

            // Create the observable source for the first element.
            string rootElement = pathElements[ROOT_INDEX];

            if (rootElement == string.Empty)
            {
                pathIsAbsolute = true;

                // remove the initial (empty) path component.
                pathElements.CopyTo(pathElements, 1);

                // Find our current root.
                next = GetRootPropBag(_ourNode);

                result.Add(CreateAndListen(next, rootElement, SourceKindEnum.AbsRoot));
            }
            else
            {
                pathIsAbsolute = false;
                next = _ourNode;
            }

            // As each step is processed, next is set to the Object Node that is the "home" for the next path (either a ".." or a property name.)
            // If all objects have not been instantiated, processing may stop before the source property is reached.

            // Process each step, except for the last.
            for (int nPtr = 0; next != null && nPtr < compCount - 1; nPtr++)
            {
                string pathComp = pathElements[nPtr];
                if (pathComp == "..")
                {
                    if(nPtr > 0)
                    { 
                        SourceKindEnum parentSourceKind = result[nPtr - 1].SourceKind;

                        if(parentSourceKind != SourceKindEnum.RootUp && parentSourceKind != SourceKindEnum.Up)
                        {
                            throw new InvalidOperationException("A path cannot have '..' components that folllow a component that indentifies a property by name.");
                        }
                    }

                    SourceKindEnum nextResultKind = nPtr == 0 ? SourceKindEnum.RootUp : SourceKindEnum.Up;
                    result.Add(CreateAndListen(next, pathComp, nextResultKind));
                    //next = GetHostingPropBag(next);
                    next = next.Parent?.Parent;
                }
                else
                {
                    if (GetChild(next, pathComp, out PropStoreNode child, out IPropBagInternal propBag))
                    {
                        SourceKindEnum nextResultKind = nPtr == 0 ? SourceKindEnum.RootDown : SourceKindEnum.Down;
                        result.Add(CreateAndListen(propBag, pathComp, nextResultKind));

                        // Get the Prop Store Node associated with the IPropBag being hosted by the found PropItem.
                        next = child.OnlyChildOfPropItem;
                    }
                    else
                    {
                        // Our client is no longer with us, or no propItem named pathComp exists.
                        next = null;
                    }
                }
            }

            // Add the terminal node.
            if(next != null)
            {
                string pathComp = pathElements[compCount - 1];

                if (GetChild(next, pathComp, out PropStoreNode child, out IPropBagInternal propBag))
                {
                    result.Add(CreateAndListen(propBag, pathComp, SourceKindEnum.TerminalNode));

                    // We have subscribed to the property that is the source of the binding.
                    complete = true;
                }
            }

            return result;
        }

        private ObservableSource<T> CreateAndListen(IPropBagInternal propBag, string pathComp, SourceKindEnum sourceKind)
        {
            ObservableSource<T> result = new ObservableSource<T>((IPropBag) propBag, pathComp, sourceKind, BINDER_NAME);
            result.DataSourceChanged += DataSourceHasChanged;
            //result.PropertyChangedWithTVals += PropertyChangedWithTVals;
            return result;
        }

        private ObservableSource<T> CreateAndListen(PropStoreNode propStoreNode, string pathComp, SourceKindEnum sourceKind)
        {
            ObservableSource<T> result = new ObservableSource<T>(propStoreNode, pathComp, sourceKind, BINDER_NAME);
            result.DataSourceChanged += DataSourceHasChanged;
            return result;
        }

        private PropStoreNode GetRootPropBag(PropStoreNode objectNode)
        {
            PropStoreNode last = objectNode;

            PropStoreNode next = objectNode;
            while(null != (next = GetHostingPropBag(last))) { }

            return last;
        }

        private string[] GetPathComponents(string path, out int count)
        {
            string[] components = path.Split('/');
            count = components.Length;
            return components;
        }

        private PropStoreNode GetHostingPropBag(PropStoreNode propStoreNode)
        {
            PropStoreNode result = propStoreNode.IsObjectNode ? propStoreNode.Parent : propStoreNode.Parent?.Parent;

            return result;

            //// See if this PropStoreNode has a parent PropItem
            //PropStoreNode parent;
            //if (null == (parent = propStoreNode.Parent)) return null;

            //System.Diagnostics.Debug.Assert(parent.Int_PropData != null, "Any parent of an ObjectId type PropStoreNode should have an instance of an IPropDataInternal.");
            //System.Diagnostics.Debug.Assert(parent.Int_PropData.TypedProp != null, "All objects that implement IPropDataInternal must have a non-null value for TypedProp.");
            //System.Diagnostics.Debug.Assert(parent.Int_PropData.TypedProp.Type is IPropBag, "All calls to GetHostingPropBag must be for children");

            //object test = parent.Int_PropData.TypedProp.TypedValueAsObject;

            //if (test == null) return null;

            //PropStoreNode result;
            //if (propStoreNode.PropBagProxy.PropBagRef.TryGetTarget(out IPropBagInternal propBag))
            //{
            //    PSAccessServiceType propStoreAccessService = propBag.ItsStoreAccessor;
            //    if(propStoreAccessService is IHaveTheKeyIT keyProvider)
            //    {
            //        result = keyProvider.GetObjectNodeForPropVal(parentsData);
            //    }
            //    else
            //    {
            //        result = null;
            //    }

            //    //IHaveTheKeyIT keyAccessor = (IHaveTheKeyIT)((IPropBagInternal)propBag).ItsStoreAccessor;
            //    //result = keyAccessor.GetObjectNodeForPropVal(parentsData);
            //}
            //else
            //{
            //    result = null;
            //}
            //return result;
        }

        private bool GetChild(PropStoreNode objectNode, string propertyName, out PropStoreNode child, out IPropBagInternal propBag)
        {
            if (!objectNode.IsObjectNode) throw new ArgumentException("The PropStoreNode from which to get the child, must be for a PropBag object, not a property.");

            // Unwrap the weak reference held by the objectNode in it's PropBagProxy.PropBagRef.
            if (objectNode.PropBagProxy.PropBagRef.TryGetTarget(out propBag))
            {
                // Each PropBag has a reference to its StoreAccessor which can fetch its children.
                // This saves us from having to find the PropStoreNode for this objectNode, using only
                // our PropStoreNode, or a reference to the PropStoreAccessService.
                IHaveTheKeyIT keyProvider = (IHaveTheKeyIT)propBag.ItsStoreAccessor;

                // TODO: We could have the keyAccessor do the name to id lookup.
                PropIdType propId = objectNode.PropBagProxy.Level2KeyManager.FromRaw(propertyName);

                bool result = keyProvider.TryGetAChildOfMine(propId, out child);
                return result;
            }
            else
            {
                // The Weak Reference holds no target.
                child = null;
                return false;
            }
        }

        //private IHaveTheKeyIT GetKeyProvider(IPropBagInternal propBag)
        //{
        //    PSAccessServiceType propStoreAccessService = propBag.ItsStoreAccessor;
        //    if (propStoreAccessService is IHaveTheKeyIT keyProvider)
        //    {
        //        return keyProvider;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

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

        private void RefreshPathListeners(WeakReference<IPropBagInternal> bindingTarget, LocalBindingInfo bInfo, OSCollection<T> pathListeners,
            ObservableSource<T> signalingOs, DataSourceChangedEventArgs changeInfo)
        {
            Complete = false;
            int nodeIndex = _listeners.IndexOf(signalingOs);
            if (nodeIndex == -1) throw new InvalidOperationException($"Could not get pointer to path element while processing DataSourceChanged event for {BINDER_NAME}.");

            string[] pathElements = GetPathComponents(bInfo.PropertyPath.Path, out int compCount);
            PropStoreNode next;

            if (signalingOs.SourceKind == SourceKindEnum.AbsRoot)
            {
                System.Diagnostics.Debug.Assert(changeInfo.ChangeType == DataSourceChangeTypeEnum.ParentHasChanged,
                    $"Received a DataSourceChanged event from on a node of kind = AbsRoot. " +
                    $"The DataSourceChangeType should be {nameof(DataSourceChangeTypeEnum.ParentHasChanged)}, but is {changeInfo.ChangeType}.");

                System.Diagnostics.Debug.Assert(nodeIndex == 0,
                    "Received a ParentHasChanged on a node of kind = AbsRoot and the node index is not zero.");

                System.Diagnostics.Debug.Assert(PathIsAbsolute,
                    "Received a ParentHasChanged on a node of kind = AbsRoot, but our 'PathIsAbsolute' property is set to false.");

                // We have a new root, start at the beginning.
                next = GetRootPropBag(_ourNode);
            }
            else
            {
                if (PathIsAbsolute)
                {
                    next = GetRootPropBag(_ourNode);
                }
                else
                {
                    next = _ourNode;
                }

                next = GetChangedNode(next, pathElements, nodeIndex + 1);
            }

            int nPtr = nodeIndex + 1;
            // Process each step, except for the last.
            for (; next != null && nPtr < compCount - 1; nPtr++)
            {
                string pathComp = pathElements[nPtr];
                if (pathComp == "..")
                {
                    SourceKindEnum nextResultKind = nPtr == 0 ? SourceKindEnum.RootUp : SourceKindEnum.Up;
                    ObservableSource<T> newListener = CreateAndListen(next, pathComp, nextResultKind);

                    if (_listeners.Count > nPtr)
                    {
                        ObservableSource<T> listener = _listeners[nPtr];
                        TearDownListener(listener);
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
                    if (GetChild(next, pathComp, out PropStoreNode child, out IPropBagInternal propBag))
                    {
                        SourceKindEnum nextResultKind = nPtr == 0 ? SourceKindEnum.RootDown : SourceKindEnum.Down;
                        ObservableSource<T> newListener = CreateAndListen(propBag, pathComp, nextResultKind);

                        if (_listeners.Count > nPtr)
                        {
                            ObservableSource<T> listener = _listeners[nPtr];
                            TearDownListener(listener);
                            _listeners[nPtr] = newListener;
                        }
                        else
                        {
                            _listeners.Add(newListener);
                        }

                        // Get the Prop Store Node associated with the IPropBag being hosted by the found PropItem.
                        next = child.OnlyChildOfPropItem;
                    }
                    else
                    {
                        // Our client is no longer with us, or no propItem named pathComp exists.
                        next = null;
                    }
                }
            }

            // Add the terminal node.
            if (next != null)
            {
                System.Diagnostics.Debug.Assert(nPtr == compCount - 1, $"The counter variable: nPtr should be {compCount - 1}, but is {nPtr} instead.");

                string pathComp = pathElements[nPtr];

                if (GetChild(next, pathComp, out PropStoreNode child, out IPropBagInternal propBag))
                {
                    ObservableSource<T> newListener = CreateAndListen(propBag, pathComp, SourceKindEnum.TerminalNode);

                    if (_listeners.Count > nPtr)
                    {
                        ObservableSource<T> listener = _listeners[nPtr];
                        TearDownListener(listener);
                        _listeners[nPtr] = newListener;
                    }
                    else
                    {
                        _listeners.Add(newListener);
                    }

                    // We have subscribed to the property that is the source of the binding.
                    Complete = true;
                    nPtr++;
                    System.Diagnostics.Debug.WriteLine("The binding source has been reached.");
                    UpdateTarget(bindingTarget, child);
                }
            }

            for(; nPtr < _listeners.Count; nPtr ++)
            {
                ObservableSource<T> listener = _listeners[nPtr];
                TearDownListener(listener);
                _listeners.RemoveAt(nPtr);
            }
        }

        private void TearDownListener(ObservableSource<T> listener)
        {
            listener.RemoveSubscriptions();
            if(listener.SourceKind == SourceKindEnum.AbsRoot || listener.SourceKind == SourceKindEnum.RootUp || listener.SourceKind == SourceKindEnum.Up)
            {
                listener.Unsubscribe(PropertyChangedWithTVals);
            }
            else
            {
                listener.Unsubscribe(DataSourceHasChanged);
            }
        }

        private PropStoreNode GetChangedNode(PropStoreNode objectNode, string[] pathElements, int startIndex)
        {
            for (int nPtr = 0; objectNode != null && nPtr < startIndex; nPtr++)
            {
                string pathComp = pathElements[nPtr];
                if (pathComp == "..")
                {
                    objectNode = objectNode.Parent?.Parent;
                }
                else
                {
                    if (GetChild(objectNode, pathComp, out PropStoreNode child, out IPropBagInternal propBag))
                    {
                        // Get the Prop Store Node associated with the IPropBag being hosted by the found PropItem.
                        objectNode = child.OnlyChildOfPropItem;
                    }
                    else
                    {
                        // Our client is no longer with us, or no propItem named pathComp exists.
                        objectNode = null;
                    }
                }
            }
            return objectNode;
        }

        private void DataSourceHasChanged(object sender, DataSourceChangedEventArgs e)
        {
            // Check for unsupported Change Type.
            if (!(e.ChangeType == DataSourceChangeTypeEnum.ParentHasChanged || e.ChangeType == DataSourceChangeTypeEnum.PropertyChanged))
            {
                throw new ApplicationException($"The DataSourceChangedType: {e.ChangeType} is not recognized.");
            }

            ObservableSource<T> listener = (ObservableSource<T>)sender;
            RefreshPathListeners(_targetObject, BindingInfo, _listeners, listener, e);
        }

        private void PropertyChangedWithTVals(object sender, PCTypedEventArgs<T> e)
        {
            System.Diagnostics.Debug.WriteLine("The terminal node's property value has been updated.");

            if (_targetObject.TryGetTarget(out IPropBagInternal propBag))
            {
                bool wasSet = ((IPropBag)propBag).SetIt(e.NewValue, e.PropertyName);
                string status = wasSet ? "has been set" : "could not be updated";
                System.Diagnostics.Debug.WriteLine($"The binding target {status}.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Target IPropBag was found to be 'not with us' on call to Update Target.");
            }
        }

        #endregion

        #region Update Target

        private void UpdateTarget(WeakReference<IPropBagInternal> bindingTarget, PropStoreNode sourcePropNode)
        {
            if (sourcePropNode.Parent.PropBagProxy.PropBagRef.TryGetTarget(out IPropBagInternal propBag))
            {
                IHaveTheKeyIT keyProvider = (IHaveTheKeyIT)propBag.ItsStoreAccessor;

                // TODO: We would not need to do this if we had a Level1 Key Manager
                // or PropBag would accept a PropId, instead of a name.
                // or if we had a way to get a node from a CKey.
                if (keyProvider.PropStoreNode.PropBagProxy.Level2KeyManager.TryGetFromRaw(PropertyName, out PropIdType propId))
                {
                    if (propBag.ItsStoreAccessor.TryGetValue((IPropBag)propBag, propId, out IPropData propData))
                    {
                        UpdateTarget(bindingTarget, (T)propData.TypedProp.TypedValueAsObject);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Update Target could not find that property by name: {PropertyName}.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("The Weak Reference held by the PropBagProxy contains not object.");
            }
        }

        private void UpdateTarget(WeakReference<IPropBagInternal> bindingTarget, T newValue)
        {
            if(bindingTarget.TryGetTarget(out IPropBagInternal propBag))
            {
                bool result = ((IPropBag)propBag).SetIt(newValue, this.PropertyName);
                System.Diagnostics.Debug.WriteLine("The target has been updated.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Target IPropBag was found to be 'not with us' on call to Update Target.");
            }
        }

        #endregion
    }
}
