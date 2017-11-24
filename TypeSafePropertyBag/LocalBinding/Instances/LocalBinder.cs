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

        #region Private Properties

        const string BINDER_NAME = "PB_LocalBinder";

        PSAccessServiceType _ourStoreAccessor { get; }
        IHaveTheKeyIT _ourStoreKeyProvider { get; }
        PropStoreNode _ourNode { get; }

        /// <summary>
        /// Used to listen to changes to the sources of data for each step in the path.
        /// </summary>
        OSCollection<T> _dataSourceChangeListeners { get; set; }

        bool PathIsAbsolute { get; }

        #endregion

        #region Public Properties

        public ExKeyT BindingTarget { get; }
        public LocalBindingInfo BindingInfo { get; private set; }

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
            _ourStoreAccessor = propStoreAccessService ?? throw new ArgumentNullException(nameof(propStoreAccessService));
            BindingTarget = TargetPropId;
            BindingInfo = bindingInfo;

            _ourNode = GetPropStore(propStoreAccessService, out IHaveTheKeyIT storeProvider);
            _ourStoreKeyProvider = storeProvider;

            //UseMultiBinding = useMultiBinding;
            //_dataSourceChangeListeners = null;

            StartBinding(BindingTarget, BindingInfo);
        }

        private PropStoreNode GetPropStore(PSAccessServiceType propStoreAccessService, out IHaveTheKeyIT storeKeyProvider)
        {
            if (propStoreAccessService is IHaveTheKeyIT skp)
            {
                storeKeyProvider = skp;
                PropStoreNode propStoreNode = storeKeyProvider.PropStoreNode;
                return propStoreNode;
            }
            else
            {
                throw new InvalidOperationException($"The {nameof(propStoreAccessService)} does not implement the {nameof(IHaveTheKeyIT)} interface.");
            }
        }

        private bool StartBinding(ExKeyT bindingTarget, LocalBindingInfo bindingInfo)
        {
            _dataSourceChangeListeners = PreparePathListeners(BindingTarget, BindingInfo, out bool pathIsAbsolute);

            //InitPathListeners(BindingTarget, BindingInfo, _dataSourceChangeListeners);

            //CreateTheBinding(BindingTarget, BindingInfo, _dataSourceChangeListeners);

            return pathIsAbsolute;
        }

        #endregion

        #region Provide Value

        protected virtual void SetDefaultConverter()
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

        protected virtual IConvertValues GetConverter(IPropBagProxy bindingTarget, LocalBindingInfo bInfo, Type sourceType,
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

        protected virtual object OurDefaultConverterParameterBuilder(IPropBagProxy bindingTarget, LocalBindingInfo bInfo, Type sourceType,
            string pathElement)
        {
            //return new TwoTypes(sourceType, bindingTarget.PropertyType);
            return null;
        }

        //protected virtual ObservableSource<T> GetSourceRoot(IPropBagProxy bindingTarget, object source,
        //    string pathElement = ROOT_PATH_ELEMENT, string binderName = "")
        //{
        //    ObservableSource<T> osp = ObservableSource<T>.GetSourceRoot(bindingTarget, source, pathElement, binderName);
        //    return osp;
        //}

        #endregion

        #region Path Analysis

        protected OSCollection<T> PreparePathListeners(ExKeyT bindingTarget, LocalBindingInfo bInfo, out bool pathIsAbsolute)
        {
            string[] pathElements = GetPathComponents(bInfo.PropertyPath.Path, out int compCount);

            if (compCount == 0)
            {
                throw new InvalidOperationException("The path has no components.");
            }

            OSCollection<T> result = new OSCollection<T>();
            PropStoreNode next = _ourNode;

            // Create the observable source for the first element.
            string rootElement = pathElements[ROOT_INDEX];

            if (rootElement == string.Empty)
            {
                // Add place holder of type AbsRoot.
                pathIsAbsolute = true;
                result.Add(new ObservableSource<T>(rootElement, BINDER_NAME, SourceKindEnum.AbsRoot));
            }
            else
            {
                pathIsAbsolute = false;
                if(rootElement == "..")
                {
                    result.Add(new ObservableSource<T>(next, rootElement, SourceKindEnum.RootUp, BINDER_NAME));
                    IPropDataInternal parentsData = next?.Parent?.Int_PropData;
                    if (parentsData != null)
                        next = ((IHaveTheKeyIT)next).GetNodeForPropVal(parentsData);
                    else
                        next = null;
                }
                else
                {
                    if(next.PropBagProxy.PropBagRef.TryGetTarget(out IPropBag propBag))
                    {
                        PropIdType propId = next.PropBagProxy.Level2KeyManager.FromRaw(rootElement);
                        ExKeyT exKey = ((IHaveTheKeyIT)next).GetTheKey(next.PropBagProxy, propId);

                        if(next.TryGetChild(exKey.CKey, out PropStoreNode child))
                        {
                            result.Add(new ObservableSource<T>(propBag, rootElement, SourceKindEnum.RootDown, BINDER_NAME));

                            // Get the Prop Store Node associated with the IPropBag being 
                            // hosted by the found PropItem.
                            next = ((IHaveTheKeyIT)next).GetNodeForPropVal(child.Int_PropData);
                        }
                        else
                        {
                            // Could not locate a property with that name.
                            next = null;
                        }
                    }
                    else
                    {
                        // Our client is no longer with us.
                        next = null;
                    }
                }
            }

            // Add the intervening nodes if any.
            for (int nPtr = 1; next != null && nPtr < compCount - 1; nPtr++)
            {
                string pathComp = pathElements[nPtr];

                if (pathComp == "..")
                {
                    if(!(result[nPtr -1].SourceKind == SourceKindEnum.RootUp || result[nPtr - 1].SourceKind == SourceKindEnum.Up))
                    {
                        throw new InvalidOperationException("A path cannot have '..' components that folllow a component that indentifies a property by name.");
                    }

                    result.Add(new ObservableSource<T>(next, rootElement, SourceKindEnum.RootUp, BINDER_NAME));
                    IPropDataInternal parentsData = next?.Parent?.Int_PropData;
                    if (parentsData != null)
                        next = ((IHaveTheKeyIT)next).GetNodeForPropVal(parentsData);
                    else
                        next = null;
                }
                else
                {
                    if (next.PropBagProxy.PropBagRef.TryGetTarget(out IPropBag propBag))
                    {
                        PropIdType propId = next.PropBagProxy.Level2KeyManager.FromRaw(rootElement);
                        ExKeyT exKey = ((IHaveTheKeyIT)next).GetTheKey(next.PropBagProxy, propId);

                        if (next.TryGetChild(exKey.CKey, out PropStoreNode child))
                        {
                            result.Add(new ObservableSource<T>(propBag, rootElement, SourceKindEnum.RootDown, BINDER_NAME));

                            // Get the Prop Store Node associated with the IPropBag being 
                            // hosted by the found PropItem.
                            next = ((IHaveTheKeyIT)next).GetNodeForPropVal(child.Int_PropData);
                        }
                        else
                        {
                            // Could not locate a property with that name.
                            next = null;
                        }
                    }
                    else
                    {
                        // Our client is no longer with us.
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
                    if (next.PropBagProxy.PropBagRef.TryGetTarget(out IPropBag propBag))
                    {
                        PropIdType propId = next.PropBagProxy.Level2KeyManager.FromRaw(rootElement);
                        ExKeyT exKey = ((IHaveTheKeyIT)next).GetTheKey(next.PropBagProxy, propId);

                        if (next.TryGetChild(exKey.CKey, out PropStoreNode child))
                        {
                            result.Add(new ObservableSource<T>(propBag, rootElement, SourceKindEnum.TerminalNode, BINDER_NAME));

                            // Get the Prop Store Node associated with the IPropBag being 
                            // hosted by the found PropItem.
                            next = ((IHaveTheKeyIT)next).GetNodeForPropVal(child.Int_PropData);
                        }
                        else
                        {
                            // Could not locate a property with that name.
                            next = null;
                        }
                    }
                    else
                    {
                        // Our client is no longer with us.
                        next = null;
                    }
                }
            }

            return result;
        }

        protected bool InitPathListeners(ExKeyT bindingTarget, LocalBindingInfo bInfo, OSCollection<T> pathListeners)
        {
            DataSourceChangedEventArgs dsChangedEventArgs = new DataSourceChangedEventArgs(DataSourceChangeTypeEnum.Initializing, null, typeof(T), true, true);

            return RefreshPathListeners(bindingTarget, bInfo, pathListeners,
                dsChangedEventArgs, pathListeners[ROOT_INDEX]);
        }

        protected bool RefreshPathListeners(ExKeyT bindingTarget, LocalBindingInfo bInfo, OSCollection<T> pathListeners,
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

                if(pathListeners[0].SourceKind == SourceKindEnum.AbsRoot)
                {

                }
                else if(pathListeners[0].SourceKind == SourceKindEnum.RootUp)
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

        protected virtual string[] GetPathComponents(string path, out int count)
        {
            string[] components = path.Split('/');
            count = components.Length;
            return components;
        }

        #endregion

        #region Build Bindings

        protected void CreateTheBinding(ExKeyT bindingTarget, LocalBindingInfo bInfo, OSCollection<T> pathListeners)
        {
            //System.Diagnostics.Debug.Assert(sourceType != null, "The SourceType should never be null here.");

            //// One node:    1 parent, 0 intervening objects, the terminal path element is "." -- The binding binds to the source data itself.
            //// Two nodes:   1 parent, 0 intervening objects, 1 terminal path element.
            //// Three nodes: 1 parent, 1 intervening object, 1 terminal path element.
            //// Four nodes:  1 parent, 2 intervening objects, 1 terminal path element.

            //// The next to last node, must have data, in order to avoid binding warnings.
            //ObservableSource<T> lastParent = pathListeners[pathListeners.Count - 2];
            //string strNewPath = pathListeners.GetNewPath(justForDiag: false);
            //ObservableSourceStatusEnum lastParentStatus = lastParent.Status;
            //System.Diagnostics.Debug.WriteLine($"Path = {bInfo.PropertyPath.Path}, NewPath = {strNewPath}, " +
            //    $"Terminal Node Status = {lastParentStatus}.");

            //if (lastParentStatus.IsReadyOrWatching())
            //{
            //    // Check to see if DataContext holds source property.
            //    // When a DataContext is set via a binding, in some cases,
            //    // bindings that rely on that DataContext may get set
            //    // before the binding that provides the correct DataContext is set.

            //    ObservableSource<T> root = pathListeners[ROOT_INDEX];
            //    if (root.SourceKind == SourceKindEnum.FrameworkElement || root.SourceKind == SourceKindEnum.FrameworkContentElement)
            //    {
            //        string firstChildPathElement = pathListeners[ROOT_INDEX + 1].PathElement;
            //        if (!root.DoesChildExist(firstChildPathElement))
            //        {
            //            System.Diagnostics.Debug.WriteLine($"No Binding is being created. Data is present, but doesn't contain source property: {firstChildPathElement}.");
            //            isCustom = false;
            //            return null;
            //        }
            //    }

            //    // TODO: What about the original PropertyPath parameters?
            //    PropertyPath newPath = new PropertyPath(strNewPath);

            //    isCustom = lastParent.IsPropBagBased;
            //    return CreateBinding(bindingTarget, bInfo, sourceType, newPath, isCustom);
            //}
            //else
            //{
            //    System.Diagnostics.Debug.Assert(pathListeners[0].IsListeningForNewDC, "There are no active listeners!");

            //    System.Diagnostics.Debug.WriteLine("No Binding is being created, not enough data.");
            //    isCustom = false;
            //    return null;
            //}
        }

        protected virtual void CreateBinding(ExKeyT bindingTarget, LocalBindingInfo bInfo, string newPath)
        {
            //Binding result;

            //if (isPropBagBased)
            //{
            //    IValueConverter converter = GetConverter(bindingTarget, bInfo, sourceType,
            //        newPath.Path, isPropBagBased, out object converterParameter);

            //    result = new Binding
            //    {
            //        Path = newPath,
            //        Converter = converter,
            //        ConverterParameter = converterParameter,
            //    };
            //    ApplyStandardBindingParams(bInfo, ref result);
            //}
            //else
            //{
            //    result = CreateDefaultBinding(bindingTarget, bInfo, sourceType, newPath);
            //}

            //return result;
        }

        //protected virtual void ApplyStandardBindingParams(MyBindingInfo bInfo, ref Binding binding)
        //{
        //    if (bInfo.ElementName != null) binding.ElementName = bInfo.ElementName;
        //    if (bInfo.RelativeSource != null) binding.RelativeSource = bInfo.RelativeSource;
        //    if (bInfo.Source != null) binding.Source = bInfo.Source;

        //    binding.BindingGroupName = bInfo.BindingGroupName;
        //    binding.BindsDirectlyToSource = bInfo.BindsDirectlyToSource;
        //    binding.Delay = bInfo.Delay;

        //    binding.FallbackValue = bInfo.FallBackValue;

        //    binding.NotifyOnSourceUpdated = bInfo.NotifyOnSourceUpdated;
        //    binding.NotifyOnTargetUpdated = bInfo.NotifyOnTargetUpdated;
        //    binding.NotifyOnValidationError = bInfo.NotifyOnValidationError;

        //    binding.StringFormat = bInfo.StringFormat;
        //    binding.TargetNullValue = bInfo.TargetNullValue;

        //    binding.UpdateSourceExceptionFilter = bInfo.UpdateSourceExceptionFilter;

        //    binding.UpdateSourceTrigger = bInfo.UpdateSourceTrigger;
        //    binding.ValidatesOnDataErrors = bInfo.ValidatesOnDataErrors;
        //    binding.ValidatesOnExceptions = bInfo.ValidatesOnExceptions;
        //    binding.ValidatesOnNotifyDataErrors = bInfo.ValidatesOnNotifyDataErrors;

        //    binding.XPath = bInfo.XPath;
        //}

        #endregion

        #region Binding Operation Support

        //protected virtual BindingBase GetTheBindingBase(DependencyObject depObj, DependencyProperty targetProperty)
        //{
        //    return BindingOperations.GetBindingBase(depObj, targetProperty);
        //}

        //protected virtual BindingExpressionBase GetTheBindingExpressionBase(DependencyObject depObj, DependencyProperty targetProperty)
        //{
        //    BindingExpressionBase bExpBase = BindingOperations.GetBindingExpressionBase(depObj, targetProperty);
        //    return bExpBase;
        //}

        //protected virtual BindingExpressionBase SetTheBinding(DependencyObject targetObject, DependencyProperty targetProperty, BindingBase bb)
        //{
        //    BindingExpressionBase bExp = BindingOperations.SetBinding(targetObject, targetProperty, bb);
        //    return bExp;
        //}

        //protected virtual void ClearTheBinding(DependencyObject depObj, DependencyProperty targetProperty)
        //{
        //    BindingOperations.ClearBinding(depObj, targetProperty);
        //}

        #endregion

        #region Data Source Changed Handler

        private void PropertyChangedWithTVals(object sender, PCTypedEventArgs<T> e)
        {
            System.Diagnostics.Debug.WriteLine("One of our data sources has changed");
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
