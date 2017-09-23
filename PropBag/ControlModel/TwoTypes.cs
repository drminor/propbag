using System;
using System.ComponentModel;
using System.Globalization;

using System.Reflection;
//using System.ComponentModel.Design.Serialization;

using DRM.PropBag.Caches;


//using System.Windows.Markup;
//using System.Windows.Data;

namespace DRM.PropBag.ControlModel
{
    public struct TwoTypes
    {
        bool _beenInitialized;
        private Type _sourceType;
        private Type _destType;

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
