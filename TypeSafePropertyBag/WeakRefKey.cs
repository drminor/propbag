using System;

/// <remarks>
/// Copied from src\Framework\MS\Internal\Data\ViewManager.cs (Assembly: PresentationFramework)
/// Modified to not allow references to null.
/// </remarks>

namespace DRM.TypeSafePropertyBag
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
                if(IsEmpty)
                {
                    throw new InvalidOperationException("The WeakRefKey is Empty");
                }
                return _weakRef.Target;
            }
        }

        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        static public WeakRefKey Empty => new WeakRefKey();

        public bool IsEmpty => _weakRef == null;

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
            if (IsEmpty != other.IsEmpty) return false;

            object c1 = Target;
            object c2 = other.Target;

            if (c1 != null && c2 != null)
            {
                // Neither has been Garbage Collected.
                return (c1 == c2);
            }
            else
            {
                // Use reference equality to compare the 'containers.'
                return (_weakRef == other._weakRef);
            }
        }

        // overload operator for ==, to be same as Equal implementation.
        public static bool operator ==(WeakRefKey left, WeakRefKey right)
        {
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
        int _hashCode;  // cache target's hashcode, lest it get GC'd out from under us
    }
}
