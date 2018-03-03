using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag
{
    using PropNameType = String;

    using PropCreatorType = Func<String, object, bool, IPropTemplate, IProp>;

    public class PropItemModel : NotifyPropertyChangedBase, IPropModelItem
    {
        PropNameType _propertyName;
        PropKindEnum _propKind;
        Type _propertyType; // Also used to store the collection type.
        Type _itemType;
        ITypeInfoField _propTypeInfoField;
        IPropInitialValueField _propInitialValueField;
        IPropComparerField _propComparerField;
        IPropDoWhenChangedField _propDoWhenChangedField;
        IPropBinderField _propBinderField;
        IMapperRequest _mapperRequest;
        string _mapperRequestResourceKey;

        PropStorageStrategyEnum _hasStore;
        bool _typeIsSolid;
        object _extraInfo;
        PropCreatorType _propCreator;

        [XmlElement("name")]
        public PropNameType PropertyName { get {return _propertyName;}  set { SetIfDifferent<PropNameType>(ref _propertyName, value); } }

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

        [XmlElement("mapper-request-resource-key")]
        public string MapperRequestResourceKey { get { return _mapperRequestResourceKey; } set { SetIfDifferent<string>(ref _mapperRequestResourceKey, value); } }

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
        public object ExtraInfo { get { return _extraInfo; } set { SetAlways<object>(ref _extraInfo, value); } }

        [XmlIgnore]
        public IPropTemplate PropTemplate { get; set; }

        [XmlIgnore]
        public PropCreatorType PropCreator
            { get { return _propCreator; } set { SetAlways<PropCreatorType>(ref _propCreator, value); } }

        [XmlIgnore]
        public object InitialValueCooked { get; set; }

        public PropItemModel(Type type, string name)
            : this
            (
            type: type,
            name: name,
            storageStrategy: PropStorageStrategyEnum.Internal,
            typeIsSolid: true,
            propKind: PropKindEnum.Prop,
            propTypeInfoField: null,
            initialValueField: null,
            extraInfo: null,
            comparer: null,
            itemType: null,
            binderField: null,
            mapperRequest: null,
            propCreator: null
            )
        {
        }

        public PropItemModel(Type type, string name, PropStorageStrategyEnum storageStrategy, 
                PropKindEnum propKind, IPropInitialValueField initialValueField)
            : this
            (
            type: type,
            name: name,
            storageStrategy: storageStrategy,
            typeIsSolid: true,
            propKind: propKind,
            propTypeInfoField: null,
            initialValueField: initialValueField,
            extraInfo: null,
            comparer: null,
            itemType: null,
            binderField: null,
            mapperRequest: null,
            propCreator: null
            )
        {
        }

        public PropItemModel(Type type, string name, PropStorageStrategyEnum storageStrategy,
                PropKindEnum propKind, IPropInitialValueField initialValueField, Type itemType)
            : this
            (
            type: type,
            name: name,
            storageStrategy: storageStrategy,
            typeIsSolid: true,
            propKind: propKind,
            propTypeInfoField: null,
            initialValueField: initialValueField,
            extraInfo: null,
            comparer: null,
            itemType: itemType,
            binderField: null,
            mapperRequest: null,
            propCreator: null
            )
        {
        }

        public PropItemModel
            (
            Type type,
            string name,
            PropStorageStrategyEnum storageStrategy,
            bool typeIsSolid,
            PropKindEnum propKind,
            ITypeInfoField propTypeInfoField,
            IPropInitialValueField initialValueField,
            object extraInfo,
            IPropComparerField comparer,
            Type itemType,
            IPropBinderField binderField,
            IMapperRequest mapperRequest,
            PropCreatorType propCreator
            )
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
            _propBinderField = binderField;
            _mapperRequest = mapperRequest;
            _propCreator = propCreator;

            InitialValueCooked = null;
        }

        public object Clone()
        {
            object extraInfo;

            if(ExtraInfo is ICloneable cloneable)
            {
                extraInfo = cloneable.Clone();
            }
            else
            {
                extraInfo = ExtraInfo;
            }

            PropItemModel result = new PropItemModel(PropertyType, PropertyName, StorageStrategy, TypeIsSolid, PropKind,
                (ITypeInfoField)PropTypeInfoField.Clone(),
                (IPropInitialValueField)InitialValueField.Clone(),
                extraInfo,
                ComparerField,
                ItemType,
                (IPropBinderField) BinderField.Clone(),
                (IMapperRequest) MapperRequest.Clone(),
                propCreator: null);
                
                return result;
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
                    _propCreator = null;
                    if(ComparerField is IDisposable disable1)
                    {
                        disable1.Dispose();
                    }

                    if(_propInitialValueField is IDisposable disable2)
                    {
                        disable2.Dispose();
                    }

                    if(InitialValueCooked is IDisposable disable3)
                    {
                        disable3.Dispose();
                    }

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
