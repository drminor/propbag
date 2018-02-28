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
    using PropItemSetInterface = IPropItemSet<String>;

    public class PropModel : NotifyPropertyChangedBase, IEquatable<IPropModel>, IPropModel, IDisposable
    {
        #region Properties

        //public SimpleExKey TestObject { get; set; }


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

        string cn;
        [XmlAttribute(AttributeName = "class-name")]
        public string ClassName { get { return cn; } set { SetIfDifferent<string>(ref cn, value); } }

        //string ik;
        //[XmlIgnore]
        //public string InstanceKey { get { return ik; } set { SetIfDifferent<string>(ref ik, value); } }

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


        //Type _propStoreServiceProviderType;
        //[XmlElement("prop-store-service-provider-type")]
        //public Type PropStoreServiceProviderType { get { return _propStoreServiceProviderType; } set { _propStoreServiceProviderType = value; } }

        ObservableCollection<string> _namespaces;
        [XmlArray("namespaces")]
        [XmlArrayItem("namespace")]
        public ObservableCollection<string> Namespaces
        {
            get { return _namespaces; }
            set { this.SetCollection<ObservableCollection<string>, string>(ref _namespaces, value); }
        }
        
        ObservableCollection<IPropModelItem> _props;
        [XmlArray("props")]
        [XmlArrayItem("prop")]
        public ObservableCollection<IPropModelItem> Props
        {
            get { return _props; }
            set { this.SetCollection<ObservableCollection<IPropModelItem>, IPropModelItem>(ref _props, value); }
        }

        IProvidePropModels pmp;
        [XmlIgnore]
        public IProvidePropModels PropModelProvider { get { return pmp; } set { SetAlways<IProvidePropModels>(ref pmp, value); } }

        PropItemSetInterface _propItemSet;
        [XmlIgnore]
        public PropItemSetInterface PropItemSet { get { return _propItemSet; } set { SetAlways<PropItemSetInterface>(ref _propItemSet, value); } }

        [XmlIgnore]
        public PropertyDescriptorCollection PropertyDescriptorCollection { get; set; }


        #endregion Other Properties

        #endregion Dependency Properties

        #region Constructors

        public PropModel(string className, string namespaceName,
            DeriveFromClassModeEnum deriveFrom,
            Type targetType,
            IPropFactory propFactory,
            IProvidePropModels propModelProvider,
            PropBagTypeSafetyMode typeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered,
            bool deferMethodRefResolution = true,
            bool requireExplicitInitialValue = true)
        {
            ClassName = className;
            NamespaceName = namespaceName;
            DeriveFromClassMode = deriveFrom;
            TargetType = targetType;
            PropFactory = propFactory;
            PropFactoryType = propFactory?.GetType();

            PropModelProvider = propModelProvider;

            TypeSafetyMode = typeSafetyMode;
            DeferMethodRefResolution = deferMethodRefResolution;
            RequireExplicitInitialValue = requireExplicitInitialValue;

            Namespaces = new ObservableCollection<string>();
            Props = new ObservableCollection<IPropModelItem>();
        }

        public PropModel(string className, string namespaceName,
            DeriveFromClassModeEnum deriveFrom,
            Type targetType,
            Type propFactoryType,
            IProvidePropModels propModelProvider,

            PropBagTypeSafetyMode typeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered,
            bool deferMethodRefResolution = true,
            bool requireExplicitInitialValue = true)
        {
            ClassName = className;
            NamespaceName = namespaceName;
            DeriveFromClassMode = deriveFrom;
            TargetType = targetType;
            PropFactoryType = propFactoryType;
            PropModelProvider = propModelProvider;

            TypeSafetyMode = typeSafetyMode;
            DeferMethodRefResolution = deferMethodRefResolution;
            RequireExplicitInitialValue = requireExplicitInitialValue;

            Namespaces = new ObservableCollection<string>();
            Props = new ObservableCollection<IPropModelItem>();

            PropFactory = null;
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
                    _typeToCreate = GetTargetType(this.DeriveFromClassMode, this.TargetType, this.WrapperTypeInfoField);
                }
                return _typeToCreate;
            }
        }

        private Type GetTargetType(DeriveFromClassModeEnum deriveFrom, Type typeToWrap, ITypeInfoField typeInfofield)
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
                        result = typeToWrap;
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

        #region IEquatable support and Object overrides

        public override int GetHashCode()
        {
            var hashCode = -1621535375;
            hashCode = hashCode * -1521134295 + DeriveFromClassMode.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullClassName);
            hashCode = hashCode * -1521134295 + Props?.Count.GetHashCode() ?? 1521134295;
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
            if(ReferenceEquals(other, null) || !(other is IPropModel)) return false;
            return Equals((IPropModel)other);
        }

        public bool Equals(IPropModel other)
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
                && Props.Count == other.Props.Count;
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
                    foreach(IPropModelItem pi in Props)
                    {
                        pi.Dispose();
                    }
                    Props.Clear();
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
