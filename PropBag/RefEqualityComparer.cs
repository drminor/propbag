using System;
using System.Collections.Generic;

using System.Threading;

namespace DRM.PropBag
{

    ///<remarks>
    /// This was guided and inspied by this article: https://stackoverflow.com/questions/1890058/iequalitycomparert-that-uses-referenceequals
    /// Author: Snowbear (Marat Yuldashev, Ukraine)
    /// With improvements from: AnorZaken (Real name unknown.)
    /// 
    /// I futher improved it by using System.Lazy to instantiate the singleton, thereby making it (a little bit more) thread safe.
    /// 
    /// PublicationOnly is the right amount of thread safety here, since all callers who attempt to initialze the value
    /// of the single instance will produce the same value.
    /// 
    /// This implements IEqualityComparer<typeparam name="T"></typeparam> to be compatible with ValueWithType class.
    ///</remarks>

    /// <summary>
    /// A generic object comparerer that only returns true if the two values are exactly the same object.
    /// </summary>


    public sealed class RefEqualityComparer<T> : IEqualityComparer<T>
    {
        static Lazy<RefEqualityComparer<T>> singleInstance;
        public static RefEqualityComparer<T> Default { get { return singleInstance.Value; } }

        static RefEqualityComparer()
        {
            singleInstance = new Lazy<RefEqualityComparer<T>>(() => new RefEqualityComparer<T>(), LazyThreadSafetyMode.PublicationOnly);
        }

        private RefEqualityComparer() { } // Mark as private to disallow creation using the default constructor.

        /// <summary>
        /// Compares the object reference for each value to see if they are referencing the same object.
        /// It ignores the implemtations of IEquatable<typeparamref name="T"/> or Equals that may be present for this type.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>True if these objects are the exact same and in fact use the same storage. Otherwise returns false.</returns>
        public bool Equals(T x, T y)
        {
            return (object)x == (object)y; // This is reference equality! 
        }

        /// <summary>
        /// Get's the hashcode by first casting the value to an object, and then using the implementation of GetHashCode 
        /// provided by object.
        /// </summary>
        /// <param name="x"></param>
        /// <returns>The value of GetHashCode of this object.</returns>
        public int GetHashCode(T x)
        {
            return ((object) x).GetHashCode();
        }
    }
}
