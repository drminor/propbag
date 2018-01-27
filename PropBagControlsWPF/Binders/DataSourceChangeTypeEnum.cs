namespace DRM.PropBagControlsWPF.Binders
{
    public enum DataSourceChangeTypeEnum
    {
        /// <summary>
        /// A CLR, or Dependency Object has signaled that one of its properties has received
        /// a new value.
        /// </summary>
        PropertyChanged,

        /// <summary>
        /// A CLR collection object has signaled that its content have changed.
        /// </summary>
        CollectionChanged,

        /// <summary>
        /// A FrameworkElement or a FrameworkContentElement has signaled that the value
        /// of its DataContext property now points to a different object,
        /// or a DataSourceProvider has signaled that it's data now returns a different object.
        /// </summary>
        DataContextUpdated,

        /// <summary>
        /// Used internally to begin listening to the FrameworkElement's DataContextChanged
        /// or the DataSourceProvider, DataChanged event.
        /// </summary>
        Initializing
    }
}
