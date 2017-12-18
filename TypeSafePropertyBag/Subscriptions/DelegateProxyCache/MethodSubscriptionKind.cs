using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    public struct MethodSubscriptionKind : IEquatable<MethodSubscriptionKind>
    {
        private readonly int _hashCode;

        public readonly MethodInfo Method;
        public readonly Type DelegateType;
        public readonly SubscriptionKind SubscriptionKind;

        public MethodSubscriptionKind(MethodInfo method, SubscriptionKind subscriptionKind) : this()
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            SubscriptionKind = subscriptionKind;

            DelegateType = GetDelegateType(method);

            _hashCode = ComputeHashCode();
        }

        public Type GetDelegateType(MethodInfo method)
        {
            Type result = Expression.GetDelegateType
                (
                new Type[] { method.ReflectedType }
                .Concat(method.GetParameters().Select(p => p.ParameterType)
                .Concat(new Type[] { method.ReturnType }))
                .ToArray());
            return result;
        }

        private int ComputeHashCode()
        {
            var hashCode = 2081916855;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<MethodInfo>.Default.GetHashCode(Method);
            hashCode = hashCode * -1521134295 + ((int)SubscriptionKind).GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is MethodSubscriptionKind && Equals((MethodSubscriptionKind)obj);
        }

        public bool Equals(MethodSubscriptionKind other)
        {
            return EqualityComparer<MethodInfo>.Default.Equals(Method, other.Method) &&
                   int.Equals((int)SubscriptionKind, (int)other.SubscriptionKind);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public static bool operator ==(MethodSubscriptionKind kind1, MethodSubscriptionKind kind2)
        {
            return kind1.Equals(kind2);
        }

        public static bool operator !=(MethodSubscriptionKind kind1, MethodSubscriptionKind kind2)
        {
            return !(kind1 == kind2);
        }
    }
}
