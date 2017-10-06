
using DRM.PropBag.ControlModel;
using DRM.PropBag.ViewModelBuilder;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

using DRM.PropBag.ControlsWPF.WPFHelpers;
using System.Text;

/// <remarks>
/// This is loosely based on the code available from
/// https://www.codeproject.com/Articles/356294/Using-path-parameters-when-binding-data-in-WPF
/// Written by Michael Soloduha.
/// License: "The Code Project Open License (CPOL)
/// </remarks>

namespace DRM.PropBag.ControlsWPF.Binders
{
    [MarkupExtensionReturnType(typeof(object)), Localizability(LocalizationCategory.None, Modifiability = Modifiability.Unmodifiable, Readability = Readability.Unreadable)]
    public class BindingExtension : MarkupExtension
    {
        #region Member Declarations

        DependencyObject _targetObject;
        DependencyProperty _targetProperty;

        List<ObservableSource> _dataSourceChangeListeners;

        /// <summary>
        /// Type of the binding source, i.e., DataContext
        /// </summary>
        private Type SourceRootType
        {
            get
            {
                return _dataSourceChangeListeners.Count > 0 ? _dataSourceChangeListeners[0].Type : null;
            }
        }

        /// <summary>
        /// True if binding source implements IPropBag
        /// </summary>
        private bool? IsPropBagBased
        {
            get
            {
                //return SourceType?.IsPropGenBased();
                return _dataSourceChangeListeners.Count > 0 ? _dataSourceChangeListeners[0].IsPropBagBased : null;
            }
        }

        #endregion

        #region Constructors

        public BindingExtension() : this(null) { }

        public BindingExtension(string path)
        {
            Path = path;
            SourceType = null;
            Source = null;
            ElementName = null;
            Mode = BindingMode.Default;
        }

        #endregion

        #region Public Properties

        [DefaultValue(null), ConstructorArgument("path")]
        public string Path { get; set; }

        public string ElementName { get; set; }

        public object Source { get; set; }

        public RelativeSource RelativeSource { get; set; }

        public Type SourceType { get; set; }

        public BindingMode Mode { get; set; }


        #endregion

        #region Provide Value Implementation

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (SourceType == null)
            {
                throw new InvalidOperationException("The SourceType must be specified.");
            }

            // Get the Target Object and Target Property
            if (!SetOurEnv(serviceProvider))
                return this;

            // Used to listen to changes to the sources of data
            // for each step in the path.
            _dataSourceChangeListeners = GetPathListeners(serviceProvider,
                _targetObject, Source, ElementName, RelativeSource, Path);

            BindingBase bb = GetTheBinding(_targetObject, _targetProperty, Path, _dataSourceChangeListeners, SourceType);

            foreach (ObservableSource oss in _dataSourceChangeListeners)
            {
                oss.ReleaseData();
            }

            // create wpf binding
            MyMultiValueConverter mValueConverter = new MyMultiValueConverter(Mode);

            mValueConverter.Add(bb);

            // return the expression provided by the multi-binding
            MultiBindingExpression exp = mValueConverter.GetMultiBindingExpression(serviceProvider);

            return exp;
        }

        private bool SetOurEnv(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) return false;

            IProvideValueTarget provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (provideValueTarget == null) return false;

            _targetObject = provideValueTarget.TargetObject as DependencyObject;
            if (_targetObject == null) return false;

            _targetProperty = provideValueTarget.TargetProperty as DependencyProperty;
            return true;
        }

        #endregion

        #region Path Analysis

        private List<ObservableSource> GetPathListeners(IServiceProvider serviceProvider,
            DependencyObject targetObject,
            object source,
            string elementName,
            RelativeSource relativeSource,
            string path)
        {
            List<ObservableSource> result =  new List<ObservableSource>();

            ObservableSource os = GetSourceRoot(serviceProvider, targetObject, source,
                elementName, relativeSource);

            if(os == null)
            {
                System.Diagnostics.Debug.WriteLine($"No DataSouce found, therefore, no listner was created for {_targetProperty.ToString()}.");
                return result;
            }

            os.DataSourceChanged += DataSourceHasChanged;

            // Save the event source.
            result.Add(os);
            System.Diagnostics.Debug.WriteLine($"Listner was created for {_targetProperty.ToString()}.");

            string[] nodes = GetPathComponents(path, out int compCount);

            foreach(string node in nodes)
            {
                os = os.GetChild(node);

                if (os == null)
                {
                    System.Diagnostics.Debug.WriteLine($"No DataSouce found, therefore, no listner was created for {_targetProperty.ToString()}.");
                    break;
                }

                os.DataSourceChanged += DataSourceHasChanged;

                // Save the event source.
                result.Add(os);
                System.Diagnostics.Debug.WriteLine($"Listner was created for {_targetProperty.ToString()}.");

            }

            return result;
        }

        private string[] GetPathComponents(string path, out int count)
        {
            string[] components = path.Split('.');
            count = components.Length;
            return components;
        }

        #endregion

        #region Build Bindings

        private BindingBase GetTheBinding(DependencyObject targetObject, DependencyProperty targetProperty,
            string path, List<ObservableSource> dsChangeListeners, Type sourceType)
        {
            string[] pathNameComponents = GetPathComponents(path, out int compCount);

            //if (compCount > 2) throw new ApplicationException("Path cannot have more than two components.");

            if (compCount > 1)
            {
                //throw new ApplicationException("Path cannot have more than two components.");
                System.Diagnostics.Debug.WriteLine("Handling a two parter.");
            }

            if(compCount != dsChangeListeners.Count)
            {
                throw new ApplicationException("Not enough, or too many, Change Listeners.");
            }

            StringBuilder sb = new StringBuilder();
            bool isPropBagBased = false;

            for (int nPtr = 0; nPtr < compCount; nPtr++)
            {
                if (nPtr > 0) sb.Append(".");

                string node = pathNameComponents[nPtr];
                ObservableSource os = dsChangeListeners[nPtr];

                isPropBagBased = os.IsPropBagBased.HasValue && os.IsPropBagBased.Value;

                if (isPropBagBased)
                {
                    sb.Append($"[{os.Type.FullName},{node}]");
                }
                else
                {
                    sb.Append(node);
                }
            }

            string newPath = sb.ToString();

            Binding binding = null;
            Type propertyType = targetProperty.PropertyType;

            if (isPropBagBased)
            {
                // We are going to assume that it has (or soon will be) registered
                // as a "virtual" IProp<T>

                System.Diagnostics.Debug.Assert(sourceType != null, "The SourceType should never be null here.");

                binding = new Binding
                {
                    Path = new PropertyPath(newPath),
                    Converter = new PropValueConverter(),
                    ConverterParameter = new TwoTypes(sourceType, propertyType)
                };
            }
            else
            {
                Type srcType = sourceType ?? typeof(object);
                binding = CreateDefaultBinding(path, srcType, propertyType);
            }

            BindingExpressionBase bExp = 
                BindingOperations.SetBinding(_targetObject, _targetProperty, binding);

            return binding;
        }

        #endregion

        #region Data Source Changed Handing

        private void DataSourceHasChanged(object sender, DataSourceChangedEventArgs e)
        {
            ObservableSource os = (ObservableSource)sender;

            int stepNo =_dataSourceChangeListeners.IndexOf(os);

            if(stepNo == -1)
            {
                System.Diagnostics.Debug.WriteLine($"Could not find stepNo on DataSourceChanged in PropBagControlsWPF.Binders.MyBinder.");
                throw new InvalidOperationException("Could not get stepno.");
            }
           

            //BindingBase newBinding = CreateBindingForPathElement(Path, _targetObject,
            //    _targetProperty, os, SourceType);

            //Binding oldBinding = BindingOperations.GetBinding(_targetObject, _targetProperty);

            //if (oldBinding != null)
            //{
            //    BindingOperations.ClearBinding(_targetObject, _targetProperty);
            //}

            //BindingExpressionBase bExp = BindingOperations.SetBinding(_targetObject, _targetProperty, newBinding);

        }

        #endregion

        #region Create Binding For an Element

        //// TODO: Create a struct with Path, Source, ElementName, Converter, etc.
        //// and all such required to build a binding.
        //private BindingBase CreateBindingForPathElement(string path,
        //    DependencyObject targetObject, DependencyProperty targetProperty,
        //    List<ObservableSource> obSources, Type sourceType)
        //{
        //    Binding binding = null;
        //    Type propertyType = targetProperty.PropertyType;

        //    bool isPropBagBased = IsPropBagBased.HasValue && IsPropBagBased.Value;

        //    //isPropBagBased = true;

        //    //if (isPropBagBased && !IsMemberReal(path, sourceType))
        //    if(isPropBagBased)
        //    {
        //        // We are going to assume that it has (or soon will be) registered
        //        // as a "virtual" IProp<T>

        //        System.Diagnostics.Debug.Assert(sourceType != null, "The SourceType should never be null here.");

        //        //string newPath = sourceType.FullName + "\\," + path;
        //        string newPath = $"[{sourceType.FullName},{path}]";
        //        binding = new Binding
        //        {
        //            Path = new PropertyPath(newPath),
        //            Converter = new PropValueConverter(),
        //            ConverterParameter = new TwoTypes(sourceType, propertyType)
        //        };
        //    }
        //    else
        //    {
        //        Type srcType = sourceType ?? typeof(object);
        //        binding = CreateDefaultBinding(path, srcType, propertyType);
        //    }

        //    System.Diagnostics.Debug.Assert(binding != null, "CreateBindingForPathElement is returning a null binding.");

        //    return binding;
        //}

        private Binding CreateDefaultBinding(string path, Type sourceType, Type propertyType)
        {
            Binding binding = new Binding
            {
                Path = new PropertyPath(path),
                Converter = new PropValueConverter(),
                ConverterParameter = new TwoTypes(sourceType, propertyType)
            };

            return binding;
        }

        #endregion

        #region GetSource Support

        private ObservableSource GetSourceRoot(IServiceProvider serviceProvider,
            DependencyObject targetObject,
            object source,
            string elementName,
            RelativeSource relativeSource)
        {
            ObservableSource obSrc;

            if (source != null && GetSourceFromSource(source, out obSrc))
            {
                return obSrc;
            }
            else if(relativeSource != null && GetSourceFromRelativeSource(targetObject, relativeSource, out obSrc))
            {
                return obSrc;
            }
            else if(elementName != null && GetSourceFromELementName(targetObject, elementName, out obSrc))
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
            if(source is DataSourceProvider)
            {
                obSrc = new ObservableSource(source as DataSourceProvider);
                return true;
            }
            else if(source is DependencyObject)
            {
                obSrc = new ObservableSource(source as DependencyObject);
                return true;
            }
            else if(source is INotifyPropertyChanged)
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
            switch(relativeSource.Mode)
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
                obSrc = null;
                return false;
            } 
            else
            {
                if(foundNode is FrameworkElement fe)
                {
                    obSrc = new ObservableSource(fe);
                }
                else if(foundNode is FrameworkContentElement fce)
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

        #region Type Processing

        // TODO: This needs to use a PropModel
        // Note: This is only called for types that derive from IPropBagMin.

        /// <summary>
        /// Returns true if the member exists and was not created by 
        /// our ViewModel builder.
        /// </summary>
        /// <param name="pathElement"></param>
        /// <returns></returns>
        private bool IsMemberReal(string pathElement, Type declaringType)
        {
            if(declaringType == null)
            {
                return false;
            }

            PropertyInfo pi = declaringType.GetDeclaredProperty(pathElement);
            if (pi == null)
            {
                //throw new InvalidOperationException($"The source path {pathElement} cannot be found.");
                return false;
            }

            bool isPropBagBased = IsPropBagBased.HasValue && IsPropBagBased.Value;

            if (!isPropBagBased || !declaringType.IsEmittedProxy())
            {
                return true;
            }
            else
            {
                // Some Members may be emitted, check this one's provenance.
                IEnumerable<WasEmittedAttribute> attributes = pi.GetCustomAttributes<WasEmittedAttribute>();

                // If there are no "WasEmittedAttribute" then it must be real.
                return attributes.Count() == 0;
            }
        }

        //private bool PropertyExists(string pathElement, Type declaringType)
        //{
        //    return null != declaringType.GetDeclaredProperty(pathElement);
        //}

        //private Type GetTypeOfParentElement(string pathElement, object source)
        //{
        //    Type result;

        //    bool isPropBagBased = IsPropBagBased.HasValue && IsPropBagBased.Value;
        //    if (isPropBagBased)
        //    {
        //        // TODO: implement a surrogate for System.Reflection for IPropBagMin.
        //        result = null;
        //    }
        //    else
        //    {
        //        // Attempt to use reflection to get the type of the intervening
        //        // path components.
        //        result = GetTypeOfPathElement(pathElement, source);
        //    }

        //    return result;
        //}



        #endregion

        public enum SourceKindEnum
        {
            DataContext,
            DependencyObject,
            PropertyObject,
            CollectionObject,
            DataSourceProvider
        }

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
                    if(_data != null)
                    {
                        return _data;
                    }

                    if(_dataPromise != null)
                    {
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
                    if(value is Lazy<object>)
                    {
                        _dataPromise = (Lazy<object>) value;
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

            public bool? IsPropBagBased
            {
                get
                {
                    return Type?.IsPropBagBased();
                }
            }

            //bool? _isPropBagBased;
            //public bool? IsPropBagBased
            //{
            //    get
            //    {
            //        if(_isPropBagBased.HasValue)
            //        {
            //            return _isPropBagBased;
            //        }
            //        else
            //        {
            //            return Type?.IsPropBagBased();
            //        }
            //    }
            //    private set
            //    {
            //        _isPropBagBased = value;
            //    }
            //}

            #endregion

            #region Public Methods

            public void ReleaseData()
            {
                // Make sure we get the type, before we let go of our reference.
                if(_type == null)
                {
                    _type = GetTypeOfData(Data);
                }

                Data = null;
            }

            public ObservableSource GetChild(string propertyName)
            {
                switch(SourceKind)
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
                            bool isPropBagBased = IsPropBagBased.HasValue && IsPropBagBased.Value;

                            if (isPropBagBased)
                            {
                                IPropBagMin pb = (IPropBagMin)Data;
                                IPropGen pg = pb.GetPropGen(propertyName, null);
                                Lazy<object> data = new Lazy<object>(() => pg.Value);
                                //bool isPropBag = pg.Type.IsPropBagBased();

                                return new ObservableSource(data, pg.Type);
                            } 
                            else
                            {
                                Type = GetTypeOfPathElement(propertyName, this.Type);


                            }
                            return null;
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


}

}

