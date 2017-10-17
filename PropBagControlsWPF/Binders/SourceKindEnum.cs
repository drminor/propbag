namespace DRM.PropBag.ControlsWPF.Binders
{
    public enum SourceKindEnum
    {
        DataContext,
        DataContextBinder, // Used when the binding target is the DataContext property.
        DataSourceProvider,
        PropertyObject,
        CollectionObject,
        Empty,
        TerminalNode
    }

    public enum PathConnectorTypeEnum
    {
        Dot,
        Slash,

        DotIndexer,
        SlashIndexer,

        DotAttached,
        SlashAttached,

        DotIndexerAttached,
        SlashIndexerAttached
    }

 }
