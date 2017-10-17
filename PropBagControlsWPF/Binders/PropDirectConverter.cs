using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

using DRM.PropBag.ControlsWPF.Binders;
using DRM.PropBag.ControlsWPF;
using DRM.PropBag.ControlModel;

namespace DRM.PropBag.ControlsWPF
{
    public class PropDirectConverter : MarkupExtension, IValueConverter
    {
        #region Static Constructor and Properties

        static IConvertValues ValueConverter_Default { get; }

        static PropDirectConverter()
        {
            ValueConverter_Default = new PropFactoryValueConverter();
        }

        #endregion

        #region Instance Constructor and Properties

        IConvertValues _valueConverter;
        public IConvertValues ValueConverter
        {
            get
            {
                if(_valueConverter == null)
                {
                    _valueConverter = ValueConverter_Default;
                }
                return _valueConverter;
            }
            set
            {
                _valueConverter = value;
            }
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider) { return this; }

        // Value is native object, we need to return a targetType (hopefully a string at this point.)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine("Calling Convert from PropDirectConverter.");

            DirectConverterParameter dcp = (DirectConverterParameter)parameter;
            TwoTypes tt = dcp.GetTwoTypes();

            object pValue = ((IPropBagMin)value).GetValWithType(dcp.PropertyName, dcp.SourceType);

            try
            {
                object result = ValueConverter.Convert(pValue, targetType, tt, culture);
                return result;
            }
            catch
            {
                return Binding.DoNothing;
            }
        }

        // Value is a string, we need to create a native object.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine("Calling ConvertBack from PropDirectConverter.");

            DirectConverterParameter dcp = (DirectConverterParameter)parameter;
            TwoTypes tt = dcp.GetTwoTypes();

            if(value == null)
            {
                return Binding.DoNothing;
            }

            try
            {
                object pValue = ((IPropBagMin)value).GetValWithType(dcp.PropertyName, dcp.SourceType);
                object result = ValueConverter.ConvertBack(pValue, targetType, tt, culture);

                // Set the value, here, since this converter expects an entire IPropBagMin.
                ((IPropBagMin)value).SetValWithType(dcp.PropertyName, dcp.SourceType, pValue);
            }
            catch
            {
                // Suppress all errors.
            }
            return Binding.DoNothing;
        }

        ProperTBridgeActivator _properTBridge;
        private ProperTBridgeActivator ProperTBridge(Type type) 
        {
            if(_properTBridge == null)
            {
                _properTBridge = ProperTBridgeCreator.GetActivator(type);
            }
            return _properTBridge;
        }

        //Type pmod = typeof(ProperTBridge<MyModel4>);
        //ProperTBridgeActivator br = ProperTBridgeCreator.GetActivator(pmod);
        //ProperTBridge<MyModel4> bridge = (ProperTBridge<MyModel4>)br();

        //MyModel4 mod4Test = (MyModel4)bridge.GetValue(rbvm, "Deep");

    }
}
