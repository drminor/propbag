using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;
using System.ComponentModel;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag
{
    public class PropInitialValueField : NotifyPropertyChangedBase, IEquatable<PropInitialValueField>, IPropInitialValueField, IDisposable
    {
        object iv;
        bool stu;
        bool std;
        bool stn;
        bool stes;
        Func<object> _valueCreator;

        //[XmlText]
        [XmlIgnore]
        public object InitialValue { get { return iv; } set { SetIfDifferentRefEqu<object>(ref iv, value); } }

        [XmlAttribute("use-undefined")]
        public bool SetToUndefined { get { return stu; } set { SetIfDifferent<bool>(ref stu, value); } }

        [XmlAttribute("use-default")]
        public bool SetToDefault { get { return std; } set { SetIfDifferent<bool>(ref std, value); } }

        [XmlAttribute("use-null")]
        public bool SetToNull { get { return stn; } set { SetIfDifferent<bool>(ref stn, value); } }

        [XmlAttribute("use-empty-string")]
        public bool SetToEmptyString { get { return stes; } set { SetIfDifferent<bool>(ref stes, value); } }

        public bool CreateNew { get; set; }

        public string PropBagFCN { get; set; }

        public Func<object> ValueCreator { get { return _valueCreator; } set { SetIfDifferentDelegate<Func<object>>(ref _valueCreator, value); } }

        public Func<T> GetInitialValue<T>() => GetInitialValue_Internal<T>;

        private T GetInitialValue_Internal<T>()
        {
            // TODO: Fix Me.
            return default(T);
        }

        #region Constructors

        /// <summary>
        /// This is the same as static IPropInitialValueField UseDefault.
        /// </summary>
        public PropInitialValueField()
            : this(initialValue: null, setToDefault: true, setToUndefined: false, setToNull: false, setToEmptyString: false,
                  valueCreator: null, createNew: false, propBagFCN: null)
        {
        }

        public PropInitialValueField(string initialValue)
            : this(initialValue: initialValue, setToDefault: false, setToUndefined: false, setToNull: false, setToEmptyString: false,
                valueCreator: null, createNew: false, propBagFCN: null)
        {
        }

        public PropInitialValueField(string initialValue, bool setToDefault, bool setToUndefined, bool setToNull, bool setToEmptyString)
            : this(initialValue: initialValue, setToDefault: setToDefault, setToUndefined: setToUndefined, setToNull: setToNull, setToEmptyString: setToEmptyString,
                valueCreator: null, createNew: false, propBagFCN: null)
        {
        }

        public PropInitialValueField
            (
            object initialValue,
            bool setToDefault,
            bool setToUndefined,
            bool setToNull,
            bool setToEmptyString,
            Func<object> valueCreator,
            bool createNew,
            string propBagFCN
            )
        {
            InitialValue = initialValue;
            SetToUndefined = setToUndefined;
            SetToDefault = setToDefault;
            SetToNull = setToNull;
            SetToEmptyString = setToEmptyString;
            ValueCreator = valueCreator;
            CreateNew = createNew;
            PropBagFCN = propBagFCN;
        }

        #endregion

        public object Clone()
        {
            PropInitialValueField result = new PropInitialValueField(InitialValue, SetToDefault, SetToUndefined, SetToNull, SetToEmptyString,
                valueCreator: null, createNew: CreateNew, propBagFCN: PropBagFCN);

            return result;
        }

        // TODO: Include the two new properties: CreateNew and PropBagResourceKey
        // TODO: The value creators
        public bool Equals(PropInitialValueField other)
        {
            if (other == null) return false;

            if (other.InitialValue == InitialValue &&
                other.SetToUndefined == SetToUndefined &&
                other.SetToDefault == SetToDefault &&
                other.SetToNull == SetToNull &&
                other.SetToEmptyString == SetToEmptyString &&
                ReferenceEquals(other.ValueCreator, ValueCreator))
            {
                return true;
            }

            return false;
        }

        public string GetStringValue()
        {
            if (SetToDefault || SetToNull || SetToUndefined)
            {
                return null;
            }
            else if (SetToEmptyString)
            {
                return "";
            }
            else
            {
                return InitialValue.ToString();
            }
        }

        public static IPropInitialValueField UseUndefined
        {
            get
            {
                return new PropInitialValueField
                    (
                    initialValue: null,
                    setToDefault: false,
                    setToUndefined: true,
                    setToNull: false,
                    setToEmptyString: false,
                    valueCreator: null,
                    createNew: false,
                    propBagFCN: null
                    );
            }
        }

        public static IPropInitialValueField UseDefault
        {
            get
            {
                return new PropInitialValueField();
                    //(
                    //initialValue: null,
                    //setToDefault: true,
                    //setToUndefined: false,
                    //setToNull: false,
                    //setToEmptyString: false,
                    //valueCreator: null,
                    //createNew: false,
                    //propBagResourceKey: null
                    //);
            }
        }

        public static IPropInitialValueField UseNull
        {
            get
            {
                return new PropInitialValueField
                (
                initialValue: null,
                setToDefault: false,
                setToUndefined: false,
                setToNull: true,
                setToEmptyString: false,
                valueCreator: null,
                createNew: false,
                propBagFCN: null
                );
            }
        }

        public static IPropInitialValueField UseEmptyString
        {
            get
            {
                return new PropInitialValueField
                (
                initialValue: null,
                setToDefault: false,
                setToUndefined: false,
                setToNull: true,
                setToEmptyString: false,
                valueCreator: null,
                createNew: false,
                propBagFCN: null
                );
            }
        }

        public static IPropInitialValueField UseCreateNew
        {
            get
            {
                return new PropInitialValueField
                (
                initialValue: null,
                setToDefault: false,
                setToUndefined: false,
                setToNull: false,
                setToEmptyString: false,
                valueCreator: null,
                createNew: true,
                propBagFCN: null
                );
            }
        }

        public static IPropInitialValueField FromPropBagFCN(string propBagFCN)
        {
            return new PropInitialValueField
            (
            initialValue: null,
            setToDefault: false,
            setToUndefined: false,
            setToNull: false,
            setToEmptyString: false,
            valueCreator: null,
            createNew: true,
            propBagFCN: propBagFCN
            );
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    _valueCreator = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

    }
}