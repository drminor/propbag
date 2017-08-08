using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DRM.Ipnwvc;


namespace DRM.PropBag
{
    public interface IProp<T> : IPropGen
    {
        T TypedValue { get; set; }

        bool ValueIsDefined { get; }

        /// <summary>
        /// Marks the property as having an undefined value.
        /// </summary>
        /// <returns>True, if the value was defined at the time this call was made.</returns>
        bool SetValueToUndefined();

        bool CompareTo(T value);
        bool Compare(T val1, T val2);

        void DoWhenChanged(T oldVal, T newVal);
        bool DoAfterNotify { get; set; }
        bool UpdateDoWhenChangedAction(Action<T, T> doWHenChangedAction, bool? doAfterNotify);
        //bool HasCallBack { get; }

        // Property Changed with typed values support
        event PropertyChangedWithTValsHandler<T> PropertyChangedWithTVals;
        void OnPropertyChangedWithTVals(string propertyName, T oldVal, T newVal);

        void SubscribeToPropChanged(Action<T, T> doOnChange);
        bool UnSubscribeToPropChanged(Action<T, T> doOnChange);
    }
}
