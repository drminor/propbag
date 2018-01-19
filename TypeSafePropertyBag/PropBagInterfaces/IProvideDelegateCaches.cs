
namespace DRM.TypeSafePropertyBag
{
    public interface IProvideDelegateCaches
    {
        //ITypeDescBasedTConverterCache TypeDescBasedTConverterCache { get; }

        ICacheDelegates<DoSetDelegate> DoSetDelegateCache { get; }
        ICacheDelegatesForTypePair<CVPropFromDsDelegate> CreateCViewPropCache { get; }

        ICacheDelegates<CreatePropFromStringDelegate> CreatePropFromStringCache { get; }

        ICacheDelegates<CreatePropWithNoValueDelegate> CreatePropWithNoValCache { get; }

        ICacheDelegates<CreatePropFromObjectDelegate> CreatePropFromObjectCache { get; }

        ICacheDelegatesForTypePair<CreateEPropFromStringDelegate> CreateCPropFromStringCache { get; }

        ICacheDelegatesForTypePair<CreateEPropWithNoValueDelegate> CreateCPropWithNoValCache { get; }

        ICacheDelegatesForTypePair<CreateEPropFromObjectDelegate> CreateCPropFromObjectCache { get; }

        ICacheDelegatesForTypePair<CreateMappedDSPProviderDelegate> CreateDSPProviderCache { get; }


        //ICacheDelegates<CreateCVSPropDelegate> CreateCVSPropCache { get; }
        //ICacheDelegates<CreateCVPropDelegate> CreateCVPropCache { get; }

    }
}