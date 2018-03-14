using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    public interface IHaveACustomTypeDescriptor
    {
        ICustomTypeDescriptor GetCustomTypeDescriptor(ICustomTypeDescriptor parent);
    }
}
