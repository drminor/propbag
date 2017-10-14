using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF.Binders
{
    using OSList = List<MyBindingEngine.ObservableSource>;

    public class MyBindingEngine
    {
        #region Member Declarations

        MyBindingInfo _bindingInfo;
        Type _sourceType;
        DependencyObject _targetObject;
        DependencyProperty _targetProperty;

        /// <summary>
        /// Used to listen to changes to the sources of data for each step in the path.
        /// </summary>
        OSList _dataSourceChangeListeners;

        private const string ROOT_PATH_ELEMENT = "root";
        private const int ROOT_INDEX = 0;

        private const string DEFAULT_BINDER_NAME = "pbb:Binder";

        // Holds the number of data source listeners that are listening.
        int _resolvedOsCount;

        int ResolvedOsCount
        {
            get
            {
                int test = GetNumResolvedOS(_dataSourceChangeListeners);
                if (test != _resolvedOsCount)
                {
                    System.Diagnostics.Debug.WriteLine("Resolved OS count does not match calculated value, returning calculated value.");
                    return test;
                }
                return _resolvedOsCount;
            }
            set
            {
                _resolvedOsCount = value;
            }
        }

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
            _resolvedOsCount = 0;
        }

        #endregion

        #region Provide Value

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if(IsSourceADependencyObject(_bindingInfo))
            {
                return GetStandardMultiConverterExp(serviceProvider, _targetObject, _targetProperty, _sourceType, _bindingInfo);
            }

            SetDefaultConverter();

            _dataSourceChangeListeners = PreparePathListeners(_bindingInfo, _sourceType);

            _resolvedOsCount = InitPathListeners(_dataSourceChangeListeners,_targetObject, _bindingInfo);

            Binding binding;

            if (_dataSourceChangeListeners.Count > 1)
            {
                binding = GetTheBinding(_targetObject, _targetProperty, _dataSourceChangeListeners,
                    _sourceType, _bindingInfo);

                try
                {
                    BindingExpressionBase bExp =
                        BindingOperations.SetBinding(_targetObject, _targetProperty, binding);

                    System.Diagnostics.Debug.WriteLine($" {_targetProperty.Name} on object: {GetNameFromDepObject(_targetObject)}.");
                    System.Diagnostics.Debug.WriteLine("This binding is the one that our MultiValueConverter is using.");
                }
                catch
                {
                    // System.Diagnostics.Debug.WriteLine("Suppressed exception thrown when setting the binding during call to Provide Value.");
                    // Ignore the exception, we don't really need to set the binding.
                    // TODO: Is there anyway to avoid getting to here?
                    System.Diagnostics.Debug.WriteLine("Attempt to SetBinding failed. The MultiValueConverter will contain 0 child bindings.");
                    binding = null; // Set it to null, so that it will not be used.
                }
            }
            else
            {
                binding = null;
                System.Diagnostics.Debug.WriteLine("No Binding was created.");
            }

            BindingMode mode = binding?.Mode ?? _bindingInfo.Mode;

            MultiBindingExpression exp = WrapBindingInMultiValueConverter(serviceProvider, binding, mode);
            return exp;
        }

        private bool IsSourceADependencyObject(MyBindingInfo bInfo)            
        {
            return (
                bInfo.RelativeSource != null ||
                bInfo.ElementName != null ||
                bInfo.Source is DependencyObject);
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

        #endregion

        #region Path Analysis

        private int InitPathListeners(OSList pathListeners,
            DependencyObject targetObject, MyBindingInfo bInfo)
        {
            DataSourceChangedEventArgs dsChangedEventArgs = new DataSourceChangedEventArgs(
                DataSourceChangeTypeEnum.Initializing);

            return RefreshPathListeners(dsChangedEventArgs, pathListeners, 
                pathListeners[ROOT_INDEX], targetObject, bInfo);
        }

        private int RefreshPathListeners(DataSourceChangedEventArgs changeInfo, 
            OSList pathListeners, ObservableSource signalingOs,
            DependencyObject targetObject, MyBindingInfo bInfo)
        {
            int resolvedOsCount; 

            int stepNo =_dataSourceChangeListeners.IndexOf(signalingOs);

            if (stepNo == -1)
            {
                throw new InvalidOperationException($"Could not get step number on DataSourceChanged for {BinderName} in PropBagControlsWPF.Binders.");
            }

            ObservableSource root = pathListeners[ROOT_INDEX];

            if (changeInfo.ChangeType == DataSourceChangeTypeEnum.Initializing)
            {
                ObservableSourceProvider osp = GetSourceRoot(targetObject, bInfo.Source, ROOT_PATH_ELEMENT);

                bool readyToMonitor = ReplaceListener(pathListeners, ROOT_INDEX, ref osp, this.DataSourceHasChanged,
                    out root);

                if(!root.IsListeningForNewDC)
                {
                    throw new InvalidOperationException($"{BinderName} could not locate a data source.");
                }

                System.Diagnostics.Debug.Assert(stepNo == ROOT_INDEX, $"The StepNo should refer to the ObservableSource for " +
                    $"the root when DataSourceChangeType = {nameof(DataSourceChangeTypeEnum.Initializing)}.");
                resolvedOsCount = 1;
            }
            else if(changeInfo.ChangeType == DataSourceChangeTypeEnum.DataContextUpdated)
            {
                // Ask the root to remove its handlers, if any, from the DataContext just replaced.
                root.StopListeningToSource();

                // Ask the root to begin listening to its new DataContext, or its new Data provided by its DataSourceProvider.
                root.UpdateData(BinderName);

                System.Diagnostics.Debug.Assert(stepNo == ROOT_INDEX, $"The StepNo should refer to the ObservableSource for" +
                    $" the root when DataSourceChangeType = {nameof(DataSourceChangeTypeEnum.DataContextUpdated)}.");
                resolvedOsCount = 1;

                // Now re-calculate all dependent Observable Sources

            }
            else if(changeInfo.ChangeType == DataSourceChangeTypeEnum.CollectionChanged)
            {
                resolvedOsCount = this.ResolvedOsCount;


            }
            else if(changeInfo.ChangeType == DataSourceChangeTypeEnum.PropertyChanged)
            {
                resolvedOsCount = this.ResolvedOsCount;

                // The PropModel of the value of this property may have changed, and so we need to start over
                // with our path calculations.
                // This depends on the definition of IsPropBagBased to be (x => x.Type != null && x.Type.Implements(IPropBagMin))

                if (stepNo > 1) throw new NotImplementedException("Need to update intervening propbag parents.");
                // Refresh childern if they were or are now parented by an PropBag
                // until the signaled step is reached.
                // As soon as non-propbag step is reached, stop.

                System.Diagnostics.Debug.Assert(ResolvedOsCount > stepNo + 1,
                    "While refreshing listeners found that the signaled step was deeper than the number of listening steps.");



            }
            else
            {
                throw new ApplicationException($"The DataSourceChangedType: {changeInfo.ChangeType} is not recognized.");
            }


            // Starting at step: stepNo + 1, replace or refresh each listener. 
            ObservableSource os = pathListeners[stepNo];

            for (int nPtr = stepNo + 1; nPtr < pathListeners.Count - 1; nPtr++)
            {
                string pathElement = pathListeners[nPtr].PathElement;

                ObservableSourceProvider osp = os.GetChild(pathElement);

                bool isListening = ReplaceListener(pathListeners, nPtr, ref osp, this.DataSourceHasChanged, out os);

                if (isListening)
                {
                    resolvedOsCount++;
                    System.Diagnostics.Debug.WriteLine($"Listener was created for {pathElement}.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No DataSouce found, therefore, no listener was created for {pathElement}.");
                }
            }

            // If the last os returned has data 
            // the we need to have that os begin listening
            // and we need to start listening to the os for PropertyChanged where
            // the PropertyName = the TerminalNode's pathElement or "Item[]"


            return resolvedOsCount;
        }

        private void ResetPathListeners(OSList pathListeners, int startIndex)
        {

            for (int nPtr = 1; nPtr < pathListeners.Count - 1; nPtr++)
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

        /// <summary>
        /// Starting from the root, how many ObservableSources have data for which at least one
        /// event handler is attached.
        /// </summary>
        /// <param name="pathListeners"></param>
        /// <returns></returns>
        private int GetNumResolvedOS(OSList pathListeners)
        {
            int result = 0;
            for (int nPtr = 0; nPtr < pathListeners.Count; nPtr++)
            {
                if (pathListeners[nPtr].IsListeningForNewDC)
                    result++;
                else
                    break;
            }

            return result;
        }

        /// <summary>
        /// Starting from the root, how many ObservableSources have the information they 
        /// need to support creating a binding.
        /// 
        /// A binding can be created for a regular CLR object
        /// if the parent's node has discovered the type of data at that (parent) level.
        /// 
        /// A binding can be created for a PropBag-based object
        /// if the parent's node has a reference to the current value of that (parent) level.
        /// </summary>
        /// <param name="pathListeners"></param>
        /// <param name="numWithType"></param>
        /// <returns></returns>
        private int GetNumResolvedOS2(OSList pathListeners)
        {
            int result = 0;

            for (int nPtr = 0; nPtr < pathListeners.Count; nPtr++)
            {
                ObservableSource os = pathListeners[nPtr];
                bool hasType = os.GetHasTypeAndHasData(out bool hasData);

                if (hasData || (hasType && !os.IsPropBagBased))
                {
                    result++;
                    continue;
                }
                else
                {
                    break;
                }
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

            return result ?? string.Empty;
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
        private bool ReplaceListener(OSList pathListeners, int index, 
            ref ObservableSourceProvider newListenerProvider, DataSourceChangedEventHandler subscriber,
            out ObservableSource newListener)
        {
            if(index == pathListeners.Count - 1)
            {
                System.Diagnostics.Debug.Assert(index != pathListeners.Count - 1,
                    "We are replacing the terminal listener. This is not good.");
            }

            ObservableSource oldListener = pathListeners[index];

            newListener = newListenerProvider.CreateObservableSource(); newListenerProvider = null;

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
                return false;
            }

            if (newListener.IsListeningForNewDC)
            {
                bool isListening = newListener.Subscribe(subscriber);
                System.Diagnostics.Debug.Assert(isListening, "Subscriber was already present, but should not have been.");
                isListening = newListener.IsListeningForNewDC;
            }

            pathListeners[index] = newListener;

            bool hasType = newListener.GetHasTypeAndHasData(out bool hasData);

            // The new path listener is ready to start monitoring Property and/or Collection Changed events if 
            // it still has a reference to the object that will be monitored.
            return hasData;

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
            OSList pathListeners, Type sourceType, MyBindingInfo bInfo)
        {
            System.Diagnostics.Debug.Assert(sourceType != null, "The SourceType should never be null here.");

            Type propertyType = targetProperty.PropertyType;
            bool isPropBagBased = false;

            int compCount = pathListeners.Count; // The SourceRoot does not have a corresponding path element.

            if (compCount > 2)
            {
                //throw new ApplicationException("Path cannot have more than two components.");
                System.Diagnostics.Debug.WriteLine("Handling a two parter.");
            }


            int readyPathListeners = GetNumResolvedOS2(pathListeners);
            string strNewPath = GetNewPath(pathListeners);
            System.Diagnostics.Debug.WriteLine($"Path = {bInfo.PropertyPath.Path}, NewPath = {strNewPath}, ReadyListenerCount = {readyPathListeners}.");


            // One node:    1 parent;
            // Two nodes:   1 parent, 1 intervening object
            // Three nodes:  1 parent, 2 intervening objects

            // The next to last node, must have data, in order to avoid binding warnings.
            bool hasType = pathListeners[pathListeners.Count - 2].GetHasTypeAndHasData(out bool hasData);

            if(!hasData) 
            //if (readyPathListeners + 2 < compCount) // Although there is one more PathListener than path components, the terminal node should not be listening.
            {
                // TODO: Instead of evaluating only the top-level,
                // see how far down the chain we can go before we run out of info.

                //System.Diagnostics.Debug.WriteLine($"Path = {bInfo.PropertyPath}, NewPath = , Not enough, or too many, Change Listeners. We are creating a binding to the DataContext itself.");

                System.Diagnostics.Debug.Assert(ResolvedOsCount > 0, "There are no active listeners!");

                System.Diagnostics.Debug.WriteLine("No Binding is being created, not enough data.");

                //isPropBagBased = IsThisNodePropBagBased(pathListeners[0]);


                //return CreateDefaultBinding(bInfo, sourceType, propertyType);
                //PropertyPath pathSelf = new PropertyPath("", new object[0]);
                //return CreateBinding(bInfo.PropertyPath, bInfo, false, typeof(object), propertyType);
                return null;
            }

            StringBuilder sb = new StringBuilder();


            for (int nPtr = 1; nPtr < compCount; nPtr++)
            {
                
                ObservableSource nodeSource = pathListeners[nPtr];
                string pathElement = nodeSource.PathElement;
                Type nodeType = nodeSource.Type;

                // Is the parent PropBagBased?
                isPropBagBased = IsThisNodePropBagBased(pathListeners[nPtr - 1]);

                if (nPtr > 1) sb.Append(".");
                if (isPropBagBased)
                {
                    if(nodeType == null)
                    {
                        nodeType = typeof(object);
                    }

                    nodeType = nodeType ?? typeof(object);
                    string newNode = $"[{nodeType.FullName},{pathElement}]";
                    nodeSource.NewPathElement = newNode;
                    sb.Append(newNode);
                }
                else
                {
                    nodeSource.NewPathElement = pathElement;
                    sb.Append(pathElement);
                }
            }

            // TODO: What about the original PropertyPath parameters?
            PropertyPath newPath = new PropertyPath(sb.ToString());
            // DONT DO THIS -- bInfo.PropertyPath = newPath;

            return CreateBinding(newPath, bInfo, isPropBagBased, sourceType, propertyType);
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
                    ConverterParameter = converterParameter
                };

                System.Diagnostics.Debug.WriteLine("CREATING PropBag BINDING");
                System.Diagnostics.Debug.Write($"From {newPath.Path} to ");
            }
            else
            {
                result = CreateDefaultBinding(newPath, bInfo, sourceType, propertyType);

                System.Diagnostics.Debug.WriteLine("CREATING standard BINDING");
                System.Diagnostics.Debug.Write($"From {newPath.Path} to ");
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

        private bool IsThisNodePropBagBased(ObservableSource os)
        {
            if (os == null)
            {
                System.Diagnostics.Debug.WriteLine("Returning false for IsThisNodePropBagBased because the ObservableSource is null.");
                return false;
            }
            return os.IsPropBagBased;
        }

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

        #region Data Source Changed Handing

        private void DataSourceHasChanged(object sender, DataSourceChangedEventArgs e)
        {
            ObservableSource os = (ObservableSource)sender;

            ResolvedOsCount = RefreshPathListeners(e, _dataSourceChangeListeners, os, _targetObject, _bindingInfo);

            int test = ResolvedOsCount; // This will force a check.

            BindingBase newBinding = GetTheBinding(_targetObject, _targetProperty, _dataSourceChangeListeners,
                _sourceType, _bindingInfo);

            Binding oldBinding = BindingOperations.GetBinding(_targetObject, _targetProperty);

            if (oldBinding != null)
            {
                BindingOperations.ClearBinding(_targetObject, _targetProperty);
            }

            if(newBinding != null)
            {
                try
                {
                    BindingExpressionBase bExp = BindingOperations.SetBinding(_targetObject,
                        _targetProperty, newBinding);

                    System.Diagnostics.Debug.WriteLine($" {_targetProperty.Name} on object: {GetNameFromDepObject(_targetObject)}.");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Attempt to SetBinding failed.");
                }

                if (oldBinding != null)
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
                if (oldBinding != null)
                {
                    System.Diagnostics.Debug.WriteLine($"{BinderName} removed binding on some target and replaced it with no binding.");
                }
            }


            //foreach (ObservableSource oss in _dataSourceChangeListeners)
            //{
            //    oss.ReleaseData();
            //}

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


            if (targetObject is FrameworkElement fe)
            {
                osp = new ObservableSourceProvider(fe, pathElement);
            }
            else if (targetObject is FrameworkContentElement fce)
            {
                osp = new ObservableSourceProvider(fce, pathElement);
            }
            else
            {
                throw new ApplicationException($"targetObject in {BinderName}.ObservableSourceProvider was neither a FrameworkElement or a FrameworkContentElement.");
            }

            return true;

            ////object dc = LogicalTree.GetDataContext(targetObject, out DependencyObject foundNode);

            //// Changed to use foundNode on 10/10/17 @ 12:15 PM.
            //if (dc == null)
            //{
            //    if (foundNode is FrameworkElement fe)
            //    {
            //        osp = new ObservableSourceProvider(fe, node);
            //        return true;
            //    }
            //    else if (foundNode is FrameworkContentElement fce)
            //    {
            //        osp = new ObservableSourceProvider(fce, node);
            //        return true;
            //    }
            //    else
            //    {
            //        osp = null;
            //        return false;
            //    }
            //}
            //else
            //{
            //    if (foundNode is FrameworkElement fe)
            //    {
            //        osp = new ObservableSourceProvider(fe, node);
            //    }
            //    else if (foundNode is FrameworkContentElement fce)
            //    {
            //        osp = new ObservableSourceProvider(fce, node);
            //    }
            //    else
            //    {
            //        throw new ApplicationException($"Found node in {BinderName}.ObservableSourceProvider was neither a FrameworkElement or a FrameworkContentElement.");
            //    }

            //    return true;
            //}
        }

#endregion

        #region Observable Source nested class

        public class ObservableSource
        {
            #region Public events and properties

            public event DataSourceChangedEventHandler DataSourceChanged = null; // delegate { };

            string _pathElement;
            public string PathElement
            {
                get
                {
                    return _pathElement;
                }
                private set
                {
                    if(value == "PersonCollectionVM")
                    {
                        System.Diagnostics.Debug.WriteLine($"Building ObservableSource for {value}");
                    }
                    _pathElement = value;
                }
            }

            public string NewPathElement { get; set; }

            // For an ObservableSource of SourceKind: DataContext, the Container is the FrameworkElement
            // and the Data is the DataContext.

            // For an ObservableSource of SourceKind: DataSourceProvider, the Container is the DSP
            // and the Data is the result of accessing DSP.Data.

            private bool _fcOrFce;
            private WeakReference _wrContainer;
            private object Container
            {
                get
                {
                    //if (_containerWasNull) return null;
                    if (_wrContainer == null) return null;
                    if (SourceKind == SourceKindEnum.DataContext)
                    {
                        if (_wrContainer.IsAlive)
                        {
                            if (_fcOrFce)
                            {
                                FrameworkContentElement fce = (FrameworkContentElement)_wrContainer.Target;
                                return fce;
                            }
                            else
                            {
                                FrameworkElement fe = (FrameworkElement)_wrContainer.Target;
                                return fe;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (SourceKind == SourceKindEnum.DataSourceProvider)
                    {
                        if (_wrContainer.IsAlive)
                        {
                            DataSourceProvider dsp = (DataSourceProvider)_wrContainer.Target;
                            return dsp;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        // TODO: Fix this up.
                        throw new InvalidOperationException("Attempting to get the value for Container is not valid when the SourceKind is not DataContext or DataSourceProvider.");
                    }
                }
                set
                {
                    if (SourceKind == SourceKindEnum.DataContext)
                    {
                        if(value == null)
                        {
                            //_containerWasNull = true;
                            _wrContainer = null;
                            _fcOrFce = false;
                        } 
                        else
                        {
                            //_containerWasNull = false;
                            _wrContainer = new WeakReference(value);
                            if(value is FrameworkElement)
                            {
                                _fcOrFce = false;
                            }
                            else if(value is FrameworkContentElement)
                            {
                                _fcOrFce = true;
                            }
                            else
                            {
                                throw new InvalidOperationException("Only FrameworkElements and FrameworkContentElements can be use to set the value of the container for an ObservableSource with SourceKind = DataContext.");
                            }
                        }
                    }
                    else if (SourceKind == SourceKindEnum.DataSourceProvider)
                    {
                        if (value == null)
                        {
                            //_containerWasNull = true;
                            _wrContainer = null;
                        }
                        else
                        {
                            //_containerWasNull = false;
                            _wrContainer = new WeakReference(value);
                            if (!(value is DataSourceProvider))
                            {
                                throw new InvalidOperationException("Only DataSourceProviders can be used to set the value of the container for an ObservableSource with SourceKind = DataSourceProvider.");
                            }
                        }
                        _fcOrFce = false;
                    }
                    else
                    {
                        // TODO: Fix this up.
                        throw new InvalidOperationException("Attempting to set the value for Container is not valid when the SourceKind is not DataContext or DataSourceProvider.");
                    }

                }
            }

            private WeakReference _wrData;
            public object Data
            {
                get
                {
                    if (SourceKind == SourceKindEnum.DataSourceProvider)
                    {
                        if (_wrData == null)
                        {
                            DataSourceProvider dsp = (DataSourceProvider)Container;
                            if (dsp == null) return null;

                            object data = dsp.Data;
                            if (data != null)
                            {
                                // Save weak reference in case this value is accessed again.
                                _wrData = new WeakReference(data);
                            }
                            return data;
                        }
                        else
                        {
                            if (_wrData.IsAlive)
                            {
                                return _wrData.Target;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    else //if(SourceKind != SourceKindEnum.DependencyObject && SourceKind != SourceKindEnum.Empty && SourceKind != SourceKindEnum.TerminalNode)
                    {
                        if (_wrData == null || !(_wrData.IsAlive))
                        {
                            return null;
                        }
                        else
                        {
                            return _wrData.Target;
                        }
                    }
                    //else
                    //{
                    //    throw new InvalidOperationException("Only PropertyChanged is supported.");
                    //}
                }
                private set
                {
                    // TODO: Consider checking the type of the value to see if it's valid
                    // for this instance's SourceKind.
                    if (value == null)
                    {
                        _wrData = null;
                    }
                    else
                    {
                        _wrData = new WeakReference(value);
                    }
                }
            }

            // TODO: This should not be needed -- we should be able to determine type upon construction.
            private Type GetTypeOfData(object data)
            {
                if (data == null)
                {
                    return null;
                }
                else
                {
                    return data.GetType();
                }
            }

            private Type _type;
            public Type Type
            {
                get
                {
                    if (_type == null)
                    {
                        _type = GetTypeOfData(Data);
                    }
                    return _type;
                }
                private set
                {
                    _type = value;
                }
            }

            internal bool GetHasTypeAndHasData(out bool hasData)
            {

                if(SourceKind == SourceKindEnum.DataSourceProvider)
                {
                    hasData = _wrContainer != null && _wrContainer.IsAlive;
                }
                else
                {
                    hasData = _wrData != null && _wrData.IsAlive;
                }

                return _type != null;

            }

            public SourceKindEnum SourceKind { get; private set; }

            public bool IsListeningForNewDC { get; private set; }

            public bool IsDcListeningToProp => Status == ObservableSourceStatusEnum.IsWatchingProp || Status == ObservableSourceStatusEnum.IsWatchingPropAndColl;
            public bool IsDcListeningToColl => Status == ObservableSourceStatusEnum.IsWatchingColl || Status == ObservableSourceStatusEnum.IsWatchingPropAndColl;
            public bool IsDcListening => Status == ObservableSourceStatusEnum.IsWatchingProp || Status == ObservableSourceStatusEnum.IsWatchingColl || Status == ObservableSourceStatusEnum.IsWatchingPropAndColl;

            public ObservableSourceStatusEnum Status { get; private set; }

            public bool IsPropBagBased
            {
                get
                {
                    Type test = Type;
                    return test == null ? false : test.IsPropBagBased();
                }
            }

            //public bool IsPossiblyPropBagBased
            //{
            //    get
            //    {
            //        Type test = Type;
            //        return test == null ? true : test.IsPropBagBased();
            //    }
            //}

            #endregion

            #region Public Methods

            public void Reset(DataSourceChangedEventHandler subscriber = null)
            {
                if(subscriber != null) Unsubscribe(subscriber);

                if(SourceKind != SourceKindEnum.Empty && IsDcListening)
                {
                    object data = Data;
                    if (data != null)
                    {
                        RemoveSubscriptions(data);
                    }
                }
            }

            public void UpdateData(string binderInstanceName)
            {
                if( !(SourceKind == SourceKindEnum.DataContext || SourceKind == SourceKindEnum.DataSourceProvider))
                {
                    throw new InvalidOperationException($"Only ObservableSources with SourceKind = {nameof(SourceKindEnum.DataContext)} or {nameof(SourceKindEnum.DataSourceProvider)} can have their data updated.");
                }

                object oldData = Data;
                object newData;
                Type newType = null;
                ObservableSourceStatusEnum newStatus = ObservableSourceStatusEnum.NoType;

                if(SourceKind == SourceKindEnum.DataSourceProvider)
                {
                    newData = ((DataSourceProvider)Container).Data;
                    if(newData != null)
                    {
                        newType = newData.GetType();
                    }

                    newStatus = SetReady(newData != null);
                } 
                else
                {
                    if (!GetDcFromFrameworkElement(Container, out newData, out newType, out newStatus))
                    {
                        throw new ApplicationException($"TargetObject in {binderInstanceName}.ObservableSource was neither a FrameworkElement or a FrameworkContentElement.");
                    }
                }

                if(oldData != null && newData != null)
                {
                    if(object.ReferenceEquals(oldData, newData))
                    {
                        System.Diagnostics.Debug.WriteLine("Update ObservableSource found identical data already present.");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Update ObservableSource found (different) data already present.");
                    }
                }

                // TODO: consolidate this -- they use the same logic.
                if (this.SourceKind == SourceKindEnum.DataContext)
                {
                    // Remove existing subscriptions if any for the existing Data.
                    if (oldData != null && IsDcListening)
                    {
                        RemoveSubscriptions(oldData);
                    }

                    Data = newData;
                    Type = newType;
                    Status = newStatus;
                }
                else
                {
                    // Our SourceKind must be DataSourceProvider.

                    // Remove event hander from old data if present.
                    if (oldData!= null && IsDcListening)
                    {
                        RemoveSubscriptions(oldData);
                    }

                    Data = newData;
                    Type = newType;
                    Status = newStatus;
                }
            }

            public ObservableSourceProvider GetChild(string pathElement)
            {
                if (SourceKind == SourceKindEnum.Empty || SourceKind == SourceKindEnum.TerminalNode)
                {
                    throw new ApplicationException($"Cannot build a new ObservableSource from an ObservableSource with SourceKind = {nameof(SourceKindEnum.Empty)} or {nameof(SourceKindEnum.TerminalNode)}.");
                }

                Type parentType = this.Type;
                if (parentType == null) return null;

                if (IsPropBagBased)
                {
                    return GetChildFromPropBag((IPropBagMin)Data, parentType, pathElement);
                }
                else
                {
                    return GetChildFromClr(Data, parentType, pathElement);
                }
            }

            private ObservableSourceProvider GetChildFromPropBag(IPropBagMin data, Type type, string pathElement)
            {
                object newData;
                Type newType;

                if (data.TryGetPropGen(pathElement, null, out IPropGen iPg))
                {
                    if (iPg is PropGen)
                    {
                        newData = ((PropGen)iPg).TypedProp?.TypedValueAsObject;
                        newType = ((PropGen)iPg).Type;
                    }
                    else
                    {
                        newData = iPg?.TypedProp?.TypedValueAsObject;
                        newType = iPg?.Type;
                    }

                    if (newData != null)
                    {
                        ObservableSourceProvider child = CreateChild(newData, newType, pathElement);
                        return child;
                    }
                    else
                    {
                        return new ObservableSourceProvider(pathElement, newType);
                    }
                }
                else
                {
                    // Property value could not be retreived.
                    if (data.TryGetTypeOfProperty(pathElement, out newType))
                    {
                        // Create an ObservableSource with SourceKind = TerminalNode.
                        return new ObservableSourceProvider(pathElement, newType);
                    }
                    else
                    {
                        return null;
                    }
                }

            }

            private ObservableSourceProvider GetChildFromClr(object data, Type type, string pathElement)
            {
                if (data != null)
                {
                    object val = GetMemberValue(pathElement, data, type, this.PathElement,
                        out Type pt);

                    ObservableSourceProvider child = CreateChild(val, pt, pathElement);
                    return child;
                }
                else
                {
                    // Using reflection, get the type.
                    Type pt = GetTypeOfPathElement(pathElement, type, this.PathElement);

                    // Create an ObservableSource with SourceKind = TerminalNode.
                    return new ObservableSourceProvider(pathElement, pt);
                }
            }

            public bool Subscribe(DataSourceChangedEventHandler subscriber)
            {
                if (DataSourceChanged == null)
                {
                    DataSourceChanged = subscriber; 
                    return true; // We added it.
                }
                else
                {
                    Delegate[] subscriberList = DataSourceChanged.GetInvocationList();
                    if (subscriberList.FirstOrDefault((x) => x == (Delegate)subscriber) == null)
                    {
                        DataSourceChanged += subscriber; 
                        return true; // We added it.
                    }
                    else
                    {
                        return false; // Already there.
                    }
                }
            }

            public bool Unsubscribe(DataSourceChangedEventHandler subscriber)
            {
                if (DataSourceChanged == null)
                {
                    return false; // It's not there.
                }
                else
                {
                    Delegate[] subscriberList = DataSourceChanged.GetInvocationList();
                    if (subscriberList.FirstOrDefault((x) => x == (Delegate)subscriber) == null)
                    {
                        return false; // Not there.
                    }
                    else
                    {
                        DataSourceChanged -= subscriber; 
                        return true; // We removed it.
                    }
                }
            }

            public void BeginListeningToSource()
            {
                switch(SourceKind)
                {
                    case SourceKindEnum.PropertyObject:
                        {
                            AddSubscriptions(Data);
                            break;
                        }
                    case SourceKindEnum.CollectionObject:
                        {
                            AddSubscriptions(Data);
                            break;
                        }
                    case SourceKindEnum.DataContext:
                        {
                            AddSubscriptions(Data);
                            break;
                        }
                    case SourceKindEnum.DataSourceProvider:
                        {
                            AddSubscriptions(Data);
                            break;
                        }
                    default:
                        {
                            string msg = $"ObservableSouce with SourceKind value: {SourceKind} is not supported or " +
                                "is not recognized on call to BeginListeningToSource.";

                            System.Diagnostics.Debug.WriteLine(msg);
                            break;
                        }
                }
            }

            public void StopListeningToSource()
            {
                switch (SourceKind)
                {
                    case SourceKindEnum.PropertyObject:
                        {
                            RemoveSubscriptions(Data);
                            break;
                        }
                    case SourceKindEnum.CollectionObject:
                        {
                            RemoveSubscriptions(Data);
                            break;
                        }
                    case SourceKindEnum.DataContext:
                        {
                            RemoveSubscriptions(Data);
                            break;
                        }
                    case SourceKindEnum.DataSourceProvider:
                        {
                            RemoveSubscriptions(Data);
                            break;
                        }
                    default:
                        {
                            string msg = $"ObservableSouce with SourceKind value: {SourceKind} is not supported or " +
                                "is not recognized on call to StopListeningToSource.";

                            System.Diagnostics.Debug.WriteLine(msg);
                            break;
                        }
                }
            }

            #endregion

            #region Private Methods

            private ObservableSourceProvider CreateChild(object data, Type type, string pathElement)
            {
                if (typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                {
                    return new ObservableSourceProvider(data as INotifyPropertyChanged, pathElement);
                }
                else if (typeof(INotifyCollectionChanged).IsAssignableFrom(type))
                {
                    return new ObservableSourceProvider(data as INotifyCollectionChanged, pathElement);
                }
                else if (type == typeof(DataSourceProvider))
                {
                    return new ObservableSourceProvider(data as DataSourceProvider, pathElement);
                }

                throw new InvalidOperationException("Cannot create child: it does not implement INotifyPropertyChanged or INotifyCollectionChanged.");
            }

            private bool GetDcFromFrameworkElement(object feOrFce, out object dc, out Type type, out ObservableSourceStatusEnum status)
            {
                type = null;
                status = ObservableSourceStatusEnum.NoType;
                if (feOrFce is FrameworkElement fe)
                {
                    dc = fe.DataContext;
                    if(dc != null)
                    {
                        type = dc.GetType();
                    }
                    status = SetReady(dc != null);
                    return true;
                }
                else if (feOrFce is FrameworkContentElement fce)
                {
                    dc = fce.DataContext;
                    if (dc != null)
                    {
                        type = dc.GetType();
                    }
                    status = SetReady(dc != null);
                    return true;
                }
                else
                {
                    dc = null;
                    return false;
                }
            }

            private void AddSubscriptions(object dc)
            {
                if (dc is INotifyPropertyChanged pc)
                {
                    WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
                        .AddHandler(pc, "PropertyChanged", OnPCEvent);

                    Status = ObservableSourceStatusEnum.IsWatchingProp;
                }

                if (dc is INotifyCollectionChanged cc)
                {
                    WeakEventManager<INotifyCollectionChanged, CollectionChangeEventArgs>
                        .AddHandler(cc, "CollectionChanged", OnCCEvent);

                    Status = SetWatchingColl(Status);
                }

                if (!IsDcListening)
                {
                    throw new ApplicationException("Cannot create subscriptions. Object does not implement INotifyPropertyChanged, nor does it implement INotifyCollectionChanged.");
                }
            }

            private ObservableSourceStatusEnum SetWatchingColl(ObservableSourceStatusEnum curValue)
            {
                return curValue == ObservableSourceStatusEnum.IsWatchingProp ? ObservableSourceStatusEnum.IsWatchingPropAndColl : ObservableSourceStatusEnum.IsWatchingColl;
            }

            private ObservableSourceStatusEnum SetReady(bool haveData)
            {
                return haveData ? ObservableSourceStatusEnum.Ready : ObservableSourceStatusEnum.NoType;
            }


            private void RemoveSubscriptions(object dc)
            {
                bool removedIt = false;
                if (dc is INotifyPropertyChanged pc)
                {
                    WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
                        .RemoveHandler(pc, "PropertyChanged", OnPCEvent);
                    removedIt = true;
                }

                if (dc is INotifyCollectionChanged cc)
                {
                    WeakEventManager<INotifyCollectionChanged, CollectionChangeEventArgs>
                        .RemoveHandler(cc, "CollectionChanged", OnCCEvent);
                    removedIt = true;
                }

                Status = ObservableSourceStatusEnum.Ready;

                if (!removedIt)
                {
                    System.Diagnostics.Debug.WriteLine("Could not remove subscriptions. Object Object does not implement INotifyPropertyChanged, nor does it implement INotifyCollectionChanged.");
                }
            }

            #endregion

            #region Constructors and their handlers

            private ObservableSource(SourceKindEnum sourceKind, string pathElement, bool isListening)
            {
                this.SourceKind = sourceKind;
                PathElement = pathElement;
                IsListeningForNewDC = isListening;
                
                _wrData = null;
                _wrContainer = null;
                
                NewPathElement = null;
                Type = null;
            }

            #region Empty 
            public ObservableSource(string pathElement)
                : this(SourceKindEnum.Empty, pathElement, false)
            {
                Data = null;
                Type = null;
                Status = ObservableSourceStatusEnum.NoType;
            }
            #endregion

            #region Terminal Node 
            public ObservableSource(string pathElement, Type type)
                : this(SourceKindEnum.TerminalNode, pathElement, false)
            {
                Data = null;
                Type = type;
                Status = ObservableSourceStatusEnum.HasType;

            }
            #endregion

            #region From Framework Element and Framework Content Element
            public ObservableSource(FrameworkElement fe, string pathElement)
                : this(SourceKindEnum.DataContext, pathElement, true)
            {
                Container = fe;
                Data = fe.DataContext;

                // TOOD: Verify that these already use weak events.
                fe.DataContextChanged += Fe_or_fce_DataContextChanged;

                if(fe.DataContext != null)
                {
                    Type = fe.DataContext.GetType();
                }
                Status = SetReady(fe.DataContext != null);
            }

            public ObservableSource(FrameworkContentElement fce, string pathElement)
                : this(SourceKindEnum.DataContext, pathElement, true)
            {
                Container = fce;
                Data = fce.DataContext;

                // TOOD: Verify that these already use weak events.
                fce.DataContextChanged += Fe_or_fce_DataContextChanged;

                if (fce.DataContext != null)
                {
                    Type = fce.DataContext.GetType();
                }
                Status = SetReady(fce.DataContext != null);
            }

            private void Fe_or_fce_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                OnDataSourceChanged(DataSourceChangeTypeEnum.DataContextUpdated);
            }
            #endregion

            #region From INotifyPropertyChanged
            public ObservableSource(INotifyPropertyChanged itRaisesPropChanged, string pathElement)
                : this(SourceKindEnum.PropertyObject, pathElement, false)
            {
                Data = itRaisesPropChanged ?? throw new ArgumentNullException($"{nameof(itRaisesPropChanged)} was null when constructing Observable Source.");
                Type = itRaisesPropChanged.GetType();

                Status = ObservableSourceStatusEnum.Ready;

                //WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
                //    .AddHandler(itRaisesPropChanged, "PropertyChanged", OnPCEvent);
            }

            private void OnPCEvent(object source, PropertyChangedEventArgs args)
            {
                OnDataSourceChanged(args.PropertyName);
            }
            #endregion

            #region From INotifyCollection Changed
            public ObservableSource(INotifyCollectionChanged itRaisesCollectionChanged, string pathElement)
                : this(SourceKindEnum.CollectionObject, pathElement, false)
            {
                Data = itRaisesCollectionChanged ?? throw new ArgumentNullException($"{nameof(itRaisesCollectionChanged)} was null when constructing Observable Source.");
                Type = itRaisesCollectionChanged.GetType();

                Status = ObservableSourceStatusEnum.Ready;

                //WeakEventManager<INotifyCollectionChanged, CollectionChangeEventArgs>
                //    .AddHandler(itRaisesCollectionChanged, "CollectionChanged", OnCCEvent);
            }

            private void OnCCEvent(object source, CollectionChangeEventArgs args)
            {
                OnDataSourceChanged(args.Action, args.Element);
            }
            #endregion

            #region From DataSourceProvider
            public ObservableSource(DataSourceProvider dsp, string pathElement)
                : this(SourceKindEnum.DataSourceProvider, pathElement, true)
            {
                Container = dsp;
                Data = null;
                Type = null;

                WeakEventManager<DataSourceProvider, EventArgs>
                    .AddHandler(dsp, "DataChanged", OnPlainEvent);

                Status = ObservableSourceStatusEnum.Undetermined;
            }

            private void OnPlainEvent(object source, EventArgs args)
            {
                OnDataSourceChanged(DataSourceChangeTypeEnum.DataContextUpdated);
            }
            #endregion

            #endregion Constructors and their handlers

            #region Raise Event Helpers

            private void OnDataSourceChanged(DataSourceChangeTypeEnum changeType)
            {
                DataSourceChangedEventHandler handler = Interlocked.CompareExchange(ref DataSourceChanged, null, null);

                if (handler != null)
                {
                    handler(this, new DataSourceChangedEventArgs(changeType));
                }
            }

            private void OnDataSourceChanged(string propertyName)
            {
                DataSourceChangedEventHandler handler = Interlocked.CompareExchange(ref DataSourceChanged, null, null);

                if (handler != null)
                {
                    handler(this, new DataSourceChangedEventArgs(propertyName));
                }
            }

            private void OnDataSourceChanged(CollectionChangeAction action, object element)
            {
                DataSourceChangedEventHandler handler = Interlocked.CompareExchange(ref DataSourceChanged, null, null);

                if (handler != null)
                {
                    handler(this, new DataSourceChangedEventArgs(action, element));
                }
            }

            #endregion Raise Event Helpers

            #region Type Support

            private Type GetTypeOfPathElement(string pathElement, Type parentType, string parentsPathElement)
            {
                PropertyInfo pi = parentType.GetDeclaredProperty(pathElement);
                if (pi == null)
                {
                    throw new InvalidOperationException($"{pathElement} does not exist on data source: {parentsPathElement}.");
                }

                try
                {
                    return pi.PropertyType;
                }
                catch
                {
                    throw new InvalidOperationException($"Cannot get the type from {pathElement} on source: {parentsPathElement}.");
                }
            }

            private object GetMemberValue(string pathElement, object parent, Type parentType,
                string parentsPathElement, out Type memberType)
            {
                // Let's see if its a field.
                MemberInfo mi = parentType.FindMember(pathElement);

                if(mi == null)
                {
                    throw new InvalidOperationException($"{pathElement} does not exist on data source: {parentsPathElement}.");
                }

                // TODO: Handle cases where property accessor has index paramter(s.)
                switch(mi.MemberType)
                {
                    case MemberTypes.Property:
                        {
                            memberType = ((PropertyInfo)mi).PropertyType;
                            return ((PropertyInfo)mi).GetValue(parent);
                        }
                    case MemberTypes.Field:
                        {
                            memberType = ((FieldInfo)mi).FieldType;
                            return ((FieldInfo)mi).GetValue(parent);
                        }
                    case MemberTypes.Method:
                        {
                            memberType = ((MethodInfo)mi).ReturnType;
                            return ((MethodInfo)mi).Invoke(parent, new object[0]);
                        }
                    default:
                        {
                            throw new InvalidOperationException($"Members of type {mi.MemberType} are not suppoted. Occured while accessing {pathElement} on {parentsPathElement}.");
                        }
                }

            }

            #endregion
        }

        #endregion Observable Source nested class

        #region Observable Source Provider nested class

        public class ObservableSourceProvider
        {
            #region Private properties

            string PathElement { get; set; }
            object Data { get; set; }
            Type Type { get; set; }
            SourceKindEnum SourceKind { get; set; }

            #endregion

            #region Public Methods

            public ObservableSource CreateObservableSource()
            {
                switch(this.SourceKind)
                {
                    case SourceKindEnum.Empty: return new ObservableSource(this.PathElement);
                    case SourceKindEnum.TerminalNode: return new ObservableSource(this.PathElement, this.Type);
                    case SourceKindEnum.DataContext:
                        {
                            if (this.Data is FrameworkElement fe) return new ObservableSource(fe, this.PathElement);
                            if (this.Data is FrameworkContentElement fce) return new ObservableSource(fce, this.PathElement);
                            throw new InvalidOperationException("ObservableSourceProvider of SourceKind = DataContext is neither FrameworkElement or FrameworkContentElement.");
                        }
                    case SourceKindEnum.PropertyObject: return new ObservableSource((INotifyPropertyChanged)this.Data, this.PathElement);
                    case SourceKindEnum.CollectionObject: return new ObservableSource((INotifyCollectionChanged)this.Data, this.PathElement);
                    case SourceKindEnum.DataSourceProvider: return new ObservableSource((DataSourceProvider)this.Data, this.PathElement);
                    default: throw new InvalidOperationException("That Source Kind is not recognized.");
                }
            }

            #endregion

            #region Constructors and their handlers

            #region Terminal Node 
            public ObservableSourceProvider(string pathElement, Type type)
            {
                Data = null;
                Type = type;
                SourceKind = SourceKindEnum.TerminalNode;
                PathElement = pathElement;
            }
            #endregion

            #region From Framework Element and Framework Content Element
            public ObservableSourceProvider(FrameworkElement fe, string pathElement)
            {
                Data = fe;
                Type = null;
                SourceKind = SourceKindEnum.DataContext;
                PathElement = pathElement;
            }

            public ObservableSourceProvider(FrameworkContentElement fce, string pathElement)
            {
                Data = fce;
                Type = null;
                SourceKind = SourceKindEnum.DataContext;
                PathElement = pathElement;
            }

            #endregion

            #region From INotifyPropertyChanged
            public ObservableSourceProvider(INotifyPropertyChanged itRaisesPropChanged, string pathElement)
            {
                Data = itRaisesPropChanged ?? throw new ArgumentNullException($"{nameof(itRaisesPropChanged)} was null when constructing Observable Source.");
                Type = null;
                SourceKind = SourceKindEnum.PropertyObject;
                PathElement = pathElement;
            }

            #endregion

            #region From INotifyCollection Changed
            public ObservableSourceProvider(INotifyCollectionChanged itRaisesCollectionChanged, string pathElement)
            {
                Data = itRaisesCollectionChanged ?? throw new ArgumentNullException($"{nameof(itRaisesCollectionChanged)} was null when constructing Observable Source.");
                Type = null;
                SourceKind = SourceKindEnum.CollectionObject;
                PathElement = pathElement;
            }

            #endregion

            #region From DataSourceProvider
            public ObservableSourceProvider(DataSourceProvider dsp, string pathElement)
            {
                Data = dsp;
                Type = null;
                SourceKind = SourceKindEnum.DataSourceProvider;
                PathElement = pathElement;
            }
            #endregion

            #endregion Constructors and their handlers
        }

        #endregion
    }



}
