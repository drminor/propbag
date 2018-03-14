
namespace DRM.TypeSafePropertyBag
{
    public interface IProvideDelegateCaches
    {
        ICacheDelegates<DoSetDelegate> DoSetDelegateCache { get; }

        //ICacheDelegatesForTypePair<CVPropFromDsDelegate> CreateCViewPropCache { get; }
        ICacheDelegatesForTypePair<CViewManagerFromDsDelegate> GetOrAddCViewManagerCache { get; }
        ICacheDelegatesForTypePair<CViewManagerProviderFromDsDelegate> GetOrAddCViewManagerProviderCache { get; }

        ICacheDelegates<CreateScalarProp> CreateScalarPropCache { get; }

        ICacheDelegatesForTypePair<CreateCPropFromStringDelegate> CreateCPropFromStringCache { get; }

        ICacheDelegatesForTypePair<CreateCPropWithNoValueDelegate> CreateCPropWithNoValCache { get; }

        ICacheDelegatesForTypePair<CreateCPropFromObjectDelegate> CreateCPropFromObjectCache { get; }

        ICacheDelegatesForTypePair<CreateMappedDSPProviderDelegate> CreateDSPProviderCache { get; }

        IProvidePropTemplates PropTemplateCache { get; }
    }
}