
namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public class SimpleObjectIdDictionary<PropDataT> : AbstractObjectIdDictionary<SimpleExKey, ulong, uint, uint, string, PropDataT> where PropDataT : IPropGen
    {
        public SimpleObjectIdDictionary(ICKeyMan<SimpleExKey, ulong, uint, uint, string> compKeyManager,
            IL2KeyMan<uint, string> level2KeyManager)
            : base(compKeyManager, level2KeyManager)
        {
        }
    }
}
