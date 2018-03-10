using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag.TypeDescriptors
{
    public interface IMyTypeDescriptor
    {
        AttributeCollection Attributes { get; }
        string Category { get; }
        Type ComponentType { get; }
        TypeConverter Converter { get; }
        string Description { get; }
        bool DesignTimeOnly { get; }
        string DisplayName { get; }
        bool IsBrowsable { get; }
        bool IsLocalizable { get; }
        bool IsReadOnly { get; }
        string Name { get; }
        Type PropertyType { get; }
        bool SupportsChangeEvents { get; }

        void AddValueChanged(object component, EventHandler handler);
        bool CanResetValue(object component);


        //PropertyDescriptorCollection GetChildProperties(object instance);
        //PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter);
        PropertyDescriptorCollection GetChildProperties();
        //PropertyDescriptorCollection GetChildProperties(Attribute[] filter);

        object GetEditor(Type editorBaseType);
        object GetValue(object component);
        void RemoveValueChanged(object component, EventHandler handler);
        void ResetValue(object component);
        void SetValue(object component, object value);
        bool ShouldSerializeValue(object component);
    }
}
