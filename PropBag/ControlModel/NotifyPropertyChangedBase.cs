using System;
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DRM.PropBag.ControlModel
{
    public class NotifyPropertyChangedBase : INotifyPropertyChanged, INotifyPropertyChanging
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        //protected void Set<T>(ref T oldVal, T newVal, [CallerMemberName]string propertyName = null)
        //{
        //    OnPropertyChanging(propertyName);
        //    oldVal = newVal;
        //    OnPropertyChanged(propertyName);
        //}

        protected bool SetIfDifferent<T>(ref T oldVal, T newVal, [CallerMemberName]string propertyName = null) where T : IEquatable<T>
        {
            if ((oldVal == null && newVal != null) || (oldVal != null && !oldVal.Equals(newVal)))
            {
                OnPropertyChanging(propertyName);
                oldVal = newVal;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        protected bool SetIfDifferentVT<T>(ref T oldVal, T newVal, [CallerMemberName]string propertyName = null) where T : struct
        {
            if (!oldVal.Equals(newVal))
            {
                OnPropertyChanging(propertyName);
                oldVal = newVal;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        protected bool SetCollection<T,IT>(ref T oldVal, T newVal, [CallerMemberName]string propertyName = null) where T : ObservableCollection<IT>
        {
            if (oldVal == null && newVal == null)
                return false; // No change here.

            if (IComparable<object>.Equals(oldVal, newVal))
                return false; // They are the same object.

            // Assume that the contents are different 
            OnPropertyChanging(propertyName);
            oldVal = newVal;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChanged, null, null);

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanging([CallerMemberName]string propertyName = null)
        {
            PropertyChangingEventHandler handler = Interlocked.CompareExchange(ref PropertyChanging, null, null);

            if (handler != null)
                handler(this, new PropertyChangingEventArgs(propertyName));
        }


    }
}
