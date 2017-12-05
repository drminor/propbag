﻿namespace DRM.PropBag.ControlsWPF.Binders
{
    public enum SourceKindEnum
    {
        FrameworkElement,
        FrameworkContentElement,
        DataGridColumn,
        DataSourceProvider,
        PCGen,
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
