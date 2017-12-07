using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    #region Collection-Type Methods

    // From Object
    public delegate object CreateCPropFromObjectDelegate(IPropFactory propFactory,
        object value,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    // From String
    public delegate object CreateCPropFromStringDelegate(IPropFactory propFactory,
        string value, bool useDefault,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    // With No Value
    public delegate object CreateCPropWithNoValueDelegate(IPropFactory propFactory,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);
    #endregion

    #region Property-Type Methods

    // From Object
    public delegate object CreatePropFromObjectDelegate(IPropFactory propFactory,
        object value,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    // From String
    public delegate object CreatePropFromStringDelegate(IPropFactory propFactory,
        string value, bool useDefault,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    // With No Value
    public delegate object CreatePropWithNoValueDelegate(IPropFactory propFactory,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    #endregion
}
