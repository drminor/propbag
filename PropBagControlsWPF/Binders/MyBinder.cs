﻿
using DRM.PropBagControlsWPF.WPFHelpers;
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
using System.Xaml;

/// <remarks>
/// This was inspired from the code available at
/// https://www.codeproject.com/Articles/356294/Using-path-parameters-when-binding-data-in-WPF
/// Written by Michael Soloduha.
/// License: "The Code Project Open License (CPOL)
/// </remarks>

namespace DRM.PropBagControlsWPF.Binders
{
    [MarkupExtensionReturnType(typeof(object)), 
        Localizability(LocalizationCategory.None,
        Modifiability = Modifiability.Unmodifiable,
        Readability = Readability.Unreadable)]
    //[RuntimeNamePropertyAttribute("BindingInstanceName")]
    public class BindingExtension : MarkupExtension
    {
        #region Member Declarations

        BindingTarget _bindingTarget;

        #endregion

        #region Constructors

        public BindingExtension() : this(null) { }

        public BindingExtension(PropertyPath path)
        {
            Path = path;

            SourceType = null;
            UseMultiBinding = true;

            Source = null;
            ElementName = null;
            Mode = BindingMode.Default;

            BindingInstanceName = Guid.NewGuid().ToString();
        }

        #endregion

        #region Public Properties

        [DefaultValue(null), ConstructorArgument("path")]
        public PropertyPath Path { get; set; }

        public string BindingInstanceName { get; private set; }
        public Type SourceType { get; set; }
        public bool UseMultiBinding { get; set; }

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
        public bool ValidatesOnNotifyDataErrors { get; set; }

        public int Delay { get; set; }

        public string XPath { get; set; }

        #endregion

        #region Provide Value

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (SourceType == null)
            {
                throw new InvalidOperationException("The SourceType must be specified.");
            }

            // Get the Target Object and Target Property
            if (!SetOurEnv(serviceProvider, out _bindingTarget))
            {
                return this;
            }

            MyBindingInfo bindingInfo = GatherBindingInfo(Path, Mode);

            MyBindingEngine mb = new MyBindingEngine(bindingInfo, SourceType, _bindingTarget, UseMultiBinding);

            return mb.ProvideValue(serviceProvider);
        }

        private bool SetOurEnv(IServiceProvider serviceProvider, out BindingTarget bindingTarget)
        {
            bindingTarget = null;

            if (serviceProvider == null) return false;

            IProvideValueTarget provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (provideValueTarget == null) return false;

            IXamlSchemaContextProvider schemaProvider = serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider;

            // Currently working on getting the xmlns prefix used for this instance.
            //string ns = GetXmlnsPrefix(schemaProvider, this.GetType());


            DependencyObject targetObject = provideValueTarget.TargetObject as DependencyObject;
            if (targetObject == null) return false;

            if(provideValueTarget.TargetProperty is DependencyProperty dp)
            {
                bindingTarget = new BindingTarget(targetObject, dp);
                return true;
            }
            else if(provideValueTarget.TargetProperty is PropertyInfo pi)
            {
                bindingTarget = new BindingTarget(targetObject, pi);
                return true;
            }
            else
            {
                throw new InvalidOperationException("The target of the binding must be a DependencyProperty or a PropertyInfo.");
            }
        }

        //private string GetXmlnsPrefix(IXamlSchemaContextProvider schemaProvider, Type type)
        //{
        //    string ourName = type.Name;
        //    XamlSchemaContext context = schemaProvider.SchemaContext;

        //    IEnumerable<string> namespaces = context.GetAllXamlNamespaces();

        //    foreach(string ns in namespaces)
        //    {
        //        ICollection<XamlType> xamlTypes = context.GetAllXamlTypes(ns);

        //        foreach(XamlType xt in xamlTypes)
        //        {
        //            if(xt.IsMarkupExtension)
        //            {
        //                if(xt.Name == ourName)
        //                {
        //                    return ns;
        //                }
        //            }
        //        }
        //    }

        //    return null;
        //}

        #endregion

        public Binding GetBinding(BindingTarget bindingTarget)
        {
            MyBindingInfo bindingInfo = GatherBindingInfo(Path, Mode);

            MyBindingEngine mb = new MyBindingEngine(bindingInfo, SourceType, _bindingTarget, UseMultiBinding);

            return mb.ProvideTheBindingDirectly();

        }

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

                FallBackValue = FallBackVallue,
                IsAsync = IsAsync,

                NotifyOnSourceUpdated = NotifyOnSourceUpdated,
                NotifyOnTargetUpdated = NotifyOnTargetUpdated,
                NotifyOnValidationError = NotifyOnValidationError,

                StringFormat = StringFormat,
                TargetNullValue = TargetNullValue,

                UpdateSourceTrigger = UpdateSourceTrigger,

                ValidatesOnDataErrors = ValidatesOnDataErrors,
                ValidatesOnExceptions = ValidatesOnExceptions,
                ValidatesOnNotifyDataErrors = ValidatesOnNotifyDataErrors,
                
                UpdateSourceExceptionFilter = null,
                ValidationRules = null,

                Delay = Delay,

                XPath = XPath
            };

            return result;
        }

        #endregion
    }

}

