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

        string _className;
        [XmlAttribute(AttributeName = "class-name")]
        public string ClassName
        {
            get { return _className; }
            set
            {
                OpenIfNotRegisteredWithCache();

                if(SetIfDifferent<string>(ref _className, value))
                {
                    _fullClassName = null; // This will force a rebuild of the public FullClassName property.
                }
            }
        }

        string _namespaceName;
        [XmlAttribute(AttributeName = "output-namespace")]
        public string NamespaceName
        {
            get { return _namespaceName; }
            set
            {
                OpenIfNotRegisteredWithCache();
                if(SetIfDifferent<string>(ref _namespaceName, value))
                {
                    _fullClassName = null; // This will force a rebuild of the public FullClassName property.
                }
            }
        }

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
        
        Dictionary<PropNameType, IPropItemModel> _propDictionary { get; set; }

        [XmlElement, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlArray("props")]
        [XmlArrayItem("prop")]
        public List<IPropItemModel> Props
        {
            get => GetPropItems().ToList();
            set
            {
                OpenIfNotRegisteredWithCache();

                if (_propDictionary != null)
                {
                    Clear();
                }
                _propDictionary = value.ToDictionary(k => k.PropertyName, v=> v);
            }
        }

        PropModelCacheInterface pmp;
        [XmlIgnore]
        public PropModelCacheInterface PropModelCache
        {
            get
            {
                return pmp;
            }
            set
            {
                if(ReferenceEquals(pmp, value))
                {
                    // The new value is the same as the existing value.
                    return;
                }
                else
                {
                    if(value != null && pmp != null)
                    {
                        throw new InvalidOperationException("Once a PropModel has been associated with a PropModelCache, it cannot be associated with a different PropModelCache.");
                    }
                }
                SetAlways<PropModelCacheInterface>(ref pmp, value, nameof(PropModelCache));
            }
        }

        ICustomTypeDescriptor _ctd;
        [XmlIgnore]
        public ICustomTypeDescriptor CustomTypeDescriptor
        {
            get => _ctd;
            set
            {
                if(IsFixed)
                {
                    if(_ctd != null)
                    {
                        string theSameNote = ReferenceEquals(_ctd, value) ? "is the same" : "is not the same";
                        System.Diagnostics.Debug.WriteLine($"WARNING: A PropModel that has previously been assigned a CustomTypeDesriptor is being assigned a CustomTypeDescriptor. The new value {theSameNote} as the old value.");
                    }
                    _ctd = value;
                }
                else
                {
                    throw new InvalidOperationException("The CustomTypeDescriptor can only be set on fixed PropModels.");
                }
            }
        }

        internal void SetCustomTypeDescriptor(ICustomTypeDescriptor newValue)
        {
            if(_ctd != null)
            {
                throw new InvalidOperationException("The internal SetCustomTypeDescriptor can only be used on PropModels for which no CustomTypeDescriptor has been set.");
            }
            _ctd = newValue;
        }

        object _syncLock = new object();
        public const long GEN_NONE = -1;

        #endregion Other Properties

        #endregion Dependency Properties

        #region Constructors

        private PropModel()
        {
            throw new NotSupportedException("PropModels cannot be created using the parameterless constructor.");
        }

        public PropModel
            (
            string className,
            string namespaceName,
            DeriveFromClassModeEnum deriveFrom,
            Type targetType,
            IPropFactory propFactory,
            Type propFactoryType,
            PropModelCacheInterface propModelCache,
            PropBagTypeSafetyMode typeSafetyMode,
            bool deferMethodRefResolution,
            bool requireExplicitInitialValue
            )
            : this
            (
                className, namespaceName, deriveFrom, targetType, propFactory, propFactoryType, propModelCache,
                typeSafetyMode: typeSafetyMode,
                deferMethodRefResolution: deferMethodRefResolution,
                requireExplicitInitialValue: requireExplicitInitialValue,
                wrapperTypeInfo: null,
                newEmittedType: null,
                parent: null,
                isFixed: false,
                generationId: GEN_NONE,
                customTypeDescriptor: null
            )
        {
        }

        public PropModel
            (
            string className,
            string namespaceName,
            DeriveFromClassModeEnum deriveFrom,
            Type targetType,
            IPropFactory propFactory,
            Type propFactoryType,
            PropModelCacheInterface propModelCache,
            PropBagTypeSafetyMode typeSafetyMode,
            bool deferMethodRefResolution,
            bool requireExplicitInitialValue,
            ITypeInfoField wrapperTypeInfo,
            Type newEmittedType,
            PropModelType parent,
            bool isFixed,
            long generationId,
            ICustomTypeDescriptor customTypeDescriptor
            )
        {
            ClassName = className ?? throw new ArgumentNullException(nameof(className));
            NamespaceName = namespaceName;
            DeriveFromClassMode = deriveFrom;
            TargetType = targetType;
            PropFactory = propFactory;
            PropFactoryType = propFactoryType; // ?? propFactory?.GetType() ?? throw new ArgumentNullException("Either the propFactory or the propFactoryType must be specified when creating a PropModel.");

            if(propFactory == null && propFactoryType == null)
            {
                throw new ArgumentNullException("Either the propFactory or the propFactoryType must be specified when creating a PropModel.");
            }

            PropModelCache = propModelCache;

            TypeSafetyMode = typeSafetyMode;
            DeferMethodRefResolution = deferMethodRefResolution;
            RequireExplicitInitialValue = requireExplicitInitialValue;


            if (deriveFrom == DeriveFromClassModeEnum.Custom)
            {
                if (targetType == null)
                {
                    throw new ArgumentNullException("The targetType must be specified when the deriveFrom is set to 'Custom'.");
                }
            }

            WrapperTypeInfoField = wrapperTypeInfo;
            NewEmittedType = newEmittedType;

            Parent = parent;
            IsFixed = isFixed;

            // Use the backing store to avoid the "No PropModelCache" error.
            _generationId = generationId;

            // Use the backing store to avoid the "Can only be set on fixed PropModels" error.
            _ctd = null;

            Namespaces = new ObservableCollection<string>();
            _propDictionary = new Dictionary<PropNameType, IPropItemModel>();
        }

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

        string _fullClassName;
        public string FullClassName
        {
            get
            {
                if(_fullClassName == null)
                {
                    _fullClassName = GetFullClassName(this.NamespaceName, this.ClassName.Trim());
                }
                return _fullClassName;
            }
        }

        #endregion

        #region Public Members

        public object Clone()
        {
            object result;
            if(PropModelCache != null)
            {
                result = CloneIt(GEN_NONE);
            }
            else
            {
                if(PropModelCache.TryClone(this, out PropModelType clonedCopy))
                {
                    result = clonedCopy;
                }
                else
                {
                    throw new InvalidOperationException("The PropModel cache could not clone this instance.");
                }
            }
            return result;
        }

        public PropModelType CloneIt()
        {
            PropModelType clonedCopy = CloneIt(GEN_NONE);
            return clonedCopy;
        }


        public PropModelType CloneIt(long generationId)
        {
            if (PropModelCache != null)
            {
                throw new InvalidOperationException("Cannot clone PropModels that are associated with a PropModel cache directly. Use the TryClone method on the PropModel Cache.");
            }

            PropModel result;

            lock (_syncLock)
            {
                //// Set the propFactory to null, if we have a PropFactoryType.
                //IPropFactory propFactoryToUse = PropFactoryType != null ? null : PropFactory;

                // Use the PropFactory currently set on the PropModel.
                // Note: This PropFactory could have been "generated on the fly" from the PropFactoryFactory.
                // Note: This single PropFactory could potentially be used by many PropBag instances.

                IPropFactory propFactoryToUse = PropFactory;  //PropFactoryType != null ? null : PropFactory;

                result = new PropModel
                    (
                    ClassName, NamespaceName, DeriveFromClassMode, TargetType,
                    propFactoryToUse, PropFactoryType, PropModelCache,
                    TypeSafetyMode, DeferMethodRefResolution, RequireExplicitInitialValue,
                    WrapperTypeInfoField, NewEmittedType, Parent,
                    IsFixed, generationId, CustomTypeDescriptor
                    );

                //result.WrapperTypeInfoField = WrapperTypeInfoField;
                //result.NewEmittedType = NewEmittedType;

                //// Use our internal Set routine.
                //result.SetCustomTypeDescriptor(CustomTypeDescriptor);

                result.Namespaces = new ObservableCollection<PropNameType>();
                foreach (string ns in Namespaces)
                {
                    result.Namespaces.Add(ns);
                }

                result._propDictionary = new Dictionary<PropNameType, IPropItemModel>();
                foreach (IPropItemModel pi in GetPropItems())
                {
                    result._propDictionary.Add(pi.PropertyName, (IPropItemModel)pi.Clone());
                }
            }

            return result;
        }

        public bool IsFixed { get; private set; }

        public int Count => _propDictionary.Count;

        PropModelType _parent;
        public PropModelType Parent
        {
            get => _parent;

            set
            {
                OpenIfNotRegisteredWithCache();

                if (_parent != value)
                {
                    _parent = value;
                }
            }
        }

        long _generationId;
        public long GenerationId
        {
            get
            {
                return _generationId;
            }

            set
            {
                //if(PropModelCache == null)
                //{
                //    throw new InvalidOperationException($"The PropModel with Full Class Name: {this} has no PropModelCache assigned to it. Setting the GenerationId is not allowed in this context.");
                //}

                //if (IsFixed) throw new InvalidOperationException($"The PropModel with Full Class Name: {this} is fixed. The GenerationId cannot be changed.");

                if (IsFixed && PropModelCache != null)
                {
                    throw new InvalidOperationException($"The PropModel with Full Class Name: {this} is fixed and is associated with a PropModel Cache. The GenerationId cannot be changed.");
                }

                _generationId = value;
            }
        }

        public void Fix()
        {
            _hashCode = ComputeHashCode();
            IsFixed = true;
        }

        public void Open()
        {
            lock(_syncLock)
            {
                if(PropModelCache != null)
                {
                    if(IsFixed)
                    {
                        throw new InvalidOperationException("Cannot open a fixed PropModel that is associated with a PropModel cache.");
                    }
                    return;
                }
                else
                {
                    if(IsFixed)
                    {
                        IsFixed = false;
                    }
                }

            }
        }

        public void Add(string propertyName, IPropItemModel propModelItem)
        {
            lock(_syncLock)
            {
                OpenIfNotRegisteredWithCache();

                _propDictionary.Add(propertyName, propModelItem);
            }
        }

        public bool Contains(string propertyName)
        {
            lock(_syncLock)
            {
                bool result = _propDictionary.ContainsKey(propertyName);
                return result;
            }
        }

        public bool Contains(IPropItemModel propModelItem)
        {
            lock (_syncLock)
            {
                bool result = _propDictionary.ContainsValue(propModelItem);
                return result;
            }
        }

        public IEnumerable<string> GetPropNames()
        {
            IEnumerable<PropNameType> result = _propDictionary.Select(x => x.Key);
            return result;
        }

        public IEnumerable<IPropItemModel> GetPropItems()
        {
            IEnumerable<IPropItemModel> result = _propDictionary.Select(x => x.Value);
            return result;
        }

        public bool TryGetPropModelItem(string propertyName, out IPropItemModel propModelItem)
        {
            lock(_syncLock)
            {
                bool result = _propDictionary.TryGetValue(propertyName, out propModelItem);
                return result;
            }
        }

        public bool TryRemove(string propertyName, out IPropItemModel propModelItem)
        {
            lock (_syncLock)
            {
                OpenIfNotRegisteredWithCache();

                if (_propDictionary.TryGetValue(propertyName, out propModelItem))
                {
                    _propDictionary.Remove(propertyName);
                    return true;
                }
            }

            propModelItem = null;
            return false;
        }

        public void Clear()
        {
            OpenIfNotRegisteredWithCache();

            foreach (KeyValuePair<PropNameType, IPropItemModel> kvp in _propDictionary)
            {
                kvp.Value.Dispose();
            }

            _propDictionary.Clear();
            CustomTypeDescriptor = null;
        }

        #endregion

        #region Private Methods

        private void OpenIfNotRegisteredWithCache()
        {
            if (IsFixed)
            {
                if (PropModelCache != null)
                {
                    throw new InvalidOperationException($"The PropModel with Full Class Name: {this} is fixed. Cannot make changed to this PropModel.");
                }

                System.Diagnostics.Debug.Assert(GenerationId == GEN_NONE, $"The PropModel with Full Class Name: {this} has no PropModelCache assigned to it, however the GenerationId is not {GEN_NONE}.");
                Open();
            }
        }

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

        #endregion

        #region IEquatable Support and Object Overrides

        public override string ToString()
        {
            return $"PropModel: {FullClassName}, {GenerationId}";
        }

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
                    Open();
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
