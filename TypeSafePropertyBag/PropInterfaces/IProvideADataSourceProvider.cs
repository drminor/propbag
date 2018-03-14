using System;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    public interface IProvideADataSourceProvider
    {
        DataSourceProvider DataSourceProvider { get; } 
        Type CollectionItemRunTimeType { get; }
        bool IsCollection(); 
        bool IsReadOnly();
    }
}
