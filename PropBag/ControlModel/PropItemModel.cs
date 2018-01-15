using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag
{
    public class PropItemModel : NotifyPropertyChangedBase, IPropItem
    {
        string _propertyName;
        PropKindEnum _propKind;
        Type _propertyType; // Also used to store the collection type.
        Type _itemType;
        ITypeInfoField _propTypeInfoField;
        IPropInitialValueField _propInitialValueField;
        IPropComparerField _propComparerField;
        IPropDoWhenChangedField _propDoWhenChangedField;
        IPropBinderField _propBinderField;
        IMapperRequest _mapperRequest;

        PropStorageStrategyEnum _hasStore;
        bool _typeIsSolid;
        string _extraInfo;

        [XmlElement("name")]
        public string PropertyName { get {return _propertyName;}  set { SetIfDifferent<string>(ref _propertyName, value); } }

        [XmlElement("prop-kind")]
        public PropKindEnum PropKind { get { return _propKind; } set { _propKind = value; } }

        [XmlElement("type")]
        public Type PropertyType { get { return _propertyType; } set { _propertyType = value; } }

        /// <summary>
        /// Alias for PropertyType, helpful when this PropItem defines a Collection.
        /// </summary>
        [XmlIgnore]
        public Type CollectionType { get { return _propertyType; } set { _propertyType = value; } }

        [XmlElement("item-type")]
        public Type ItemType { get { return _itemType; } set { _itemType = value; } }

        [XmlElement("type-info")]
        public ITypeInfoField PropTypeInfoField
        {
            get { return _propTypeInfoField; }
            set { SetAlways<ITypeInfoField>(ref _propTypeInfoField, value); }
        }

        [XmlElement("initial-value")]
        public IPropInitialValueField InitialValueField { get { return _propInitialValueField; }
            set { SetAlways<IPropInitialValueField>(ref _propInitialValueField, value); }
        }

        [XmlElement("comparer")]
        public IPropComparerField ComparerField { get { return _propComparerField; }
            set { SetAlways<IPropComparerField>(ref _propComparerField, value); }
        }

        // TODO fix the IEquatable for the DoWhenChangedField.
        [XmlElement("do-when-changed")]
        public IPropDoWhenChangedField DoWhenChangedField { 
            get { return _propDoWhenChangedField; }
            set { _propDoWhenChangedField = value; }
        }

        [XmlElement("bind-to-local-property")]
        public IPropBinderField BinderField
        {
            get { return _propBinderField; }
            set { _propBinderField = value; }
        }

        [XmlElement("mapper-request-for-local-binding")]
        public IMapperRequest MapperRequest
        {
            get { return _mapperRequest; }
            set { _mapperRequest = value; }
        }

        [XmlAttribute(AttributeName = "caller-provides-storage")]
        public PropStorageStrategyEnum StorageStrategy
        {
            get
            {
                return _hasStore;
            }
            set
            {
                int ov = (int)_hasStore;
                int nv = (int)value;
                SetIfDifferentEnum<int>(ref ov, nv, "storageStrategy");
                _hasStore = (PropStorageStrategyEnum) ov;
            }
        }

        [XmlIgnore]
        public bool TypeIsSolid { get { return _typeIsSolid; } set { SetIfDifferent<bool>(ref _typeIsSolid, value); } }

        [XmlIgnore]
        public string ExtraInfo { get { return _extraInfo; } set { SetIfDifferent<string>(ref _extraInfo, value); } }

        //public PropItem() : this(null, null, null, true, true, PropKindEnum.Prop, null, null, null, null) { }

        //public PropItem(Type type, string name) : this(type, name, null, true, true, PropKindEnum.Prop,
        //    null, null, null, null) { }

        public PropItemModel(Type type, string name, PropStorageStrategyEnum storageStrategy, bool typeIsSolid, PropKindEnum propKind,
            ITypeInfoField propTypeInfoField = null,
            IPropInitialValueField initialValueField = null,
            string extraInfo = null, PropComparerField comparer = null, Type itemType = null)
        {
            PropertyType = type;
            PropertyName = name;
            ExtraInfo = extraInfo;
            StorageStrategy = storageStrategy;
            TypeIsSolid = typeIsSolid;
            PropKind = propKind;
            PropTypeInfoField = _propTypeInfoField;
            InitialValueField = initialValueField;
            ComparerField = comparer;
            _itemType = itemType;
            _propBinderField = null;
            _mapperRequest = null;
        }
    }
}
