using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    public class PropertyDescriptorStaticListProvider
    {
        IList<PropertyDescriptor> _list;

        public Func<IList<PropertyDescriptor>> CustomPropsGetter => GetPropertyDescriptors;

        public PropertyDescriptorStaticListProvider(IList<PropertyDescriptor> list)
        {
            _list = list;
        }

        public IList<PropertyDescriptor> GetPropertyDescriptors()
        {
            return _list;
        }
    }
}
