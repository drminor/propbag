using DRM.TypeSafePropertyBag.DelegateCaches;
using System;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PropItemSetKeyType = PropItemSetKey<String>;

    using PSAccessServiceProviderInterface = IProvidePropStoreAccessService<UInt32, String>;

    using PSFastAccessServiceInterface = IPropStoreFastAccess<UInt32, String>;


    internal class SimplePropStoreFastAccess : PSFastAccessServiceInterface
    {
        PSAccessServiceProviderInterface _propStoreAccessServiceProvider;

        public SimplePropStoreFastAccess(PSAccessServiceProviderInterface propStoreAccessServiceProvider)
        {
            _propStoreAccessServiceProvider = propStoreAccessServiceProvider;
        }

        public object GetValueFast(ExKeyT compKey, PropItemSetKeyType propItemSetKey)
        {
            object result = _propStoreAccessServiceProvider.GetValueFast(compKey, propItemSetKey);
            return result;
        }

        public bool SetValueFast(ExKeyT compKey, PropItemSetKeyType propItemSetKey, object value)
        {
            bool result = _propStoreAccessServiceProvider.SetValueFast(compKey, propItemSetKey, value);
            return result;
        }

        public object GetValueFast(IPropBag component, PropIdType propId, PropItemSetKeyType propItemSetKey)
        {
            if (component is IPropBagInternal ipbi)
            {
                //if(ipbi.ItsStoreAccessor is IHaveTheStoreNode ihtsn)
                //{
                //    BagNode propBagNode = ihtsn.PropBagNode;
                //    object result1 = _propStoreAccessServiceProvider.GetValueFast(propBagNode, propId, propItemSetKey);
                //    return result1;
                //}

                BagNode propBagNode = ((IHaveTheStoreNode)ipbi.ItsStoreAccessor).PropBagNode;
                object result1 = _propStoreAccessServiceProvider.GetValueFast(propBagNode, propId, propItemSetKey);
                return result1;
            }

            //ExKeyT compKey = GetCompKey(component, propId);
            WeakRefKey<IPropBag> propBag_wrKey = new WeakRefKey<IPropBag>(component);
            object result2 = _propStoreAccessServiceProvider.GetValueFast(propBag_wrKey, propId, propItemSetKey);
            return result2;
        }

        public bool SetValueFast(IPropBag component, PropIdType propId, PropItemSetKeyType propItemSetKey, object value)
        {
            if (component is IPropBagInternal ipbi)
            {
                if (ipbi is IHaveTheStoreNode ihtsn)
                {
                    BagNode propBagNode = (ipbi as IHaveTheStoreNode)?.PropBagNode;
                    bool result1 = _propStoreAccessServiceProvider.SetValueFast(propBagNode, propId, propItemSetKey, value);
                    return result1;
                }
            }

            //ExKeyT compKey = GetCompKey(component, propId);
            WeakRefKey<IPropBag> propBag_wrKey = new WeakRefKey<IPropBag>(component);
            bool result2 = _propStoreAccessServiceProvider.SetValueFast(propBag_wrKey, propId, propItemSetKey, value);
            return result2;
        }

    }
}
