using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;
using System.ComponentModel;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.ControlModel
{
    public class PropInitialValueField : NotifyPropertyChangedBase, IEquatable<PropInitialValueField>, IPropInitialValueField
    {
        string iv;
        bool stu;
        bool std;
        bool stn;
        bool stes;
        Func<object> vc;

        [XmlText]
        public string InitialValue { get { return iv; } set { SetIfDifferent<string>(ref iv, value); } }

        [XmlAttribute("use-undefined")]
        public bool SetToUndefined { get { return stu; } set { SetIfDifferent<bool>(ref stu, value); } }

        [XmlAttribute("use-default")]
        public bool SetToDefault { get { return std; } set { SetIfDifferent<bool>(ref std, value); } }

        [XmlAttribute("use-null")]
        public bool SetToNull { get { return stn; } set { SetIfDifferent<bool>(ref stn, value); } }

        [XmlAttribute("use-empty-string")]
        public bool SetToEmptyString { get { return stes; } set { SetIfDifferent<bool>(ref stes, value); } }

        public Func<object> ValueCreator { get { return vc; } set { SetIfDifferentDelegate<Func<object>>(ref vc, value); } }

        public PropInitialValueField() : this(null) { }

        public PropInitialValueField(string initialValue, bool setToDefault = false,
            bool setToUndefined = false, bool setToNull = false,
            bool setToEmptyString = false, Func<object> valueCreator = null)
        {
            InitialValue = initialValue;
            SetToUndefined = setToUndefined;
            SetToDefault = setToDefault;
            SetToNull = setToNull;
            SetToEmptyString = setToEmptyString;
            ValueCreator = valueCreator;
        }

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
            //else if (CreateNew)
            //{
            //    // TODO: We need another parameter in the Create method to avoid using this 'magic' string.
            //    string MAGIC_VAL = "-0.-0.-0";
            //    return MAGIC_VAL;
            //}
            else
            {
                return InitialValue;
            }
        }

        public static IPropInitialValueField UndefinedInitialValueField
        {
            get
            {
                return new PropInitialValueField(initialValue: null, setToDefault: false, setToUndefined: true, setToNull: false, setToEmptyString: false, valueCreator: null);
            }
        }
    }
}