
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

/// <remarks>
/// This is a heavily-modified version of the code available from
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

        MyMultiValueConverter _mainMultiBinding;
        List<ObservableSource> _dataSourceChangeListeners;

        MultiBindingExpression _multiBindingExpression;
        DependencyObject _targetObject;
        DependencyProperty _targetProperty;

        string _path;
        Type _sourceType; // Type of the source property

        object _source;
        string _elementName;
        RelativeSource _relativeSource;
        BindingMode _mode;

        /// <summary>
        /// Type of the binding source, i.e., DataContext
        /// </summary>
        public Type SourceRootType
        {
            get
            {
                return _dataSourceChangeListeners?[0]?.Type;
            }
        }

        /// <summary>
        /// True if binding source implements IPropBag
        /// </summary>
        public bool? IsPropBagBased
        {
            get
            {
                return _dataSourceChangeListeners?[0]?.IsPropGenBased;
            }
        }

        #endregion

        #region Constructors

        public BindingExtension() : this(null) { }

        public BindingExtension(string path)
        {
            _path = path;
            _sourceType = null;
            _source = null;
            _elementName = null;
            _mode = BindingMode.Default;

            _dataSourceChangeListeners = new List<ObservableSource>();
        }

        //public BindingExtension(string path, Type sourceType, object source, string elementName = null)
        //{
        //    _path = path;
        //    _sourceType = sourceType;
        //    _source = source;
        //    _elementName = elementName;
        //    _mode = BindingMode.Default;
        //}

        #endregion

        #region Public Properties

        [DefaultValue(null), ConstructorArgument("path")]
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public string ElementName
        {
            get { return _elementName; }
            set { _elementName = value; }
        }

        public object Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public RelativeSource RelativeSource
        {
            get { return _relativeSource; }
            set { _relativeSource = value; }
        }

        public Type SourceType
        {
            get { return _sourceType; }
            set { _sourceType = value; }
        }

        public BindingMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        #endregion

        #region Provide Value Implementation

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) return this;

            if (!SetOurEnv(serviceProvider)) return this;

            // create wpf binding
            _mainMultiBinding = new MyMultiValueConverter(_mode);

            if (!GetSourceRoot(Source, ElementName, _targetObject,
                _relativeSource, serviceProvider, out ObservableSource os))
            {
                // TODO Consider throwing an exception.
                return null;
            }

            if (os.Data == null) return null;

            os.DataSourceChanged += DataSourceHasChanged;

            // Save the event source.
            _dataSourceChangeListeners.Add(os);

            BindingBase bb = GetTheBinding(Path, _dataSourceChangeListeners);

            _mainMultiBinding.Add(bb);

            // return the expression 
            // provided by the multi-binding
            _multiBindingExpression = _mainMultiBinding.GetMultiBindingExpression(serviceProvider);

            foreach(ObservableSource oss in _dataSourceChangeListeners)
            {
                oss.ReleaseData();
            }

            return _multiBindingExpression;
            //return null;
        }

        private bool SetOurEnv(IServiceProvider serviceProvider)
        {
            IProvideValueTarget provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (provideValueTarget == null) return false;

            _targetObject = provideValueTarget.TargetObject as DependencyObject;
            if (_targetObject == null) return false;

            _targetProperty = provideValueTarget.TargetProperty as DependencyProperty;
            return true;
        }

        private BindingBase GetTheBinding(string path, List<ObservableSource> dsChangeListeners)
        {
            string[] pathNameComponents = path.Split('.');
            //if (pathNameComponents.Length > 2) throw new ApplicationException("Path cannot have more than two components.");

            if (pathNameComponents.Length > 1)
            {
                System.Diagnostics.Debug.WriteLine("Handling a two parter.");
            }

            ObservableSource parentElementSource = dsChangeListeners[0];

            int compCount = pathNameComponents.Length;

            // Subscribe to DataSource changes for all intermediate path components
            for (int pPtr = 1; pPtr < compCount - 1; pPtr++)
            {
                string node = pathNameComponents[pPtr];
                System.Diagnostics.Debug.WriteLine($"We are handling intermediate path element: {node} at index {pPtr}");

                // need to use the binding just created to get the value of this.
                //parentElementSource = 
            }

            // Now build a binding for the last component.
            string pathElement = pathNameComponents[pathNameComponents.Length - 1];

            BindingBase ourBinding = CreateBindingForPathElement(pathElement, _targetObject,
                _targetProperty, parentElementSource, SourceType);

            return ourBinding;
        }

        private void DataSourceHasChanged(object sender, DataSourceChangedEventArgs e)
        {
            BindingBase b = GetTheBinding(Path, _dataSourceChangeListeners);

            //Collection<BindingBase> x = _mainMultiBinding.MultiBinding.Bindings;

            //BindingBase y = x.Last();
            //Binding z = (Binding)y;

            //x[x.Count - 1] = b;

            //y = x.Last();
            //z = (Binding)y;
        }

        #endregion

        #region Custom Binding Work

        private Type GetTypeOfParentElement(string pathElement, object source)
        {
            Type result;

            bool isPropBagBased = IsPropBagBased.HasValue && IsPropBagBased.Value;
            if (isPropBagBased)
            {
                // TODO: implement a surrogate for System.Reflection for IPropBagMin.
                result = null;
            }
            else
            {
                // Attempt to use reflection to get the type of the intervening
                // path components.
                result = GetTypeOfPathElement(pathElement, source);
            }

            return result;
        }

        private Type GetTypeOfPathElement(string pathElement, object source)
        {
            if (source == null)
                return null;

            Type h = source.GetType();
            PropertyInfo pi = h.GetDeclaredProperty(pathElement);
            if(pi == null)
            {
                throw new InvalidOperationException($"{pathElement} does not exist on data source: {source.ToString()}.");
            }
            try
            {
                return pi.PropertyType;
            } 
            catch
            {
                throw new InvalidOperationException($"Cannot get the type from {pathElement} on source: {source.ToString()}.");
            }
        }

        // TODO: Create a struct with Path, Source, ElementName, Converter, etc.
        // and all such required to build a binding.
        private BindingBase CreateBindingForPathElement(string pathElement,
            DependencyObject targetObject, DependencyProperty targetProperty,
            ObservableSource obSrc, Type sourceType)
        {
            Binding binding;
            Type propertyType = targetProperty.PropertyType;

            bool isPropBagBased = IsPropBagBased.HasValue && IsPropBagBased.Value;

            if (isPropBagBased && !IsMemberReal(pathElement, sourceType))
            {
                // We are going to assume that it has (or soon will be) registered
                // as a "virtual" IProp<T>

                System.Diagnostics.Debug.Assert(sourceType != null, "The SourceType should never be null here.");

                string path = $"[{sourceType.FullName},{pathElement}]";
                binding = new Binding
                {
                    Path = new PropertyPath(path),
                    Converter = new PropValueConverter(),
                    ConverterParameter = new TwoTypes(sourceType, propertyType)
                };
            }
            else
            {
                Type srcType = sourceType ?? typeof(object);
                binding = CreateDefaultBinding(pathElement, srcType, propertyType);
            }

            BindingExpressionBase bExp = BindingOperations.SetBinding(targetObject, targetProperty, binding);

            return binding;
        }

        private Binding CreateDefaultBinding(string pathElement, Type sourceType, Type propertyType)
        {
            Binding binding = new Binding
            {
                Path = new PropertyPath(pathElement),
                Converter = new PropValueConverter(),
                ConverterParameter = new TwoTypes(sourceType, propertyType)
            };

            return binding;
        }

        private bool GetSourceRoot(object source,
            string elementName,
            DependencyObject targetObject,
            RelativeSource relativeSource,
            IServiceProvider serviceProvider,
            out ObservableSource obSrc)
        {
            if (source != null && GetSourceFromSource(source, out obSrc))
            {
                return true;
            }
            else if(relativeSource != null && GetSourceFromRelativeSource(targetObject, relativeSource, out obSrc))
            {
                return true;
            }
            else if(elementName != null && GetSourceFromELementName(targetObject, elementName, out obSrc))
            {
                return true;
            }
            else
            {
                return GetDefaultSource(targetObject, out obSrc);
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
                obSrc = new ObservableSource(foundNode);
                return true;
            }
        }

        private Type GetTypeOfSourceRoot(ObservableSource obSrc)
        {
            Type t = obSrc.Data.GetType();
            return t;
        }

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

        private bool PropertyExists(string pathElement, Type declaringType)
        {
            return null != declaringType.GetDeclaredProperty(pathElement);
        }

        #endregion

        #region Observable Source nested class

        public class ObservableSource
        {
            #region Member Declarations
            object _data;
            public object Data
            {
                get
                {
                    if(_data is DataSourceProvider)
                    {
                        return ((DataSourceProvider)_data).Data;
                    } 
                    else
                    {
                        return _data;
                    }
                }
                set
                {
                    _data = value;
                }
            }

            public event DataSourceChangedEventHandler DataSourceChanged = delegate {};
            #endregion

            #region Public Members

            //private Type type;
            public Type Type
            {
                get
                {
                    if(Data == null)
                    {
                        return null;
                    }
                    else
                    {
                        // TODO: Can we cache this value?
                        return Data.GetType();
                    }
                }
            }

            public bool? IsPropGenBased
            {
                get
                {
                    return Type?.IsPropGenBased();
                }
            }

            public void ReleaseData()
            {
                Data = null;
            }

            public object GetChild(string propertyName)
            {
                return null;
            }
            #endregion

            #region Constructors and their handlers

            #region From Framework Element and Framework Content Element
            public ObservableSource(FrameworkElement fe)
            {
                Data = fe.DataContext;

                //DependencyObject depObj = fe as DependencyObject;
               
                fe.DataContextChanged += Fe_DataContextChanged;
            }

            public ObservableSource(FrameworkContentElement fce)
            {
                Data = fce.DataContext;

                //DependencyObject depObj = fe as DependencyObject;

                fce.DataContextChanged += Fe_DataContextChanged;
            }

            private void Fe_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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
            }

            private void OnCCEvent(object source, CollectionChangeEventArgs args)
            {
                OnDataSourceChanged(DataSourceChangeTypeEnum.Refresh);
            }
            #endregion

            #region From DataSourceProvider
            public ObservableSource(DataSourceProvider dsp)
            {
                Data = dsp;
                dsp.DataChanged += Dsp_DataChanged;
            }

            private void Dsp_DataChanged(object sender, EventArgs e)
            {
                OnDataSourceChanged(DataSourceChangeTypeEnum.Refresh);
            }
            #endregion

            #region From Dependency Object
            public ObservableSource(DependencyObject depObj)
            {
                Data = depObj;
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
        }

        #endregion Observable Source nested class


    }

}

