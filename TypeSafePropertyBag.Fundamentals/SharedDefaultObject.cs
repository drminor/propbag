
namespace DRM.TypeSafePropertyBag.Fundamentals
{
    class SharedDefaultObject
    {
        private static SharedDefaultObject _default = new SharedDefaultObject();

        public static SharedDefaultObject Instance => _default;
    }
}
