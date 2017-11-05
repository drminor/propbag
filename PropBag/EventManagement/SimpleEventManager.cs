using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.EventManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DRM.PropBag
{
    public class SimpleEventManager : IEventManager<INotifyPropertyChangedWithVals, PropertyChangedWithValsEventArgs> 
    {
        public SimpleEventManager() { }

        public void AddHandler(INotifyPropertyChangedWithVals source, string eventName, EventHandler<PropertyChangedWithValsEventArgs> handler)
        {
            WeakEventManager<INotifyPropertyChangedWithVals, PropertyChangedWithValsEventArgs>.AddHandler(source, eventName, handler);
        }

        public void RemoveHandler(INotifyPropertyChangedWithVals source, string eventName, EventHandler<PropertyChangedWithValsEventArgs> handler)
        {
            WeakEventManager<INotifyPropertyChangedWithVals, PropertyChangedWithValsEventArgs>.RemoveHandler(source, eventName, handler);
        }
    }

    public class SimpleTypedEventManager<T> 
        : IEventManager<INotifyPropertyChangedWithTVals<T>, PropertyChangedWithTValsEventArgs<T>>
    {
        public SimpleTypedEventManager() { }

        public void AddHandler(INotifyPropertyChangedWithTVals<T> source, string eventName, EventHandler<PropertyChangedWithTValsEventArgs<T>> handler)
        {
            WeakEventManager<INotifyPropertyChangedWithTVals<T>, PropertyChangedWithTValsEventArgs<T>>.AddHandler(source, eventName, handler);
        }

        public void RemoveHandler(INotifyPropertyChangedWithTVals<T> source, string eventName, EventHandler<PropertyChangedWithTValsEventArgs<T>> handler)
        {
            WeakEventManager<INotifyPropertyChangedWithTVals<T>, PropertyChangedWithTValsEventArgs<T>>.RemoveHandler(source, eventName, handler);
        }

    }
}
