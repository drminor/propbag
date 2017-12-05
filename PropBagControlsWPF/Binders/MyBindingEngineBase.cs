using DRM.PropBag.ControlModel;
using System;
using System.Windows;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF.Binders
{
    /// <summary>
    /// This binding implementation requires that the type of the property for the binding source be specified.
    /// It may be possible to include a facility for PropBags that mirrors the functionality provided by System.Reflection
    /// so that this is not neccessary.
    /// </summary>
    public abstract class MyBindingEngineBase
    {
        #region Member Declarations

        /// <summary>
        /// Used to listen to changes to the sources of data for each step in the path.
        /// </summary>
        protected OSCollection _dataSourceChangeListeners { get; private set; }

        protected const string ROOT_PATH_ELEMENT = "root";
        protected const int ROOT_INDEX = 0;
        protected const string DEFAULT_BINDER_NAME = "pbb:Binder";

        #endregion

        #region Public Properties

        public BindingTarget BindingTarget { get; protected set; }
        public MyBindingInfo BindingInfo { get; protected set; }
        public Type SourceType { get; protected set; }

        public string BinderName { get; protected set; }
        public bool UseMultiBinding { get; protected set; }

        Lazy<IValueConverter> _defaultConverter;
        public virtual Lazy<IValueConverter> DefaultConverter
        {
            get
            {
                if(_defaultConverter == null)
                {
                    return new Lazy<IValueConverter>(() => new PropValueConverter());
                }
                return _defaultConverter;
            }
            set
            {
                _defaultConverter = value;
            }
        }

        Func<BindingTarget, MyBindingInfo, Type, string, object> _defConvParamBuilder;
        public virtual Func<BindingTarget, MyBindingInfo, Type, string, object> DefaultConverterParameterBuilder
        {
            get
            {
                if (_defConvParamBuilder == null)
                {
                    return OurDefaultConverterParameterBuilder;
                }
                return _defConvParamBuilder;
            }
            set
            {
                _defConvParamBuilder = value;
            }
        }

        #endregion

        #region Constructor

        public MyBindingEngineBase(BindingTarget bindingTarget, MyBindingInfo bindingInfo, Type sourceType,
            bool useMultiBinding = true, string binderInstanceName = DEFAULT_BINDER_NAME)
        {
            BindingInfo = bindingInfo;
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            BindingTarget = bindingTarget ?? throw new ArgumentNullException(nameof(bindingTarget));

            UseMultiBinding = useMultiBinding;
            BinderName = binderInstanceName;

            _dataSourceChangeListeners = null;
        }

        #endregion

        #region Provide Value

        public virtual object ProvideValue(IServiceProvider serviceProvider)
        {
            if(SourceIsDependencyObject(BindingInfo))
            {
                if(BindingTarget.IsDependencyProperty)
                {
                    if (UseMultiBinding)
                        return ProvideStandardMultiBindingExp(serviceProvider, BindingTarget, BindingInfo, SourceType);
                    else
                        return ProvideStandardBindingExp(serviceProvider, BindingTarget, BindingInfo, SourceType);
                }
            }


            _dataSourceChangeListeners = PreparePathListeners(BindingTarget, BindingInfo, SourceType);
            InitPathListeners(BindingTarget, BindingInfo, SourceType, _dataSourceChangeListeners);

            Binding binding = CreateTheBinding(BindingTarget, BindingInfo, SourceType,
                _dataSourceChangeListeners, out bool isCustom);

            if (BindingTarget.IsDependencyProperty)
            {
                if(UseMultiBinding)
                {
                    BindingExpressionBase bExpBase = BuildMultiBindingExpression(serviceProvider, binding, isCustom);
                    return bExpBase;
                }
                else
                {
                    return binding;
                    //BindingExpressionBase bExpBase = BuildBindingExpressionBase(serviceProvider, binding, isCustom);
                    //return bExpBase;
                }

            }
            else if (BindingTarget.IsProperty) 
            {
                System.Diagnostics.Debug.WriteLine("Returning a binder -- the target property is a PropertyInfo.");
                return binding;
            }
            else
            {
                throw new InvalidOperationException("The Binding Target was neither a DataContext, a DependencyProperty or a System.Reflection.PropertyInfo. Cannot provide a binding or binding expression.");
            }
        }

        protected virtual object ProvideStandardBindingExp(IServiceProvider serviceProvider,
            BindingTarget bindingTarget, MyBindingInfo bInfo, Type sourceType)
        {
            Binding binding = CreateDefaultBinding(bindingTarget, bInfo, sourceType, bInfo.PropertyPath);
            return binding;

            //BindingExpressionBase bExp = BuildBindingExpressionBase(serviceProvider, binding, isCustom: false);

            //return bExp;
        }

        protected virtual MultiBindingExpression ProvideStandardMultiBindingExp(IServiceProvider serviceProvider,
            BindingTarget bindingTarget, MyBindingInfo bInfo,Type sourceType)
        {
            // create wpf binding
            MyMultiValueConverter mValueConverter = new MyMultiValueConverter(BindingInfo.Mode);

            Binding binding = CreateDefaultBinding(bindingTarget, bInfo, sourceType, bInfo.PropertyPath);

            // This also sets the binding.
            MultiBindingExpression mExp = BuildMultiBindingExpression(serviceProvider, binding, isCustom: false);

            return mExp;
            
            //// TODO: Should we SetTheBinding here? We need to test this.
            //return WrapBindingInMultiValueConverter(serviceProvider, binding, binding.Mode);
        }

        public Binding ProvideTheBindingDirectly()
        {
            _dataSourceChangeListeners = PreparePathListeners(BindingTarget, BindingInfo, SourceType);

            InitPathListeners(BindingTarget, BindingInfo, SourceType,
                _dataSourceChangeListeners);

            Binding binding = CreateTheBinding(BindingTarget, BindingInfo, SourceType,
                _dataSourceChangeListeners, out bool isCustom);

            return binding;
        }

        protected virtual BindingExpressionBase BuildBindingExpressionBase(IServiceProvider serviceProvider,
            Binding binding, bool isCustom)
        {
            if (binding != null)
            {
                try
                {
                    BindingExpressionBase bExp = SetTheBinding(BindingTarget.DependencyObject, BindingTarget.DependencyProperty, binding);
                    string bType = isCustom ? "PropBag-Based" : "Standard";
                    System.Diagnostics.Debug.WriteLine($"CREATING {bType} BINDING from {binding.Path.Path} to {BindingTarget.PropertyName} on object: {BindingTarget.ObjectName}.");
                    System.Diagnostics.Debug.WriteLine("This binding is the one that our MultiValueConverter is using.");
                    return bExp;
                }
                catch
                {
                    // System.Diagnostics.Debug.WriteLine("Suppressed exception thrown when setting the binding during call to Provide Value.");
                    // Ignore the exception, we don't really need to set the binding.
                    // TODO: Is there anyway to avoid getting to here?
                    System.Diagnostics.Debug.WriteLine("Attempt to SetBinding failed: Returning a null BindingExpressionBase.");
                    return null;
                }
            }
            else
            {
                binding = null;
                System.Diagnostics.Debug.WriteLine("No Binding was created: Returning a null BindingExpressionBase.");
                return null;
            }
        }

        protected virtual MultiBindingExpression BuildMultiBindingExpression(IServiceProvider serviceProvider,
            Binding binding, bool isCustom)
        {
            if (binding != null)
            {
                try
                {
                    BindingExpressionBase bExp = SetTheBinding(BindingTarget.DependencyObject, BindingTarget.DependencyProperty, binding);
                    string bType = isCustom ? "PropBag-Based" : "Standard";
                    System.Diagnostics.Debug.WriteLine($"CREATING {bType} BINDING from {binding.Path.Path} to {BindingTarget.PropertyName} on object: {BindingTarget.ObjectName}.");
                    System.Diagnostics.Debug.WriteLine("This binding is the one that our MultiValueConverter is using.");
                }
                catch
                {
                    // System.Diagnostics.Debug.WriteLine("Suppressed exception thrown when setting the binding during call to Provide Value.");
                    // Ignore the exception, we don't really need to set the binding.
                    // TODO: Is there anyway to avoid getting to here?
                    System.Diagnostics.Debug.WriteLine("Attempt to SetBinding failed: The MultiValueConverter will contain 0 child bindings.");
                    binding = null; // Set it to null, so that it will not be used.
                }
            }
            else
            {
                binding = null;
                System.Diagnostics.Debug.WriteLine("No Binding was created: The MultiValueConverter will contain 0 child bindings.");
            }

            BindingMode mode = binding?.Mode ?? BindingInfo.Mode;

            MultiBindingExpression exp = WrapBindingInMultiValueConverter(serviceProvider, binding, mode);
            return exp;
        }

        protected virtual MultiBindingExpression WrapBindingInMultiValueConverter(IServiceProvider serviceProvider,
            Binding binding, BindingMode mode)
        {
            // create wpf binding

            MyMultiValueConverter mValueConverter = new MyMultiValueConverter(mode);

            if (binding != null)
            {
                mValueConverter.Add(binding);
            }

            // return the expression provided by the multi-binding
            MultiBindingExpression exp = mValueConverter.GetMultiBindingExpression(serviceProvider);

            return exp;
        }

        protected virtual bool SourceIsDependencyObject(MyBindingInfo bInfo) => (
                bInfo.RelativeSource != null ||
                bInfo.ElementName != null ||
                bInfo.Source is DependencyObject ||
                bInfo.Source is DataSourceProvider && bInfo.BindsDirectlyToSource
                );

        //protected virtual void SetDefaultConverter()
        //{
        //    if (DefaultConverter == null)
        //    {
        //        if(DefaultConverterParameterBuilder != null)
        //        {
        //            throw new InvalidOperationException("The DefaultParameterBuilder has been given a value, but the DefaultConverter is unassigned.");
        //        }
        //        DefaultConverter = new Lazy<IValueConverter>(() => new PropValueConverter());
        //        DefaultConverterParameterBuilder = OurDefaultConverterParameterBuilder;
        //    }
        //}

        protected virtual IValueConverter GetConverter(BindingTarget bindingTarget, MyBindingInfo bInfo, Type sourceType,
            string pathElement, bool isPropBagBased, out object converterParameter)
        {

            if (isPropBagBased)
            {
                if (bInfo.Converter != null)
                {
                    // Use the one specified in the MarkUp
                    converterParameter = bInfo.ConverterParameterBuilder(bindingTarget, bInfo, sourceType, pathElement);
                    return bInfo.Converter;
                }
                else
                {
                    // Use the default for ItemsSource
                    if(bindingTarget.DependencyProperty.Name == "SelectedItem")
                    {
                        // If this is being used to supply a SelectedItem property.
                        converterParameter = null;
                        return new SelectedItemConverter();
                    }
                    else
                    {
                        // Use the default converter provided by the caller or if no default provide our standard PropValueConverter.
                        converterParameter = DefaultConverterParameterBuilder(bindingTarget, bInfo, sourceType, pathElement);
                        return DefaultConverter.Value;
                    }
                }
            }
            else
            {
                // If no converter specified and the target is SelectedItem,
                // use the SelectedItemConverter.
                if (bInfo.Converter == null && bindingTarget.DependencyProperty?.Name == "SelectedItem")
                {
                    converterParameter = null;
                    return new SelectedItemConverter();
                }
                else
                {
                    // TODO: Check this. I beleive that the Microsoft Binder supplies a default converter.

                    // If no converter specified in the markup, no converter will be used.
                    converterParameter = bInfo.ConverterParameterBuilder(bindingTarget, bInfo, sourceType, pathElement);
                    return bInfo.Converter;
                }
            }
        }

        protected virtual object OurDefaultConverterParameterBuilder(BindingTarget bindingTarget, MyBindingInfo bInfo, Type sourceType,
            string pathElement)
        {
            return new TwoTypes(sourceType, bindingTarget.PropertyType);
        }

        protected virtual ObservableSourceProvider GetSourceRoot(BindingTarget bindingTarget, object source,
            string pathElement = ROOT_PATH_ELEMENT, string binderName = DEFAULT_BINDER_NAME)
        {
            ObservableSourceProvider osp = ObservableSourceProvider.GetSourceRoot(bindingTarget, source, pathElement, binderName);
            return osp;
        }

        #endregion

        #region Path Analysis

        protected OSCollection PreparePathListeners(BindingTarget bindingTarget, MyBindingInfo bInfo, Type sourceType)
        {
            string[] nodes = GetPathComponents(bInfo.PropertyPath, out int compCount);

            return new OSCollection(ROOT_PATH_ELEMENT, nodes, sourceType, BinderName);
        }

        protected bool InitPathListeners(BindingTarget bindingTarget, MyBindingInfo bInfo, Type sourceType, OSCollection pathListeners)
        {
            DataSourceChangedEventArgs dsChangedEventArgs = new DataSourceChangedEventArgs(
                DataSourceChangeTypeEnum.Initializing, dataWasChanged:true);

            return RefreshPathListeners(bindingTarget, bInfo, sourceType, pathListeners,
                dsChangedEventArgs, pathListeners[ROOT_INDEX]);
        }

        protected bool RefreshPathListeners(BindingTarget bindingTarget, MyBindingInfo bInfo, Type sourceType, OSCollection pathListeners,
            DataSourceChangedEventArgs changeInfo, ObservableSource signalingOs)
        {
            // Assume that this operation will not require the binding to be updated,
            // until proven otherwise.
            bool bindingInfoChanged = false;

            int nodeIndex =_dataSourceChangeListeners.IndexOf(signalingOs);

            if (nodeIndex == -1)
            {
                throw new InvalidOperationException($"Could not get pointer to path element while processing " +
                    $"DataSourceChanged event for {BinderName} in PropBagControlsWPF.Binders.");
            }

            ObservableSource parentOs = pathListeners[nodeIndex];
            ObservableSourceStatusEnum status;

            // Initializing
            // Attempt to get a reference to the Source Root (Object with DataContext property or DataSourceProvider.)
            if (changeInfo.ChangeType == DataSourceChangeTypeEnum.Initializing)
            {
                System.Diagnostics.Debug.Assert(nodeIndex == ROOT_INDEX, $"The node index should refer to the ObservableSource for " +
                    $"the root when DataSourceChangeType = {nameof(DataSourceChangeTypeEnum.Initializing)}.");

                string rootElementName = GetRootPathElementName(pathListeners);
                ObservableSourceProvider osp = GetSourceRoot(bindingTarget, bInfo.Source, rootElementName, BinderName);

                if(osp == null)
                {
                    throw new InvalidOperationException($"{BinderName} could not locate a data source.");
                }

                status = pathListeners.ReplaceListener(ROOT_INDEX, ref osp, this.DataSourceHasChanged,
                    out parentOs);

                if (parentOs?.IsListeningForNewDC != true)
                {
                    throw new InvalidOperationException($"{BinderName} could not locate a data source.");
                }
            }

            // DataContextUpdated
            // Refresh the Source Root
            else if(changeInfo.ChangeType == DataSourceChangeTypeEnum.DataContextUpdated)
            {
                System.Diagnostics.Debug.Assert(nodeIndex == ROOT_INDEX, $"The node index should refer to the ObservableSource for" +
                    $" the root when DataSourceChangeType = {nameof(DataSourceChangeTypeEnum.DataContextUpdated)}.");

                status = parentOs.Status;

                pathListeners.ResetListeners(ROOT_INDEX + 1, this.DataSourceHasChanged);
                bindingInfoChanged = true;
            }

            // Handle Collection Change
            else if(changeInfo.ChangeType == DataSourceChangeTypeEnum.CollectionChanged)
            {
                return bindingInfoChanged; 
            }

            // Handle Property Changed
            else if(changeInfo.ChangeType == DataSourceChangeTypeEnum.PropertyChanged)
            {
                // The PropModel of the value of this node's child may have changed.
                // Replace the child with a new SourceListner, if appropriate
                // and then fall-through to normal processing, at signaled step + 2.

                string changedPropName = changeInfo.PropertyName;

                nodeIndex++;
                ObservableSource child = pathListeners[nodeIndex];
                bool matched =
                        changedPropName == child.NewPathElement
                    ||  changedPropName == "Item[]" && child.NewPathElement.StartsWith("[");

                if(!matched || !(nodeIndex < pathListeners.Count - 1))
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

                if(!child.Status.IsReadyOrWatching())
                {
                    string prevValueForNewPathElement = child.NewPathElement;
                    ObservableSourceProvider newChildProvider = parentOs.GetChild(child.PathElement);

                    if(newChildProvider?.Data == null)
                    {
                        // Still no data ??
                        System.Diagnostics.Debug.WriteLine("Child was null and is still null.");

                        //ResetPathListeners(pathListeners, nodeIndex);
                        pathListeners.ResetListeners(nodeIndex, this.DataSourceHasChanged);


                        bindingInfoChanged = true;
                        return bindingInfoChanged;
                    }

                    //ObservableSourceStatusEnum newCStatus =
                    //    ReplaceListener(pathListeners, nodeIndex, ref newChildProvider,
                    //    this.DataSourceHasChanged, out ObservableSource newChild);

                    ObservableSourceStatusEnum newCStatus = pathListeners.ReplaceListener(nodeIndex, ref newChildProvider,
                        this.DataSourceHasChanged, out ObservableSource newChild);

                    if (newChild != null)
                    {
                        string newPathElement = GetNewPathElement(newChild.PathElement, newChild.Type, parentOs.IsPropBagBased);
                        newChild.NewPathElement = newPathElement;

                        bindingInfoChanged = (newPathElement != prevValueForNewPathElement);
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
                    // Make sure we are listening to events that this ObservableSource may raise.
                    // We know that we are subscribed to the root.
                    parentOs.Subscribe(this.DataSourceHasChanged);
                }

                string pathElement = pathListeners[nPtr].PathElement;
                string prevValueForNewPathElement = pathListeners[nPtr].NewPathElement;

                ObservableSourceProvider osp = parentOs.GetChild(pathElement);

                //status = ReplaceListener(pathListeners, nPtr, ref osp, this.DataSourceHasChanged,
                //    out ObservableSource os);

                status = pathListeners.ReplaceListener(nPtr, ref osp, this.DataSourceHasChanged,
                    out ObservableSource os);

                if (os != null)
                {
                    string newPathElement = GetNewPathElement(pathElement, os.Type, parentOs.IsPropBagBased);
                    os.NewPathElement = newPathElement;

                    bindingInfoChanged = (newPathElement != prevValueForNewPathElement);
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
                ObservableSource lastNode = pathListeners[nPtr];
                if (status.IsReadyOrWatching())
                {
                    string newPathElement = GetNewPathElement(lastNode.PathElement, lastNode.Type, parentOs.IsPropBagBased);
                    if(lastNode.NewPathElement != newPathElement)
                    {
                        lastNode.NewPathElement = newPathElement;
                        bindingInfoChanged = true;
                    }
                    if(!status.IsWatching())
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

        protected virtual string GetRootPathElementName(OSCollection pathListeners)
        {
            return $"{ROOT_PATH_ELEMENT}+for+{pathListeners?[1]?.PathElement}.";
        }       

        protected virtual string GetNewPathElement(string pathElement, Type nodeType, bool isParentAPropBag)
        {
            string result;
            if (isParentAPropBag)
            {
                if (nodeType == null)
                {
                    nodeType = typeof(object);
                }

                result = $"[{nodeType.FullName},{pathElement}]";
            }
            else
            {
                result = pathElement;
            }
            return result;
        }

        protected virtual string[] GetPathComponents(PropertyPath path, out int count)
        {
            string[] components = path.Path.Split('.');
            count = components.Length;
            return components;
        }

        #endregion

        #region Build Bindings

        protected Binding CreateTheBinding(BindingTarget bindingTarget, MyBindingInfo bInfo, Type sourceType,
            OSCollection pathListeners, out bool isCustom)
        {
            System.Diagnostics.Debug.Assert(sourceType != null, "The SourceType should never be null here.");

            // One node:    1 parent, 0 intervening objects, the terminal path element is "." -- The binding binds to the source data itself.
            // Two nodes:   1 parent, 0 intervening objects, 1 terminal path element.
            // Three nodes: 1 parent, 1 intervening object, 1 terminal path element.
            // Four nodes:  1 parent, 2 intervening objects, 1 terminal path element.

            // The next to last node, must have data, in order to avoid binding warnings.
            ObservableSource lastParent = pathListeners[pathListeners.Count - 2];
            string strNewPath = pathListeners.GetNewPath(justForDiag:false);
            ObservableSourceStatusEnum lastParentStatus = lastParent.Status;
            System.Diagnostics.Debug.WriteLine($"Path = {bInfo.PropertyPath.Path}, NewPath = {strNewPath}, " +
                $"Terminal Node Status = {lastParentStatus}.");

            if (lastParentStatus.IsReadyOrWatching())
            {
                // Check to see if DataContext holds source property.
                // When a DataContext is set via a binding, in some cases,
                // bindings that rely on that DataContext may get set
                // before the binding that provides the correct DataContext is set.

                ObservableSource root = pathListeners[ROOT_INDEX];
                if (root.SourceKind == SourceKindEnum.FrameworkElement || root.SourceKind == SourceKindEnum.FrameworkContentElement)
                {
                    string firstChildPathElement = pathListeners[ROOT_INDEX + 1].PathElement;
                    if (!root.DoesChildExist(firstChildPathElement))
                    {
                        System.Diagnostics.Debug.WriteLine($"No Binding is being created. Data is present, but doesn't contain source property: {firstChildPathElement}.");
                        isCustom = false;
                        return null;
                    }
                }

                // TODO: What about the original PropertyPath parameters?
                PropertyPath newPath = new PropertyPath(strNewPath);

                isCustom = lastParent.IsPropBagBased;
                return CreateBinding(bindingTarget, bInfo, sourceType, newPath, isCustom);
            } 
            else
            {
                System.Diagnostics.Debug.Assert(pathListeners[0].IsListeningForNewDC, "There are no active listeners!");

                System.Diagnostics.Debug.WriteLine("No Binding is being created, not enough data.");
                isCustom = false;
                return null;
            }
        }

        protected virtual Binding CreateBinding(BindingTarget bindingTarget, MyBindingInfo bInfo, Type sourceType,
            PropertyPath newPath, bool isPropBagBased)
        {
            Binding result;

            if (isPropBagBased)
            {
                IValueConverter converter = GetConverter(bindingTarget, bInfo, sourceType, 
                    newPath.Path, isPropBagBased, out object converterParameter);

                result = new Binding
                {
                    Path = newPath,
                    Converter = converter,
                    ConverterParameter = converterParameter,
                };
                ApplyStandardBindingParams(bInfo, ref result);
            }
            else
            {
                result = CreateDefaultBinding(bindingTarget, bInfo, sourceType, newPath);
            }

            return result;
        }

        protected virtual Binding CreateDefaultBinding(BindingTarget bindingTarget, MyBindingInfo bInfo, Type sourceType,
            PropertyPath newPath)
        {
            IValueConverter converter = GetConverter(bindingTarget, bInfo, sourceType,
                newPath.Path, false, out object converterParameter);

            Binding binding = new Binding
            {
                Path = newPath,
                Converter = converter,
                ConverterParameter = converterParameter
                //ConverterCulture = bInfo.ConverterCulture,
            };
            ApplyStandardBindingParams(bInfo, ref binding);

            return binding;
        }

        protected virtual void ApplyStandardBindingParams(MyBindingInfo bInfo, ref Binding binding)
        {
            if (bInfo.ElementName != null) binding.ElementName = bInfo.ElementName;
            if (bInfo.RelativeSource != null) binding.RelativeSource = bInfo.RelativeSource;
            if (bInfo.Source != null) binding.Source = bInfo.Source;

            binding.BindingGroupName = bInfo.BindingGroupName;
            binding.BindsDirectlyToSource = bInfo.BindsDirectlyToSource;
            binding.Delay = bInfo.Delay;

            binding.FallbackValue = bInfo.FallBackValue;

            binding.NotifyOnSourceUpdated = bInfo.NotifyOnSourceUpdated;
            binding.NotifyOnTargetUpdated = bInfo.NotifyOnTargetUpdated;
            binding.NotifyOnValidationError = bInfo.NotifyOnValidationError;

            binding.StringFormat = bInfo.StringFormat;
            binding.TargetNullValue = bInfo.TargetNullValue;

            binding.UpdateSourceExceptionFilter = bInfo.UpdateSourceExceptionFilter;

            binding.UpdateSourceTrigger = bInfo.UpdateSourceTrigger;
            binding.ValidatesOnDataErrors = bInfo.ValidatesOnDataErrors;
            binding.ValidatesOnExceptions = bInfo.ValidatesOnExceptions;
            binding.ValidatesOnNotifyDataErrors = bInfo.ValidatesOnNotifyDataErrors;

            binding.XPath = bInfo.XPath;
        }

        #endregion

        #region Binding Operation Support

        protected virtual BindingBase GetTheBindingBase(DependencyObject depObj, DependencyProperty targetProperty)
        {
            return BindingOperations.GetBindingBase(depObj, targetProperty);
        }

        protected virtual BindingExpressionBase GetTheBindingExpressionBase(DependencyObject depObj, DependencyProperty targetProperty)
        {
            BindingExpressionBase bExpBase = BindingOperations.GetBindingExpressionBase(depObj, targetProperty);
            return bExpBase;
        }

        protected virtual BindingExpressionBase SetTheBinding(DependencyObject targetObject, DependencyProperty targetProperty, BindingBase bb)
        {
            BindingExpressionBase bExp = BindingOperations.SetBinding(targetObject, targetProperty, bb);
            return bExp;
        }

        protected virtual void ClearTheBinding(DependencyObject depObj, DependencyProperty targetProperty)
        {
            BindingOperations.ClearBinding(depObj, targetProperty);
        }

        #endregion

        #region Data Source Changed Handler

        protected virtual void DataSourceHasChanged(object sender, DataSourceChangedEventArgs e)
        {
            BindingBase oldBinding;
            if (BindingTarget.IsDependencyProperty)
            {
                oldBinding = GetTheBindingBase(BindingTarget.DependencyObject, BindingTarget.DependencyProperty);
            }
            else
            {
                oldBinding = null;
            }

            bool hadBinding = oldBinding != null;

            if(!e.DataWasUpdated && oldBinding != null)
            {
                return;
            }

            ObservableSource os = (ObservableSource)sender;
            bool bindingInfoChanged = RefreshPathListeners(BindingTarget, BindingInfo, SourceType,
                _dataSourceChangeListeners, e, os);

            if (!bindingInfoChanged && oldBinding != null)
            {
                return;
            }

            Binding newBinding = CreateTheBinding(BindingTarget, BindingInfo, SourceType, 
                _dataSourceChangeListeners, out bool isCustom);

            if (hadBinding && BindingTarget.IsDependencyProperty)
            {
                ClearTheBinding(BindingTarget.DependencyObject, BindingTarget.DependencyProperty);
            }

            if (newBinding != null)
            {
                try
                {

                    //BindingExpressionBase bExp = BindingOperations.SetBinding(_targetObject,
                    //    _targetProperty, newBinding);

                    if(BindingTarget.IsDependencyProperty)
                    {
                        ClearTheBinding(BindingTarget.DependencyObject, BindingTarget.DependencyProperty);
                        BindingExpressionBase bExp = SetTheBinding(BindingTarget.DependencyObject, BindingTarget.DependencyProperty, newBinding);
                        string bType = isCustom ? "PropBag-Based" : "Standard";
                        System.Diagnostics.Debug.WriteLine($"CREATING {bType} BINDING from {newBinding.Path.Path} to {BindingTarget.PropertyName} on object: {BindingTarget.ObjectName}.");
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Attempt to SetBinding failed.");
                }

                if (hadBinding)
                {
                    System.Diagnostics.Debug.WriteLine($"{BinderName} set a new binding on some target that is replacing an existing binding.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"{BinderName} set a new binding on some target that had no binding previously.");
                }
            }
            else
            {
                if (hadBinding)
                {
                    System.Diagnostics.Debug.WriteLine($"{BinderName} removed binding on some target and replaced it with no binding.");
                }
            }
        }

        #endregion

    }

}
