using System;

namespace DRM.TypeSafePropertyBag
{
    #region Enumerable-Type Methods

    // From Object
    public delegate IProp CreateEPropFromObjectDelegate(IPropFactory propFactory,
        object value,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    // From String
    public delegate IProp CreateEPropFromStringDelegate(IPropFactory propFactory,
        string value, bool useDefault,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    // With No Value
    public delegate IProp CreateEPropWithNoValueDelegate(IPropFactory propFactory,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);
    #endregion

    #region Property-Type Methods

    // From Object
    public delegate IProp CreatePropFromObjectDelegate(IPropFactory propFactory,
        object value,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    // From String
    public delegate IProp CreatePropFromStringDelegate(IPropFactory propFactory,
        string value, bool useDefault,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    // With No Value
    public delegate IProp CreatePropWithNoValueDelegate(IPropFactory propFactory,
        string propertyName, object extraInfo,
        bool hasStorage, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    #endregion
}
