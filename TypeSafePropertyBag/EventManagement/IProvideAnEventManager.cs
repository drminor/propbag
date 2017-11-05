using System;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    public interface IProvideAnEventManager
    {
        //IEventManager<TEventSource, TEventArgs> GetTheEventManager<TEventSource, TEventArgs>() where TEventArgs: EventArgs;

        IEventManager<INotifyPropertyChangedWithVals, PropertyChangedWithValsEventArgs>
            GetTheEventManager<INotifyPropertyChangedWithVals, PropertyChangedWithValsEventArgs>() where PropertyChangedWithValsEventArgs : EventArgs;
    }

    public interface IProvideATypedEventManager<T>
    {
        IEventManager<INotifyPropertyChangedWithTVals<T>, PropertyChangedWithTValsEventArgs<T>>
            GetTheEventManger();
    }
}
