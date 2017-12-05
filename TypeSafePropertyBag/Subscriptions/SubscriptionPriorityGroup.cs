
namespace DRM.TypeSafePropertyBag
{
    public enum SubscriptionPriorityGroup : int
    {
        // Note the order that members within the same group are called is indeterminate.

        /// <summary>
        /// Call backs to the global property store are called first.
        /// </summary>
        Internal = 0,
        
        /// <summary>
        /// These are called before any members of the Standard group are called.
        /// </summary>
        First = 1,

        /// <summary>
        /// These are called after all of the members of the first group are called.
        /// </summary>
        Standard = 2,

        /// <summary>
        /// These are called after all of the members of the Standard group are called.
        /// </summary>
        Last = 3,

        /// <summary>
        /// These may be called before or after any group. These may be called asynchronously.
        /// Members of this group may all be called after or before any of the other groups,
        /// or the calls may be spread out over the entire sequence of calls.
        /// </summary>
        Logging = 4
    }
}
