using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF.WPFHelpers;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;

using MS.Internal.Data;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public class MyBindingEngine
    {
        #region Member Declarations

        MyBindingInfo _bindingInfo;
        Type _sourceType;
        DependencyObject _targetObject;
        DependencyProperty _targetProperty;

        List<ObservableSource> _dataSourceChangeListeners;

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
        }

        #endregion

        #region Provide Value

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            SetDefaultConverter();

            // Used to listen to changes to the sources of data
            // for each step in the path.
            _dataSourceChangeListeners = GetPathListeners(_targetObject, _bindingInfo);

            BindingBase bb = GetTheBinding(_targetObject, _targetProperty, _dataSourceChangeListeners,
                _sourceType, _bindingInfo);

            if (_dataSourceChangeListeners.Count > 0)
            {
                try
                {
                    BindingExpressionBase bExp =
                        BindingOperations.SetBinding(_targetObject, _targetProperty, bb);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Suppressed exception thrown when setting the binding during call to Provide Value.");
                    // Ignore the exception, we don't really need to set the binding.
                    // TODO: Is there anyway to avoid getting to here?
                }
            }

            foreach (ObservableSource oss in _dataSourceChangeListeners)
            {
                oss.ReleaseData();
            }

            // create wpf binding
            MyMultiValueConverter mValueConverter = new MyMultiValueConverter(_bindingInfo.Mode);

            mValueConverter.Add(bb);

            // return the expression provided by the multi-binding
            MultiBindingExpression exp = mValueConverter.GetMultiBindingExpression(serviceProvider);

            return exp;
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

        private List<ObservableSource> GetPathListeners(DependencyObject targetObject, MyBindingInfo bInfo)
        {
            List<ObservableSource> result = new List<ObservableSource>();

            ObservableSource os = GetSourceRoot(targetObject, bInfo.Source, bInfo.ElementName, bInfo.RelativeSource);

            if (os == null)
            {
                System.Diagnostics.Debug.WriteLine($"No DataSouce found, therefore, no listner was created for {_targetProperty.ToString()}.");
                return result;
            }

            os.DataSourceChanged += DataSourceHasChanged;

            // Save the event source.
            result.Add(os);
            System.Diagnostics.Debug.WriteLine($"Listner was created for {_targetProperty.ToString()}.");

            string[] nodes = GetPathComponents(bInfo.PropertyPath, out int compCount);

            for (int nPtr = 0; nPtr < compCount - 1; nPtr++)
            {
                string node = nodes[nPtr];
                os = os.GetChild(node);

                if (os == null)
                {
                    System.Diagnostics.Debug.WriteLine($"No DataSouce found, therefore, no listner was created for {node}.");
                    break;
                }

                os.DataSourceChanged += DataSourceHasChanged;

                // Save the event source.
                result.Add(os);
                System.Diagnostics.Debug.WriteLine($"Listner was created for {node}.");
            }

            return result;
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

            string[] pathNameComponents = GetPathComponents(bInfo.PropertyPath, out int compCount);

            if (compCount > 1)
            {
                //throw new ApplicationException("Path cannot have more than two components.");
                System.Diagnostics.Debug.WriteLine("Handling a two parter.");
            }

            // One node:    1 parent;
            // Two nodes:   1 parent, 1 intervening object
            // Three nodes:  1 parent, 2 intervening objects
            if (compCount != dsChangeListeners.Count)
            {
                // TODO: Instead of evaluating only the top-level,
                // see how far down the chain we can go before we run out of info.
                System.Diagnostics.Debug.WriteLine("Not enough, or too many, Change Listeners.");

                if (dsChangeListeners.Count > 0)
                {
                    isPropBagBased = IsThisNodePropBagBased(dsChangeListeners[0]);
                }

                //return CreateDefaultBinding(bInfo, sourceType, propertyType);
                return CreateBinding(bInfo, isPropBagBased, sourceType, propertyType);
            }

            StringBuilder sb = new StringBuilder();


            for (int nPtr = 0; nPtr < compCount; nPtr++)
            {
                string node = pathNameComponents[nPtr];

                // If this is the terminal node, use the source type given to us.
                // if its one of the intervening nodes use the type discovered by the parent Observable source.
                Type nodeType = (nPtr == compCount - 1) ? sourceType : dsChangeListeners[nPtr + 1]?.Type;

                // Is the parent PropBagBased?
                isPropBagBased = IsThisNodePropBagBased(dsChangeListeners[nPtr]);


                if (nPtr > 0) sb.Append(".");
                if (isPropBagBased)
                {
                    if(nodeType != null)
                    {
                        //IPropBagMin parent = (IPropBagMin)dsChangeListeners[nPtr].Data;

                        //int idx = parent.IndexOfProp(node, nodeType);
                        //sb.Append($"[{idx}]");
                        nodeType = nodeType ?? typeof(object);
                        sb.Append($"[{nodeType.FullName},{node}]");
                    }
                    else
                    {
                        nodeType = nodeType ?? typeof(object);
                        sb.Append($"[{nodeType.FullName},{node}]");
                    }
                }
                else
                {
                    sb.Append(node);
                }
            }

            // TODO: What about the original PropertyPath parameters?
            PropertyPath newPath = new PropertyPath(sb.ToString());
            bInfo.PropertyPath = newPath;

            return CreateBinding(bInfo, isPropBagBased, sourceType, propertyType);
        }

        private Binding CreateBinding(MyBindingInfo bInfo, bool isPropBagBased, Type sourceType, Type propertyType)
        {
            Binding result;


            if (isPropBagBased)
            {
                IValueConverter converter = GetConverter(bInfo, isPropBagBased, sourceType, propertyType, out object converterParameter);

                result = new Binding
                {
                    Path = bInfo.PropertyPath,
                    Converter = converter,
                    ConverterParameter = converterParameter
                };
            }
            else
            {
                result = CreateDefaultBinding(bInfo, sourceType, propertyType);
            }

            return result;
        }

        private Binding CreateDefaultBinding(MyBindingInfo bInfo, Type sourceType, Type propertyType)
        {

            Binding binding = new Binding
            {
                Path = bInfo.PropertyPath,
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

            _dataSourceChangeListeners = GetPathListeners(_targetObject, _bindingInfo);


            //int stepNo =_dataSourceChangeListeners.IndexOf(os);

            //if(stepNo == -1)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Could not find stepNo on DataSourceChanged in PropBagControlsWPF.Binders.MyBinder.");
            //    throw new InvalidOperationException("Could not get stepno.");
            //}

            BindingBase newBinding = GetTheBinding(_targetObject, _targetProperty, _dataSourceChangeListeners,
                _sourceType, _bindingInfo);

            //BindingBase newBinding = CreateBindingForPathElement(Path, _targetObject,
            //    _targetProperty, os, SourceType);

            Binding oldBinding = BindingOperations.GetBinding(_targetObject, _targetProperty);

            if (oldBinding != null)
            {
                BindingOperations.ClearBinding(_targetObject, _targetProperty);
            }

            BindingExpressionBase bExp = BindingOperations.SetBinding(_targetObject,
                _targetProperty, newBinding);

        }

        #endregion

        #region GetSource Support

        private ObservableSource GetSourceRoot(DependencyObject targetObject,
            object source,
            string elementName,
            RelativeSource relativeSource)
        {
            ObservableSource obSrc;

            if (source != null && GetSourceFromSource(source, out obSrc))
            {
                return obSrc;
            }
            else if (relativeSource != null && GetSourceFromRelativeSource(targetObject, relativeSource, out obSrc))
            {
                return obSrc;
            }
            else if (elementName != null && GetSourceFromELementName(targetObject, elementName, out obSrc))
            {
                return obSrc;
            }
            else
            {
                GetDefaultSource(targetObject, out obSrc);
                return obSrc;
            }

        }

        private bool GetSourceFromSource(object source, out ObservableSource obSrc)
        {
            if (source is DataSourceProvider)
            {
                obSrc = new ObservableSource(source as DataSourceProvider);
                return true;
            }
            else if (source is DependencyObject)
            {
                obSrc = new ObservableSource(source as DependencyObject);
                return true;
            }
            else if (source is INotifyPropertyChanged)
            {
                obSrc = new ObservableSource(source as INotifyPropertyChanged);
                return true;
            }
            else if (source is INotifyCollectionChanged)
            {
                obSrc = new ObservableSource(source as INotifyCollectionChanged);
                return true;
            }
            else
            {
                obSrc = null;
                return false;
            }
        }

        /// <summary>
        /// Finds a Dependency Object and uses it as the Data Source.
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="relativeSource"></param>
        /// <param name="obSrc"></param>
        /// <returns></returns>
        private bool GetSourceFromRelativeSource(DependencyObject targetObject,
            RelativeSource relativeSource, out ObservableSource obSrc)
        {
            switch (relativeSource.Mode)
            {
                case RelativeSourceMode.Self:
                    {
                        obSrc = new ObservableSource(targetObject as DependencyObject);
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
            out ObservableSource obSrc)
        {
            obSrc = null;

            if (targetObject is FrameworkElement fe)
            {
                object element = fe.FindName(elementName);
                if (element is DependencyObject depObject)
                {
                    if (GetSourceFromDepObject(depObject, out obSrc))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Uses the Framework Element's DataContext as the source.
        /// It listens to the Framework Elemen'ts DataContext changed event.
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="obSrc"></param>
        /// <returns></returns>
        private bool GetSourceFromDepObject(DependencyObject targetObject, out ObservableSource obSrc)
        {
            obSrc = new ObservableSource(targetObject);
            return true;
        }

        private bool GetDefaultSource(DependencyObject targetObject, out ObservableSource obSrc)
        {
            object dc = LogicalTree.GetDataContext(targetObject, out DependencyObject foundNode);

            if (dc == null)
            {
                if (targetObject is FrameworkElement fe)
                {
                    obSrc = new ObservableSource(fe);
                    return true;
                }
                else if (targetObject is FrameworkContentElement fce)
                {
                    obSrc = new ObservableSource(fce);
                    return true;
                }
                else
                {
                    obSrc = null;
                    return false;
                }
            }
            else
            {
                if (foundNode is FrameworkElement fe)
                {
                    obSrc = new ObservableSource(fe);
                }
                else if (foundNode is FrameworkContentElement fce)
                {
                    obSrc = new ObservableSource(fce);
                }
                else
                {
                    throw new ApplicationException("Found node in MyBinder.ObservableSource was neither a FrameworkElement or a FrameworkContentElement.");
                }

                return true;
            }
        }

        #endregion

        #region Observable Source nested class

        public class ObservableSource
        {
            #region Public events and properties

            public event DataSourceChangedEventHandler DataSourceChanged = delegate { };

            Lazy<object> _dataPromise;

            object _data;
            public object Data
            {
                get
                {
                    if (_data != null)
                    {
                        return _data;
                    }

                    if (_dataPromise != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Accessing a Lazy<object> to get the data of an ObservableSource.");
                        _data = _dataPromise.Value;
                    }

                    return _data;

                    //if(_data is DataSourceProvider)
                    //{
                    //    return ((DataSourceProvider)_data).Data;
                    //} 
                    //else
                    //{
                    //    return _data;
                    //}
                }
                private set
                {
                    if (value is Lazy<object>)
                    {
                        _dataPromise = (Lazy<object>)value;
                        _data = null;
                    }
                    else
                    {
                        _data = value;
                        _dataPromise = null;
                    }

                }
            }

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

            public bool IsPropBagBased
            {
                get
                {
                    Type test = Type;
                    return test == null ? false : test.IsPropBagBased();
                }
            }

            #endregion

            #region Public Methods

            public void ReleaseData()
            {
                // Make sure we get the type, before we let go of our reference.
                if (_type == null)
                {
                    _type = GetTypeOfData(Data);
                }

                Data = null;
            }

            public ObservableSource GetChild(string propertyName)
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
                            Type parentType = this.Type;

                            if (parentType == null)
                            {
                                return null;
                            }

                            if (IsPropBagBased)
                            {
                                IPropBagMin pb = (IPropBagMin)Data;

                                

                                if (pb.TryGetTypeOfProperty(propertyName, out Type pt))
                                {
                                    Lazy<object> data = new Lazy<object>(() =>
                                    {
                                        // TODO: need to Get method to set value.
                                        if (pb.TryGetPropGen(propertyName, null, out IPropGen iPg))
                                        {
                                            if(iPg is PropGen)
                                            {
                                                return ((PropGen)iPg).TypedProp?.TypedValueAsObject;
                                            } 
                                            else
                                            {
                                                //IPropGen pg = pb.GetPropGen(propertyName, pt);
                                                return iPg?.TypedProp?.TypedValueAsObject;
                                            }
                                        }
                                        else
                                        {
                                            return null;
                                        }

                                    });

                                    return new ObservableSource(data, pt);
                                }
                                else
                                {
                                    return null;
                                }

                            }
                            else
                            {
                                Type pt = GetTypeOfPathElement(propertyName, parentType);

                                // TOOD: need to use Reflection to get the getter
                                // for this property.
                                //Lazy<object> data = new Lazy<object>(() => 

                                return new ObservableSource(null, pt);

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
            #endregion

            #region Constructors and their handlers

            #region Use Lazy 
            public ObservableSource(Lazy<object> data, Type type)
            {
                Data = data;
                Type = type;
            }
            #endregion

            #region From Framework Element and Framework Content Element
            public ObservableSource(FrameworkElement fe)
            {
                Data = fe.DataContext;

                // TOOD: Verify that these already use weak events.
                fe.DataContextChanged += Fe_or_fce_DataContextChanged;
                SourceKind = SourceKindEnum.DataContext;
            }

            public ObservableSource(FrameworkContentElement fce)
            {
                Data = fce.DataContext;

                // TOOD: Verify that these already use weak events.
                fce.DataContextChanged += Fe_or_fce_DataContextChanged;
                SourceKind = SourceKindEnum.DataContext;
            }

            private void Fe_or_fce_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                OnDataSourceChanged(DataSourceChangeTypeEnum.Refresh);
            }
            #endregion

            #region From INotifyPropertyChanged
            public ObservableSource(INotifyPropertyChanged itRaisesPropChanged)
            {
                Data = itRaisesPropChanged;

                WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
                    .AddHandler(itRaisesPropChanged, "OnPropertyChanged", OnPCEvent);
                SourceKind = SourceKindEnum.PropertyObject;
            }

            private void OnPCEvent(object source, PropertyChangedEventArgs args)
            {
                OnDataSourceChanged(DataSourceChangeTypeEnum.Refresh);
            }
            #endregion

            #region From INotifyCollection Changed
            public ObservableSource(INotifyCollectionChanged itRaisesCollectionChanged)
            {
                Data = itRaisesCollectionChanged;

                WeakEventManager<INotifyCollectionChanged, CollectionChangeEventArgs>
                    .AddHandler(itRaisesCollectionChanged, "OnCollectionChanged", OnCCEvent);

                SourceKind = SourceKindEnum.CollectionObject;
            }

            private void OnCCEvent(object source, CollectionChangeEventArgs args)
            {
                OnDataSourceChanged(DataSourceChangeTypeEnum.Refresh);
            }
            #endregion

            #region From DataSourceProvider
            public ObservableSource(DataSourceProvider dsp)
            {
                // TODO: do we really need the data,
                // or just its type.
                Lazy<object> data = new Lazy<object>(() => dsp.Data);
                Data = data;

                WeakEventManager<DataSourceProvider, EventArgs>
                    .AddHandler(dsp, "DataChanged", OnPlainEvent);

                SourceKind = SourceKindEnum.DataSourceProvider;
            }

            private void OnPlainEvent(object source, EventArgs args)
            {
                OnDataSourceChanged(DataSourceChangeTypeEnum.Refresh);
            }
            #endregion

            #region From Dependency Object
            // TODO: Remove this, or make it work using DependencyObject call backs.
            public ObservableSource(DependencyObject depObj)
            {
                Data = depObj;

                SourceKind = SourceKindEnum.DependencyObject;

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

            #endregion Raise Event Helpers

            #region Type Support

            private Type GetTypeOfPathElement(string pathElement, Type elementType)
            {
                //if (source == null)
                //    return null;

                //Type h = source.GetType();
                PropertyInfo pi = elementType.GetDeclaredProperty(pathElement);
                if (pi == null)
                {
                    throw new InvalidOperationException($"{pathElement} does not exist on data source: .");
                }
                try
                {
                    return pi.PropertyType;
                }
                catch
                {
                    throw new InvalidOperationException($"Cannot get the type from {pathElement} on source: .");
                }
            }

            #endregion
        }

        #endregion Observable Source nested class

        //internal class MyExp : BindingExpressionBase, IWeakEventListener
        //{
        //    BindingExpression b;

        //    internal override void ChangeSourcesForChild(BindingExpressionBase bindingExpression, WeakDependencySource[] newSources)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override bool CheckValidationRules(BindingGroup bindingGroup, ValidationStep validationStep)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override object ConvertProposedValue(object rawValue)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override object GetSourceItem(object newValue)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override void HandlePropertyInvalidation(DependencyObject d, DependencyPropertyChangedEventArgs args)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override void InvalidateChild(BindingExpressionBase bindingExpression)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override bool ObtainConvertedProposedValue(BindingGroup bindingGroup)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override void ReplaceChild(BindingExpressionBase bindingExpression)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override void StoreValueInBindingGroup(object value, BindingGroup bindingGroup)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override void UpdateBindingGroup(BindingGroup bg)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override object UpdateSource(object convertedValue)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override bool UpdateSource(BindingGroup bindingGroup)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override bool ValidateAndConvertProposedValue(out Collection<ProposedValue> values)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

    }




}
