﻿using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag.TypeDescriptors
{
    // ToDo: This is not used.
    public class PropItemPropertyDescriptor_Typed<T, PT, BagT> : PropertyDescriptor where PT : IProp<T> where BagT : IPropBag
    {
        #region Private Members

        PT _propItem;
        PropertyDescriptorValues<BagT> _tdConfig;

        PropertyDescriptorCollection _children;

        #endregion

        #region PropertyDescriptor Property Overrides

        public override Type ComponentType => _tdConfig.ComponentType;
        public override bool IsReadOnly => _tdConfig.IsReadOnly;
        public override Type PropertyType => _tdConfig.PropertyType;

        public override bool SupportsChangeEvents => _tdConfig.SupportsChangeEvents;

        #endregion

        #region Constructors

        //public PropItemTypeDescriptor_Typed(string propertyName, Type propertyType, Attribute[] attributes)
        //    : base(propertyName, attributes)
        //{
        //    _propBag = null;
        //    _propItem = null;

        //    _tdConfig = new TypeDescriptorConfig(attributes, typeof(IPropBag), false, propertyName, propertyType, true);

        //    _children = this.GetChildProperties();
        //}

        public PropItemPropertyDescriptor_Typed(string propertyName, BagT propBag, PT propItem)
            : base(propertyName, propItem.TypedPropTemplate.Attributes)
        {
            _propItem = propItem;

            _tdConfig = new PropertyDescriptorValues<BagT>
                (
                attributes: _propItem.TypedPropTemplate.Attributes,
                isReadOnly: false,
                name: propertyName,
                propertyType: _propItem.TypedPropTemplate.Type,
                supportsChangeEvents: true
                );

            _children = this.GetChildProperties();
        }

        #endregion

        public void Add(PropertyDescriptor pd)
        {
            _children.Add(pd);
        }

        #region PropertyDescriptor Method Overrides

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return _propItem.TypedValue;

            //if (_propItem != null)
            //    return _propItem.TypedValue;
            //else
            //    return ((BagT)component)[_tdConfig.PropertyType, Name];
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            _propItem.TypedValue = (T)value;

            //if (_propItem != null)
            //    _propItem.TypedValue = (T)value;
            //else
            //    ((BagT)component)[_tdConfig.PropertyType, Name] = (T)value;
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
        new public virtual PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
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
        new public virtual object GetEditor(Type editorBaseType)
        {
            throw new NotImplementedException();
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
        new protected virtual void OnValueChanged(object component, EventArgs e)
        {
            base.OnValueChanged(component, e);
        }

        #endregion
    }
}
