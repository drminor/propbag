
using System;
using System.Globalization;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Threading;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.ControlModel;
using System.Reflection;
using DRM.PropBag.ViewModelBuilder;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;

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
        ObservableSource _observableSource;

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
        public Type SourceRootType { get; private set; }

        /// <summary>
        /// True if binding source implements IPropBag
        /// </summary>
        public bool IsPropBagBased { get; private set; }

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

            IProvideValueTarget provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (provideValueTarget == null) return this;

            _targetObject = provideValueTarget.TargetObject as DependencyObject;
            if (_targetObject == null) return this;

            _targetProperty = provideValueTarget.TargetProperty as DependencyProperty;

            // create wpf binding
            _mainMultiBinding = new MyMultiValueConverter(_mode);

            //string pn = _path;
            //Type st = _sourceType;
            //string o = _targetObject.ToString();
            //string p = _targetProperty.ToString();


            if (!GetSourceRoot(Source, ElementName, _targetObject,
                _relativeSource, serviceProvider, out _observableSource))
            {
                // TODO Consider throwing an exception.
                return null;
            }

            SourceRootType = GetTypeOfSourceRoot(_observableSource);
            IsPropBagBased = SourceRootType.IsPropGenBased();

            string[] pathNameComponents = _path.Split('.');
            if (pathNameComponents.Length > 2) throw new ApplicationException("Path cannot have more than two components.");

            int compCount = pathNameComponents.Length;
            for(int pPtr = 0; pPtr < compCount; pPtr++)
            {
                string pathElement = pathNameComponents[pPtr];

                Type sType;
                if(pPtr < compCount - 1)
                {
                    sType = GetTypeOfInter(pathElement, Source);
                }
                else
                {
                    sType = _sourceType;
                }

                BindingBase ourBinding = CreateBindingForPathElement(pathElement, _targetObject,
                    _targetProperty, _observableSource, pathElement, sType);

                _mainMultiBinding.Add(ourBinding);
            }

            // return the expression 
            // provided by the multi-binding
            _multiBindingExpression = _mainMultiBinding.GetMultiBindingExpression(serviceProvider);

            return _multiBindingExpression;
        }

        #endregion

        #region Custom Binding Work

        private Type GetTypeOfInter(string pathElement, object source)
        {
            Type h = source.GetType();
            PropertyInfo pi = h.GetDeclaredProperty(pathElement);
            return pi.PropertyType;
        }

        // TODO: Create a struct with Path, Source, ElementName, Converter, etc.
        // and all such required to build a binding.
        private BindingBase CreateBindingForPathElement(string pathElement, DependencyObject targetObject,
            DependencyProperty targetProperty, ObservableSource obSrc, 
            string propertyName, Type sourceType)
        {
            Binding binding;
            Type propertyType = targetProperty.PropertyType;

            if (IsPropBagBased && !IsMemberReal(pathElement))
            {
                // We are going to assume that it has (or soon will be) registered
                // as a "virtual" IProp<T>

                string path = $"[{sourceType.FullName},{propertyName}]";
                binding = new Binding
                {
                    Path = new PropertyPath(path),
                    Converter = new PropValueConverter(),
                    ConverterParameter = new TwoTypes(sourceType, propertyType)
                };
            }
            else
            {
                binding = CreateDefaultBinding(_path, sourceType, propertyType);
            }

            BindingExpressionBase bExp = BindingOperations.SetBinding(targetObject, targetProperty, binding);

            return binding;
        }

        private Binding CreateDefaultBinding(string path, Type sourceType, Type propertyType)
        {
            Binding binding = new Binding
            {
                Path = new PropertyPath(_path),
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
            else if(ElementName != null && GetSourceFromELementName(targetObject, elementName, out obSrc))
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
            obSrc = new ObservableSource(targetObject as DependencyObject);
            return true;
        }

        private bool GetDefaultSource(DependencyObject targetObject, out ObservableSource obSrc)
        {
            if (targetObject is FrameworkElement fe)
            {
                obSrc = new ObservableSource(fe);
                return true;
            }
            else
            {
                obSrc = null;
                return false;
            }
        }



        private Type GetTypeOfSourceRoot(ObservableSource obSrc)
        {
            Type t = obSrc.Data.GetType();
            return t;
        }

        // TODO: Make this work for all member types, or rename the method.

        /// <summary>
        /// Returns true if the member exists and was not created by 
        /// our ViewModel builder.
        /// </summary>
        /// <param name="pathElement"></param>
        /// <returns></returns>
        private bool IsMemberReal(string pathElement)
        {
            PropertyInfo pi = SourceRootType.GetDeclaredProperty(pathElement);
            if (pi == null)
            {
                //throw new InvalidOperationException($"The source path {pathElement} cannot be found.");
                return false;
            }

            if (!IsPropBagBased || !SourceRootType.IsEmittedProxy())
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

        private bool PropertyExists(string pathElement)
        {
            return null != SourceRootType.GetDeclaredProperty(pathElement);
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

            #region Public Methods
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

            #region From Framework Element
            public ObservableSource(FrameworkElement fe)
            {
                Data = fe.DataContext;

                //DependencyObject depObj = fe as DependencyObject;
               
                fe.DataContextChanged += Fe_DataContextChanged;
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

