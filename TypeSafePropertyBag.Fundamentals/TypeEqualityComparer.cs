using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public class TypeEqualityComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type x, Type y)
        {
            return Type.Equals(x, y);
        }

        public int GetHashCode(Type obj)
        {
            return obj.GetHashCode();
        }
    }
}
