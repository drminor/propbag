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
        OSCollection<T> _dataSourceChangeListeners { get; set; }

        #endregion

        #region Public Properties

        public ExKeyT BindingTarget { get; }
        public LocalBindingInfo BindingInfo { get; private set; }

        public bool PathIsAbsolute { get; }
        public bool Complete { get; private set; }

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

        public LocalBinder(PSAccessServiceType propStoreAccessService, ExKeyT TargetPropId, LocalBindingInfo bindingInfo)
        {
            _ourNode = GetPropStore(propStoreAccessService);
            BindingTarget = TargetPropId;
            BindingInfo = bindingInfo;

            _dataSourceChangeListeners = StartBinding(BindingTarget, BindingInfo, out bool pathIsAbsolute, out bool complete);
            PathIsAbsolute = pathIsAbsolute;
            Complete = complete;
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

        private OSCollection<T> StartBinding(ExKeyT bindingTarget, LocalBindingInfo bInfo, out bool pathIsAbsolute, out bool complete)
        {
            complete = false;
            string[] pathElements = GetPathComponents(bInfo.PropertyPath.Path, out int compCount);

            if (compCount == 0)
            {
                throw new InvalidOperationException("The path has no components.");
            }

            if(compCount == 1 && pathElements[0] == ".")
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

                result.Add(new ObservableSource<T>(next, rootElement, SourceKindEnum.AbsRoot, BINDER_NAME));
                next = _ourNode;
            }
            else
            {
                pathIsAbsolute = false;
                next = _ourNode;
            }

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

                    SourceKindEnum nextResultKind = nPtr > 0 ? SourceKindEnum.Up : SourceKindEnum.RootUp;
                    result.Add(new ObservableSource<T>(next, pathComp, nextResultKind, BINDER_NAME));
                    next = GetHostingPropBag(next);
                }
                else
                {
                    if (GetChild(next, pathComp, out PropStoreNode child, out IPropBag propBag))
                    {
                        SourceKindEnum nextResultKind = nPtr > 0 ? SourceKindEnum.Down : SourceKindEnum.RootDown;
                        result.Add(new ObservableSource<T>(propBag, pathComp, nextResultKind, BINDER_NAME));

                        // Get the Prop Store Node associated with the IPropBag being hosted by the found PropItem.
                        next = ((IHaveTheKeyIT)next).GetObjectNodeForPropVal(child.Int_PropData);
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
                if (pathComp == "..")
                {
                    throw new InvalidOperationException("The last component of the path cannot be '..'.");
                }
                else
                {
                    if (GetChild(next, pathComp, out PropStoreNode child, out IPropBag propBag))
                    {
                        result.Add(new ObservableSource<T>(propBag, pathComp, SourceKindEnum.TerminalNode, BINDER_NAME));

                        // We have subscribed to the property that is the source of the binding.
                        complete = true;
                    }
                }
            }

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

        private PropStoreNode GetHostingPropBag(PropStoreNode objectNode)
        {
            // See if this PropStoreNode has a parent PropItem
            IPropDataInternal parentsData = objectNode?.Parent?.Int_PropData;

            if (parentsData != null)
            {
                // Get the PropStoreNode for the PropBag that "owns" the found PropItem.
                PropStoreNode result = ((IHaveTheKeyIT)objectNode).GetObjectNodeForPropVal(parentsData);
                return result;
            }
            else
            {
                return null;
            }
        }

        private bool GetChild(PropStoreNode objectNode, string propertyName, out PropStoreNode child, out IPropBag propBag)
        {
            if (objectNode.PropBagProxy.PropBagRef.TryGetTarget(out propBag))
            {
                PropIdType propId = objectNode.PropBagProxy.Level2KeyManager.FromRaw(propertyName);
                ExKeyT exKey = ((IHaveTheKeyIT)objectNode).GetTheKey(objectNode.PropBagProxy, propId);

                if (objectNode.TryGetChild(exKey.CKey, out child))
                {
                    return true;
                }
                else
                {
                    // Could not locate a property with that name.
                    return false;
                }
            }
            else
            {
                // Our client is no longer with us.
                child = null;
                return false;
            }
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

        private bool RefreshPathListeners(ExKeyT bindingTarget, LocalBindingInfo bInfo, OSCollection<T> pathListeners,
            DataSourceChangedEventArgs changeInfo, ObservableSource<T> signalingOs)
        {
            // Assume that this operation will not require the binding to be updated,
            // until proven otherwise.
            bool bindingInfoChanged = false;

            int nodeIndex = _dataSourceChangeListeners.IndexOf(signalingOs);

            if (nodeIndex == -1)
            {
                throw new InvalidOperationException($"Could not get pointer to path element while processing " +
                    $"DataSourceChanged event for {BINDER_NAME} in PropBagControlsWPF.Binders.");
            }

            ObservableSource<T> parentOs = pathListeners[nodeIndex];
            ObservableSourceStatusEnum status;

            // Initializing
            // Attempt to get a reference to the Source Root (Object with DataContext property or DataSourceProvider.)
            if (changeInfo.ChangeType == DataSourceChangeTypeEnum.Initializing)
            {
                System.Diagnostics.Debug.Assert(nodeIndex == ROOT_INDEX, $"The node index should refer to the ObservableSource<T> for " +
                    $"the root when DataSourceChangeType = {nameof(DataSourceChangeTypeEnum.Initializing)}.");

                if (pathListeners[0].SourceKind == SourceKindEnum.AbsRoot)
                {

                }
                else if (pathListeners[0].SourceKind == SourceKindEnum.RootUp)
                {
                    //ObservableSource<T> ru = new ObservableSource<T>(pathListeners[0].PathElement, BINDER_NAME, )
                }
                else
                {
                    System.Diagnostics.Debug.Assert(pathListeners[0].SourceKind == SourceKindEnum.RootDown, "Should be root down here.");
                }


                // Fix Me
                //ObservableSourceProvider<T> osp = GetSourceRoot(bindingTarget, bInfo.Source, rootElementName, BinderName);
                ObservableSource<T> osp = null;

                if (osp == null)
                {
                    throw new InvalidOperationException($"{BINDER_NAME} could not locate a data source.");
                }

                parentOs = null;

                status = pathListeners.ReplaceListener(ROOT_INDEX, parentOs, this.DataSourceHasChanged, this.PropertyChangedWithTVals);

                if (parentOs?.IsListeningForNewDC != true)
                {
                    throw new InvalidOperationException($"{BINDER_NAME} could not locate a data source.");
                }
            }

            // Fix Me.
            //// DataContextUpdated
            //// Refresh the Source Root
            //else if (changeInfo.ChangeType == DataSourceChangeTypeEnum.PropertyChanged)
            //{
            //    System.Diagnostics.Debug.Assert(nodeIndex == ROOT_INDEX, $"The node index should refer to the ObservableSource<T> for" +
            //        $" the root when DataSourceChangeType = {nameof(DataSourceChangeTypeEnum.DataContextUpdated)}.");

            //    status = parentOs.Status;

            //    pathListeners.ResetListeners(ROOT_INDEX + 1, this.DataSourceHasChanged);
            //    bindingInfoChanged = true;
            //}

            // Handle Property Changed
            else if (changeInfo.ChangeType == DataSourceChangeTypeEnum.PropertyChanged)
            {
                // The PropModel of the value of this node's child may have changed.
                // Replace the child with a new SourceListner, if appropriate
                // and then fall-through to normal processing, at signaled step + 2.

                string changedPropName = changeInfo.PropertyName;

                nodeIndex++;
                ObservableSource<T> child = pathListeners[nodeIndex];
                bool matched = changedPropName == child.PathElement;


                if (!matched || !(nodeIndex < pathListeners.Count - 1))
                {
                    // Action is not required if
                    // The updated property is not part of our path,
                    // or our child in the terminal node,
                    return bindingInfoChanged;
                }

                // Replace the child, if 
                //  1. It was null and now is not, or
                //  2. It is now null and was not null, or
                //  3. It was PropBag-based and is now is not, or
                //  4. It was a regular CLR object and is now PropBag-based, or
                //  5. It is still PropBag-Based and the prop item corresponding
                //      to the grandchild node had a change in type.

                if (!child.Status.IsReadyOrWatching())
                {
                    string prevValueForPathElement = child.PathElement;
                    ObservableSource<T> newChild = parentOs.GetChild(child.PathElement);

                    if (newChild?.PropId == null)
                    {
                        // Still no data ??
                        System.Diagnostics.Debug.WriteLine("Child was null and is still null.");

                        //ResetPathListeners(pathListeners, nodeIndex);
                        pathListeners.ResetListeners(nodeIndex, this.DataSourceHasChanged);


                        bindingInfoChanged = true;
                        return bindingInfoChanged;
                    }

                    ObservableSourceStatusEnum newCStatus = pathListeners.ReplaceListener(nodeIndex, newChild,
                        this.DataSourceHasChanged, PropertyChangedWithTVals);

                    if (newChild != null)
                    {
                        string pathElement = "";
                        newChild.PathElement = pathElement;

                        bindingInfoChanged = (pathElement != prevValueForPathElement);
                    }
                }

                // If the child was updated, begin processing our grand children.
                parentOs = pathListeners[nodeIndex];
                status = parentOs?.Status ?? ObservableSourceStatusEnum.NoType;
            }
            else
            {
                throw new ApplicationException($"The DataSourceChangedType: {changeInfo.ChangeType} is not recognized.");
            }

            // Now, starting at step: stepNo + 1, replace or refresh each listener. 

            int nPtr = nodeIndex + 1;
            for (; nPtr < pathListeners.Count - 1 && status.IsReadyOrWatching(); nPtr++)
            {
                // This is an itermediate step and the path has at least two components.

                parentOs.BeginListeningToSource();
                if (nPtr > 1)
                {
                    // Make sure we are listening to events that this ObservableSource<T> may raise.
                    // We know that we are subscribed to the root.
                    parentOs.Subscribe(this.DataSourceHasChanged);
                }

                string pathElement = pathListeners[nPtr].PathElement;
                string prevValueForPathElement = pathListeners[nPtr].PathElement;

                // Fix Me
                ObservableSource<T> os = parentOs.GetChild(pathElement);

                //status = ReplaceListener(pathListeners, nPtr, ref osp, this.DataSourceHasChanged,
                //    out ObservableSource<T> os);

                status = pathListeners.ReplaceListener(nPtr, os, this.DataSourceHasChanged, PropertyChangedWithTVals);

                if (os != null)
                {
                    // Fix Me
                    pathElement = "";
                    os.PathElement = pathElement;

                    bindingInfoChanged = (pathElement != prevValueForPathElement);
                }
                // Note: if os is null, status will be "NoType", i.e. not ready.

                if (status == ObservableSourceStatusEnum.Ready)
                {
                    System.Diagnostics.Debug.WriteLine($"Listener for {pathElement} is ready.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Listener for {pathElement} is not ready.");
                }

                parentOs = os;
                //status = os.Status; -- Not needed, ReplaceListener sets this variable.
            }

            if (nPtr == pathListeners.Count - 1)
            {
                // Process terminal node.
                ObservableSource<T> lastNode = pathListeners[nPtr];
                if (status.IsReadyOrWatching())
                {
                    // Fix Me:
                    string pathElement = "";
                    if (lastNode.PathElement != pathElement)
                    {
                        lastNode.PathElement = pathElement;
                        bindingInfoChanged = true;
                    }
                    if (!status.IsWatching())
                    {
                        parentOs.BeginListeningToSource();
                    }
                }
            }
            else
            {
                // Don't have all of the data present required to create a binding.
                System.Diagnostics.Debug.WriteLine($"RefreshPathListeners claims that no binding should be created for path = {bInfo.PropertyPath.Path}.");
                //ResetPathListeners(pathListeners, nPtr);
                pathListeners.ResetListeners(nPtr, this.DataSourceHasChanged);
            }

            return bindingInfoChanged;
        }

        private void PropertyChangedWithTVals(object sender, PCTypedEventArgs<T> e)
        {
            System.Diagnostics.Debug.WriteLine("The terminal node's property value has been updated.");
        }

        protected virtual void DataSourceHasChanged(object sender, DataSourceChangedEventArgs e)
        {
            //BindingBase oldBinding;
            //if (BindingTarget.IsDependencyProperty)
            //{
            //    oldBinding = GetTheBindingBase(BindingTarget.DependencyObject, BindingTarget.DependencyProperty);
            //}
            //else
            //{
            //    oldBinding = null;
            //}

            //bool hadBinding = oldBinding != null;

            //if (!e.DataWasUpdated && oldBinding != null)
            //{
            //    return;
            //}

            //ObservableSource<T> os = (ObservableSource<T>)sender;
            //bool bindingInfoChanged = RefreshPathListeners(BindingTarget, BindingInfo, SourceType,
            //    _dataSourceChangeListeners, e, os);

            //if (!bindingInfoChanged && oldBinding != null)
            //{
            //    return;
            //}

            //Binding newBinding = CreateTheBinding(BindingTarget, BindingInfo, SourceType,
            //    _dataSourceChangeListeners, out bool isCustom);

            //if (hadBinding && BindingTarget.IsDependencyProperty)
            //{
            //    ClearTheBinding(BindingTarget.DependencyObject, BindingTarget.DependencyProperty);
            //}

            //if (newBinding != null)
            //{
            //    try
            //    {

            //        //BindingExpressionBase bExp = BindingOperations.SetBinding(_targetObject,
            //        //    _targetProperty, newBinding);

            //        if (BindingTarget.IsDependencyProperty)
            //        {
            //            ClearTheBinding(BindingTarget.DependencyObject, BindingTarget.DependencyProperty);
            //            BindingExpressionBase bExp = SetTheBinding(BindingTarget.DependencyObject, BindingTarget.DependencyProperty, newBinding);
            //            string bType = isCustom ? "PropBag-Based" : "Standard";
            //            System.Diagnostics.Debug.WriteLine($"CREATING {bType} BINDING from {newBinding.Path.Path} to {BindingTarget.PropertyName} on object: {BindingTarget.ObjectName}.");
            //        }
            //    }
            //    catch
            //    {
            //        System.Diagnostics.Debug.WriteLine("Attempt to SetBinding failed.");
            //    }

            //    if (hadBinding)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"{BinderName} set a new binding on some target that is replacing an existing binding.");
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine($"{BinderName} set a new binding on some target that had no binding previously.");
            //    }
            //}
            //else
            //{
            //    if (hadBinding)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"{BinderName} removed binding on some target and replaced it with no binding.");
            //    }
            //}
        }


        #endregion
    }
}
