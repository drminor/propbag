using System;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PropItemSetKeyType = PropItemSetKey<String>;

    public interface IPropStoreFastAccess<L2T, L2TRaw>
    {
        object GetValueFast(ExKeyT compKey, PropItemSetKeyType propItemSetKey);
        bool SetValueFast(ExKeyT compKey, PropItemSetKeyType propItemSetKey, object value);

        object GetValueFast(IPropBag component, L2T propId, PropItemSetKey<L2TRaw> propItemSetKey);
        bool SetValueFast(IPropBag component, L2T propId, PropItemSetKey<L2TRaw> propItemSetKey, object value);
    }
}
