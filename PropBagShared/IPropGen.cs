using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.Ipnwvc;

namespace DRM.PropBag
{
    public interface IPropGen
    {
        Type Type { get; }
        bool TypeIsSolid { get;}
        bool HasStore { get; }

        List<PropertyChangedWithValsHandler> PropChangedWithValsHandlerList { get; }

        //DoSetDelegate DoSetProVal { get; set; }

        object Value { get; }

        //void UpdateWithSolidType(Type typeOfThisValue, object curValue);

        //bool UpdateDoWhenChanged<T>(Action<T, T> doWhenChanged, bool doAfterNotify);

    }
}
