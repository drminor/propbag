using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.PropBag.TypeWrapper
{
    public struct TypeDescription : IEquatable<TypeDescription> 
    {
        public TypeDescription(TypeName nameForNewType, Type baseType)
            : this(nameForNewType, baseType, PropertyDescription.Empty) { }

        public TypeDescription(TypeName name, Type baseType, IEnumerable<PropertyDescription> additionalProperties)
        {
            TypeName = name;
            BaseType = baseType;
            PropertyDescriptions = additionalProperties?.ToArray() ?? throw new ArgumentNullException(nameof(additionalProperties));
        }

        public TypeName TypeName { get; }

        public Type BaseType { get; }

        public PropertyDescription[] PropertyDescriptions { get; }

        //public Dictionary<string, Type> TypeDefs
        //{
        //    get
        //    {
        //        Dictionary<string, Type> result =
        //            PropertyDescriptions
        //            .Select(x => new KeyValuePair<string, Type>(x.Name, x.Type))
        //            .ToDictionary(pair => pair.Key, pair => pair.Value);

        //        return result;
        //    }
        //}

        // TODO: Should we include the Array of Property Descriptions in the hashcode?
        public override int GetHashCode()
        {
            var hashCode = 391836097;
            hashCode = hashCode * -1521134295 + TypeName.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(BaseType);
            //hashCode = hashCode * -1521134295 + EqualityComparer<PropertyDescription[]>.Default.GetHashCode(PropertyDescriptions);
            return hashCode;
        }

        //public override int GetHashCode()
        //{
        //    return GenerateHash.CustomHash(TypeName.GetHashCode(), BaseType.GetHashCode());
        //    //var hashCode = Type.GetHashCode();
        //    //foreach (var property in AdditionalProperties)
        //    //{
        //    //    hashCode = GenerateHash.CustomHash(hashCode, property.GetHashCode());
        //    //}
        //    //return hashCode;
        //}

        public override bool Equals(object other) => other is TypeDescription && Equals((TypeDescription)other);

        public bool Equals(TypeDescription other) => TypeName == other.TypeName && BaseType == other.BaseType; // && AdditionalProperties.SequenceEqual(other.AdditionalProperties);

        public static bool operator ==(TypeDescription left, TypeDescription right) => left.Equals(right);

        public static bool operator !=(TypeDescription left, TypeDescription right) => !left.Equals(right);
    }

}
