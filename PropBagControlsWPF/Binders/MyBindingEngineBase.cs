using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF.WPFHelpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public abstract class MyBindingEngineBase
    {
        #region Member Declarations

        /// <summary>
        /// This binding implementation requires that the type of the property
        /// for the binding source be specified.
        /// It may be possible to include a facility for PropBags that mirrors
        /// the functionality provided by System.Reflection so that this 
        /// is not neccessary.
        /// </summary>
        protected Type _sourceType { get; set; }

        protected MyBindingInfo _bindingInfo { get; set; }

        //protected DependencyObject _targetObject { get; set; }
        //protected object _targetProperty { get; set; }
        protected BindingTarget _bindingTarget { get; set; }

        /// <summary>
        /// Used to listen to changes to the sources of data for each step in the path.
        /// </summary>
        protected OSCollection _dataSourceChangeListeners { get; private set; }

        protected const string ROOT_PATH_ELEMENT = "root";
        protected const int ROOT_INDEX = 0;
        protected const string DEFAULT_BINDER_NAME = "pbb:Binder";

        #endregion

        #region Public Properties

        public Lazy<IValueConverter> DefaultConverter { get; set; }
        public Func<MyBindingInfo, string, Type, Type, object> DefaultConverterParameterBuilder { get; set; }
        public string BinderName { get; set; }

        #endregion

        #region Constructor

        public MyBindingEngineBase(MyBindingInfo bindingInfo, Type sourceType, BindingTarget bindingTarget, string binderInstanceName = DEFAULT_BINDER_NAME)
        {
            _bindingInfo = bindingInfo;
            _sourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            _bindingTarget = bindingTarget ?? throw new ArgumentNullException(nameof(bindingTarget));

            BinderName = binderInstanceName;

            _dataSourceChangeListeners = null;
        }

        #endregion

        #region Provide Value

        public virtual object ProvideValue(IServiceProvider serviceProvider)
        {
            if(SourceIsDependencyObject(_bindingInfo))
            {
                if(_bindingTarget.IsDependencyProperty)
                {
                    // TODO: Consider simply creating and setting a standard Binding.
                    return ProvideStandardMultiBindingExp(serviceProvider, _bindingTarget, _sourceType, _bindingInfo);
                }
            }

            SetDefaultConverter();

            _dataSourceChangeListeners = PreparePathListeners(_bindingInfo, _sourceType);

            InitPathListeners(_dataSourceChangeListeners, _bindingTarget, _bindingInfo);

            Binding binding = CreateTheBinding(_bindingTarget.DependencyObject, _bindingTarget.PropertyType, _dataSourceChangeListeners,
                _sourceType, _bindingInfo, out bool isCustom);

            if (_bindingTarget.IsDependencyProperty)
            {
                MultiBindingExpression exp = BuildMultiBindingExpression(serviceProvider, binding, isCustom);
                return exp;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Returning a binder -- the target property is a PropertyInfo.");
                return binding;
            }
        }

        public Binding ProvideTheBindingDirectly()
        {
            SetDefaultConverter();

            _dataSourceChangeListeners = PreparePathListeners(_bindingInfo, _sourceType);

            InitPathListeners(_dataSourceChangeListeners, _bindingTarget, _bindingInfo);

            Binding binding = CreateTheBinding(_bindingTarget.DependencyObject, _bindingTarget.PropertyType, _dataSourceChangeListeners,
                _sourceType, _bindingInfo, out bool isCustom);

            return binding;
        }

        protected virtual MultiBindingExpression BuildMultiBindingExpression(IServiceProvider serviceProvider,
            Binding binding, bool isCustom)
        {
            if (binding != null)
            {
                try
                {
                    BindingExpressionBase bExp = SetTheBinding(_bindingTarget.DependencyObject, _bindingTarget.DependencyProperty, binding);
                    string bType = isCustom ? "PropBag-Based" : "Standard";
                    System.Diagnostics.Debug.WriteLine($"CREATING {bType} BINDING from {binding.Path.Path} to {_bindingTarget.PropertyName} on object: {_bindingTarget.ObjectName}.");
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

            BindingMode mode = binding?.Mode ?? _bindingInfo.Mode;

            MultiBindingExpression exp = WrapBindingInMultiValueConverter(serviceProvider, binding, mode);
            return exp;
        }

        protected virtual BindingExpression ProvideStandardBindingExp(IServiceProvider serviceProvider,
            BindingTarget bindingTarget, Type sourceType, MyBindingInfo bInfo)

        {
            throw new NotImplementedException();
        }


        protected virtual MultiBindingExpression ProvideStandardMultiBindingExp(IServiceProvider serviceProvider, 
            BindingTarget bindingTarget, Type sourceType, MyBindingInfo bInfo)
        {
            // create wpf binding
            MyMultiValueConverter mValueConverter = new MyMultiValueConverter(_bindingInfo.Mode);

            // TODO: Check This. If we do not set a default Converter will WPF provide a default implementation?
            SetDefaultConverter();

            Binding binding = CreateDefaultBinding(bInfo.PropertyPath, bInfo, sourceType, bindingTarget.PropertyType);

            // TODO: Should we SetTheBinding here? We need to test this.

            return WrapBindingInMultiValueConverter(serviceProvider, binding, binding.Mode);
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

        protected virtual void SetDefaultConverter()
        {
            if (DefaultConverter == null)
            {
                if(DefaultConverterParameterBuilder != null)
                {
                    throw new InvalidOperationException("The DefaultParameterBuilder has been given a value, but the DefaultConverter is unassigned.");
                }
                DefaultConverter = new Lazy<IValueConverter>(() => new PropValueConverter());
                DefaultConverterParameterBuilder = OurDefaultConverterParameterBuilder;
            }
        }

        protected virtual IValueConverter GetConverter(MyBindingInfo bInfo, string pathElement, bool isPropBagBased,
            Type sourceType, Type propertyType, out object converterParameter)
        {

            if (isPropBagBased)
            {
                if (bInfo.Converter != null)
                {
                    converterParameter = bInfo.ConverterParameterBuilder(bInfo, pathElement, sourceType, propertyType);
                    return bInfo.Converter;
                }
                else
                {
                    converterParameter = DefaultConverterParameterBuilder(bInfo, pathElement, sourceType, propertyType);
                    return DefaultConverter.Value;
                }
            }
            else
            {
                converterParameter = bInfo.ConverterParameterBuilder(bInfo, pathElement, sourceType, propertyType);
                return bInfo.Converter;
            }
        }

        protected virtual object OurDefaultConverterParameterBuilder(MyBindingInfo bInfo, string pathElement, Type sourceType, Type propertyType)
        {
            return new TwoTypes(sourceType, propertyType);
        }

        protected virtual ObservableSourceProvider GetSourceRoot(BindingTarget bindingTarget, object source, string binderName = DEFAULT_BINDER_NAME, string pathElement = ROOT_PATH_ELEMENT)
        {
            ObservableSourceProvider osp = ObservableSourceProvider.GetSourceRoot(bindingTarget, source, pathElement, binderName);
            return osp;
        }

        protected virtual bool SourceIsDependencyObject(MyBindingInfo bInfo)
        {
            return (
                bInfo.RelativeSource != null ||
                bInfo.ElementName != null ||
                bInfo.Source is DependencyObject ||
                bInfo.Source is DataSourceProvider && bInfo.BindsDirectlyToSource);
        }

        #endregion

        #region Path Analysis

        protected OSCollection PreparePathListeners(MyBindingInfo bInfo, Type sourceType)
        {
            string[] nodes = GetPathComponents(bInfo.PropertyPath, out int compCount);

            return new OSCollection(ROOT_PATH_ELEMENT, nodes, sourceType, BinderName);
        }

        protected bool InitPathListeners(OSCollection pathListeners, BindingTarget bindingTarget, MyBindingInfo bInfo)
        {
            DataSourceChangedEventArgs dsChangedEventArgs = new DataSourceChangedEventArgs(
                DataSourceChangeTypeEnum.Initializing);

            return RefreshPathListeners(dsChangedEventArgs, pathListeners, 
                pathListeners[ROOT_INDEX], bindingTarget, bInfo);
        }

        protected bool RefreshPathListeners(DataSourceChangedEventArgs changeInfo, 
            OSCollection pathListeners, ObservableSource signalingOs,
            BindingTarget bindingTarget, MyBindingInfo bInfo)
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

                ObservableSourceProvider osp = GetSourceRoot(bindingTarget, bInfo.Source, BinderName);

                if(osp == null)
                {
                    throw new InvalidOperationException($"{BinderName} could not locate a data source.");
                }

                //status = ReplaceListener(pathListeners, ROOT_INDEX, ref osp, this.DataSourceHasChanged,
                //    out parentOs);

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

                ObservableSourceStatusEnum originalStatus = parentOs.Status;
                // Ask the root to remove its handlers, if any, from the DataContext just replaced.
                parentOs.StopListeningToSource();

                // Ask the root to begin listening to its new DataContext, or its new Data provided by its DataSourceProvider.
                parentOs.UpdateData(BinderName);

                status = parentOs.Status;
                bindingInfoChanged = true;

                if (status.NoLongerReady(originalStatus))
                {
                    pathListeners.ResetListeners(1, this.DataSourceHasChanged);
                    //ResetPathListeners(pathListeners, 1);
                    return bindingInfoChanged;
                }
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

        protected Binding CreateTheBinding(DependencyObject targetObject, Type propertyType,
            OSCollection pathListeners, Type sourceType, MyBindingInfo bInfo, out bool isCustom)
        {
            System.Diagnostics.Debug.Assert(sourceType != null, "The SourceType should never be null here.");

            // One node:    1 parent;
            // Two nodes:   1 parent, 1 intervening object
            // Three nodes: 1 parent, 2 intervening objects
            // The next to last node, must have data, in order to avoid binding warnings.
            ObservableSource lastParent = pathListeners[pathListeners.Count - 2];
            string strNewPath = pathListeners.NewPath;
            ObservableSourceStatusEnum lastParentStatus = lastParent.Status;
            System.Diagnostics.Debug.WriteLine($"Path = {bInfo.PropertyPath.Path}, NewPath = {strNewPath}, " +
                $"Terminal Node Status = {lastParentStatus}.");

            if (lastParentStatus.IsReadyOrWatching())
            {
                // TODO: What about the original PropertyPath parameters?
                PropertyPath newPath = new PropertyPath(strNewPath);

                isCustom = lastParent.IsPropBagBased;
                return CreateBinding(newPath, bInfo, isCustom, sourceType, propertyType);
            } 
            else
            {
                System.Diagnostics.Debug.Assert(pathListeners[0].IsListeningForNewDC, "There are no active listeners!");

                System.Diagnostics.Debug.WriteLine("No Binding is being created, not enough data.");
                isCustom = false;
                return null;
            }
        }

        protected virtual Binding CreateBinding(PropertyPath newPath, MyBindingInfo bInfo, bool isPropBagBased, Type sourceType, Type propertyType)
        {
            Binding result;

            if (isPropBagBased)
            {
                IValueConverter converter = GetConverter(bInfo, newPath.Path, isPropBagBased, sourceType, propertyType, out object converterParameter);

                result = new Binding
                {
                    Path = newPath,
                    Converter = converter,
                    ConverterParameter = converterParameter,
                };
                ApplyStandardBindingParams(ref result, bInfo);
            }
            else
            {
                result = CreateDefaultBinding(newPath, bInfo, sourceType, propertyType);
            }

            return result;
        }

        protected virtual Binding CreateDefaultBinding(PropertyPath newPath, MyBindingInfo bInfo, Type sourceType, Type propertyType)
        {

            Binding binding = new Binding
            {
                Path = newPath,
                Converter = bInfo.Converter,
                ConverterParameter = bInfo.ConverterParameterBuilder(bInfo, newPath.Path, sourceType, propertyType),
                ConverterCulture = bInfo.ConverterCulture,
        };
            ApplyStandardBindingParams(ref binding, bInfo);
            return binding;
        }

        protected virtual void ApplyStandardBindingParams(ref Binding binding, MyBindingInfo bInfo)
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
            ObservableSource os = (ObservableSource)sender;

            bool bindingInfoChanged = RefreshPathListeners(e, _dataSourceChangeListeners, os, _bindingTarget, _bindingInfo);

            //BindingBase oldBinding = BindingOperations.GetBindingBase(_targetObject, _targetProperty);

            BindingBase oldBinding;
            if (_bindingTarget.IsDependencyProperty)
            {
                oldBinding = GetTheBindingBase(_bindingTarget.DependencyObject, _bindingTarget.DependencyProperty);
            }
            else
            {
                oldBinding = null;
            }

            bool hadBinding = oldBinding != null;

            if (!bindingInfoChanged && oldBinding != null)
            {
                return;
            }

            Binding newBinding = CreateTheBinding(_bindingTarget.DependencyObject, _bindingTarget.PropertyType, _dataSourceChangeListeners,
                _sourceType, _bindingInfo, out bool isCustom);

            if (hadBinding && _bindingTarget.IsDependencyProperty)
            {
                ClearTheBinding(_bindingTarget.DependencyObject, _bindingTarget.DependencyProperty);
            }

            if (newBinding != null)
            {
                try
                {

                    //BindingExpressionBase bExp = BindingOperations.SetBinding(_targetObject,
                    //    _targetProperty, newBinding);

                    if(_bindingTarget.IsDependencyProperty)
                    {
                        BindingExpressionBase bExp = SetTheBinding(_bindingTarget.DependencyObject, _bindingTarget.DependencyProperty, newBinding);
                        string bType = isCustom ? "PropBag-Based" : "Standard";
                        System.Diagnostics.Debug.WriteLine($"CREATING {bType} BINDING from {newBinding.Path.Path} to {_bindingTarget.PropertyName} on object: {_bindingTarget.ObjectName}.");
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
