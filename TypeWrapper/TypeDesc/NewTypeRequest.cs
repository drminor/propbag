using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.TypeWrapper
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public struct NewTypeRequest : IEquatable<NewTypeRequest>
    {

        public NewTypeRequest(PropModelType propModel, Type typeToWrap, string fullClassName)
        {
            PropModel = propModel ?? throw new ArgumentNullException(nameof(propModel));
            TypeToWrap = typeToWrap;
            FullClassName = fullClassName ?? propModel.FullClassName;
        }

        public PropModelType PropModel { get; }
        public Type TypeToWrap { get; }
        public string FullClassName { get; }

        public override bool Equals(object obj)
        {
            return obj is NewTypeRequest && Equals((NewTypeRequest)obj);
        }

        // Note: The PropModel references must point to the same instance.
        // Callers should cache PropModel refereneces to avoid creating duplicate types.
        public bool Equals(NewTypeRequest other)
        {
            return EqualityComparer<PropModelType>.Default.Equals(PropModel, other.PropModel) &&
                   EqualityComparer<Type>.Default.Equals(TypeToWrap, other.TypeToWrap) &&
                   FullClassName == other.FullClassName;
        }

        public override int GetHashCode()
        {
            var hashCode = 55120964;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<PropModelType>.Default.GetHashCode(PropModel);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(TypeToWrap);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullClassName);
            return hashCode;
        }

        public static bool operator ==(NewTypeRequest request1, NewTypeRequest request2)
        {
            return request1.Equals(request2);
        }

        public static bool operator !=(NewTypeRequest request1, NewTypeRequest request2)
        {
            return !(request1 == request2);
        }
    }
}
