using System;

/// <remarks>
/// Based on the non-generic version copied from src\Framework\MS\Internal\Data\ViewManager.cs (Assembly: PresentationFramework)
/// </remarks>

namespace DRM.TypeSafePropertyBag
{
    // for use as the key to a hashtable, when the "real" key is an object
    // that we should not keep alive by a strong reference.
    public struct WeakRefKey<T>: IEquatable<WeakRefKey<T>> where T: class
    {
        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        public WeakRefKey(T target)
        {
            _weakRef = new WeakReference<T>(target);
            _hashCode = (target != null) ? target.GetHashCode() : 314159;
        }

        //------------------------------------------------------
        //
        //  Internal Properties
        //
        //------------------------------------------------------

        public T Target
        {
            get
            {
                //T result = null;
                //_weakRef?.TryGetTarget(out result);
                //return result;

                //if(_weakRef == null)
                //{
                //    return null;
                //}
                //else
                //{
                //    if(_weakRef.TryGetTarget(out T result))
                //    {
                //        return result;
                //    }
                //    else
                //    {
                //        return null;
                //    }
                //}

                if (_weakRef == null || !_weakRef.TryGetTarget(out T result))
                {
                    return null;
                }
                else
                {
                    return result;
                }
            }
        }

        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        public bool TryGetTarget(out T target)
        {
            if(_weakRef != null)
            {
                return _weakRef.TryGetTarget(out target);
            }
            else
            {
                target = null;
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is WeakRefKey<T> && Equals((WeakRefKey<T>)obj);
        }

        public bool Equals(WeakRefKey<T> other)
        {
            //if (_weakRef == null && other._weakRef == null) return true;

            //if (_weakRef == null || other._weakRef == null) return false;

            //T c1 = Target;
            //T c2 = other.Target;

            //if (c1 != null && c2 != null)
            //    return (c1 == c2); // Reference Equality, could have used ReferenceEquals(c1, c2)
            //else
            //    return (_weakRef == other._weakRef);

            T c1 = Target;
            T c2 = other.Target;

            if (c1 != null && c2 != null)
                return ReferenceEquals(c1, c2);
            else
                return ReferenceEquals(_weakRef, other._weakRef);
        }

        //public override bool Equals(object o)
        //{
        //    if (o is WeakRefKey)
        //    {
        //        WeakRefKey ck = (WeakRefKey)o;
        //        object c1 = Target;
        //        object c2 = ck.Target;

        //        if (c1 != null && c2 != null)
        //            return (c1 == c2);
        //        else
        //            return (_weakRef == ck._weakRef);
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public override string ToString()
        {
            if (_weakRef == null)
                return $"Null value of type: WeakRef of type: {typeof(T)}.";

            if(_weakRef.TryGetTarget(out T val))
            {
                return $"WeakRef of type: {typeof(T)} with value: {val.ToString()}.";
            }
            else
            {
                return $"WeakRef of type: {typeof(T)} that references an object that has been Garbage Collected.";
            }
        }

        // overload operator for ==, to be same as Equal implementation.
        public static bool operator ==(WeakRefKey<T> left, WeakRefKey<T> right)
        {
            if ((object)left == null)
                return (object)right == null;

            return left.Equals(right);
        }

        // overload operator for !=, to be same as Equal implementation.
        public static bool operator !=(WeakRefKey<T> left, WeakRefKey<T> right)
        {
            return !(left == right);
        }

        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        WeakReference<T> _weakRef;
        int _hashCode;  // cache target's hashcode, lest it get GC'd out from under us
    }

}
