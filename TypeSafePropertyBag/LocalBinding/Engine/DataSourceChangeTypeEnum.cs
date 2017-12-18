namespace DRM.TypeSafePropertyBag.LocalBinding.Engine
{
    public enum DataSourceChangeTypeEnum
    {
        /// <summary>
        /// A PropBag PropItem (IProp) has signaled that its value has been updated.
        /// The parent IPropBag raises a PropertyChangedWithTVals event on behalf of it's IProp prop item.
        /// </summary>
        PropertyChanged,

        /// <summary>
        /// A PropBag object is now a guest of a different PropItem, or is the guest of no PropItem.
        /// The PropStoreNode raises a ParentNodeHasChanged event.
        /// </summary>
        ParentHasChanged,
    }
}
