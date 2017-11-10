using DRM.TypeSafePropertyBag.Fundamentals;


namespace DRM.TypeSafePropertyBag.EventManagement
{
    public class SimpleLocalBinder<PropDataT> : AbstractLocalBinder<PropDataT> where PropDataT : IPropGen
    {
        // TODO: let's see if we can avoid using one or more of the dependencies!
        public SimpleLocalBinder
            (
            SimpleObjectIdDictionary<PropDataT> propertyStore,
            SimpleCompKeyMan compKeyManager,
            SimpleLevel2KeyMan level2KeyManager
            )
            : base(propertyStore, compKeyManager, level2KeyManager)
        {
        }

    }
}
