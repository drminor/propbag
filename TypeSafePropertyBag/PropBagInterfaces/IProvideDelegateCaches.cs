
namespace DRM.TypeSafePropertyBag
{
    public interface IProvideDelegateCaches
    {
        TypeDescBasedTConverterCache TypeDescBasedTConverterCache { get; }

        ICacheDelegates<DoSetDelegate> DoSetDelegateCache { get; }

        ICacheDelegates<CreatePropFromStringDelegate> CreatePropFromStringCache { get; }

        ICacheDelegates<CreatePropWithNoValueDelegate> CreatePropWithNoValCache { get; }

        ICacheDelegates<CreatePropFromObjectDelegate> CreatePropFromObjectCache { get; }

        ICacheDelegatesForTypePair<CreateEPropFromStringDelegate> CreateCPropFromStringCache { get; }
        //ICacheDelegatesForTypePair<CreateEPropFromStringDelegate> CreateCPropFromStringFBCache { get; }

        ICacheDelegatesForTypePair<CreateEPropWithNoValueDelegate> CreateCPropWithNoValCache { get; }

        ICacheDelegatesForTypePair<CreateEPropFromObjectDelegate> CreateCPropFromObjectCache { get; }


        //ICacheDelegates<CreateCVSPropDelegate> CreateCVSPropCache { get; }
        //ICacheDelegates<CreateCVPropDelegate> CreateCVPropCache { get; }

    }
}