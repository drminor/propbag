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
    using OSList = List<ObservableSource>;

    public class MyBindingEngine
    {
        #region Member Declarations

        /// <summary>
        /// This binding implementation requires that the type of the property
        /// for the binding source be specified.
        /// It may be possible to include a facility for PropBags that mirrors
        /// the functionality provided by System.Reflection so that this 
        /// is not neccessary.
        /// </summary>
        Type _sourceType;

        MyBindingInfo _bindingInfo;

        DependencyObject _targetObject;
        DependencyProperty _targetProperty;

        /// <summary>
        /// Used to listen to changes to the sources of data for each step in the path.
        /// </summary>
        OSList _dataSourceChangeListeners;

        private const string ROOT_PATH_ELEMENT = "root";
        private const int ROOT_INDEX = 0;

        private const string DEFAULT_BINDER_NAME = "pbb:Binder";

        #endregion

        #region Public Properties

        public Lazy<IValueConverter> DefaultConverter { get; set; }
        public Func<MyBindingInfo, Type, Type, object> DefaultConverterParameterBuilder { get; set; }
        public string BinderName { get; set; }

        #endregion

        #region Constructor

        public MyBindingEngine(MyBindingInfo bindingInfo, Type sourceType, DependencyObject targetObject, DependencyProperty targetProperty, string binderInstanceName = DEFAULT_BINDER_NAME)
        {
            _bindingInfo = bindingInfo;
            _sourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            _targetObject = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
            _targetProperty = targetProperty ?? throw new ArgumentNullException(nameof(targetProperty));

            BinderName = binderInstanceName;

            _dataSourceChangeListeners = null;
        }

        #endregion

        #region Provide Value

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if(SourceIsDependencyObject(_bindingInfo))
            {
                return GetStandardMultiConverterExp(serviceProvider, _targetObject, _targetProperty, _sourceType, _bindingInfo);
            }

            SetDefaultConverter();

            _dataSourceChangeListeners = PreparePathListeners(_bindingInfo, _sourceType);

            InitPathListeners(_dataSourceChangeListeners,_targetObject, _bindingInfo);

            Binding binding = GetTheBinding(_targetObject, _targetProperty, _dataSourceChangeListeners,
                _sourceType, _bindingInfo, out bool isCustom);

            if(binding != null)
            { 
                try
                {
                    BindingExpressionBase bExp =
                        BindingOperations.SetBinding(_targetObject, _targetProperty, binding);

                    string bType = isCustom ? "PropBag-Based" : "Standard";
                    System.Diagnostics.Debug.WriteLine($"CREATING {bType} BINDING from {binding.Path.Path} to {_targetProperty.Name} on object: {GetNameFromDepObject(_targetObject)}.");
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

        private MultiBindingExpression GetStandardMultiConverterExp(IServiceProvider serviceProvider, 
            DependencyObject targetObject, DependencyProperty targetProperty, Type sourceType, MyBindingInfo bInfo)
        {
            // create wpf binding
            MyMultiValueConverter mValueConverter = new MyMultiValueConverter(_bindingInfo.Mode);

            // TODO: Check This. If we do not set a default Converter will WPF provide a default implementation?
            SetDefaultConverter();

            Binding binding = CreateDefaultBinding(bInfo.PropertyPath, bInfo, sourceType, targetProperty.PropertyType);

            return WrapBindingInMultiValueConverter(serviceProvider, binding, binding.Mode);
        }

        private MultiBindingExpression WrapBindingInMultiValueConverter(IServiceProvider serviceProvider,
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

        private string GetNameFromDepObject(DependencyObject depObj)
        {
            if(depObj is FrameworkElement fe)
            {
                return fe.Name;
            }
            else if(depObj is FrameworkContentElement fce)
            {
                return $"(fce:) {fce.Name}";
            }
            else
            {
                return $"{depObj.ToString()}";
            }
        }

        private void SetDefaultConverter()
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

        private object OurDefaultConverterParameterBuilder(MyBindingInfo bInfo, Type sourceType, Type propertyType)
        {
            return new TwoTypes(sourceType, propertyType);
        }

        private bool SourceIsDependencyObject(MyBindingInfo bInfo)
        {
            return (
                bInfo.RelativeSource != null ||
                bInfo.ElementName != null ||
                bInfo.Source is DependencyObject ||
                bInfo.Source is DataSourceProvider && bInfo.BindsDirectlyToSource);
        }

        #endregion

        #region Path Analysis

        private bool InitPathListeners(OSList pathListeners,
            DependencyObject targetObject, MyBindingInfo bInfo)
        {
            DataSourceChangedEventArgs dsChangedEventArgs = new DataSourceChangedEventArgs(
                DataSourceChangeTypeEnum.Initializing);

            return RefreshPathListeners(dsChangedEventArgs, pathListeners, 
                pathListeners[ROOT_INDEX], targetObject, bInfo);
        }

        private bool RefreshPathListeners(DataSourceChangedEventArgs changeInfo, 
            OSList pathListeners, ObservableSource signalingOs,
            DependencyObject targetObject, MyBindingInfo bInfo)
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

                ObservableSourceProvider osp = GetSourceRoot(targetObject, bInfo.Source, ROOT_PATH_ELEMENT);

                if(osp == null)
                {
                    throw new InvalidOperationException($"{BinderName} could not locate a data source.");
                }

                status = ReplaceListener(pathListeners, ROOT_INDEX, ref osp, this.DataSourceHasChanged,
                    out parentOs);

                if(parentOs?.IsListeningForNewDC != true)
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

                if(status.NoLongerReady(originalStatus))
                {
                    ResetPathListeners(pathListeners, 1);

                    bindingInfoChanged = true;
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

                        ResetPathListeners(pathListeners, ++nodeIndex);

                        bindingInfoChanged = true;
                        return bindingInfoChanged;
                    }

                    ObservableSourceStatusEnum newCStatus =
                        ReplaceListener(pathListeners, nodeIndex, ref newChildProvider,
                        this.DataSourceHasChanged, out ObservableSource newChild);

                    if(newChild != null)
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

                status = ReplaceListener(pathListeners, nPtr, ref osp, this.DataSourceHasChanged,
                    out ObservableSource os);

                if(os != null)
                {
                    string newPathElement = GetNewPathElement(pathElement, os.Type, parentOs.IsPropBagBased);
                    os.NewPathElement = newPathElement;

                    bindingInfoChanged = (newPathElement != prevValueForNewPathElement);
                }
                // Note: if os is null, status will be "NoType", i.e. not ready.

                if (status == ObservableSourceStatusEnum.Ready)
                {
                    //resolvedOsCount++;
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
                ResetPathListeners(pathListeners, nPtr);
            }

            return bindingInfoChanged;
        }

        private void ResetPathListeners(OSList pathListeners, int startIndex)
        {
            // Note: Resetting the TerminalNode is not supported, and never needed.
            for (int nPtr = startIndex; nPtr < pathListeners.Count - 1; nPtr++)
            {
                pathListeners[nPtr].Reset(this.DataSourceHasChanged);
            }
        }

        //private bool IsUpdatable(ObservableSource pathListener)
        //{
        //    return pathListener.IsListening && pathListener.SourceKind == SourceKindEnum.DataSourceProvider || pathListener.SourceKind == SourceKindEnum.DataContext;
        //}

        private OSList PreparePathListeners(MyBindingInfo bInfo, Type sourceType)
        {
            OSList result = new OSList
            {
                // Add node to hold the Source Root.
                new ObservableSource(ROOT_PATH_ELEMENT)
            };

            string[] nodes = GetPathComponents(bInfo.PropertyPath, out int compCount);

            // Add the intervening nodes if any.
            for (int nPtr = 0; nPtr < compCount - 1; nPtr++)
            { 
                result.Add(new ObservableSource(nodes[nPtr]));
            }

            if(compCount == 0)
            {
                // Path is empty, bind to the datasource.
                result.Add(new ObservableSource(string.Empty, sourceType));
            }
            else
            {
                // Add the terminal node.
                result.Add(new ObservableSource(nodes[compCount - 1], sourceType));
            }

            return result;
        }

        private string GetNewPathElement(string pathElement, Type nodeType, bool isParentAPropBag)
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

        private string GetNewPath(OSList pathListeners)
        {
            string result = null;

            for (int nPtr = 1; nPtr < pathListeners.Count; nPtr++)
            {
                if (result != null) result += ".";
                result += pathListeners[nPtr].NewPathElement;
            }

            return result ?? ".";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathListeners"></param>
        /// <param name="index"></param>
        /// <param name="newListenerProvider"></param>
        /// <param name="subscriber"></param>
        /// <param name="newListener"></param>
        /// <returns>True if the path listener is ready to start listening to property and/or collection change events.</returns>
        private ObservableSourceStatusEnum ReplaceListener(OSList pathListeners, int index, 
            ref ObservableSourceProvider newListenerProvider, DataSourceChangedEventHandler subscriber,
            out ObservableSource newListener)
        {
            if(index == pathListeners.Count - 1)
            {
                System.Diagnostics.Debug.Assert(index != pathListeners.Count - 1,
                    "We are replacing the terminal listener. This is not good.");
            }

            ObservableSource oldListener = pathListeners[index];

            if(newListenerProvider != null)
            {
                newListener = newListenerProvider.CreateObservableSource(); newListenerProvider = null;
            }
            else
            {
                newListener = null;
            }

            bool wasListening = oldListener?.IsListeningForNewDC == true;

            if (newListener?.IsListeningForNewDC != true)
            {
                // The new ObservableSource is not responding to any change events.
                if(index == 0)
                {
                    System.Diagnostics.Debug.WriteLine("The Source Root is not listening.");
                }
                else if(wasListening) 
                {
                    System.Diagnostics.Debug.WriteLine($"The ObservableSource at node {index} is no longer listening to any change events.");
                }
            }

            if (oldListener != null)
            {
#if DEBUG
                bool wasRemoved = oldListener.Unsubscribe(subscriber);
                System.Diagnostics.Debug.Assert(wasListening == wasRemoved, "Was Listening does not agree with WasRemoved.");

                // Remove PropertyChanged or CollectionChanged event handlers, if any.
                oldListener.Reset();
#else
                // Remove all event handlers.
                oldListener.Reset(subscriber);
#endif
            }

            if (newListener == null)
            {
                // It is the caller's responsibility to cleanup the dependent nodes.
                return ObservableSourceStatusEnum.NoType;
            }

            if (newListener.IsListeningForNewDC)
            {
                bool isListening = newListener.Subscribe(subscriber);
                System.Diagnostics.Debug.Assert(isListening, "Subscriber was already present, but should not have been.");
                isListening = newListener.IsListeningForNewDC;
            }

            // TODO: Although this (and PrepareListeners) is the only code that sets in item
            // in the _dataSourceChangeListeners,
            // we probably should encapsulate this list in a class.
            pathListeners[index] = newListener;

#if DEBUG
            bool hasType = newListener.GetHasTypeAndHasData(out bool hasData);
#endif

            // The new path listener is ready to start monitoring Property and/or Collection Changed events if 
            // it still has a reference to the object that will be monitored.
            return newListener.Status;
        }

        private string[] GetPathComponents(PropertyPath path, out int count)
        {
            string[] components = path.Path.Split('.');
            count = components.Length;
            return components;
        }

        #endregion

        #region Build Bindings

        private Binding GetTheBinding(DependencyObject targetObject, DependencyProperty targetProperty,
            OSList pathListeners, Type sourceType, MyBindingInfo bInfo, out bool isCustom)
        {
            System.Diagnostics.Debug.Assert(sourceType != null, "The SourceType should never be null here.");

            Type propertyType = targetProperty.PropertyType;

            //int readyPathListeners = 0; // GetNumResolvedOS2(pathListeners);

            // One node:    1 parent;
            // Two nodes:   1 parent, 1 intervening object
            // Three nodes: 1 parent, 2 intervening objects
            // The next to last node, must have data, in order to avoid binding warnings.
            ObservableSource lastParent = pathListeners[pathListeners.Count - 2];
            string strNewPath = GetNewPath(pathListeners);
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

        private Binding CreateBinding(PropertyPath newPath, MyBindingInfo bInfo, bool isPropBagBased, Type sourceType, Type propertyType)
        {
            Binding result;


            if (isPropBagBased)
            {
                IValueConverter converter = GetConverter(bInfo, isPropBagBased, sourceType, propertyType, out object converterParameter);

                result = new Binding
                {
                    Path = newPath,
                    Converter = converter,
                    ConverterParameter = converterParameter,
                };

                //System.Diagnostics.Debug.WriteLine("CREATING PropBag BINDING");
                //System.Diagnostics.Debug.Write($"From {newPath.Path} to... ");
            }
            else
            {
                result = CreateDefaultBinding(newPath, bInfo, sourceType, propertyType);

                //System.Diagnostics.Debug.WriteLine("CREATING standard BINDING");
                //System.Diagnostics.Debug.Write($"From {newPath.Path} to... ");
            }

            ApplyStandardBindingParams(ref result, bInfo);

            return result;
        }

        private Binding CreateDefaultBinding(PropertyPath newPath, MyBindingInfo bInfo, Type sourceType, Type propertyType)
        {

            Binding binding = new Binding
            {
                Path = newPath,
                Converter = bInfo.Converter,
                ConverterParameter = bInfo.ConverterParameterBuilder(bInfo, sourceType, propertyType),
                ConverterCulture = bInfo.ConverterCulture,
        };

            return binding;
        }

        private void ApplyStandardBindingParams(ref Binding binding, MyBindingInfo bInfo)
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

        //private bool IsThisNodePropBagBased(ObservableSource os)
        //{
        //    if (os == null)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Returning false for IsThisNodePropBagBased because the ObservableSource is null.");
        //        return false;
        //    }
        //    return os.IsPropBagBased;
        //}

        private IValueConverter GetConverter(MyBindingInfo bInfo, bool isPropBagBased,
            Type sourceType, Type propertyType, out object converterParameter)
        {

            if (isPropBagBased)
            {
                if (bInfo.Converter != null)
                {
                    converterParameter = bInfo.ConverterParameterBuilder(bInfo, sourceType, propertyType);
                    return bInfo.Converter;
                }
                else
                {
                    converterParameter = DefaultConverterParameterBuilder(bInfo, sourceType, propertyType);
                    return DefaultConverter.Value;
                }
            }
            else
            {
                converterParameter = bInfo.ConverterParameterBuilder(bInfo, sourceType, propertyType);
                return bInfo.Converter;
            }
        }

        #endregion

        #region Data Source Changed Handling

        private void DataSourceHasChanged(object sender, DataSourceChangedEventArgs e)
        {
            ObservableSource os = (ObservableSource)sender;

            bool bindingInfoChanged = RefreshPathListeners(e, _dataSourceChangeListeners, os, _targetObject, _bindingInfo);

            BindingBase oldBinding = BindingOperations.GetBindingBase(_targetObject, _targetProperty);
            bool hadBinding = oldBinding != null;

            if (!bindingInfoChanged && oldBinding != null)
            {
                return;
            }

            Binding newBinding = GetTheBinding(_targetObject, _targetProperty, _dataSourceChangeListeners,
                _sourceType, _bindingInfo, out bool isCustom);

            if (hadBinding)
            {
                BindingOperations.ClearBinding(_targetObject, _targetProperty);
            }

            if (newBinding != null)
            {
                try
                {
                    BindingExpressionBase bExp = BindingOperations.SetBinding(_targetObject,
                        _targetProperty, newBinding);

                    string bType = isCustom ? "PropBag-Based" : "Standard";
                    System.Diagnostics.Debug.WriteLine($"CREATING {bType} BINDING from {newBinding.Path.Path} to {_targetProperty.Name} on object: {GetNameFromDepObject(_targetObject)}.");
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

        #region GetSource Support

        private ObservableSourceProvider GetSourceRoot(DependencyObject targetObject,
            object source, string pathElement)
        {
            ObservableSourceProvider osp;

            if (source != null && GetSourceFromSource(source, pathElement, out osp))
            {
                return osp;
            }
            else
            {
                GetDefaultSource(targetObject, pathElement, out osp);
                return osp;
            }

        }

        private bool GetSourceFromSource(object source, string pathElement, out ObservableSourceProvider osp)
        {
            if (source is DataSourceProvider)
            {
                osp = new ObservableSourceProvider(source as DataSourceProvider, pathElement);
                return true;
            }
            else if (source is INotifyPropertyChanged)
            {
                osp = new ObservableSourceProvider(source as INotifyPropertyChanged, pathElement);
                return true;
            }
            else if (source is INotifyCollectionChanged)
            {
                osp = new ObservableSourceProvider(source as INotifyCollectionChanged, pathElement);
                return true;
            }
            else
            {
                osp = null;
                return false;
            }
        }

        private bool GetDefaultSource(DependencyObject targetObject, string pathElement, out ObservableSourceProvider osp)
        {
            //// Just for debug
            //if(targetObject is FrameworkElement feTest)
            //{
            //    if(feTest.Name == "TestGrid1")
            //    {
            //        System.Diagnostics.Debug.WriteLine("Received Grid TestGrid1");
            //    }
            //}
            //// End just for debug


            //if (targetObject is FrameworkElement fe)
            //{
            //    osp = new ObservableSourceProvider(fe, pathElement);
            //}
            //else if (targetObject is FrameworkContentElement fce)
            //{
            //    osp = new ObservableSourceProvider(fce, pathElement);
            //}
            //else
            //{
            //    throw new ApplicationException($"targetObject in {BinderName}.ObservableSourceProvider was neither a FrameworkElement or a FrameworkContentElement.");
            //}

            //return true;
            string fwElementName = GetNameFromDepObject(targetObject);
            System.Diagnostics.Debug.WriteLine($"Fetching DataContext from {fwElementName}.");

            object dc = LogicalTree.GetDataContext(targetObject, out DependencyObject foundNode);

            // Changed to use foundNode on 10/10/17 @ 12:15 PM.
            if (dc == null)
            {
                if (foundNode is FrameworkElement fe)
                {
                    osp = new ObservableSourceProvider(fe, pathElement);
                    return true;
                }
                else if (foundNode is FrameworkContentElement fce)
                {
                    osp = new ObservableSourceProvider(fce, pathElement);
                    return true;
                }
                else
                {
                    throw new ApplicationException($"Found node in {BinderName}.ObservableSourceProvider was neither a FrameworkElement or a FrameworkContentElement.");
                }
            }
            else
            {
                if (foundNode is FrameworkElement fe)
                {
                    osp = new ObservableSourceProvider(fe, pathElement);
                }
                else if (foundNode is FrameworkContentElement fce)
                {
                    osp = new ObservableSourceProvider(fce, pathElement);
                }
                else
                {
                    throw new ApplicationException($"Found node in {BinderName}.ObservableSourceProvider was neither a FrameworkElement or a FrameworkContentElement.");
                }

                return true;
            }
        }

        #endregion

    }

}
