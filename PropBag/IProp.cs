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
        //bool TypeIsSolid { get; set; }
        bool CompareTo(T value);
        Action<T, T> DoWHenChanged { get; set; }
        bool DoAfterNotify { get; set; }
        bool HasCallBack { get; }
    }
}
