using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.EventManagement
{
    public interface IProvideAnEventManager
    {
        //IEventManager<TEventSource, TEventArgs> GetTheEventManager<TEventSource, TEventArgs>() where TEventArgs: EventArgs;

        IEventManager<INotifyPropertyChangedWithVals, PropertyChangedWithValsEventArgs>
            GetTheEventManager<INotifyPropertyChangedWithVals, PropertyChangedWithValsEventArgs>() where PropertyChangedWithValsEventArgs : EventArgs;
    }

    public interface IProvideATypedEventManager<T>
    {
        IEventManager<INotifyPCTyped<T>, PCTypedEventArgs<T>>
            GetTheEventManger();
    }
}
