using System;
using System.Collections;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag.TypeDescriptors
{
    public class PropItemPropertyDescriptor<T> : PropertyDescriptor where T : IPropBag
    {
        #region Private Members

        PropertyDescriptorValues _tdConfig;

        //PropertyDescriptorCollection _children;

        #endregion

        #region PropertyDescriptor Property Overrides

        public override Type ComponentType => _tdConfig.ComponentType;
        public override bool IsReadOnly => _tdConfig.IsReadOnly;
        public override Type PropertyType => _tdConfig.PropertyType;

        public override bool SupportsChangeEvents => _tdConfig.SupportsChangeEvents;

        #endregion

        #region Constructors

        public PropItemPropertyDescriptor(string propertyName, Type propertyType, Attribute[] attributes)
            : base(propertyName, attributes)
        {
            _tdConfig = new PropertyDescriptorValues(attributes, typeof(IPropBag), false, propertyName, propertyType, true);
            //_children = this.GetChildProperties();
        }

        #endregion

        //public void Add(PropertyDescriptor pd)
        //{
        //    _children.Add(pd);
        //}

        #region PropertyDescriptor Method Overrides

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return ((T)component)[PropertyType, Name];
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            ((T)component)[PropertyType, Name] = value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            // TODO: Consider adding ShouldSerializeValue to IProp<T> interface.
            return false;
        }

        #endregion

        #region PropertyDescriptor Method Replacements
        //
        // Summary:
        //     Returns a System.ComponentModel.PropertyDescriptorCollection for a given object
        //     using a specified array of attributes as a filter.
        //
        // Parameters:
        //   instance:
        //     A component to get the properties for.
        //
        //   filter:
        //     An array of type System.Attribute to use as a filter.
        //
        // Returns:
        //     A System.ComponentModel.PropertyDescriptorCollection with the properties that
        //     match the specified attributes for the specified component.
        public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
        {
            return base.GetChildProperties(instance, filter);
        }

        //
        // Summary:
        //     Gets an editor of the specified type.
        //
        // Parameters:
        //   editorBaseType:
        //     The base type of editor, which is used to differentiate between multiple editors
        //     that a property supports.
        //
        // Returns:
        //     An instance of the requested editor type, or null if an editor cannot be found.
        public override object GetEditor(Type editorBaseType)
        {
            object x = base.GetEditor(editorBaseType);
            return x;
        }

        //
        // Summary:
        //     Raises the ValueChanged event that you implemented.
        //
        // Parameters:
        //   component:
        //     The object that raises the event.
        //
        //   e:
        //     An System.EventArgs that contains the event data.
        protected override void OnValueChanged(object component, EventArgs e)
        {
            base.OnValueChanged(component, e);
        }

        protected override void FillAttributes(IList attributeList)
        {
            base.FillAttributes(attributeList);

            foreach(Attribute a in _tdConfig.Attributes)
            {
                attributeList.Add(a);
            }
        }

        #endregion
    }
}
