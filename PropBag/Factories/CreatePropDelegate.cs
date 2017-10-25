using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    #region Collection-Type Methods

    // From Object
    public delegate object CreateCPropFromObjectDelegate(AbstractPropFactory propFactory,
        object value,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality);

    // From String
    public delegate object CreateCPropFromStringDelegate(AbstractPropFactory propFactory,
        string value, bool useDefault,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality);

    // With No Value
    public delegate object CreateCPropWithNoValueDelegate(AbstractPropFactory propFactory,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality);
    #endregion

    #region Property-Type Methods

    // From Object
    public delegate object CreatePropFromObjectDelegate(AbstractPropFactory propFactory,
        object value,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality);

    // From String
    public delegate object CreatePropFromStringDelegate(AbstractPropFactory propFactory,
        string value, bool useDefault,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality);

    // With No Value
    public delegate object CreatePropWithNoValueDelegate(AbstractPropFactory propFactory,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality);

    #endregion
}
