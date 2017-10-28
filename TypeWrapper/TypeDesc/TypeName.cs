using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.TypeWrapper
{
    public struct TypeName : IEquatable<TypeName>
    {
        public TypeName(string name) : this(name, null) { } //, null) { }

        //public TypeName(string name, string assemblyName) : this(name, null, assemblyName) { }

        public TypeName(string name, string namespaceName)//, string assemblyName = null)
        {
            Name = name;
            NamespaceName = namespaceName;
            //AssemblyName = assemblyName;
            
            _fullName = namespaceName == null ? Name : $"{NamespaceName}.{Name}";
        }

        public string Name { get; }
        public string NamespaceName { get; }
        //public string AssemblyName { get; }

        string _fullName;
        public string FullName => _fullName;

        public override int GetHashCode()
        {
            return _fullName.GetHashCode();
            //return GenerateHash.CustomHash(Name.GetHashCode(), Type.GetHashCode());
            //var hashCode = Type.GetHashCode();
            //foreach (var property in AdditionalProperties)
            //{
            //    hashCode = GenerateHash.CustomHash(hashCode, property.GetHashCode());
            //}
            //return hashCode;
        }

        public override bool Equals(object other) => other is TypeName && Equals((TypeName)other);

        public bool Equals(TypeName other) => Name == other.Name; // && AdditionalProperties.SequenceEqual(other.AdditionalProperties);

        public static bool operator ==(TypeName left, TypeName right) => left.Equals(right);

        public static bool operator !=(TypeName left, TypeName right) => !left.Equals(right);

    }
}
