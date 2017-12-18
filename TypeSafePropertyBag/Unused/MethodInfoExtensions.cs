using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DRM.TypeSafePropertyBag.Fundamentals.UnUsed
{
    public static class MethodInfoExtensions
    {
        public static Delegate CreateDelegate(this MethodInfo method)
        {
            Type subscribersType = method.GetDelegateType();

            return Delegate.CreateDelegate(subscribersType, null, method);
        }

        // TODO: Optimize this by creating the result array first and then using Array.Copy.
        public static Type GetDelegateType(this MethodInfo method)
        {
            Type result =  Expression.GetDelegateType
                (
                new Type[] { method.ReflectedType }
                .Concat(method.GetParameters().Select(p => p.ParameterType)
                .Concat(new Type[] { method.ReturnType }))
                .ToArray());
            return result;
        }
    }
}
