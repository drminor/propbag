using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.TypeSafePropertyBag
{
    public interface ICacheDelegatesForTypePair<T> where T : class
    {
        int Count { get; }

        T GetOrAdd(TypePair argumentTypes);
    }
}