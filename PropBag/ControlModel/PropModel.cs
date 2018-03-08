using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DRM.PropBag
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;
    using PropModelCacheInterface = ICachePropModels<String>;

    public class PropModel : NotifyPropertyChangedBase, PropModelType, IEquatable<PropModelType>, IDisposable
    {
        #region Properties

        #region Activation Info

        DeriveFromClassModeEnum _deriveFromClassMode;
        [XmlAttribute(AttributeName = "derive-from-class-mode")]
        public DeriveFromClassModeEnum DeriveFromClassMode
        { get { return _deriveFromClassMode; } set { SetIfDifferentVT<DeriveFromClassModeEnum>(ref _deriveFromClassMode, value, nameof(DeriveFromClassMode)); } }

        Type _targetType;
        [XmlElement("type")]
        public Type TargetType { get { return _targetType; } set { _targetType = value; } }

        ITypeInfoField _wrapperTypeInfoField;
        [XmlElement("type-info")]
        public ITypeInfoField WrapperTypeInfoField
        {
            get { return _wrapperTypeInfoField; }
            set { SetAlways<ITypeInfoField>(ref _wrapperTypeInfoField, value); }
        }

        Type _newEmittedType;
        [XmlIgnore]
        public Type NewEmittedType { get { return _newEmittedType; } set { _newEmittedType = value; } }

        string cn;
        [XmlAttribute(AttributeName = "class-name")]
        public string ClassName { get { return cn; } set { SetIfDifferent<string>(ref cn, value); } }

        string ns;
        [XmlAttribute(AttributeName = "output-namespace")]
        public string NamespaceName { get { return ns; } set { SetIfDifferent<string>(ref ns, value); } }

        #endregion

        #region Other Properties

        PropBagTypeSafetyMode tsm = PropBagTypeSafetyMode.Tight;
        [XmlAttribute(AttributeName = "type-safety-mode")]
        public PropBagTypeSafetyMode TypeSafetyMode
        { get { return tsm; } set { SetIfDifferentVT<PropBagTypeSafetyMode>(ref tsm, value, nameof(TypeSafetyMode)); } }

        bool dmrr;
        [XmlAttribute(AttributeName = "defer-method-ref-resolution")]
        public bool DeferMethodRefResolution { get { return dmrr; } set { SetIfDifferent<bool>(ref dmrr, value); } }

        bool reiv;
        [XmlAttribute(AttributeName = "require-explicit-initial-value")]
        public bool RequireExplicitInitialValue { get { return reiv; } set { SetIfDifferent<bool>(ref reiv, value); } }


        // TODO: This is not Serializable, consider providing string representation as a proxy
        IPropFactory pf;
        [XmlIgnore]
        public IPropFactory PropFactory { get { return pf; } set { SetAlways<IPropFactory>(ref pf, value); } }

        Type _propFactoryType;
        [XmlElement("type")]
        public Type PropFactoryType { get { return _propFactoryType; } set { _propFactoryType = value; } }

        ObservableCollection<string> _namespaces;
        [XmlArray("namespaces")]
        [XmlArrayItem("namespace")]
        public ObservableCollection<string> Namespaces
        {
            get { return _namespaces; }
            set { this.SetCollection<ObservableCollection<string>, string>(ref _namespaces, value); }
        }
        
        Dictionary<PropNameType, IPropModelItem> _propDictionary { get; set; }

        [XmlElement, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlArray("props")]
        [XmlArrayItem("prop")]
        public List<IPropModelItem> Props
        {
            get => GetPropItems().ToList();
            set
            {
                if(_propDictionary != null)
                {
                    Clear();
                }
                _propDictionary = value.ToDictionary(k => k.PropertyName, v=> v);
            }
        }

        PropModelCacheInterface pmp;
        [XmlIgnore]
        public PropModelCacheInterface PropModelCache { get { return pmp; } set { SetAlways<PropModelCacheInterface>(ref pmp, value); } }

        [XmlIgnore]
        public PropertyDescriptorCollection PropertyDescriptorCollection { get; set; }

        #endregion Other Properties

        #endregion Dependency Properties

        #region Constructors

        private PropModel()
        {
            throw new NotSupportedException("PropModels cannot be created using the parameterless constructor.");
        }

        // Create with resolved PropFactory
        public PropModel(string className, string namespaceName,
            DeriveFromClassModeEnum deriveFrom,
            Type targetType,
            IPropFactory propFactory,
            Type propFactoryType,
            PropModelCacheInterface propModelProvider,
            PropBagTypeSafetyMode typeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered,
            bool deferMethodRefResolution = true,
            bool requireExplicitInitialValue = true,
            PropModelType parent = null
            )
        {
            ClassName = className ?? throw new ArgumentNullException(nameof(className));
            NamespaceName = namespaceName;
            DeriveFromClassMode = deriveFrom;
            TargetType = targetType;
            PropFactory = propFactory;
            PropFactoryType = propFactoryType ?? propFactory?.GetType() ?? throw new ArgumentNullException("Either the propFactory or the propFactoryType must be specified when creating a PropModel.");

            PropModelCache = propModelProvider;

            TypeSafetyMode = typeSafetyMode;
            DeferMethodRefResolution = deferMethodRefResolution;
            RequireExplicitInitialValue = requireExplicitInitialValue;

            Parent = parent;

            _isFixed = false;

            if (deriveFrom == DeriveFromClassModeEnum.Custom)
            {
                if (targetType == null)
                {
                    throw new ArgumentNullException("The targetType must be specified when the deriveFrom is set to 'Custom'.");
                }
            }

            Namespaces = new ObservableCollection<string>();
            _propDictionary = new Dictionary<PropNameType, IPropModelItem>();
            PropertyDescriptorCollection = null;
        }

        //// Create with PropFactory Factory
        //public PropModel(string className, string namespaceName,
        //    DeriveFromClassModeEnum deriveFrom,
        //    Type targetType,
        //    Type propFactoryType,
        //    IProvidePropModels propModelProvider,

        //    PropBagTypeSafetyMode typeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered,
        //    bool deferMethodRefResolution = true,
        //    bool requireExplicitInitialValue = true,
        //    PropModelType parent = null
        //    )
        //{
        //    ClassName = className;
        //    NamespaceName = namespaceName;
        //    DeriveFromClassMode = deriveFrom;
        //    TargetType = targetType;
        //    PropFactoryType = propFactoryType;
        //    PropModelProvider = propModelProvider;

        //    TypeSafetyMode = typeSafetyMode;
        //    DeferMethodRefResolution = deferMethodRefResolution;
        //    RequireExplicitInitialValue = requireExplicitInitialValue;

        //    Namespaces = new ObservableCollection<string>();
        //    Props = new Dictionary<PropNameType, IPropModelItem>();

        //    Parent = parent;

        //    PropFactory = null;
        //    _isFixed = false;
        //}

        #endregion

        #region Type and Namespace support

        Type _typeToCreate;
        public Type TypeToCreate
        {
            get
            {
                if(_typeToCreate == null)
                {
                    _typeToCreate = GetTypeToCreate(this.DeriveFromClassMode, this.TargetType, this.WrapperTypeInfoField);
                }
                return _typeToCreate;
            }
        }

        private Type GetTypeToCreate(DeriveFromClassModeEnum deriveFrom, Type targetType, ITypeInfoField typeInfofield)
        {
            Type result;

            switch (deriveFrom)
            {
                case DeriveFromClassModeEnum.PropBag:
                    {
                        result = typeof(PropBag);
                        break;
                    }
                case DeriveFromClassModeEnum.PubPropBag:
                    {
                        result = typeof(PubPropBag);
                        break;
                    }
                case DeriveFromClassModeEnum.Custom:
                    {
                        // TODO: This needs (lots) more work.
                        result = targetType;
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException($"{deriveFrom} is not recognized or is not supported.");
                    }
            }

            return result;
        }
        
        public string FullClassName
        {
            get
            {
                return GetFullClassName(this.NamespaceName, this.ClassName.Trim());
            }
        }

        #endregion

        #region Public Members

        public object Clone()
        {
            PropModelType result = CloneIt();
            return result;
        }

        public PropModelType CloneIt()
        {
            PropModel result = new PropModel(ClassName, NamespaceName, DeriveFromClassMode, TargetType, 
                PropFactory, PropFactoryType, PropModelCache,
                TypeSafetyMode, DeferMethodRefResolution, RequireExplicitInitialValue);

            result.Namespaces = new ObservableCollection<PropNameType>();
            foreach(string ns in Namespaces)
            {
                result.Namespaces.Add(ns);
            }

            result._propDictionary = new Dictionary<PropNameType, IPropModelItem>();
            foreach(IPropModelItem pi in GetPropItems())
            {
                if (pi is IPropModelItem pmi)
                {
                    result._propDictionary.Add(pi.PropertyName, (IPropModelItem) pi.Clone());
                }
            }

            return result;
        }

        bool _isFixed;
        public bool IsFixed => _isFixed;

        public int Count => _propDictionary.Count;

        PropModelType _parent;
        public PropModelType Parent
        {
            get => _parent;

            set
            {
                if(_isFixed) throw new InvalidOperationException();

                if(_parent != value)
                {
                    _parent = value;
                }

            }
        }

        long _generationId;
        public long GenerationId
        {
            get => _generationId;
            set
            {
                if (_isFixed) throw new InvalidOperationException();
                _generationId = value;
            }
        }

        public void Fix()
        {
            _hashCode = ComputeHashCode();
            _isFixed = true;
        }

        public void Add(string propertyName, IPropModelItem propModelItem)
        {
            if (IsFixed) throw new InvalidOperationException();
            _propDictionary.Add(propertyName, propModelItem);
        }

        public bool Contains(string propertyName)
        {
            bool result = _propDictionary.ContainsKey(propertyName);
            return result;
        }

        public bool Contains(IPropModelItem propModelItem)
        {
            bool result = _propDictionary.ContainsValue(propModelItem);
            return result;
        }

        public IEnumerable<string> GetPropNames()
        {
            IEnumerable<PropNameType> result = _propDictionary.Select(x => x.Key);
            return result;
        }

        public IEnumerable<IPropModelItem> GetPropItems()
        {
            IEnumerable<IPropModelItem> result = _propDictionary.Select(x => x.Value);
            return result;
        }

        public bool TryGetPropModelItem(string propertyName, out IPropModelItem propModelItem)
        {
            bool result = _propDictionary.TryGetValue(propertyName, out propModelItem);
            return result;
        }

        public bool TryRemove(string propertyName, out IPropModelItem propModelItem)
        {
            if (IsFixed) throw new InvalidOperationException();

            if (_propDictionary.TryGetValue(propertyName, out propModelItem))
            {
                return true;
            }
            else
            {
                propModelItem = null;
                return false;
            }
        }

        public void Clear()
        {
            foreach (KeyValuePair<PropNameType, IPropModelItem> kvp in _propDictionary)
            {
                kvp.Value.Dispose();
            }

            _propDictionary.Clear();
        }

        #endregion

        #region Private Methods

        private string GetFullClassName(string namespaceName, string className)
        {
            if (namespaceName == null)
            {
                return className;
            }
            else if(ClassName == null || className.Length == 0)
            {
                throw new InvalidOperationException("The className cannot be null or empty.");
            }
            else
            {
                string separator = ".";
                string result = $"{namespaceName}{separator}{className}";
                return result;
            }
        }

        //private void AssignGenerationId()
        //{
        //    if (_parent != null)
        //    {
        //        _generationId = _parent.GetNextGenerationId();
        //    }
        //    else
        //    {
        //        _generationId = GetNextGenerationId();
        //    }
        //}

        #endregion

        #region IEquatable support and Object overrides

        int _hashCode;
        public override int GetHashCode()
        {
            if(IsFixed)
            {
                return _hashCode;
            }
            else
            {
                int result = ComputeHashCode();
                return result;
            }
        }

        private int ComputeHashCode()
        {
            var hashCode = -1621535375;
            hashCode = hashCode * -1521134295 + DeriveFromClassMode.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullClassName);
            hashCode = hashCode * -1521134295 + _propDictionary?.Count.GetHashCode() ?? 1521134295;
            hashCode = hashCode * -1521134295 + TypeSafetyMode.GetHashCode();
            hashCode = hashCode * -1521134295 + RequireExplicitInitialValue.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(TypeToCreate);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(TargetType);
            hashCode = hashCode * -1521134295 + EqualityComparer<IPropFactory>.Default.GetHashCode(PropFactory);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(PropFactoryType);
            return hashCode;
        }

        public override bool Equals(object other)
        {
            if(ReferenceEquals(other, null) || !(other is PropModelType)) return false;
            return Equals((PropModelType)other);
        }

        public bool Equals(PropModelType other)
        {
            // If parameter is null, return false.
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            // TODO: This needs to be improved.
            return this.ClassName == other.ClassName
                && NamespaceName == other.NamespaceName
                && Count == other.Count;
        }

        public static bool operator ==(PropModel left, PropModel right)
        {

            // Check for null on left side.
            if (ReferenceEquals(left, null))
            {
                if (ReferenceEquals(right, null))
                {
                    // null == null = true.
                    return true;
                }
                else
                {
                    // Only the left side is null.
                    return false;
                }
            }
            else
            {
                if(ReferenceEquals(right, null))
                {
                    // Left is not null, right is null.
                    return false;
                }
                else
                {
                    return left.Equals(right);
                }
            }

        }

        public static bool operator !=(PropModel left, PropModel right)
        {
            return !(left == right);
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //Dispose managed state (managed objects).
                    Clear();
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
