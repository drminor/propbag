
namespace DRM.TypeSafePropertyBag
{
    public enum SubscriptionPriorityGroup
    {
        // Note the order that members within the same group are called is indeterminate.

        /// <summary>
        /// These are called after all of the members of the first group are called.
        /// </summary>
        Standard,

        /// <summary>
        /// These are called before any members of the Standard group are called.
        /// </summary>
        First,

        /// <summary>
        /// These are called after all of the members of the Standard group are called.
        /// </summary>
        Last,

        /// <summary>
        /// These may be called before or after any group. These may be called asynchronously.
        /// Members of this group may all be called after or before any of the other groups,
        /// or the calls may be spread out over the entire sequence of calls.
        /// </summary>
        Logging
    }
}
