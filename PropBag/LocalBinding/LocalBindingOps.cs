using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.EventManagement;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


/// <remarks>
/// 
/// A local one way binding from source to target is a short hand way of
///     1. Wiring an eventhandler to the event fired by the source when the specified property changes.
///     2. Having the body of that event handler use the target property's set accessor to update
///         the "bound" value.
///         
/// We then say that the target's property is bound to the source's property.
/// 
/// 
/// The source is some object that supports INotifyPropertyChanged, or INotifyPCTyped<T> 
/// The target is an IProp or IProp[T] on an IPropBag.
///
/// </remarks>

namespace DRM.PropBag.LocalBinding
{
    public class LocalBindingOps
    {

        public LocalBindingOps()
        {

        }

        public EventHandler<PCTypedEventArgs<T>> CreateLBinding<T>
            (
            SimpleExKey source,
            SimpleExKey target,
            LocalBindingInfo lbInfo
            )
        {
            return null;
        }

    }
}
