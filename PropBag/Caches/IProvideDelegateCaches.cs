using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.PropBag.Caches
{
    public interface IProvideDelegateCaches
    {
        TypeDescBasedTConverterCache TypeDescBasedTConverterCache { get; }

        DelegateCache<DoSetDelegate> DoSetDelegateCache { get; }


        DelegateCache<CreatePropFromStringDelegate> CreatePropFromStringCache { get; }

        DelegateCache<CreatePropWithNoValueDelegate> CreatePropWithNoValCache { get; }

        DelegateCache<CreatePropFromObjectDelegate> CreatePropFromObjectCache { get; }

        TwoTypesDelegateCache<CreateCPropFromStringDelegate> CreateCPropFromStringCache { get; }

        TwoTypesDelegateCache<CreateCPropWithNoValueDelegate> CreateCPropWithNoValCache { get; }

        TwoTypesDelegateCache<CreateCPropFromObjectDelegate> CreateCPropFromObjectCache { get; }
    }
}