using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;
using System.ComponentModel;

namespace DRM.PropBag.ControlModel
{

    public class PropInitialValueField : NotifyPropertyChangedBase, IEquatable<PropInitialValueField>
    {
        string iv;
        bool stu;
        bool std;
        bool stn;
        bool stes;

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

        public PropInitialValueField() : this(null) { }

        public PropInitialValueField(string initialValue, bool setToDefault = false,
            bool setToUndefined = false, bool setToNull = false,
            bool setToEmptyString = false)
        {
            InitialValue = initialValue;
            SetToUndefined = setToUndefined;
            SetToDefault = setToDefault;
            SetToNull = setToNull;
            SetToEmptyString = setToEmptyString;
        }

        public bool Equals(PropInitialValueField other)
        {
            if (other == null) return false;

            if (other.InitialValue == InitialValue &&
                other.SetToUndefined == SetToUndefined &&
                other.SetToDefault == SetToDefault &&
                other.SetToNull == SetToNull &&
                other.SetToEmptyString == SetToEmptyString) return true;

            return false;
        }
    }
}