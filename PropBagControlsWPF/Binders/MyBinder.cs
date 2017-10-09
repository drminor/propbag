
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
using System.Windows.Markup;

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

        #endregion

        #region Constructors

        public BindingExtension() : this(null) { }

        public BindingExtension(PropertyPath path)
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
        public PropertyPath Path { get; set; }

        //public PropertyPath PropertyPath { get; set; }

        public Type SourceType { get; set; }

        public string ElementName { get; set; }
        public object Source { get; set; }
        public RelativeSource RelativeSource { get; set; }
        public BindingMode Mode { get; set; }

        public string BindingGroupName { get; set; }
        public bool BindsDirectlyToSource { get; set; }

        public IValueConverter Converter { get; set; }
        public CultureInfo ConverterCulture { get; set; }
        public object ConverterParameter { get; set; }

        public object FallBackVallue { get; set; }
        public bool IsAsync { get; set; }

        public bool NotifyOnSourceUpdated { get; set; }
        public bool NotifyOnTargetUpdated { get; set; }
        public bool NotifyOnValidationError { get; set; }

        public string StringFormat { get; set; }
        public object TargetNullValue { get; set; }

        public UpdateSourceTrigger UpdateSourceTrigger { get; set; }

        public bool ValidatesOnDataErrors { get; set; }
        public bool ValidatesOnExceptions { get; set; }

        public string XPath { get; set; }

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

            MyBindingInfo bindingInfo = GatherBindingInfo(Path, Mode);

            MyBindingEngine mb = new MyBindingEngine(bindingInfo, SourceType, _targetObject, _targetProperty);

            return mb.ProvideValue(serviceProvider);

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

        #region Binding Info

        private MyBindingInfo GatherBindingInfo(PropertyPath path, BindingMode mode)
        {
            MyBindingInfo result = new MyBindingInfo(path, mode)
            {
                ElementName = ElementName,
                Source = Source,
                RelativeSource = RelativeSource,

                BindingGroupName = BindingGroupName,
                BindsDirectlyToSource = BindsDirectlyToSource,
                Converter = Converter,
                ConverterCulture = ConverterCulture,
                ConverterParameter = ConverterParameter,

                FallBackVallue = FallBackVallue,
                IsAsync = IsAsync,

                NotifyOnSourceUpdated = NotifyOnSourceUpdated,
                NotifyOnTargetUpdated = NotifyOnTargetUpdated,
                NotifyOnValidationError = NotifyOnValidationError,

                StringFormat = StringFormat,
                TargetNullValue = TargetNullValue,

                UpdateSourceTrigger = UpdateSourceTrigger,

                ValidatesOnDataErrors = ValidatesOnDataErrors,
                ValidatesOnExceptions = ValidatesOnExceptions,

                XPath = XPath
            };

            return result;
        }

        #endregion
    }

}

