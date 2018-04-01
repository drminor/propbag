
namespace DRM.TypeSafePropertyBag.DelegateCaches.Unused
{
    class SharedDefaultObject
    {
        private static SharedDefaultObject _default = new SharedDefaultObject();

        public static SharedDefaultObject Instance => _default;
    }
}
