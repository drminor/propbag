using System;
using System.Collections.Generic;


/// <remarks>
/// This Simple inversion of control "Container" was copied whole sale from...
/// http://www.siepman.nl/blog/post/2014/02/15/Simple-IoC-container-easy-to-debug-and-fast.aspx
/// http://www.siepman.nl/blog/author/Admin.aspx
/// </remarks>

namespace DRM.TypeSafePropertyBag.SimpleIOC
{
    public class Container
    {
        private readonly Dictionary<string, ITypeContainer> _typeContainers =
             new Dictionary<string, ITypeContainer>();

        public void Register<T>(Func<T> createFunc, bool shared = true)
        {
            var fullTypeName = GetFullTypeName<T>();
            if (IsRegistered(fullTypeName))
            {
                throw new ContainerException<T>("The type {0} has already been registered in the container.");
            }
            var typeContainer = new TypeContainer<T>(createFunc, shared);
            _typeContainers.Add(fullTypeName, typeContainer);
        }

        public void UnRegister<T>()
        {
            var fullTypeName = GetFullTypeName<T>();
            if (!IsRegistered(fullTypeName))
            {
                throw new ContainerException<T>("The type {0} has not been registered in the container.");
            }
            _typeContainers.Remove(fullTypeName);
        }

        public T Use<T>()
        {
            var fullTypeName = GetFullTypeName<T>();
            if (!_typeContainers.TryGetValue(fullTypeName, out ITypeContainer typeContainer))
            {
                throw new ContainerException<T>("The type {0} has not been registered in the container.");
            }
            var typeContainerConcrete = (TypeContainer<T>)typeContainer;
            return typeContainerConcrete.GetObject();
        }

        public bool IsRegistered<T>()
        {
            return IsRegistered(GetFullTypeName<T>());
        }

        private bool IsRegistered(string fullName)
        {
            return _typeContainers.ContainsKey(fullName);
        }

        public void ForceCreate()
        {
            foreach (var typeContainer in _typeContainers.Values)
            {
                typeContainer.ForceCreate();
            }
        }

        private string GetFullTypeName<T>()
        {
            return typeof(T).FullName;
        }
    }
}
