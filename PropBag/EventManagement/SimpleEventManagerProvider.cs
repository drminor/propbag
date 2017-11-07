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
    public class SimpleEventManagerProvider : IProvideAnEventManager
    {
        public IEventManager<INotifyPropertyChangedWithVals, PropertyChangedWithValsEventArgs> 
            GetTheEventManager<INotifyPropertyChangedWithVals, PropertyChangedWithValsEventArgs>() where PropertyChangedWithValsEventArgs : EventArgs
        {
            var result = new SimpleEventManager();
            return (IEventManager<INotifyPropertyChangedWithVals, PropertyChangedWithValsEventArgs>)result;
        }
    }

    public class SimpleTypedEventManagerProvider<T> : IProvideATypedEventManager<T>
    {
        public IEventManager<INotifyPCTyped<T>, PCTypedEventArgs<T>> GetTheEventManger()
        {
            var result = new SimpleTypedEventManager<T>();
            return result;
        }
    }
}
