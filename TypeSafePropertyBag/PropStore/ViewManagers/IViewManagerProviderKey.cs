using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IViewManagerProviderKey : IEquatable<IViewManagerProviderKey>
    {
        LocalBindingInfo BindingInfo { get; }
        IMapperRequest MapperRequest { get; }

        //PropBagMapperCreator PropBagMapperCreator { get; }
        //CViewProviderCreator ViewBuilder { get; }
    }
}