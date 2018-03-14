using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IWatchAPropItem<T> : INotifyPCTyped<T>, IWatchAPropItemGen
    {
        bool TryGetValue(out T value);
        new T GetValue();
    }

    public interface IWatchAPropItemGen : INotifyPCGen
    {
        Type PropertyType { get; }
        bool IsAsynchronous { get; }
        bool TryGetValue(out object value);
        Object GetValue();
    }
}
