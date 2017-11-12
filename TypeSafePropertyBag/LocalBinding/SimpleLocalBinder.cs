using System;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    using PropIdType = System.UInt32;

    public class SimpleLocalBinder : IBindLocalProps<PropIdType>
    {
        //private SimpleLevel2KeyMan _level2KeyManager;
        //private SimpleCompKeyMan _compKeyManager;
        //private readonly SimpleObjectIdDictionary<PropDataT> _propertyStore;

        //public AbstractLocalBinder
        //    (
        //    SimpleObjectIdDictionary<PropDataT> propertyStore,
        //    SimpleCompKeyMan compKeyManager,
        //    SimpleLevel2KeyMan level2KeyManager
        //    )
        //{
        //    _propertyStore = propertyStore;
        //    _compKeyManager = compKeyManager;
        //    _level2KeyManager = level2KeyManager;
        //}

        //// We should get rid of this -- the caller can use her own reference to the propertyStore.
        //public bool TryGetPropData(SimpleExKey propId, out PropDataT propData)
        //{
        //    uint PropBagObjectId = _compKeyManager.SplitComp(propId.CKey, out string propertyName);
        //    if(_propertyStore.TryGetValue(propId, out propData))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public void UpdateTarget<T>(BindingSubscription<T> bs, T oldValue, T newValue, ref int counter)
        {
            // Get the target
            //Action<T, T> originalAction = bs.TypedDoWhenChanged;
            //IPropBag target = (IPropBag)originalAction.Target;

            // Use the target property key from the BindingSubscription
            SimpleExKey targetPropId = bs.TargetPropId;

            // The "local" PropId.
            PropIdType propId = targetPropId.Level2Key;

            // The object reference
            IPropBag target = SimpleExKey.UnwrapWeakRef(targetPropId.WR_AccessToken);

            // Ask the target to set the property to the newValue.
            bool result = target.SetIt<T>(newValue, propId);

            // Let the caller know that one more binding targets were updated.
            if (result)
                counter++;
        }

        #region Private Methods

        //private IPropBag UnwrapWeakRef(WeakReference<IPropBag> objectRef)
        //{
        //    if (objectRef.TryGetTarget(out IPropBag target))
        //    {
        //        IPropBag result = (IPropBag)target;
        //        return result;
        //    }
        //    else
        //    {
        //        return default(IPropBag);
        //    }
        //}

        #endregion

    }
}
