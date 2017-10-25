using DRM.TypeSafePropertyBag;
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
        PropKindEnum _propKind;
        Type _propertyType; // Also used to store the collection type.
        Type _itemType;
        PropTypeInfoField _propTypeInfoField;
        PropInitialValueField _propInitialValueField;
        PropComparerField _propComparerField;
        PropDoWhenChangedField _propDoWhenChangedField;

        bool _hasStore;
        bool _typeIsSolid;
        string _extraInfo;

        [XmlElement("name")]
        public string PropertyName { get {return _propertyName;}  set { SetIfDifferent<string>(ref _propertyName, value); } }

        [XmlElement("prop-kind")]
        public PropKindEnum PropKind { get { return _propKind; } set { _propKind = value; } }

        [XmlElement("type")]
        public Type PropertyType { get { return _propertyType; } set { _propertyType = value; } }

        [XmlIgnore]
        public Type CollectionType { get { return _propertyType; } set { _propertyType = value; } }

        [XmlElement("item-type")]
        public Type ItemType { get { return _itemType; } set { _itemType = value; } }

        [XmlElement("type-info")]
        public PropTypeInfoField PropTypeInfoField
        {
            get { return _propTypeInfoField; }
            set { SetIfDifferent<PropTypeInfoField>(ref _propTypeInfoField, value); }
        }

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

        //public PropItem() : this(null, null, null, true, true, PropKindEnum.Prop, null, null, null, null) { }

        //public PropItem(Type type, string name) : this(type, name, null, true, true, PropKindEnum.Prop,
        //    null, null, null, null) { }

        public PropItem(Type type, string name, bool hasStore, bool typeIsSolid, PropKindEnum propKind,
            PropTypeInfoField propTypeInfoField = null,
            PropInitialValueField initialValueField = null,
            PropDoWhenChangedField doWhenChanged = null,
            string extraInfo = null, PropComparerField comparer = null, Type itemType = null)
        {
            PropertyType = type;
            PropertyName = name;
            ExtraInfo = extraInfo;
            HasStore = hasStore;
            TypeIsSolid = typeIsSolid;
            PropKind = propKind;
            PropTypeInfoField = _propTypeInfoField;
            InitialValueField = initialValueField;
            ComparerField = comparer;
            DoWhenChangedField = doWhenChanged ?? new PropDoWhenChangedField();
            _itemType = itemType;
        }
    }
}
