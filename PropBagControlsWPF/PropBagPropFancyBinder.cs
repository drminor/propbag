
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Threading;

/// <remarks>
/// This is a lightly-modified version of the code available from
/// https://www.codeproject.com/Articles/356294/Using-path-parameters-when-binding-data-in-WPF
/// Written by Michael Soloduha.
/// License: "The Code Project Open License (CPOL)
/// </remarks>

namespace Emightgen
{
    [MarkupExtensionReturnType(typeof(object)), Localizability(LocalizationCategory.None, Modifiability = Modifiability.Unmodifiable, Readability = Readability.Unreadable)]
    public class BindingExtension : MarkupExtension, IMultiValueConverter, IValueConverter, INotifyPropertyChanged
    {
        #region Member Declarations

        string _path = null;
        string _elementName = null;
        object _source = null;

        //Collection<object> _parameters = null;
        object[] _parameters = new object[3];
        bool[] _paramIsSet = new bool[3];

        DependencyProperty _evaluationProperty = null;
        bool _disableNotification = false;
        bool _parametersChanged = false;


        //Collection<object> _evaluatedParameters = null;
        object[] _evaluatedParameters = null;

        MultiBindingExpression _multiBindingExpression;
        DependencyObject _targetObject;
        DependencyProperty _targetProperty;
        BindingMode _mode = BindingMode.Default;

        #endregion

        #region Constructors

        class ParameterConverterArgs
        {
            public IValueConverter OriginalConverter;
            public object OriginalParameter;
            public int ParameterIndex;
        }

        public BindingExtension()
        {
            //_parameters = new Collection<object>();
        }

        public BindingExtension(string path)
        {
            _path = path;
            //_parameters = new Collection<object>();
        }

        public BindingExtension(string path, object arg1)
        {
            _path = path;

            //_parameters = new Collection<object>();
            //_parameters.Add(arg1);

            _parameters[0] = arg1;
            _paramIsSet[0] = true;
        }

        public BindingExtension(string path, object arg1, object arg2)
        {
            _path = path;

            //_parameters = new Collection<object>();
            //_parameters.Add(arg1);
            //_parameters.Add(arg2);

            _parameters[0] = arg1;
            _paramIsSet[0] = true;

            _parameters[1] = arg2;
            _paramIsSet[1] = true;
        }

        public BindingExtension(string path, object arg1, object arg2, object arg3)
        {
            _path = path;

            //_parameters = new Collection<object>();
            //_parameters.Add(arg1);
            //_parameters.Add(arg2);
            //_parameters.Add(arg3);

            _parameters[0] = arg1;
            _paramIsSet[0] = true;

            _parameters[1] = arg2;
            _paramIsSet[1] = true;

            _parameters[2] = arg3;
            _paramIsSet[2] = true;
        }

        public BindingExtension(string path, params object[] args)
        {
            _path = path;

            _parameters = args;
            for (int ptr = 0; ptr < Math.Min(args.Length, 3); ptr++)
            {
                _paramIsSet[ptr] = true;
            }
        }

        #endregion

        #region Public Properties

        [DefaultValue(null), ConstructorArgument("arg1"), EditorBrowsable(EditorBrowsableState.Never)]
        public object Arg1
        {
            get { CheckParamHasValue(0); return _parameters[0]; }
            set { _parameters[0] = value; _paramIsSet[0] = true; }
        }

        [DefaultValue(null), ConstructorArgument("arg2"), EditorBrowsable(EditorBrowsableState.Never)]
        public object Arg2
        {
            get { CheckParamHasValue(1); return _parameters[1]; }
            set { _parameters[1] = value; _paramIsSet[1] = true; }
        }

        [DefaultValue(null), ConstructorArgument("arg3"), EditorBrowsable(EditorBrowsableState.Never)]
        public object Arg3
        {
            get { CheckParamHasValue(2); return _parameters[2]; }
            set { _parameters[2] = value; _paramIsSet[2] = true; }
        }

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

        public BindingMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }
        #endregion

        #region Provide Value Implementation

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                return this;
            }

            IProvideValueTarget provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (provideValueTarget == null)
            {
                return this;
            }

            _targetObject = provideValueTarget.TargetObject as DependencyObject;

            if (_targetObject == null)
            {
                return this;
            }

            _targetProperty = provideValueTarget.TargetProperty as DependencyProperty;

            int paramsCount = CheckForEmptyParams(_parameters, _paramIsSet);

            // create wpf binding
            MultiBinding mbinding = new MultiBinding
            {
                Mode = _mode,
                Converter = this
            };

            // binding to evaluate data context
            mbinding.Bindings.Add(new Binding
            {
                Mode = BindingMode.OneWay
            });

            // binding to evaluate target element
            mbinding.Bindings.Add(new Binding
            {
                Mode = BindingMode.OneWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            });

            // binding to private property, that will reevaluate final value when this property changes
            mbinding.Bindings.Add(new Binding
            {
                Mode = BindingMode.OneWay,
                Source = this,
                Path = new PropertyPath("EffectiveValueChanged")
            });

            _evaluatedParameters = new object[paramsCount];

            for (int i = 0; i < paramsCount; i++)
            {
                object pvalue = _parameters[i];

                if (pvalue is Binding)
                {
                    Binding pbinding = pvalue as Binding;

                    if (!(pbinding.ConverterParameter is ParameterConverterArgs))
                    {
                        pbinding.ConverterParameter = new ParameterConverterArgs()
                        {
                            OriginalConverter = pbinding.Converter,
                            OriginalParameter = pbinding.ConverterParameter,
                            ParameterIndex = i
                        };

                        pbinding.Converter = this;
                        pbinding.Mode = BindingMode.OneWay;
                    }

                    mbinding.Bindings.Add(pbinding);
                }

                _evaluatedParameters[i] = pvalue;
            }

            object value = mbinding.ProvideValue(serviceProvider);
            _multiBindingExpression = value as MultiBindingExpression;

            return value;
        }

        #endregion

        #region Raise Property Changed Callback Events

        [EditorBrowsable(EditorBrowsableState.Never)]
        public object EffectiveValueChanged
        {
            get { return null; }
        }

        internal void NotifyEffectiveValueChanged()
        {
            if (!_disableNotification)
            {
                OnPropertyChanged("EffectiveValueChanged");
            }
        }

        #endregion

        #region Parameter Helpers

        //void EnsureParametersIndex(int index)
        //{
        //	while (_parameters.Count <= index)
        //	{
        //		_parameters.Add(null);
        //	}
        //}

        private void CheckParamHasValue(int paramIndex)
        {
            if (paramIndex > 2 && paramIndex < _parameters.Length)
            {
                return;
            }

            if (_paramIsSet[paramIndex])
            {
                return;
            }

            throw new ArgumentNullException($"Arg{paramIndex + 1} has not yet been assigned a value.");
        }

        private int CheckForEmptyParams(object[] parameters, bool[] paramIsSet)
        {
            if (parameters.Length > paramIsSet.Length)
            {
                // Caller provided list, assume all have been set.
                return parameters.Length;
            }

            int len = paramIsSet.Length;
            int i;
            for (i = 0; i < len; i++)
            {
                if (!paramIsSet[i]) break;
            }

            // This is the number of set parameters.
            int result = i;

            for (; i < len; i++)
            {
                if (paramIsSet[i])
                {
                    throw new ArgumentException(message: $"Arg{i} has been set, while Arg{i - 1} has not. It is unclear how to create a value for Arg{i - 1}.", paramName: $"Arg{i - 1}");
                }
            }
            return result;
        }

        #endregion

        #region IMultiValueConverter Members

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DependencyObject targetObject = values[1] as DependencyObject;

            if (targetObject == null)
            {
                return null;
            }

            PathEvaluationBinding binding;

            if (_evaluationProperty == null)
            {
                // Get a free attached property to use for binding this evaluation property.
                // The binding on the target object for the evaluation property is also returned.
                _evaluationProperty = PathEvaluationProperties.GetFreeEvaluationProperty(
                    targetObject, this, out binding);
            }
            else
            {
                // create path evaluation binding
                binding = BindingOperations.GetBindingBase(targetObject, _evaluationProperty) as PathEvaluationBinding;
            }

            if (binding == null)
            {
                binding = new PathEvaluationBinding(this,
                    targetObject,
                    new PropertyPath(_path, _parameters));

                // set binding mode according to the specified in extension by user or in property metadata
                if (_multiBindingExpression != null)
                {
                    binding.Mode = _multiBindingExpression.ParentMultiBinding.Mode;
                }

                if (binding.Mode == BindingMode.Default)
                {
                    if (_targetProperty != null)
                    {
                        if (_targetProperty.GetMetadata(_targetObject) is FrameworkPropertyMetadata mt && mt.BindsTwoWayByDefault)
                        {
                            binding.Mode = BindingMode.TwoWay;
                        }
                    }
                }


                if (string.IsNullOrEmpty(_elementName))
                {
                    binding.Source = _source;
                }
                else
                {
                    binding.ElementName = _elementName;
                }
            }

            if (_parametersChanged)
            {
                _parametersChanged = false;

                for (int i = 0; i < _evaluatedParameters.Length; i++)
                {
                    binding.Path.PathParameters[i] = _evaluatedParameters[i];
                }
            }

            try
            {
                // notifications are sent when source value is changed and binding is TwoWay or OneWay
                // when we set binding here, notification is sent also, so disable it to prevent infinite loop
                _disableNotification = true;

                BindingOperations.SetBinding(targetObject, _evaluationProperty, binding);
            }
            finally
            {
                _disableNotification = false;
            }

            object value = binding.EffectiveValue;

            // now we have to convert the value
            if (value != null)
            {
                if (!targetType.IsAssignableFrom(value.GetType()))
                {
                    TypeConverter tc = TypeDescriptor.GetConverter(value);
                    value = tc.ConvertTo(value, targetType);
                }
            }

            return value;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] ret = new object[targetTypes.Length];

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = DependencyProperty.UnsetValue;
            }

            if (_targetObject != null)
            {
                if (BindingOperations.GetBindingBase(_targetObject, _evaluationProperty) is PathEvaluationBinding binding)
                {
                    _targetObject.SetValue(_evaluationProperty, value);
                }
            }

            return ret;
        }

        #endregion

        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ParameterConverterArgs args = (ParameterConverterArgs)parameter;

            if (args != null)
            {
                if (args.OriginalConverter != null)
                {
                    value = args.OriginalConverter.Convert(value, targetType, args.OriginalParameter, culture);
                }

                _evaluatedParameters[args.ParameterIndex] = value;
                _parametersChanged = true;
            }

            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            Interlocked.CompareExchange(ref PropertyChanged, null, null)
                ?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    class PathEvaluationBinding : Binding
    {
        BindingExtension _extension;
        DependencyObject _targetObject = null;
        object _effectiveValue;

        internal PathEvaluationBinding(BindingExtension extension,
            DependencyObject targetObject,
            PropertyPath path)
        {
            _extension = extension;
            _targetObject = targetObject;
            base.Path = path;
        }

        internal BindingExtension Extension
        {
            get { return _extension; }
        }

        internal object EffectiveValue
        {
            get { return _effectiveValue; }
        }

        internal void NotifyEffectiveValueChanged(object newValue)
        {
            _effectiveValue = newValue;
            _extension.NotifyEffectiveValueChanged();
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    class PathEvaluationProperties : DependencyObject
    {
        const int MAX_PROPERTIES = 20;
        public static readonly DependencyProperty[] EvaluationProperty;

        static PathEvaluationProperties()
        {
            EvaluationProperty = new DependencyProperty[MAX_PROPERTIES];

            for (int i = 0; i < MAX_PROPERTIES; i++)
            {
                EvaluationProperty[i] = DependencyProperty.RegisterAttached(
                    string.Format("Evaluation{0}", i + 1), typeof(object),
                    typeof(PathEvaluationProperties),
                    new FrameworkPropertyMetadata(null, new PropertyChangedCallback(EvaluationPropertyChangedCallback)));
            }
        }

        static void EvaluationPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (BindingOperations.GetBindingBase(d, e.Property) is PathEvaluationBinding b)
            {
                b.NotifyEffectiveValueChanged(e.NewValue);
            }
        }

        // to allow several dependency properties to be bound to single target object
        // use several attached properties, here just find the next available property to use
        // available, means no binding is set to this property
        internal static DependencyProperty GetFreeEvaluationProperty(
            DependencyObject d,
            BindingExtension e,
            out PathEvaluationBinding b)
        {
            for (int i = 0; i < MAX_PROPERTIES; i++)
            {
                b = BindingOperations.GetBindingBase(d, EvaluationProperty[i]) as PathEvaluationBinding;

                if (b != null && b.Extension == e)
                {
                    return EvaluationProperty[i];
                }

                if (b == null)
                {
                    return EvaluationProperty[i];
                }
            }

            throw new InvalidOperationException("There are no more free evaluation properties left");
        }
    }
}

