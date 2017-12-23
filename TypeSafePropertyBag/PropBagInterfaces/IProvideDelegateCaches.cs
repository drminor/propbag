using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.TypeSafePropertyBag
{
    public interface IProvideDelegateCaches
    {
        TypeDescBasedTConverterCache TypeDescBasedTConverterCache { get; }

        DelegateCache<DoSetDelegate> DoSetDelegateCache { get; }


        DelegateCache<CreatePropFromStringDelegate> CreatePropFromStringCache { get; }

        DelegateCache<CreatePropWithNoValueDelegate> CreatePropWithNoValCache { get; }

        DelegateCache<CreatePropFromObjectDelegate> CreatePropFromObjectCache { get; }

        TwoTypesDelegateCache<CreateEPropFromStringDelegate> CreateCPropFromStringCache { get; }
        TwoTypesDelegateCache<CreateEPropFromStringDelegate> CreateCPropFromStringFBCache { get; }

        TwoTypesDelegateCache<CreateEPropWithNoValueDelegate> CreateCPropWithNoValCache { get; }

        TwoTypesDelegateCache<CreateEPropFromObjectDelegate> CreateCPropFromObjectCache { get; }

        
        TwoTypesDelegateCache<CreateCVSPropDelegate> CreateCVSPropCache { get; }
    }
}