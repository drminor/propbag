using System;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;
    using PropIdType = UInt32;
    using PropNameType = String;

    public class SimpleObjectIdDictionary : AbstractObjectIdDictionary<SimpleExKey, CompositeKeyType, ObjectIdType, PropIdType, PropNameType>
    {
        public SimpleObjectIdDictionary(SimpleCompKeyMan compKeyManager, SimpleLevel2KeyMan level2KeyManager)
            : base(compKeyManager, level2KeyManager)
        {
        }
    }
}
