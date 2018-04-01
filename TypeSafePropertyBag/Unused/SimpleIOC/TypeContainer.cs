using System;

/// <remarks>
/// This Simple inversion of control "Container" was copied whole sale from...
/// http://www.siepman.nl/blog/post/2014/02/15/Simple-IoC-container-easy-to-debug-and-fast.aspx
/// http://www.siepman.nl/blog/author/Admin.aspx
/// </remarks>

namespace DRM.TypeSafePropertyBag.SimpleIOC
{
    class TypeContainer<T> : ITypeContainer
    {
        private readonly Func<T> _createFunc;
        public bool Shared { get; private set; }

        public TypeContainer(Func<T> createFunc, bool shared = true)
        {
            _createFunc = createFunc ?? throw new ContainerException<T>($"The Func to create the {typeof(T)} type in the container is null.");
            if (shared)
            {
                _cache = new Lazy<T>(() => _createFunc());
                Shared = true;
            }
        }

        public T GetObject()
        {
            return Shared ? GetSharedObject() : _createFunc();
        }

        private readonly Lazy<T> _cache;
        private T GetSharedObject()
        {
            return _cache.Value;
        }

        public void ForceCreate()
        {
            GetObject();
        }
    }
}
