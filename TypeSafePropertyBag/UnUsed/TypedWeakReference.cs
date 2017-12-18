using System;

// This should be removed since  WeakReference<T> can be used instead.

// This does allow one to change the value of TrackRessurection, where WeakReference<T>
// does not.

namespace DRM.TypeSafePropertyBag.UnUsed
{
    public class TypedWeakReference<T> where T : class
    {
        bool _track;
        WeakReference _wr;

        public TypedWeakReference(T value, bool trackResurrection = false)
        {
            _track = trackResurrection;
            Value = value;
        }

        public bool TrackRessurection
        {
            get
            {
                return _track;
            }
            set
            {
                if (_track == value) return; // It already has the desired value, there is nothing to do.

                _track = value;
                if (_wr != null)
                {
                    // Get a strong reference.
                    T cVal = Value;

                    // Create a new weak reference using the new value of TrackResurrection.
                    Value = cVal;
                }
            }
        }

        public bool TryGetValue(out T value)
        {
            if (_wr == null)
            {
                value = null;
                return false;
            }
            else
            {
                if(_wr.IsAlive || _wr.TrackResurrection)
                {
                    value = (T) _wr.Target;
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }

        }

        public T Value
        {
            get
            {
                if (_wr == null) return null;
                if (_wr.IsAlive || _wr.TrackResurrection)
                {
                    return (T)_wr.Target;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    _wr = null;
                }
                else
                {
                    _wr = new WeakReference(value, _track);
                }
            }
        }

    }

}
