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

        public ISubscriptionGen GetOrAdd(ISubscriptionKeyGen bindingRequest, Func<ISubscriptionKeyGen, ISubscriptionGen> factory)
        {
            lock (_sync)
            {
                if (TryGetBinding(bindingRequest, out ISubscriptionGen binding))
                {
                    System.Diagnostics.Debug.WriteLine($"The binding for {bindingRequest.SourcePropRef} has aleady been created.");
                    return binding;
                }
                else
                {
                    binding = factory(bindingRequest);
                    _bindings.Add(binding);

                    return binding;
                }
            }
        }

        //public bool AddBinding(ISubscriptionGen binding)
        //{
        //    lock (_sync)
        //    {
        //        if (ContainsBinding(binding))
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            _bindings.Add(binding);
        //            return true;
        //        }
        //    }
        //}

        public bool TryRemoveBinding(ISubscriptionKeyGen bindingRequest, out ISubscriptionGen binding)
        {
            lock (_sync)
            {
                if (TryGetBinding(bindingRequest, out binding))
                {
                    _bindings.Remove(binding);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Could not find the binding for {bindingRequest.TargetPropRef.CKey} when trying to remove it.");
                    return false;
                }
            }
        }

        public bool TryRemoveBinding(ISubscriptionGen binding)
        {
            lock (_sync)
            {
                if (TryGetBinding(binding))
                {
                    _bindings.Remove(binding);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Could not find the binding for {binding.TargetPropRef.CKey} when trying to remove it.");
                    return false;
                }
            }
        }

        public void RemoveBinding(ISubscriptionGen request)
        {
            _bindings.Remove(request);
        }

        //public bool TryRemoveBinding(ExKeyT exKey)
        //{
        //    lock (_sync)
        //    {
        //        if (TryGetBindings(exKey, out ISubscriptionGen binding))
        //        {
        //            _bindings.Remove(binding);
        //            return true;
        //        }
        //        else
        //        {
        //            System.Diagnostics.Debug.WriteLine($"Could not find the binding for {exKey.CKey} when trying to remove it.");
        //            return false;
        //        }
        //    }
        //}

        public bool ContainsBinding(ISubscriptionGen binding)
        {
            bool result = _bindings.Exists(x => x.Equals(binding));
            return result;
        }

        public bool ContainsBinding(ISubscriptionKeyGen bindingRequest)
        {
            bool result = _bindings.Exists(x => BindingIsForRequest(x, bindingRequest));
            return result;
        }

        public bool TryGetBinding(ISubscriptionGen binding)
        {
            lock (_sync)
            {
                binding = _bindings.FirstOrDefault(x => x.Equals(binding));

            }
            bool result = binding != null;
            return result;
        }

        public bool TryGetBinding(ISubscriptionKeyGen bindingRequest, out ISubscriptionGen binding)
        {
            lock (_sync)
            {
                binding = _bindings.FirstOrDefault(x => BindingIsForRequest(x, bindingRequest));

            }
            bool result = binding != null;
            return result;
        }

        private bool BindingIsForRequest(ISubscriptionGen binding, ISubscriptionKeyGen bindingRequest)
        {
            bool result = binding.TargetPropRef.CKey == bindingRequest.TargetPropRef.CKey &&
                binding.BindingInfo.PropertyPath.Equals(bindingRequest.BindingInfo.PropertyPath);

            return result;
        }

        public IEnumerable<ISubscriptionGen> TryGetBindings(ExKeyT exKey)
        {
            lock (_sync)
            {
                IEnumerable<ISubscriptionGen> result = _bindings.Where((x => x.TargetPropRef.Level2Key == exKey.Level2Key));
                return result;
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
