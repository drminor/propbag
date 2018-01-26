using System.Data;

namespace DRM.TypeSafePropertyBag.PropInterfaces
{
    // Typically implemented by TypedTableBase<T> Class
    public interface IDTPropPrivate : IProp
    {
        DataTable DataTable { get; }
    }

    // Typically implemented by TypedTableBase<T> Class
    public interface IDTProp : IProp
    {
        DataTable ReadOnlyDataTable { get; }
    }
}
