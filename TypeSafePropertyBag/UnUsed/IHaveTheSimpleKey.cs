﻿using System;

namespace DRM.TypeSafePropertyBag.Unused
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;

    /// <summary>
    /// Allows access from code in the TypeSafePropertyBag assembly, but not from the PropBag assembly.
    /// </summary>
    internal interface IHaveTheSimpleKey : IHaveTheStoreNode<CompositeKeyType, ObjectIdType, PropIdType>
    {
        new SimpleExKey GetTheKey(IPropBag propBag, PropIdType propId);



        //new SimpleExKey ParentKey { get; set; }

    }
}
