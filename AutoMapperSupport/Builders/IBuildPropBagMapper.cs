using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IBuildPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        IPropBagMapper<TSource, TDestination> GenerateMapper(IPropBagMapperKey<TSource, TDestination> mapperRequestKey);

        Func<IPropBagMapperKeyGen, IPropBagMapperGen> GenMapperCreator { get; }
    }
}
