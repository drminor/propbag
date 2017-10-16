using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DRM.PropBag.ControlModel
{
    public class PropModel : NotifyPropertyChangedBase, IEquatable<PropModel>
    {

        public const string DEFAULT_CLASS_NAME = "UndefinedClassName";

        bool dfppb;
        string cn;
        string ik;
        string ns;
        PropBagTypeSafetyMode tsm;
        bool dmrr;
        bool reiv;
        AbstractPropFactory pf;

        ObservableCollection<string> _namespaces;
        ObservableCollection<PropItem> _props;

        [XmlAttribute(AttributeName = "derive-from-pub-prop-bag")]
        public bool DeriveFromPubPropBag { get { return dfppb; } set { SetIfDifferent<bool>(ref dfppb, value); } }

        [XmlAttribute(AttributeName = "class-name")]
        public string ClassName { get { return cn; } set { SetIfDifferent<string>(ref cn, value); } }

        [XmlIgnore]
        public string InstanceKey { get { return ik; } set { SetIfDifferent<string>(ref ik, value); } }

        [XmlAttribute(AttributeName = "output-namespace")]
        public string NamespaceName { get { return ns; } set { SetIfDifferent<string>(ref ns, value); } }

        [XmlAttribute(AttributeName = "type-safety-mode")]
        public PropBagTypeSafetyMode TypeSafetyMode
        { get { return tsm; } set { SetIfDifferentVT<PropBagTypeSafetyMode>(ref tsm, value); } }


        [XmlAttribute(AttributeName = "defer-method-ref-resolution")]
        public bool DeferMethodRefResolution { get { return dmrr; } set { SetIfDifferent<bool>(ref dmrr, value); } }

        [XmlAttribute(AttributeName = "require-explicit-initial-value")]
        public bool RequireExplicitInitialValue { get { return reiv; } set { SetIfDifferent<bool>(ref reiv, value); } }


        // TODO: This is not Serializable, consider providing string representation as a proxy
        // Perhaps we should simply not serialize instances of PropBag Control Models.
        [XmlIgnore]
        public AbstractPropFactory PropFactory { get { return pf; } set { SetAlways<AbstractPropFactory>(ref pf, value); } }

        [XmlArray("namespaces")]
        [XmlArrayItem("namespace")]
        public ObservableCollection<string> Namespaces
        {
            get { return _namespaces; }
            set { this.SetCollection<ObservableCollection<string>, string>(ref _namespaces, value); }
        }

        [XmlArray("props")]
        [XmlArrayItem("prop")]
        public ObservableCollection<PropItem> Props
        {
            get { return _props; }
            set { this.SetCollection<ObservableCollection<PropItem>, PropItem>(ref _props, value); }
        }

        public bool IsClassDefined => ClassName != DEFAULT_CLASS_NAME;

        public PropModel() : this(DEFAULT_CLASS_NAME, null, null) { }

        public PropModel(string className, string instanceKey, string namespaceName,
            bool deriveFromPubPropBag = false,
            PropBagTypeSafetyMode typeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered,
            bool deferMethodRefResolution = true,
            bool requireExplicitInitialValue = true)
        {
            DeriveFromPubPropBag = deriveFromPubPropBag;
            ClassName = className;
            InstanceKey = instanceKey;
            NamespaceName = namespaceName;
            TypeSafetyMode = typeSafetyMode;
            DeferMethodRefResolution = deferMethodRefResolution;
            RequireExplicitInitialValue = requireExplicitInitialValue;

            Namespaces = new ObservableCollection<string>();
            Props = new ObservableCollection<PropItem>();
        }

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



    }
}
