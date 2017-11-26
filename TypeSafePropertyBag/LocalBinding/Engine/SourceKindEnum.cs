namespace DRM.TypeSafePropertyBag.LocalBinding.Engine
{
    public enum SourceKindEnum
    {
        AbsRoot,
        //RootUp,
        //RootDown,
        Up,
        Down,
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
