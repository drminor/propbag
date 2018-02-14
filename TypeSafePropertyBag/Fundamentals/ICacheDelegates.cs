using System;

namespace DRM.TypeSafePropertyBag
{
    public interface ICacheDelegates<T> where T : class
    {
        int Count { get; }

        T GetOrAdd(Type typeOfThisValue);
    }
}