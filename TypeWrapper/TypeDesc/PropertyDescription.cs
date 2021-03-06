﻿
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.TypeWrapper
{
    [DebuggerDisplay("{Name}-{Type.Name}")]
    public struct PropertyDescription : IEquatable<PropertyDescription>
    {
        internal static PropertyDescription[] Empty = new PropertyDescription[0];

        public PropertyDescription(string name, Type type, bool canWrite = true, bool setIsPublic = true, bool getIsPublic = true)
        {
            Name = name;
            Type = type;
            CanWrite = canWrite;
            GetIsPublic = getIsPublic;
            SetIsPublic = setIsPublic;
        }

        public PropertyDescription(PropertyInfo property)
        {
            Name = property.Name;
            Type = property.PropertyType;
            CanWrite = property.CanWrite;

            GetIsPublic = property.GetMethod != null;
            SetIsPublic = property.SetMethod != null;
        }

        public string Name { get; }

        public Type Type { get; }

        public bool CanWrite { get; }

        public bool GetIsPublic { get; }
        public bool SetIsPublic { get; }

        public override int GetHashCode()
        {
            var hashCode = -1889003361;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Name.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + CanWrite.GetHashCode();
            hashCode = hashCode * -1521134295 + GetIsPublic.GetHashCode();
            hashCode = hashCode * -1521134295 + SetIsPublic.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object other) => other is PropertyDescription && Equals((PropertyDescription)other);

        // TODO: Is This good enough?
        public bool Equals(PropertyDescription other) => 
            Name == other.Name
            && Type == other.Type
            && CanWrite == other.CanWrite;

        public static bool operator ==(PropertyDescription left, PropertyDescription right) => left.Equals(right);

        public static bool operator !=(PropertyDescription left, PropertyDescription right) => !left.Equals(right);
    }


}
