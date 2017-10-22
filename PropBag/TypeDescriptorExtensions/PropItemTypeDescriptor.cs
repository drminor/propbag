﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.PropBag;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.TypeDescriptorExtensions
{
    public class PropItemTypeDescriptor<BagT> : PropertyDescriptor, IPropItemTypeDescriptor where BagT : class, IPropBag
    {
        #region Private Members

        TypeDescriptorConfig _tdConfig;

        PropertyDescriptorCollection _children;

        #endregion

        #region PropertyDescriptor Property Overrides

        public override Type ComponentType => _tdConfig.ComponentType;
        public override bool IsReadOnly => _tdConfig.IsReadOnly;
        public override Type PropertyType => _tdConfig.PropertyType;

        public override bool SupportsChangeEvents => _tdConfig.SupportsChangeEvents;

        #endregion

        #region Constructors

        public PropItemTypeDescriptor(string propertyName, Type propertyType, Attribute[] attributes)
            : base(propertyName, attributes)
        {
            _tdConfig = new TypeDescriptorConfig(attributes, typeof(IPropBag), false, propertyName, propertyType, true);
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
            return ((BagT)component)[_tdConfig.PropertyType, Name];
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            ((BagT)component)[_tdConfig.PropertyType, Name] = value;
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
