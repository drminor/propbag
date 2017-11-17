using System;

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;

    using PropIdType = UInt32;
    using PropNameType = String;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    #endregion

    public class NodeData
    {
        #region Public Members

        // Each node holds data about an IPropBag, or a Prop Item belonging to a IPropBag.
        bool IsObjectNode => Int_PropData == null;

        // If this is an ObjectNode, the PropId portion of the CKey is 0. CKey == ObjectId.
        // If this is an PropNode, the CKey identifies both the IPropBag and the Prop. Its a globally unique PropId.
        CompositeKeyType Ckey { get; }

        // PropStoreAccess will have a value, only for ObjectNodes.
        PSAccessServiceType PropStoreAccess { get; }

        // Subscriptions will only have a value for ObjectNodes, and only if a subscription has been Added.
        //CollectionOfSubscriberCollections Subscriptions;

        // PropData will have a value, only for PropNodes.
        IPropDataInternal Int_PropData { get; }



        #endregion

        #region Constructors

        public NodeData(ulong ckey, PSAccessServiceType propStoreAccess)
        {
            Ckey = ckey;
            PropStoreAccess = propStoreAccess ?? throw new ArgumentNullException(nameof(propStoreAccess));
            Int_PropData = null;
        }

        public NodeData(ulong ckey, IPropData int_PropData)
        {
            Ckey = ckey;
            PropStoreAccess = null;

            if (int_PropData is IPropDataInternal propDataInternal)
            {
                Int_PropData = propDataInternal;
            }
            else
            {
                throw new ArgumentException("int_PropData does not implement the IPropDataInternal interface", nameof(int_PropData));
            }
        }

        #endregion
    }
}
