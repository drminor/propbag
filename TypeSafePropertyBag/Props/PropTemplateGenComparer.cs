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
            else
            {
                System.Diagnostics.Debug.WriteLine("This IPropTemplate item does not implement IEquatable<IPropTemplate>");
                return false;
            }
        }

        public int GetHashCode(IPropTemplate obj)
        {
            return obj.GetHashCode();
        }
    }
}
         