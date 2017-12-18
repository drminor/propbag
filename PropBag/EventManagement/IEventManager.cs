using System;

namespace DRM.PropBag.EventManagement
{
    public interface IEventManager<TEventSource, TEventArgs> where TEventArgs : EventArgs
    {
        void AddHandler(TEventSource source, string eventName, EventHandler<TEventArgs> handler);

        void RemoveHandler(TEventSource source, string eventName, EventHandler<TEventArgs> handler);
    }
}
