
using DRM.TypeSafePropertyBag;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    public class SimplePropBagMapperBuilderProvider : IPropBagMapperBuilderProvider
    {
        #region Constructor

        public SimplePropBagMapperBuilderProvider()
        {
        }

        #endregion

        #region Public Methods

        public IPropBagMapperBuilder<TSource, TDestination> GetPropBagMapperBuilder<TSource, TDestination>
            (
            IPropBagMapperService propBagMapperService
            )
            where TDestination: class, IPropBag
        {
            IPropBagMapperBuilder<TSource, TDestination> result
                = new SimplePropBagMapperBuilder<TSource, TDestination>
                (
                    propBagMapperService: propBagMapperService
                );

            return result;
        }

        #endregion
    }
}
