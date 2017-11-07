
namespace DRM.TypeSafePropertyBag.Fundamentals.ObjectIdDictionary
{
    public class SimpleObjectIdDictionary<TValue> : AbstractObjectIdDictionary<ulong, uint, uint, string, TValue>
    {
        public SimpleObjectIdDictionary(ICKeyMan<ulong, uint, uint, string> compKeyManager,
            IL2KeyMan<uint, string> level2KeyManager)
            : base(compKeyManager, level2KeyManager)
        {
        }
    }
}
