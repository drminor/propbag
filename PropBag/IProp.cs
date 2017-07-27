using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public interface IProp<T>
    {
        T Value { get; set; }
        bool CompareTo(T value);
        bool Compare(T val1, T val2);
        Action<T, T> DoWHenChanged { get; set; }
        bool DoAfterNotify { get; set; }
        bool HasCallBack { get; }
    }
}
