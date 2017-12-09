using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public class BindingsCollection : IEnumerable<ISubscription>
    {
        private readonly List<ISubscription> _bindings;
        private readonly object _sync;

        public BindingsCollection()
        {
            _sync = new object();
            _bindings = new List<ISubscription>();
        }

        public ISubscription GetOrAdd(ISubscriptionKeyGen bindingRequest, Func<ISubscriptionKeyGen, ISubscription> factory)
        {
            lock (_sync)
            {
                if (TryGetBinding(bindingRequest, out ISubscription binding))
                {
                    System.Diagnostics.Debug.WriteLine($"The binding for {bindingRequest.OwnerPropId} has aleady been created.");
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

        public bool TryRemoveBinding(ISubscriptionKeyGen bindingRequest, out ISubscription binding)
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
                    System.Diagnostics.Debug.WriteLine($"Could not find the binding for {bindingRequest.OwnerPropId} <= {bindingRequest.BindingInfo.PropertyPath.Path} when trying to remove it.");
                    return false;
                }
            }
        }

        public bool TryRemoveBinding(ISubscription binding)
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
                    System.Diagnostics.Debug.WriteLine($"Could not find the binding for {binding.OwnerPropId} <= {binding.BindingInfo.PropertyPath.Path} when trying to remove it.");
                    return false;
                }
            }
        }

        public void RemoveBinding(ISubscription request)
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

        public bool ContainsBinding(ISubscription binding)
        {
            bool result = _bindings.Exists(x => x.Equals(binding));
            return result;
        }

        public bool ContainsBinding(ISubscriptionKeyGen bindingRequest)
        {
            bool result = _bindings.Exists(x => BindingIsForRequest(x, bindingRequest));
            return result;
        }

        public bool TryGetBinding(ISubscription binding)
        {
            lock (_sync)
            {
                binding = _bindings.FirstOrDefault(x => x.Equals(binding));

            }
            bool result = binding != null;
            return result;
        }

        public bool TryGetBinding(ISubscriptionKeyGen bindingRequest, out ISubscription binding)
        {
            lock (_sync)
            {
                binding = _bindings.FirstOrDefault(x => BindingIsForRequest(x, bindingRequest));

            }
            bool result = binding != null;
            return result;
        }

        private bool BindingIsForRequest(ISubscription binding, ISubscriptionKeyGen bindingRequest)
        {
            bool result = 
                binding.OwnerPropId == bindingRequest.OwnerPropId &&
                binding.BindingInfo == bindingRequest.BindingInfo;

            return result;
        }

        public IEnumerable<ISubscription> TryGetBindings(ExKeyT exKey)
        {
            lock (_sync)
            {
                IEnumerable<ISubscription> result = _bindings.Where((x => x.OwnerPropId == exKey));
                return result;
            }
        }

        public int ClearBindings()
        {
            int result = _bindings.Count;
            _bindings.Clear();

            return result;
        }

        public IEnumerator<ISubscription> GetEnumerator()
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
