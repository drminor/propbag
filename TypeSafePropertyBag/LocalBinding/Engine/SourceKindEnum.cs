namespace DRM.TypeSafePropertyBag.LocalBinding.Engine
{
    public enum SourceKindEnum
    {
        FrameworkElement,
        FrameworkContentElement,
        DataGridColumn,
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
