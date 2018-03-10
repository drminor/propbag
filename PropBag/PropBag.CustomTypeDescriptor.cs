using DRM.PropBag.TypeDescriptors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DRM.PropBag
{
    public partial class PropBag : ICustomTypeDescriptor
    {
        #region Custom Access for Dervied Classes

        protected int PropertyDescriptorCount
        {
            get
            {
                return _properties?.Count ?? 0;
            }
        }

        protected void ResetPropertyDescriptors()
        {
            _properties = null;
        }

        protected ReadOnlyCollection<PropertyDescriptor> GetPropertiesROCollection()
        {
            PropertyDescriptorCollection properties = GetProperties();
            PropertyDescriptor[] pds = new PropertyDescriptor[PropertyDescriptorCount];

            properties.CopyTo(pds, 0);

            ReadOnlyCollection<PropertyDescriptor> result = new ReadOnlyCollection<PropertyDescriptor>(pds);

            return result;
        }

        protected void RebuildPropertyDescriptors()
        {
            throw new NotImplementedException("PropBag.RebuildPropertyDescriptors has not yet been implemented.");
        }

        protected void AddPropertyDescriptor()
        {
            throw new NotImplementedException("PropBag.AddPropertyDescriptor has not yet been implemented.");
        }

        #endregion

        #region ICustomTypeDescriptor - Protected Access

        protected AttributeCollection GetAttributes()
        {
            return AttributeCollection.Empty;
        }

        protected string GetComponentName()
        {
            return GetClassName();
        }

        protected string GetClassName()
        {
            return this.OurMetaData.ClassName;
        }

        protected TypeConverter GetConverter()
        {
            throw new NotImplementedException();
        }

        protected EventDescriptor GetDefaultEvent()
        {
            throw new NotImplementedException();
        }

        protected PropertyDescriptor GetDefaultProperty()
        {
            throw new NotImplementedException();
        }

        protected EventDescriptorCollection GetEvents()
        {
            throw new NotImplementedException();
        }

        protected object GetEditor(Type editorBaseType)
        {
            throw new NotImplementedException();
        }

        protected EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            throw new NotImplementedException();
        }

        PropertyDescriptorCollection _properties;
        protected PropertyDescriptorCollection GetProperties()
        {
            if (_properties == null)
            {
                PropBagTypeDescriptor<PropBag> pbTypeDescriptor = new PropBagTypeDescriptor<PropBag>(this, null /*"PropBag - Base-Test"*/);
                PropertyDescriptor[] propDescriptors = pbTypeDescriptor.BuildPropDescriptors(this);

                _properties = new PropertyDescriptorCollection(propDescriptors);

                ICustomTypeDescriptor ctd = this as ICustomTypeDescriptor;
            }
            return _properties;
        }

        protected PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            // TODO: Filter results using the list of attributes.
            System.Diagnostics.Debug.WriteLine("GetProperties was called with attributes filter.");

            PropertyDescriptorCollection result = this.GetProperties();
            return result;
        }

        protected object GetPropertyOwner(PropertyDescriptor pd)
        {
            return null;
        }

        #endregion

        #region ICustomTypeDescriptor Explicit Implementation

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return GetAttributes();
        }

        // Get Class and Component Name
        string ICustomTypeDescriptor.GetClassName()
        {
            return GetClassName();
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            // The class and component names are the same.
            return GetClassName();
        }

        // Get Converter
        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return GetConverter();
        }

        // Get Default Event
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return GetDefaultEvent();
        }

        // Get Default Property
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return GetDefaultProperty();
        }

        // Get Editor
        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return GetEditor(editorBaseType);
        }

        // Get Events
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return GetEvents();
        }

        // Get Events(attributes)
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return GetEvents(attributes);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return this.GetProperties();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            // TODO: Filter results using the list of attributes.
            System.Diagnostics.Debug.WriteLine("GetProperties was called with attributes filter.");
            return this.GetProperties();
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return GetPropertyOwner(pd);
        }

        #endregion
    }
}
