using System;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = System.UInt32;
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

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
            // Use the target property key from the BindingSubscription
            ExKeyT targetPropId = bs.TargetPropId;

            // The "local" PropId.
            PropIdType propId = targetPropId.Level2Key;

            // The object reference
            IPropBag target = SimpleExKey.UnwrapWeakRef(((SimpleExKey) targetPropId).WR_AccessToken);
            IPropBag targetAlt = SimpleExKey.UnwrapWeakRef((WeakReference<IPropBag>)bs.Target);

            // Ask the target to set the property to the newValue.
            //bool result = target.SetIt<T>(newValue, propId);

            // Let the caller know that one more binding targets were updated.
            //if (result)
            //    counter++;
        }

        #region Private Methods

        #endregion

    }
}
