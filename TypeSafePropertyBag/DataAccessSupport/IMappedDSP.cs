using System;

namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    public interface IMappedDSP<T> : IDisposable, INotifyItemEndEdit, IProvideADataSourceProvider, IHaveACrudWithMapping<T> where T : INotifyItemEndEdit
    {
    }
}
