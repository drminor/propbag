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
        List<ObservableSource> _dataSourceChangeListeners;

        private const string ROOT_PATH_ELEMENT = "root";

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

        #endregion

        #region Constructor

        public MyBindingEngine(MyBindingInfo bindingInfo, Type sourceType, DependencyObject targetObject, DependencyProperty targetProperty)
        {
            _bindingInfo = bindingInfo;
            _sourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            _targetObject = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
            _targetProperty = targetProperty ?? throw new ArgumentNullException(nameof(targetProperty));

            _dataSourceChangeListeners = null;
            _resolvedOsCount = 0;
        }

        #endregion

        #region Provide Value

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            SetDefaultConverter();

            _dataSourceChangeListeners = PreparePathListeners(_bindingInfo, _sourceType);

            _resolvedOsCount = InitPathListeners(_dataSourceChangeListeners,_targetObject, _bindingInfo);

            BindingBase bb = GetTheBinding(_targetObject, _targetProperty, _dataSourceChangeListeners,
                _sourceType, _bindingInfo);

            if (_dataSourceChangeListeners.Count > 1)
            {
                try
                {
                    BindingExpressionBase bExp =
                        BindingOperations.SetBinding(_targetObject, _targetProperty, bb);

                    System.Diagnostics.Debug.WriteLine($" {_targetProperty.Name} on object: {GetNameFromDepObject(_targetObject)}.");
                }
                catch
                {
                    // System.Diagnostics.Debug.WriteLine("Suppressed exception thrown when setting the binding during call to Provide Value.");
                    // Ignore the exception, we don't really need to set the binding.
                    // TODO: Is there anyway to avoid getting to here?
                    System.Diagnostics.Debug.WriteLine("Attempt to SetBinding failed.");
                    bb = null; // Set it to null, so that it will not be used.
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No Binding was created.");
            }
            System.Diagnostics.Debug.WriteLine("This binding is the one that our MultiValueConverter is using.");


            // create wpf binding
            MyMultiValueConverter mValueConverter = new MyMultiValueConverter(_bindingInfo.Mode);

            if(bb != null)
            {
                mValueConverter.Add(bb);
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

        private int InitPathListeners(List<ObservableSource> dsChangeListeners, DependencyObject targetObject, MyBindingInfo bInfo)
        {
            int resolvedOsCount = 0;

            ObservableSourceProvider osp = GetSourceRoot(targetObject, bInfo.Source, 
                bInfo.ElementName, bInfo.RelativeSource, ROOT_PATH_ELEMENT);

            ObservableSource os = osp.CreateObservableSource(); osp = null;

            bool wasAdded = ReplaceListener(dsChangeListeners, 0, os, this.DataSourceHasChanged);
            if (wasAdded)
            {
                resolvedOsCount++;
            } 
            else
            {
                throw new InvalidOperationException($"No DataSouce found, therefore, no listener was created for the Source Root: {_targetProperty.ToString()}.");
                //System.Diagnostics.Debug.WriteLine($"No DataSouce found, therefore, no listener was created for {_targetProperty.ToString()}.");
            }

            os = dsChangeListeners[0];

            // The terminal node has already had its Observable Source created,
            // and it does not need to listen, it only needs to hold the new and old path elements
            // and the sourceType.
            for (int nPtr = 1; nPtr < dsChangeListeners.Count - 1; nPtr++)
            {
                string pathElement = dsChangeListeners[nPtr].PathElement;

                os = os.GetChild(pathElement);

                wasAdded = ReplaceListener(dsChangeListeners, nPtr, os, this.DataSourceHasChanged);

                if (wasAdded)
                {
                    resolvedOsCount++;
                    System.Diagnostics.Debug.WriteLine($"Listener was created for {pathElement}.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No DataSouce found, therefore, no listner was created for {pathElement}.");
                }
            }

            return resolvedOsCount;
        }

        private int RefreshPathListeners(ObservableSource signalingOs, DataSourceChangedEventArgs changeInfo,
            List<ObservableSource> dsChangeListeners, DependencyObject targetObject, MyBindingInfo bInfo)
        {
            int resolvedOsCount; 

            int stepNo =_dataSourceChangeListeners.IndexOf(signalingOs);

            if (stepNo == -1)
            {
                //System.Diagnostics.Debug.WriteLine($"Could not find step No on DataSourceChanged in PropBagControlsWPF.Binders.MyBinder.");
                throw new InvalidOperationException("Could not get step no.");
            }

            if(changeInfo.ChangeType == DataSourceChangeTypeEnum.DataContextUpdated)
            {
                if (stepNo == 0)
                {
                    // Re-aquire the source root data.
                    ObservableSource root = RefreshSourceRoot(dsChangeListeners, targetObject, bInfo);
                    resolvedOsCount = 1;
                }
                else if (_dataSourceChangeListeners[0].IsPossiblyPropBagBased)
                {
                    ObservableSource root = RefreshSourceRoot(dsChangeListeners, targetObject, bInfo);
                    resolvedOsCount = 1;

                    if (stepNo > 1) throw new NotImplementedException("Need to update intervening propbag parents.");
                    // Refresh childern if they were or are now parented by an PropBag
                    // until the signaled step is reached.
                    // As soon as non-propbag step is reached, stop.

                }
                else
                {
                    System.Diagnostics.Debug.Assert(ResolvedOsCount > stepNo + 1,
                        "While refreshing listeners found that the signaled step was deeper than the number of listening steps.");

                    resolvedOsCount = stepNo + 1;

                }
            }
            else if(changeInfo.ChangeType == DataSourceChangeTypeEnum.CollectionChanged)
            {
                resolvedOsCount = this.ResolvedOsCount;
            }
            else if(changeInfo.ChangeType == DataSourceChangeTypeEnum.PropertyChanged)
            {
                resolvedOsCount = this.ResolvedOsCount;
            }
            else
            {
                throw new ApplicationException($"The DataSourceChangedType: {changeInfo.ChangeType} is not recognized.");
            }


            // Starting at step: stepNo + 1, replace or refresh each listener. 
            ObservableSource os = dsChangeListeners[stepNo];

            for (int nPtr = stepNo + 1; nPtr < dsChangeListeners.Count - 1; nPtr++)
            {
                string pathElement = dsChangeListeners[nPtr].PathElement;

                os = os.GetChild(pathElement);

                bool wasAdded = ReplaceListener(dsChangeListeners, nPtr, os, this.DataSourceHasChanged);

                if (wasAdded)
                {
                    resolvedOsCount++;
                    System.Diagnostics.Debug.WriteLine($"Listener was created for {pathElement}.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No DataSouce found, therefore, no listner was created for {pathElement}.");
                }
            }

            // If the last os returned has data 
            // the we need to have that os begin listening
            // and we need to start listening to the os for PropertyChanged where
            // the PropertyName = the TerminalNode's pathElement or "Item[]"


            return resolvedOsCount;
        }

        private ObservableSource RefreshSourceRoot(List<ObservableSource> dsChangeListeners, DependencyObject targetObject, MyBindingInfo bInfo)
        {
            ObservableSource root = _dataSourceChangeListeners[0];

            if (root.SourceKind == SourceKindEnum.DataContext || root.SourceKind == SourceKindEnum.DataSourceProvider)
            {
                root.UpdateData();
                return root;
            }
            else
            {
                ObservableSourceProvider osp = GetSourceRoot(targetObject, bInfo.Source, bInfo.ElementName, bInfo.RelativeSource, root.PathElement);

                ObservableSource os = osp.CreateObservableSource();
                if(!os.IsListening)
                {
                    throw new InvalidOperationException($"No DataSouce found, therefore, no listener was created for the Source Root: {_targetProperty.ToString()}.");
                }
                ReplaceListener(_dataSourceChangeListeners, 0, os, this.DataSourceHasChanged);
                return _dataSourceChangeListeners[0];
            }

        }

        private List<ObservableSource> PreparePathListeners(MyBindingInfo bInfo, Type sourceType)
        {
            List<ObservableSource> result = new List<ObservableSource>
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

        private int GetNumResolvedOS(List<ObservableSource> pathListeners)
        {
            int result = 0;
            for (int nPtr = 0; nPtr < pathListeners.Count; nPtr++)
            {
                if (pathListeners[nPtr].IsListening)
                    result++;
                else
                    break;
            }

            return result;
        }

        private bool ReplaceListener(List<ObservableSource> pathListeners,
            int index, ObservableSource newListener, DataSourceChangedEventHandler subscriber)
        {
            ObservableSource oldListner = pathListeners[index];

            bool wasListening = oldListner.IsListening;

            if(wasListening)
            {
                if(newListener == null && index == 0)
                {
                    System.Diagnostics.Debug.WriteLine("We are going to quit listening to the Source Root.");
                }
            }
            bool wasRemoved = oldListner.Unsubscribe(subscriber);

            System.Diagnostics.Debug.Assert(wasListening == wasRemoved, "Was Listening does not agree with WasRemoved.");

            // TODO: Need to Remove Subscriptions as well as Unsubscribing.

            if (newListener == null)
            {
                // TODO: What do we need to cleanup from existing, "orphaned" ObservableSource?
                return false;
            }

            bool wasAdded = false;
            if (newListener.IsListening)
            {
                wasAdded = newListener.Subscribe(subscriber);
                System.Diagnostics.Debug.Assert(wasAdded, "Subscriber was already present, but should not have been.");
            }

            pathListeners[index] = newListener;

            return wasAdded;
        }


        private string[] GetPathComponents(PropertyPath path, out int count)
        {
            string[] components = path.Path.Split('.');
            count = components.Length;
            return components;
        }

        #endregion

        #region Build Bindings

        // TODO: Provide Converter, Convert Params, etc to this method.
        private BindingBase GetTheBinding(DependencyObject targetObject, DependencyProperty targetProperty,
            List<ObservableSource> dsChangeListeners, Type sourceType, MyBindingInfo bInfo)
        {
            System.Diagnostics.Debug.Assert(sourceType != null, "The SourceType should never be null here.");

            Type propertyType = targetProperty.PropertyType;
            bool isPropBagBased = false;

            //string[] pathNameComponents = GetPathComponents(bInfo.PropertyPath, out int compCount);

            int compCount = dsChangeListeners.Count; // The SourceRoot does not have a corresponding path element.

            if (compCount > 2)
            {
                //throw new ApplicationException("Path cannot have more than two components.");
                System.Diagnostics.Debug.WriteLine("Handling a two parter.");
            }

            // One node:    1 parent;
            // Two nodes:   1 parent, 1 intervening object
            // Three nodes:  1 parent, 2 intervening objects
            if (ResolvedOsCount + 1 != compCount) // Although there is one more ObservableSource that path components, the terminal node should not be listening.
            {
                // TODO: Instead of evaluating only the top-level,
                // see how far down the chain we can go before we run out of info.
                System.Diagnostics.Debug.WriteLine("Not enough, or too many, Change Listeners. We are creating a binding to the DataContext itself.");

                System.Diagnostics.Debug.Assert(ResolvedOsCount > 0, "There are no active listeners!");

                //isPropBagBased = IsThisNodePropBagBased(dsChangeListeners[0]);


                //return CreateDefaultBinding(bInfo, sourceType, propertyType);
                //PropertyPath pathSelf = new PropertyPath("", new object[0]);
                //return CreateBinding(bInfo.PropertyPath, bInfo, false, typeof(object), propertyType);
                return null;
            }

            StringBuilder sb = new StringBuilder();


            for (int nPtr = 1; nPtr < compCount; nPtr++)
            {
                
                ObservableSource nodeSource = dsChangeListeners[nPtr];
                string pathElement = nodeSource.PathElement;
                Type nodeType = nodeSource.Type;

                // Is the parent PropBagBased?
                isPropBagBased = IsThisNodePropBagBased(dsChangeListeners[nPtr - 1]);

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

            return result;
        }

        private Binding CreateDefaultBinding(PropertyPath newPath, MyBindingInfo bInfo, Type sourceType, Type propertyType)
        {

            Binding binding = new Binding
            {
                Path = newPath,
                Converter = bInfo.Converter,
                ConverterParameter = bInfo.ConverterParameterBuilder(bInfo, sourceType, propertyType),
                ConverterCulture = bInfo.ConverterCulture
            };

            return binding;
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

            ResolvedOsCount = RefreshPathListeners(os, e, _dataSourceChangeListeners, _targetObject, _bindingInfo);

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
                    System.Diagnostics.Debug.WriteLine("MyBinder set a new binding on some target that is replacing an existing binding.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("MyBinder set a new binding on some target that had no binding previously.");
                }
            }
            else
            {
                if (oldBinding != null)
                {
                    System.Diagnostics.Debug.WriteLine("MyBinder removed binding on some target and replaced it with no binding.");
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
            object source,
            string elementName,
            RelativeSource relativeSource,
            string pathElement)
        {
            ObservableSourceProvider osp;

            if (source != null && GetSourceFromSource(source, pathElement, out osp))
            {
                return osp;
            }
            else if (relativeSource != null && GetSourceFromRelativeSource(targetObject, relativeSource, pathElement, out osp))
            {
                return osp;
            }
            else if (elementName != null && GetSourceFromELementName(targetObject, elementName, pathElement, out osp))
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
            else if (source is DependencyObject)
            {
                osp = new ObservableSourceProvider(source as DependencyObject, pathElement);
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

        /// <summary>
        /// Finds a Dependency Object and uses it as the Data Source.
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="relativeSource"></param>
        /// <param name="osp"></param>
        /// <returns></returns>
        private bool GetSourceFromRelativeSource(DependencyObject targetObject,
            RelativeSource relativeSource, string pathElement, out ObservableSourceProvider osp)
        {
            switch (relativeSource.Mode)
            {
                case RelativeSourceMode.Self:
                    {
                        osp = new ObservableSourceProvider(targetObject as DependencyObject, pathElement);
                        return true;
                    }
                default:
                    {
                        throw new InvalidOperationException("Only RelativeSourceMode of 'Self' is supported.");
                    }
            }
        }

        private bool GetSourceFromELementName(DependencyObject targetObject,
            string elementName,
            string pathElement,
            out ObservableSourceProvider osp)
        {
            osp = null;

            object element = GetObjFromFrameworkElement(targetObject, elementName);

            if (element is DependencyObject depObject)
            {
                if (GetSourceFromDepObject(depObject, pathElement, out osp))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (element is DataSourceProvider dsp)
            {
                osp = new ObservableSourceProvider(dsp, pathElement);
                return true;
            }
            else
            {
                return false;
            }
        }

        private object GetObjFromFrameworkElement(DependencyObject targetObject, string elementName)
        {
            if(targetObject is FrameworkElement fe)
            {
                return fe.FindName(elementName);
            }
            else if(targetObject is FrameworkContentElement fce)
            {
                return fce.FindName(elementName);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Uses the Framework Element's DataContext as the source.
        /// It listens to the Framework Elemen'ts DataContext changed event.
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="osp"></param>
        /// <returns></returns>
        private bool GetSourceFromDepObject(DependencyObject targetObject, string pathElement, out ObservableSourceProvider osp)
        {
            osp = new ObservableSourceProvider(targetObject, pathElement);
            return true;
        }

        private bool GetDefaultSource(DependencyObject targetObject, string node, out ObservableSourceProvider osp)
        {
            // Just for debug
            if(targetObject is FrameworkElement feTest)
            {
                if(feTest.Name == "TestGrid1")
                {
                    System.Diagnostics.Debug.WriteLine("Received Grid TestGrid1");
                }
            }
            // End just for debug


            if (targetObject is FrameworkElement fe)
            {
                osp = new ObservableSourceProvider(fe, node);
            }
            else if (targetObject is FrameworkContentElement fce)
            {
                osp = new ObservableSourceProvider(fce, node);
            }
            else
            {
                throw new ApplicationException("targetObject in MyBinder.ObservableSourceProvider was neither a FrameworkElement or a FrameworkContentElement.");
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
            //        throw new ApplicationException("Found node in MyBinder.ObservableSourceProvider was neither a FrameworkElement or a FrameworkContentElement.");
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

            //private bool _containerWasNull;
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

            //private bool _dataWasNull;
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

            //Lazy<object> _dataPromise;
            //object _data;
            //public object Data
            //{
            //    get
            //    {
            //        if (_data != null)
            //        {
            //            return _data;
            //        }

            //        if (_dataPromise != null)
            //        {
            //            System.Diagnostics.Debug.WriteLine("Accessing a Lazy<object> to get the data of an ObservableSource.");
            //            _data = _dataPromise.Value;
            //        }

            //        return _data;

            //        //if(_data is DataSourceProvider)
            //        //{
            //        //    return ((DataSourceProvider)_data).Data;
            //        //} 
            //        //else
            //        //{
            //        //    return _data;
            //        //}
            //    }
            //    private set
            //    {
            //        if (value is Lazy<object>)
            //        {
            //            _dataPromise = (Lazy<object>)value;
            //            _data = null;
            //        }
            //        else
            //        {
            //            _data = value;
            //            _dataPromise = null;
            //        }

            //    }
            //}

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

            public SourceKindEnum SourceKind { get; private set; }

            public bool IsListening { get; private set; }

            public bool IsDcListeningToProp { get; private set; }
            public bool IsDcListeningToColl { get; private set; }

            public bool IsPropBagBased
            {
                get
                {
                    Type test = Type;
                    return test == null ? false : test.IsPropBagBased();
                }
            }

            public bool IsPossiblyPropBagBased
            {
                get
                {
                    Type test = Type;
                    return test == null ? true : test.IsPropBagBased();
                }
            }

            #endregion

            #region Public Methods

            //public void ReleaseData()
            //{
            //    // Make sure we get the type, before we let go of our reference.
            //    if (_type == null)
            //    {
            //        _type = GetTypeOfData(Data);
            //    }

            //    Data = null;
            //}

            public void ThrowAway()
            {
                // TODO: unregister all of the event handlers.
            }

            public void UpdateData()
            {
                if( !(SourceKind == SourceKindEnum.DataContext || SourceKind == SourceKindEnum.DataSourceProvider))
                {
                    throw new InvalidOperationException($"Only ObservableSources with SourceKind = {nameof(SourceKindEnum.DataContext)} or {nameof(SourceKindEnum.DataSourceProvider)} can have their data updated.");
                }

                object oldData = Data;
                object newData;

                if(SourceKind == SourceKindEnum.DataSourceProvider)
                {
                    newData = ((DataSourceProvider)Container).Data;
                } 
                else
                {
                    if (!GetDcFromFrameworkElement(Container, out newData))
                    {
                        throw new ApplicationException("targetObject in MyBinder.ObservableSourceProvider was neither a FrameworkElement or a FrameworkContentElement.");
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
                    if (oldData != null && (IsDcListeningToProp || IsDcListeningToColl))
                    {
                        RemoveSubscriptions(oldData);
                    }

                    Data = newData;
                    Type = newData.GetType();
                }
                else
                {
                    // Our SourceKind must be DataSourceProvider.

                    // Remove event hander from old data if present.
                    if (oldData!= null && (IsDcListeningToProp || IsDcListeningToColl))
                    {
                        RemoveSubscriptions(oldData);
                    }

                    Data = newData;
                    Type = newData.GetType();
                }
            }

            public ObservableSource GetChild(string pathElement)
            {
                switch (SourceKind)
                {
                    case SourceKindEnum.PropertyObject:
                        {
                            goto case SourceKindEnum.DataContext;
                        }
                    case SourceKindEnum.CollectionObject:
                        {
                            goto case SourceKindEnum.DataContext;
                        }
                    case SourceKindEnum.DataContext:
                        {
                            object data = this.Data;
                            //if (data == null) return null;

                            Type parentType = this.Type;

                            if (parentType == null)
                            {
                                //throw new InvalidOperationException("Cannot get type, but data is non-null.");
                                return null;
                            }

                            if (IsPropBagBased)
                            {
                                IPropBagMin pb = (IPropBagMin)data;
                                object newData;
                                Type newType;

                                // TODO: need to Get method to set value.
                                if (pb.TryGetPropGen(pathElement, null, out IPropGen iPg))
                                {
                                    if (iPg is PropGen)
                                    {
                                        newData = ((PropGen)iPg).TypedProp?.TypedValueAsObject;
                                        newType = ((PropGen)iPg).Type;
                                    }
                                    else
                                    {
                                        //IPropGen pg = pb.GetPropGen(propertyName, pt);
                                        newData = iPg?.TypedProp?.TypedValueAsObject;
                                        newType = iPg?.Type;
                                    }
                                    if (newData == null)
                                    {
                                        if(pb.TryGetTypeOfProperty(pathElement, out newType))
                                        {
                                            // Create an ObservableSource with SourceKind = TerminalNode.
                                            return new ObservableSource(pathElement, newType);
                                        }
                                        else
                                        {
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        ObservableSource child = CreateChild(newData, newType, pathElement);
                                        return child;
                                    }
                                }
                                else
                                {
                                    if (pb.TryGetTypeOfProperty(pathElement, out newType))
                                    {
                                        // Create an ObservableSource with SourceKind = TerminalNode.
                                        return new ObservableSource(pathElement, newType);
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            }
                            else
                            {
                                if(data != null)
                                {
                                    object val = GetMemberValue(pathElement, data, parentType, this.PathElement,
                                        out Type pt);

                                    // TOOD: need to use Reflection to get the getter
                                    // for this property.

                                    // TODO: Get the actual value from the property at pathElement.
                                    // Currently this results in a new terminal node, everytime.


                                    ObservableSource child = CreateChild(val, pt, pathElement);
                                    return child;
                                }
                                else
                                {
                                    // Just get the type, if possible.
                                    Type pt = GetTypeOfPathElement(pathElement, parentType, this.PathElement);

                                    // Create an ObservableSource with SourceKind = TerminalNode.
                                    return new ObservableSource(pathElement, pt);
                                }

                            }

                        }
                    //case SourceKindEnum.DataSourceProvider:
                    //    {
                    //        break;
                    //    }
                    //case SourceKindEnum.DependencyObject:
                    //    {
                    //        break;
                    //    }
                    default:
                        {
                            throw new ApplicationException($"Get Child from ObservableSource does not support SourceKind {SourceKind}.");
                        }

                }
            }

            public bool Subscribe(DataSourceChangedEventHandler subscriber)
            {
                if (DataSourceChanged == null)
                {
                    DataSourceChanged = subscriber;
                    //DataSourceChanged += subscriber; // We added it.
                    return true;
                }
                else
                {
                    Delegate[] subscriberList = DataSourceChanged.GetInvocationList();
                    if (subscriberList.FirstOrDefault((x) => x == (Delegate)subscriber) == null)
                    {
                        DataSourceChanged += subscriber; // We added it.
                        return true;
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
                        DataSourceChanged -= subscriber; // We added it.
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
                            throw new ApplicationException($"SourceKind value: {SourceKind} is not supported or is not recognized on call to BeginListeningToSource.");
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
                            throw new ApplicationException($"SourceKind value: {SourceKind} is not supported or is not recognized on call to StopListeningToSource.");
                        }
                }
            }

            #endregion

            #region Private Methods

            private ObservableSource CreateChild(object data, Type type, string pathElement)
            {
                if (typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                {
                    return new ObservableSource(data as INotifyPropertyChanged, pathElement);
                }
                else if (typeof(INotifyCollectionChanged).IsAssignableFrom(type))
                {
                    return new ObservableSource(data as INotifyCollectionChanged, pathElement);
                }
                else if (type == typeof(DataSourceProvider))
                {
                    return new ObservableSource(data as DataSourceProvider, pathElement);
                }

                throw new InvalidOperationException("Cannot create child: it does not implement INotifyPropertyChanged or INotifyCollectionChanged.");
            }

            private bool GetDcFromFrameworkElement(object feOrFce, out object dc)
            {
                if (feOrFce is FrameworkElement fe)
                {
                    dc = fe.DataContext;
                    return true;
                }
                else if (feOrFce is FrameworkContentElement fce)
                {
                    dc = fce.DataContext;
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
                    IsDcListeningToProp = true;
                }

                if (dc is INotifyCollectionChanged cc)
                {
                    WeakEventManager<INotifyCollectionChanged, CollectionChangeEventArgs>
                        .AddHandler(cc, "CollectionChanged", OnCCEvent);
                    IsDcListeningToColl = true;
                }

                if (!(IsDcListeningToColl || IsDcListeningToProp))
                {
                    throw new ApplicationException("Cannot create subscriptions. Object does not implement INotifyPropertyChanged, nor does it implement INotifyCollectionChanged.");
                }
            }

            private void RemoveSubscriptions(object dc)
            {
                bool removedIt = false;
                if (dc is INotifyPropertyChanged pc)
                {
                    WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
                        .RemoveHandler(pc, "PropertyChanged", OnPCEvent);
                    IsDcListeningToProp = false;
                    removedIt = true;
                }

                if (dc is INotifyCollectionChanged cc)
                {
                    WeakEventManager<INotifyCollectionChanged, CollectionChangeEventArgs>
                        .RemoveHandler(cc, "CollectionChanged", OnCCEvent);
                    IsDcListeningToColl = false;
                    removedIt = true;
                }

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
                IsListening = isListening;

                _wrData = null;
                _wrContainer = null;
                
                NewPathElement = null;
                Type = null;

                IsDcListeningToProp = false;
                IsDcListeningToColl = false;
            }

            #region Empty 
            public ObservableSource(string pathElement)
                : this(SourceKindEnum.Empty, pathElement, false)
            {
                Data = null;
                Type = null;
            }
            #endregion

            #region Terminal Node 
            public ObservableSource(string pathElement, Type type)
                : this(SourceKindEnum.TerminalNode, pathElement, false)
            {
                Data = null;
                Type = type;
            }
            #endregion
            
            //#region Use Lazy 
            //public ObservableSource(Lazy<object> data, Type type, SourceKindEnum sourceKind, string pathElement)
            //{
            //    Data = data;
            //    Type = type;
            //    SourceKind = sourceKind;
            //    PathElement = pathElement;
            //}
            //#endregion

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

                //Data = AddSubscriptions(fe.DataContext);
                //if(fe.DataContext != null)
                //{
                //    WireUpDc(fe.DataContext);
                //}
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

                //Data = AddSubscriptions(fce.DataContext);
                //if(fce.DataContext != null)
                //{
                //    WireUpDc(fe.DataContext);
                //}
            }

            private void Fe_or_fce_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                OnDataSourceChanged(DataSourceChangeTypeEnum.DataContextUpdated);
            }
            #endregion

            #region From INotifyPropertyChanged
            public ObservableSource(INotifyPropertyChanged itRaisesPropChanged, string pathElement)
                : this(SourceKindEnum.PropertyObject, pathElement, true)
            {
                Data = itRaisesPropChanged ?? throw new ArgumentNullException($"{nameof(itRaisesPropChanged)} was null when constructing Observable Source.");
                Type = itRaisesPropChanged.GetType();

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
                : this(SourceKindEnum.CollectionObject, pathElement, true)
            {
                Data = itRaisesCollectionChanged ?? throw new ArgumentNullException($"{nameof(itRaisesCollectionChanged)} was null when constructing Observable Source.");
                Type = itRaisesCollectionChanged.GetType();

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
            }

            private void OnPlainEvent(object source, EventArgs args)
            {
                OnDataSourceChanged(DataSourceChangeTypeEnum.DataContextUpdated);
            }
            #endregion

            #region From Dependency Object
            // TODO: Remove this, or make it work using DependencyObject call backs.
            public ObservableSource(DependencyObject depObj, string pathElement)
                : this(SourceKindEnum.DependencyObject, pathElement, false)
            {
                //Data = depObj;
                //Type = null;

                throw new InvalidOperationException("Creating an ObservableSource from a DependencyObject is not yet supported.");
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
                    case SourceKindEnum.DependencyObject: return new ObservableSource((DependencyObject)this.Data, this.PathElement);
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

            #region From Dependency Object
            public ObservableSourceProvider(DependencyObject depObj, string pathElement)
            {
                Data = depObj;
                Type = null;
                SourceKind = SourceKindEnum.DependencyObject;
                PathElement = pathElement;
            }
            #endregion

            #endregion Constructors and their handlers
        }

        #endregion
    }



}
