using DRM.PropBag.Caches;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;


namespace DRM.PropBag.ControlModel
{
    public struct TwoTypes
    {
        bool _beenInitialized;
        private Type _sourceType;
        private Type _destType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType">The type of value where the data lives, i.e. Binding source or DataContext.</param>
        /// <param name="destinationType">The type that the binding target, i.e., textbox.text, or dependency property needs.</param>
        public TwoTypes(Type sourceType, Type destinationType) : this()
        {
            IsEmpty = false;
            SourceType = sourceType;
            DestType = destinationType;
        }

        public static TwoTypes Empty
        {
            get
            {
                return new TwoTypes();
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

        /// <summary>
        /// When used in a binding, this is the type of the property in the DataContext or binding source.
        /// </summary>
        public Type SourceType
        {
            get
            {
                if (IsEmpty) throw new InvalidOperationException("This instance of two types has not been initialized.");
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
                if (IsEmpty) throw new InvalidOperationException("This instance of two types has not been initialized.");
                return _destType;
            }
            private set
            {
                _destType = value;
            }
        }
    }
}
