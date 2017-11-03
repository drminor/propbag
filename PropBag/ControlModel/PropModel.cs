using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

// ToDo: Consider moving all types in the DRM.PropBag.ControlModel namespace back to the DRM.PropBag namespace.
namespace DRM.PropBag.ControlModel
{
    public class PropModel : NotifyPropertyChangedBase, IEquatable<PropModel>
    {
        #region Properties

        #region Activation Info
        //bool dfppb;
        //[XmlAttribute(AttributeName = "derive-from-pub-prop-bag")]
        //public bool DeriveFromPubPropBag { get { return dfppb; } set { SetIfDifferent<bool>(ref dfppb, value); } }

        DeriveFromClassModeEnum _deriveFromClassMode;
        [XmlAttribute(AttributeName = "derive-from-class-mode")]
        public DeriveFromClassModeEnum DeriveFromClassMode
        { get { return _deriveFromClassMode; } set { SetIfDifferentVT<DeriveFromClassModeEnum>(ref _deriveFromClassMode, value); } }

        Type _typeToWrap;
        [XmlElement("type")]
        public Type TypeToWrap { get { return _typeToWrap; } set { _typeToWrap = value; } }

        TypeInfoField _wrapperTypeInfoField;
        [XmlElement("type-info")]
        public TypeInfoField WrapperTypeInfoField
        {
            get { return _wrapperTypeInfoField; }
            set { SetIfDifferent<TypeInfoField>(ref _wrapperTypeInfoField, value); }
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

        PropBagTypeSafetyMode tsm = PropBagTypeSafetyMode.Tight;
        [XmlAttribute(AttributeName = "type-safety-mode")]
        public PropBagTypeSafetyMode TypeSafetyMode
        { get { return tsm; } set { SetIfDifferentVT<PropBagTypeSafetyMode>(ref tsm, value); } }

        bool dmrr;
        [XmlAttribute(AttributeName = "defer-method-ref-resolution")]
        public bool DeferMethodRefResolution { get { return dmrr; } set { SetIfDifferent<bool>(ref dmrr, value); } }

        bool reiv;
        [XmlAttribute(AttributeName = "require-explicit-initial-value")]
        public bool RequireExplicitInitialValue { get { return reiv; } set { SetIfDifferent<bool>(ref reiv, value); } }


        // TODO: This is not Serializable, consider providing string representation as a proxy
        // Perhaps we should simply not serialize instances of PropBag Control Models.
        IPropFactory pf;
        [XmlIgnore]
        public IPropFactory PropFactory { get { return pf; } set { SetAlways<IPropFactory>(ref pf, value); } }

        ObservableCollection<string> _namespaces;
        [XmlArray("namespaces")]
        [XmlArrayItem("namespace")]
        public ObservableCollection<string> Namespaces
        {
            get { return _namespaces; }
            set { this.SetCollection<ObservableCollection<string>, string>(ref _namespaces, value); }
        }

        ObservableCollection<PropItem> _props;
        [XmlArray("props")]
        [XmlArrayItem("prop")]
        public ObservableCollection<PropItem> Props
        {
            get { return _props; }
            set { this.SetCollection<ObservableCollection<PropItem>, PropItem>(ref _props, value); }
        }

        //public bool IsClassDefined => ClassName != DEFAULT_CLASS_NAME;

        #endregion

        #region Constructors

        //public const string DEFAULT_CLASS_NAME = "UndefinedClassName";

        //public PropModel() : this(DEFAULT_CLASS_NAME,
        //    null,
        //    DeriveFromClassModeEnum.PropBag,
        //    typeof(PropBag),
        //    null,
        //    null,
        //    PropBagTypeSafetyMode.AllPropsMustBeRegistered,
        //    deferMethodRefResolution: false,
        //    requireExplicitInitialValue: false) { }


        public PropModel(string className, string namespaceName,
            DeriveFromClassModeEnum deriveFrom,
            Type typeToWrap,
            TypeInfoField wrapperTypeInfoField,
            IPropFactory propFactory,
            PropBagTypeSafetyMode typeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered,
            bool deferMethodRefResolution = true,
            bool requireExplicitInitialValue = true)
        {
            ClassName = className;
            NamespaceName = namespaceName;
            DeriveFromClassMode = deriveFrom;
            TypeToWrap = typeToWrap;
            WrapperTypeInfoField = wrapperTypeInfoField;
            TypeSafetyMode = typeSafetyMode;
            DeferMethodRefResolution = deferMethodRefResolution;
            RequireExplicitInitialValue = requireExplicitInitialValue;

            Namespaces = new ObservableCollection<string>();
            Props = new ObservableCollection<PropItem>();
        }

        #endregion

        #region Type and Namespace support

        Type _targetType;
        public Type TargetType
        {
            get
            {
                if(_targetType == null)
                {
                    _targetType = GetTargetType(this.DeriveFromClassMode, this.TypeToWrap, this.WrapperTypeInfoField);
                }
                return _targetType;
            }
        }

        private Type GetTargetType(DeriveFromClassModeEnum deriveFrom, Type typeToWrap, TypeInfoField typeInfofield)
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

        //private string GetFullClassName(IEnumerable<string> namespaces, string className)
        //{
        //    if(namespaces == null)
        //    {
        //        return className;
        //    }
        //    else
        //    {
        //        string separator = ".";
        //        string result = $"{string.Join(separator, namespaces)}{separator}{className}";
        //        return result;
        //    }
        //}

        #endregion

        #region IEquatable support and Object overrides

        // TODO:!! Update the GetHashCode for DRM.PropBag.ControlModel.PropModel
        public override int GetHashCode()
        {
            return ClassName.GetHashCode();
            //return GenerateHash.CustomHash(Name.GetHashCode(), Type.GetHashCode());
            //var hashCode = Type.GetHashCode();
            //foreach (var property in AdditionalProperties)
            //{
            //    hashCode = GenerateHash.CustomHash(hashCode, property.GetHashCode());
            //}
            //return hashCode;
        }

        public override bool Equals(object other)
        {
            if (Object.ReferenceEquals(other, null) || !(other is PropModel)) return false;
            return Equals((PropModel)other);
        }

        // TODO: This needs to be improved!!
        public bool Equals(PropModel other)
        {
            // If parameter is null, return false.
            if (Object.ReferenceEquals(other, null))
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != other.GetType())
            {
                return false;
            }
            return this.ClassName == other.ClassName
                && NamespaceName == other.NamespaceName
                && Props.Count == other.Props.Count;
        }

        public static bool operator ==(PropModel left, PropModel right)
        {

            // Check for null on left side.
            if (Object.ReferenceEquals(left, null))
            {
                if (Object.ReferenceEquals(right, null))
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return left.Equals(right);
        }

        public static bool operator !=(PropModel left, PropModel right)
        {
            return !(left == right);
        }

        #endregion
    }
}
