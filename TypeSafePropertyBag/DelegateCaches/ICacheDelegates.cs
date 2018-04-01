using System;

namespace DRM.TypeSafePropertyBag.DelegateCaches
{
    public interface ICacheDelegates<T> where T : class
    {
        int Count { get; }

        T GetOrAdd(Type typeOfThisValue);
    }
}