using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.TypeWrapper
{
    public struct NewTypeRequest : IEquatable<NewTypeRequest>
    {
        public NewTypeRequest(IPropModel propModel, Type typeToWrap, string className)
        {
            PropModel = propModel ?? throw new ArgumentNullException(nameof(propModel));
            TypeToWrap = typeToWrap;
            ClassName = className;
        }

        public IPropModel PropModel { get; }
        public Type TypeToWrap { get; }
        public string ClassName { get; }

        public override bool Equals(object obj)
        {
            return obj is NewTypeRequest && Equals((NewTypeRequest)obj);
        }


        // TODO: check how to compare two IPropModels.
        public bool Equals(NewTypeRequest other)
        {
            return EqualityComparer<IPropModel>.Default.Equals(PropModel, other.PropModel) &&
                   EqualityComparer<Type>.Default.Equals(TypeToWrap, other.TypeToWrap) &&
                   ClassName == other.ClassName;
        }

        public override int GetHashCode()
        {
            var hashCode = 55120964;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IPropModel>.Default.GetHashCode(PropModel);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(TypeToWrap);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ClassName);
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
