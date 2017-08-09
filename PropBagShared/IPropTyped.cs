using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DRM.Ipnwvc;


namespace DRM.PropBag
{
    public interface IProp<T> : IPropGen, IProp
    {
        T TypedValue { get; set; }

        bool CompareTo(T value);
        bool Compare(T val1, T val2);

        void DoWhenChanged(T oldVal, T newVal);
        bool DoAfterNotify { get; set; }
        bool UpdateDoWhenChangedAction(Action<T, T> doWHenChangedAction, bool? doAfterNotify);

        // Property Changed with typed values support
        event PropertyChangedWithTValsHandler<T> PropertyChangedWithTVals;
        void OnPropertyChangedWithTVals(string propertyName, T oldVal, T newVal);

        void SubscribeToPropChanged(Action<T, T> doOnChange);
        bool UnSubscribeToPropChanged(Action<T, T> doOnChange);
    }

}
