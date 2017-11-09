using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.EventManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DRM.PropBag.EventManagement
{
    public class SimpleEventManager : IEventManager<INotifyPCGen, PCGenEventArgs> 
    {
        public SimpleEventManager() { }

        public void AddHandler(INotifyPCGen source, string eventName, EventHandler<PCGenEventArgs> handler)
        {
            WeakEventManager<INotifyPCGen, PCGenEventArgs>.AddHandler(source, eventName, handler);
        }

        public void RemoveHandler(INotifyPCGen source, string eventName, EventHandler<PCGenEventArgs> handler)
        {
            WeakEventManager<INotifyPCGen, PCGenEventArgs>.RemoveHandler(source, eventName, handler);
        }
    }

    public class SimpleTypedEventManager<T> 
        : IEventManager<INotifyPCTyped<T>, PCTypedEventArgs<T>>
    {
        public SimpleTypedEventManager() { }

        public void AddHandler(INotifyPCTyped<T> source, string eventName, EventHandler<PCTypedEventArgs<T>> handler)
        {
            WeakEventManager<INotifyPCTyped<T>, PCTypedEventArgs<T>>.AddHandler(source, eventName, handler);
        }

        public void RemoveHandler(INotifyPCTyped<T> source, string eventName, EventHandler<PCTypedEventArgs<T>> handler)
        {
            WeakEventManager<INotifyPCTyped<T>, PCTypedEventArgs<T>>.RemoveHandler(source, eventName, handler);
        }

    }
}
