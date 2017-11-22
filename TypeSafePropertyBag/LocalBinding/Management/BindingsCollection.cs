using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public class BindingsCollection : IEnumerable<ISubscriptionGen>
    {
        private readonly List<ISubscriptionGen> _bindings;
        private readonly object _sync;

        public BindingsCollection()
        {
            _sync = new object();
            _bindings = new List<ISubscriptionGen>();
        }

        public ISubscriptionGen GetOrAdd(ISubscriptionKeyGen bindingKey, Func<ISubscriptionKeyGen, ISubscriptionGen> factory)
        {
            lock (_sync)
            {
                if (TryGetBindings(bindingKey.TargetPropRef, out ISubscriptionGen binding))
                {
                    System.Diagnostics.Debug.WriteLine($"The binding for {bindingKey.SourcePropRef} has aleady been created.");
                    return binding;
                }
                else
                {
                    binding = factory(bindingKey);
                    AddBinding(binding);

                    return binding;
                }
            }
        }

        public bool AddBinding(ISubscriptionGen binding)
        {
            lock (_sync)
            {
                if (ContainsBinding(binding))
                {
                    return false;
                }
                else
                {
                    _bindings.Add(binding);
                    return true;
                }
            }
        }

        public bool RemoveBinding(ISubscriptionKeyGen request)
        {
            lock (_sync)
            {
                if (TryGetBindings(request.TargetPropRef, out ISubscriptionGen binding))
                {
                    // TODO: consider adding a TryRemove method.
                    _bindings.Remove(binding);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Could not find the binding for {request.TargetPropRef.CKey} when trying to remove it.");
                    return false;
                }
            }
        }

        public bool RemoveBinding(ExKeyT exKey)
        {
            lock (_sync)
            {
                if (TryGetBindings(exKey, out ISubscriptionGen binding))
                {
                    // TODO: consider adding a TryRemove method.
                    _bindings.Remove(binding);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Could not find the binding for {exKey.CKey} when trying to remove it.");
                    return false;
                }
            }
        }

        public bool ContainsBinding(ISubscriptionGen binding)
        {
            bool result = _bindings.Contains(binding);
            return result;
        }

        public bool TryGetBindings(ExKeyT exKey, out ISubscriptionGen binding)
        {
            lock (_sync)
            {
                binding = _bindings.FirstOrDefault((x => x.TargetPropId.Level2Key == exKey.Level2Key));
            }

            if(binding == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public int ClearBindings()
        {
            int result = _bindings.Count;
            _bindings.Clear();

            return result;
        }

        public IEnumerator<ISubscriptionGen> GetEnumerator()
        {
            lock (_sync)
                return _bindings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
