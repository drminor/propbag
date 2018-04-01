
namespace DRM.TypeSafePropertyBag.DelegateCaches
{
    public interface ICacheDelegatesForTypePair<T> where T : class
    {
        int Count { get; }

        T GetOrAdd(TypePair argumentTypes);
    }
}