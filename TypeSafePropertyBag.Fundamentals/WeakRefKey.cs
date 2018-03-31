using System;

/// <remarks>
/// Copied from src\Framework\MS\Internal\Data\ViewManager.cs (Assembly: PresentationFramework)
/// Modified to not allow references to null.
/// </remarks>

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    // for use as the key to a hashtable, when the "real" key is an object
    // that we should not keep alive by a strong reference.
    public struct WeakRefKey : IEquatable<WeakRefKey>
    {
        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        public WeakRefKey(object target)
        {
            if(target == null)
            {
                throw new ArgumentNullException($"Cannot create a WeakRefKey that references null.");
            }

            _weakRef = new WeakReference(target);
            //_isInitialized = true;

            _hashCode = target.GetHashCode(); // ?? 314159; // (target != null) ? target.GetHashCode() : 314159;
        }

        //------------------------------------------------------
        //
        //  Internal Properties
        //
        //------------------------------------------------------

        public object Target
        {
            get
            {
                if(!_isInitialized)
                {
                    throw new InvalidOperationException("The WeakRefKey has not been initialized.");
                }
                return _weakRef.Target;
            }
        }

        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        static public WeakRefKey Empty => new WeakRefKey(SharedDefaultObject.Instance);

        public void Clear()
        {
            _weakRef = null;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object other)
        {
            if (other is WeakRefKey wrk)
            {
                return Equals(wrk);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(WeakRefKey other)
        {
            object c1 = Target;
            object c2 = other.Target;

            if (c1 != null && c2 != null)
                return (c1 == c2);
            else
                return (_weakRef == other._weakRef);
        }

        // overload operator for ==, to be same as Equal implementation.
        public static bool operator ==(WeakRefKey left, WeakRefKey right)
        {
            if ((object)left == null)
                return (object)right == null;

            return left.Equals(right);
        }

        // overload operator for !=, to be same as Equal implementation.
        public static bool operator !=(WeakRefKey left, WeakRefKey right)
        {
            return !(left == right);
        }

        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        WeakReference _weakRef;

        ///// <summary>
        ///// Used to distinguish between a reference that has been garbage collected and
        ///// a default instance of this struct, i.e. an instance created using the parameterless constructor.
        ///// </summary>
        //bool _isInitialized;

        bool _isInitialized => _weakRef != null;
        int _hashCode;  // cache target's hashcode, lest it get GC'd out from under us
    }
}
