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

                ToEnumerable<Type>(method.ReflectedType) // This line used to read: new Type[] { method.ReflectedType } 
                .Concat(method.GetParameters().Select(p => p.ParameterType)
                .Concat(new Type[] { method.ReturnType }))
                .ToArray());
            return result;
        }

        /// <summary>
        /// Creates an IEnumerable<typeparamref name="T"/> from a single instance of T.
        /// </summary>
        /// <typeparam name="T">The type of the instance from which to create the IEnumerable<typeparamref name="T"/></typeparam>
        /// <param name="item">The instance of type T which will be the only element in the enumeration.</param>
        /// <returns>A new IEumerable<typeparamref name="T"/> containing item as its only member.</returns>
        public static IEnumerable<T> ToEnumerable<T>(T item)
        {
            if (item == null)
            {
                yield break;
            }
            else
            {
                yield return item;
            }
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
