using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public class ViewManagerProviderKey : IViewManagerProviderKey, IEquatable<IViewManagerProviderKey>
    {
        public LocalBindingInfo BindingInfo { get; }
        public IMapperRequest MapperRequest { get; }

        //PropBagMapperCreator _propBagMapperCreator;
        //CViewProviderCreator _viewBuilder;

        public ViewManagerProviderKey(LocalBindingInfo bindingInfo, IMapperRequest mr/*, PropBagMapperCreator propBagMapperCreator, CViewProviderCreator viewBuilder*/)
        {
            BindingInfo = bindingInfo;
            MapperRequest = mr ?? throw new ArgumentNullException(nameof(mr));

            //_propBagMapperCreator = propBagMapperCreator ?? throw new ArgumentNullException(nameof(propBagMapperCreator));
            //_viewBuilder = viewBuilder ?? throw new ArgumentNullException(nameof(viewBuilder));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ViewManagerProviderKey);
        }

        public bool Equals(IViewManagerProviderKey other)
        {
            return other != null &&
                   BindingInfo.Equals(other.BindingInfo) &&
                   EqualityComparer<IMapperRequest>.Default.Equals(MapperRequest, other.MapperRequest);
        }

        public override int GetHashCode()
        {
            var hashCode = 247345723;
            hashCode = hashCode * -1521134295 + EqualityComparer<LocalBindingInfo>.Default.GetHashCode(BindingInfo);
            hashCode = hashCode * -1521134295 + EqualityComparer<IMapperRequest>.Default.GetHashCode(MapperRequest);
            return hashCode;
        }

        public static bool operator ==(ViewManagerProviderKey key1, IViewManagerProviderKey key2)
        {
            return EqualityComparer<IViewManagerProviderKey>.Default.Equals(key1, key2);
        }

        public static bool operator !=(ViewManagerProviderKey key1, IViewManagerProviderKey key2)
        {
            return !(key1 == key2);
        }

        public static bool operator ==(IViewManagerProviderKey key1, ViewManagerProviderKey key2)
        {
            return EqualityComparer<IViewManagerProviderKey>.Default.Equals(key1, key2);
        }

        public static bool operator !=(IViewManagerProviderKey key1, ViewManagerProviderKey key2)
        {
            return !(key1 == key2);
        }
    }
}
