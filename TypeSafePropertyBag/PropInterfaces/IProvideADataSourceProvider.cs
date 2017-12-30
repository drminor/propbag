using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    public interface IProvideADataSourceProvider
    {
        DataSourceProvider DataSourceProvider { get; } // set will be removed once we implement the view manager.
        bool IsCollection(); 
        bool IsReadOnly();
    }
}
