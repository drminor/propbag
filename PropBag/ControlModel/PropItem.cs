using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag.ControlModel
{
    public class PropItem : NotifyPropertyChangedBase
    {
        string _propertyName;
        Type _propertyType;
        PropInitialValueField _propInitialValueField;
        PropComparerField _propComparerField;
        PropDoWhenChangedField _propDoWhenChangedField;

        bool _hasStore;
        bool _typeIsSolid;
        string _extraInfo;

        [XmlElement("name")]
        public string PropertyName { get {return _propertyName;}  set { SetIfDifferent<string>(ref _propertyName, value); } }

        [XmlElement("type")]
        public Type PropertyType { get { return _propertyType; } set { _propertyType = value; } }

        [XmlElement("initial-value")]
        public PropInitialValueField InitialValueField { get { return _propInitialValueField; }
            set { SetIfDifferent<PropInitialValueField>(ref _propInitialValueField, value); }
        }

        [XmlElement("comparer")]
        public PropComparerField ComparerField { get { return _propComparerField; }
            set { SetIfDifferent<PropComparerField>(ref _propComparerField, value); }
        }

        [XmlElement("do-when-changed")]
        public PropDoWhenChangedField DoWhenChangedField { get { return _propDoWhenChangedField; }
            set { SetIfDifferent<PropDoWhenChangedField>(ref _propDoWhenChangedField, value); }
        }

        [XmlAttribute(AttributeName = "caller-provides-storage")]
        public bool HasStore { get { return _hasStore; } set { SetIfDifferent<bool>(ref _hasStore, value); } }

        [XmlIgnore]
        public bool TypeIsSolid { get { return _typeIsSolid; } set { SetIfDifferent<bool>(ref _typeIsSolid, value); } }

        [XmlIgnore]
        public string ExtraInfo { get { return _extraInfo; } set { SetIfDifferent<string>(ref _extraInfo, value); } }

        public PropItem() : this(null, null) { }

        public PropItem(Type type, string name) : this(type, name, null, true, true, null, null, null) { }

        public PropItem(Type type, string name,
            string extraInfo = null, bool hasStore = true,
            bool typeIsSolid = true,
            PropInitialValueField initialValueField = null,
            PropDoWhenChangedField doWhenChanged = null, PropComparerField comparer = null)
        {
            PropertyType = type;
            PropertyName = name;
            ExtraInfo = extraInfo;
            HasStore = hasStore;
            TypeIsSolid = typeIsSolid;
            InitialValueField = initialValueField;
            ComparerField = comparer;
            DoWhenChangedField = doWhenChanged ?? new PropDoWhenChangedField();

        }
    }
}
