using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DRM.WrapperGenLib
{
    public struct TypeDescription : IEquatable<TypeDescription> 
    {
        public TypeDescription(TypeName name) : this(name, PropertyDescription.Empty)
        {
        }

        public TypeDescription(TypeName name, IEnumerable<PropertyDescription> additionalProperties)
        {
            TypeName = name;
            PropertyDescriptions = additionalProperties?.ToArray() ?? throw new ArgumentNullException(nameof(additionalProperties));
        }

        public TypeName TypeName { get; }

        public PropertyDescription[] PropertyDescriptions { get; }

        public Dictionary<string, Type> TypeDefs
        {
            get
            {
                Dictionary<string, Type> result =
                    PropertyDescriptions
                    .Select(x => new KeyValuePair<string, Type>(x.Name, x.Type))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);

                return result;
            }
        }

        public override int GetHashCode()
        {
            return TypeName.GetHashCode();
            //return GenerateHash.CustomHash(Name.GetHashCode(), Type.GetHashCode());
            //var hashCode = Type.GetHashCode();
            //foreach (var property in AdditionalProperties)
            //{
            //    hashCode = GenerateHash.CustomHash(hashCode, property.GetHashCode());
            //}
            //return hashCode;
        }

        public override bool Equals(object other) => other is TypeDescription && Equals((TypeDescription)other);

        public bool Equals(TypeDescription other) => TypeName == other.TypeName; // && AdditionalProperties.SequenceEqual(other.AdditionalProperties);

        public static bool operator ==(TypeDescription left, TypeDescription right) => left.Equals(right);

        public static bool operator !=(TypeDescription left, TypeDescription right) => !left.Equals(right);
    }

}
