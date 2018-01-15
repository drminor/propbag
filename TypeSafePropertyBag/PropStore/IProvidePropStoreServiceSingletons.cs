namespace DRM.TypeSafePropertyBag
{
    public interface IProvidePropStoreServiceSingletons<L2T, L2TRaw>
    {
        ITypeDescBasedTConverterCache TypeDescBasedTConverterCache { get; }
        IProvideDelegateCaches DelegateCacheProvider { get; }
        IProvideHandlerDispatchDelegateCaches HandlerDispatchDelegateCacheProvider { get; }

        IPropStoreAccessServiceCreator<L2T, L2TRaw> PropStoreEntryPoint { get; }
    }
}