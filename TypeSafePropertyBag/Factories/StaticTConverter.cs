
namespace DRM.TypeSafePropertyBag
{
    public static class StaticTConverterProvider
    {
        public static ITypeDescBasedTConverterCache TypeDescBasedTConverterCache { get; }

        static StaticTConverterProvider()
        {
            // TypeDesc<T>-based Converter Cache
            TypeDescBasedTConverterCache = new TypeDescBasedTConverterCache();
        }
    }
}
