﻿
namespace DRM.TypeSafePropertyBag
{
    public interface IProvideDelegateCaches
    {
        ICacheDelegates<DoSetDelegate> DoSetDelegateCache { get; }

        //ICacheDelegatesForTypePair<CVPropFromDsDelegate> CreateCViewPropCache { get; }
        ICacheDelegatesForTypePair<CViewManagerFromDsDelegate> GetOrAddCViewManagerCache { get; }
        ICacheDelegatesForTypePair<CViewManagerProviderFromDsDelegate> GetOrAddCViewManagerProviderCache { get; }

        ICacheDelegates<CreateScalarProp> CreateScalarPropCache { get; }
        //ICacheDelegates<CreatePropFromStringDelegate> CreateScalarPropCache { get; }
        //ICacheDelegates<CreatePropWithNoValueDelegate> CreatePropWithNoValCache { get; }
        //ICacheDelegates<CreatePropFromObjectDelegate> CreatePropFromObjectCache { get; }

        ICacheDelegatesForTypePair<CreateCPropFromStringDelegate> CreateCPropFromStringCache { get; }

        ICacheDelegatesForTypePair<CreateCPropWithNoValueDelegate> CreateCPropWithNoValCache { get; }

        ICacheDelegatesForTypePair<CreateCPropFromObjectDelegate> CreateCPropFromObjectCache { get; }

        ICacheDelegatesForTypePair<CreateMappedDSPProviderDelegate> CreateDSPProviderCache { get; }

        PropTemplateCache PropTemplateCache { get; }
    }
}