
namespace DRM.TypeSafePropertyBag.Fundamentals.ObjectIdDictionary
{
    public class SimpleObjectIdDictionary<PropDataT> : AbstractObjectIdDictionary<ulong, uint, uint, string, PropDataT> where PropDataT : IPropGen
    {
        public SimpleObjectIdDictionary(ICKeyMan<ulong, uint, uint, string> compKeyManager,
            IL2KeyMan<uint, string> level2KeyManager)
            : base(compKeyManager, level2KeyManager)
        {
        }
    }
}
