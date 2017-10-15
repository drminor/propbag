using DRM.PropBag.ControlModel;
using System;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public struct DirectConverterParameter
    {
        bool _beenInitialized;
        private string _propertyName;
        private Type _sourceType;
        private Type _destType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType">The type of value where the data lives, i.e. Binding source or DataContext.</param>
        /// <param name="destinationType">The type that the binding target, i.e., textbox.text, or dependency property needs.</param>
        public DirectConverterParameter(string propertyName, Type sourceType, Type destinationType) : this()
        {
            IsEmpty = false;

            PropertyName = propertyName;
            SourceType = sourceType;
            DestType = destinationType;
        }

        public static DirectConverterParameter Empty
        {
            get
            {
                return new DirectConverterParameter();
            }
        }

        public bool IsEmpty
        {
            get
            {
                return !_beenInitialized;
            }
            private set
            {
                _beenInitialized = !value;
            }
        }

        public string PropertyName
        {
            get
            {
                if (IsEmpty) throw new InvalidOperationException("This instance of DirectConverterParameter has not been initialized.");
                return _propertyName;
            }
            private set
            {
                _propertyName = value;
            }
        }

        /// <summary>
        /// When used in a binding, this is the type of the property in the DataContext or binding source.
        /// </summary>
        public Type SourceType
        {
            get
            {
                if (IsEmpty) throw new InvalidOperationException("This instance of DirectConverterParameter has not been initialized.");
                return _sourceType;
            }
            private set
            {
                _sourceType = value;
            }
        }

        /// <summary>
        /// When used in a binding, this is the binding target's type. (The window control is usually the target.)
        /// </summary>
        public Type DestType
        {
            get
            {
                if (IsEmpty) throw new InvalidOperationException("This instance of DirectConverterParameter has not been initialized.");
                return _destType;
            }
            private set
            {
                _destType = value;
            }
        }

        //WeakReference<IPropBagMin> _wrHost;
        //public IPropBagMin Host
        //{
        //    get
        //    {
        //        if(_wrHost.TryGetTarget(out IPropBagMin target))
        //        {
        //            return target;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    set
        //    {
        //        if (value == null)
        //            _wrHost = null;
        //        else
        //            _wrHost = new WeakReference<IPropBagMin>(value);
        //    }
        //}

        // TODO: This needs to be updated when we create a dedicated Markup Extension
        //public static DirectConverterParameter FromMkUpExtParam(string propertyName, object parameter, Type destinationType = null)
        //{
        //    if (parameter == null)
        //    {
        //        return DirectConverterParameter.Empty;
        //    }
        //    else if (parameter is DirectConverterParameter)
        //    {
        //        return (DirectConverterParameter)parameter;
        //    }
        //    else if (parameter is Type && destinationType != null)
        //    {
        //        return new DirectConverterParameter(propertyName, (Type)parameter, destinationType);
        //    }
        //    else if (parameter is IPropGen && destinationType != null)
        //    {
        //        return new DirectConverterParameter(propertyName, ((IPropGen)parameter).Type, destinationType);
        //    }
        //    else
        //    {
        //        return DirectConverterParameter.Empty;
        //    }
        //}

        public TwoTypes GetTwoTypes()
        {
            return new TwoTypes(SourceType, DestType);
        }
    }
}
