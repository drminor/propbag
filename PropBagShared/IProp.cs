using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DRM.Ipnwv;


namespace DRM.PropBag
{
    public interface IProp<T>
    {
        T Value { get; set; }
        bool CompareTo(T value);
        bool Compare(T val1, T val2);

        Action<T, T> DoWHenChanged { get; }
        bool DoAfterNotify { get; set; }

        event PropertyChangedWithTValsHandler<T> PropertyChangedWithTVals;

        bool HasCallBack { get; }

        // Raise Type Events
        void OnPropertyChangedWithTVals(string propertyName, T oldVal, T newVal);

    }
}
