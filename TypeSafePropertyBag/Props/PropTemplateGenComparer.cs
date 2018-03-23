using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public class PropTemplateGenComparer : IEqualityComparer<IPropTemplate>
    {
        public bool Equals(IPropTemplate x, IPropTemplate y)
        {
            if(x is IEquatable<IPropTemplate> a)
            {
                return a.Equals(y);
            }

            throw new NotImplementedException("This item does not implement IEquatable<IPropTemplate>");
        }

        public int GetHashCode(IPropTemplate obj)
        {
            return obj.GetHashCode();
        }
    }
}
         