using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DRM.PropBag.ControlModel
{
    public class TypeInfoField : NotifyPropertyChangedBase, IEquatable<TypeInfoField>
    {
        Type _memberType;
        [XmlElement("member-type")]
        public Type MemberType { get { return _memberType; } set { _memberType = value; } }

        string _typeName;
        [XmlElement("type-name")]
        public string TypeName { get { return _typeName; } set { SetIfDifferent<string>(ref _typeName, value); } }

        string _fqTypeName;
        [XmlElement("full-qualified-type-name")]
        public string FullyQualifiedTypeName { get { return _fqTypeName; } set { SetIfDifferent<string>(ref _fqTypeName, value); } }

        WellKnownCollectionTypeEnum? _wkCollectionType;
        [XmlElement("prop-kind")]
        public WellKnownCollectionTypeEnum? WellKnownCollectionType { get { return _wkCollectionType; } set { _wkCollectionType = value; } }

        Type _typeParameter1;
        [XmlElement("type-parameter1")]
        public Type TypeParameter1 { get { return _typeParameter1; } set { _typeParameter1 = value; } }

        Type _typeParameter2;
        [XmlElement("type-parameter2")]
        public Type TypeParameter2 { get { return _typeParameter2; } set { _typeParameter2 = value; } }

        Type _typeParameter3;
        [XmlElement("type-parameter3")]
        public Type TypeParameter3 { get { return _typeParameter3; } set { _typeParameter3 = value; } }

        ObservableCollection<TypeInfoField> _children;
        [XmlArray("children")]
        [XmlArrayItem("child")]
        public ObservableCollection<TypeInfoField> Children
        {
            get { return _children; }
            set { this.SetCollection<ObservableCollection<TypeInfoField>, TypeInfoField>(ref _children, value); }
        }

        public TypeInfoField(Type memberType, string typeName, string fullyQualifiedTypeName,
            WellKnownCollectionTypeEnum wkCollectionType,
            Type typeParameter1, Type typeParameter2, Type typeParameter3, ObservableCollection<TypeInfoField> children)
        {
            MemberType = memberType;
            TypeName = typeName;
            FullyQualifiedTypeName = fullyQualifiedTypeName;
            WellKnownCollectionType = wkCollectionType;
            TypeParameter1 = typeParameter1;
            TypeParameter2 = typeParameter2;
            TypeParameter3 = typeParameter3;
            Children = children ?? new ObservableCollection<TypeInfoField>();

            if(typeName == null & fullyQualifiedTypeName == null && memberType == null)
            {
                throw new ArgumentException("One of TypeName, FullyQualifiedTypeName or PropertyType must be specified.");
            }
        }

        public bool Equals(TypeInfoField other)
        {
            return other != null &&
                   EqualityComparer<Type>.Default.Equals(MemberType, other.MemberType) &&
                   TypeName == other.TypeName &&
                   FullyQualifiedTypeName == other.FullyQualifiedTypeName &&
                   EqualityComparer<WellKnownCollectionTypeEnum?>.Default.Equals(WellKnownCollectionType, other.WellKnownCollectionType) &&
                   EqualityComparer<Type>.Default.Equals(TypeParameter1, other.TypeParameter1) &&
                   EqualityComparer<Type>.Default.Equals(TypeParameter2, other.TypeParameter2) &&
                   EqualityComparer<Type>.Default.Equals(TypeParameter3, other.TypeParameter3) &&
                   EqualityComparer<ObservableCollection<TypeInfoField>>.Default.Equals(Children, other.Children);
        }

        public override bool Equals(object obj)
        {
            if(obj is TypeInfoField ptif)
            {
                return Equals(ptif);
            }
            return false;
        }

        public static bool operator ==(TypeInfoField field1, TypeInfoField field2)
        {
            return EqualityComparer<TypeInfoField>.Default.Equals(field1, field2);
        }

        public static bool operator !=(TypeInfoField field1, TypeInfoField field2)
        {
            return !(field1 == field2);
        }


    }
}
