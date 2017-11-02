using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DRM.PropBag.ControlModel.NotUsed
{
    public class DeriveFromClassField_NotUsed : NotifyPropertyChangedBase, IEquatable<DeriveFromClassField_NotUsed>
    {
        DeriveFromClassModeEnum _deriveFromClassMode;
        Type _typeToWrap; 
        TypeInfoField _wrapperTypeInfoField;

        [XmlAttribute(AttributeName = "derive-from-class-mode")]
        public DeriveFromClassModeEnum DeriveFromClassMode
        { get { return _deriveFromClassMode; } set { SetIfDifferentVT<DeriveFromClassModeEnum>(ref _deriveFromClassMode, value); } }

        [XmlElement("type")]
        public Type TypeToWrap { get { return _typeToWrap; } set { _typeToWrap = value; } }


        [XmlElement("type-info")]
        public TypeInfoField WrapperTypeInfoField
        {
            get { return _wrapperTypeInfoField; }
            set { SetIfDifferent<TypeInfoField>(ref _wrapperTypeInfoField, value); }
        }

        public DeriveFromClassField_NotUsed(DeriveFromClassModeEnum deriveFromClassMode,
            Type typeToWrap, TypeInfoField wrapperTypeInfoField)
        {
            DeriveFromClassMode = deriveFromClassMode;
            TypeToWrap = typeToWrap;
            WrapperTypeInfoField = wrapperTypeInfoField;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DeriveFromClassField_NotUsed);
        }

        public bool Equals(DeriveFromClassField_NotUsed other)
        {
            return other != null &&
                   DeriveFromClassMode == other.DeriveFromClassMode &&
                   EqualityComparer<Type>.Default.Equals(TypeToWrap, other.TypeToWrap) &&
                   EqualityComparer<TypeInfoField>.Default.Equals(WrapperTypeInfoField, other.WrapperTypeInfoField);
        }

        public override int GetHashCode()
        {
            var hashCode = 785585757;
            hashCode = hashCode * -1521134295 + DeriveFromClassMode.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(TypeToWrap);
            hashCode = hashCode * -1521134295 + EqualityComparer<TypeInfoField>.Default.GetHashCode(WrapperTypeInfoField);
            return hashCode;
        }

        public static bool operator ==(DeriveFromClassField_NotUsed field1, DeriveFromClassField_NotUsed field2)
        {
            return EqualityComparer<DeriveFromClassField_NotUsed>.Default.Equals(field1, field2);
        }

        public static bool operator !=(DeriveFromClassField_NotUsed field1, DeriveFromClassField_NotUsed field2)
        {
            return !(field1 == field2);
        }
    }
}
