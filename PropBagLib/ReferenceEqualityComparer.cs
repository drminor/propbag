using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PropBagLib
{

    ///<remarks>
    /// This was copied from https://stackoverflow.com/questions/1890058/iequalitycomparert-that-uses-referenceequals
    /// Author: Snowbear (Marat Yuldashev, Ukraine)
    /// With improvements from: AnorZaken (Real name unknown.)
    /// 
    /// I futher improved it by using System.Lazy to instantiate the singleton, thereby making it (a little bit more) thread safe.
    /// 
    /// PublicationOnly is the right amount of thread safety here, since all callers who attempt to initialze the value
    /// of the single instance will produce the same value.
    ///</remarks>

    /// <summary>
    /// A generic object comparerer that would only use an object's reference, 
    /// ignoring any <see cref="IEquatable{T}"/> or <see cref="object.Equals(object)"/> overrides.
    /// </summary>
    public sealed class ReferenceEqualityComparer : IEqualityComparer, IEqualityComparer<object>
    {
        static Lazy<ReferenceEqualityComparer> singleInstance;
        public static ReferenceEqualityComparer Default { get {return singleInstance.Value; }}

        static ReferenceEqualityComparer()
        {
            singleInstance = new Lazy<ReferenceEqualityComparer>(() => new ReferenceEqualityComparer(), LazyThreadSafetyMode.PublicationOnly);
        }

        private ReferenceEqualityComparer() { } // Mark as private to disallow creation using the default constructor.

        /// <remarks>
        /// The method is not hiding Object.Equals. 
        /// The issue is a collision between IEqualityComparer.Equals and IEqualityComparer<object>.Equals
        /// The collision occurs because they have the exact same signature.
        /// </remarks>

#pragma warning disable 108
        public bool Equals(object x, object y)
        {
            return x == y; // This is reference equality! (See explanation below.)
        }
#pragma warning restore 108


        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

}
