using System;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using PropNameType = String;

    public class SimpleObjectIdDictionary : AbstractObjectIdDictionary<CompositeKeyType, ObjectIdType, PropIdType, PropNameType>
    {
        public SimpleObjectIdDictionary(SimpleCompKeyMan compKeyManager) : base(compKeyManager)
        {
        }
    }
}
