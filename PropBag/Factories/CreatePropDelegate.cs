using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{

    public delegate object CreatePropFromObjectDelegate(AbstractPropFactory propFactory,
        object value,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality);

    public delegate object CreatePropFromStringDelegate(AbstractPropFactory propFactory,
        string value, bool useDefault,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality);

    public delegate object CreatePropWithNoValueDelegate(AbstractPropFactory propFactory,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality);

}
