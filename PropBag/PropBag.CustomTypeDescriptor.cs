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

        #region ICustomTypeDescriptor Support

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return GetAttributes();
        }

        protected AttributeCollection GetAttributes()
        {
            return AttributeCollection.Empty;
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

        protected string GetComponentName()
        {
            return GetClassName();
        }

        protected string GetClassName()
        {
            return this.OurMetaData.ClassName;
        }

        // Get Converter
        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return GetConverter();
        }

        protected TypeConverter GetConverter()
        {
            throw new NotImplementedException();

        }

        // Get Default Event
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return GetDefaultEvent();
        }

        protected EventDescriptor GetDefaultEvent()
        {
            throw new NotImplementedException();
        }

        // Get Default Property
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return GetDefaultProperty();
        }

        protected PropertyDescriptor GetDefaultProperty()
        {
            throw new NotImplementedException();
        }

        // Get Editor
        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return GetEditor(editorBaseType);
        }

        protected object GetEditor(Type editorBaseType)
        {
            throw new NotImplementedException();
        }

        // Get Events
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return GetEvents();
        }

        protected EventDescriptorCollection GetEvents()
        {
            throw new NotImplementedException();
        }

        // Get Events(attributes)
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return GetEvents(attributes);
        }

        protected EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            throw new NotImplementedException();
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

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return GetPropertyOwner(pd);
        }

        protected object GetPropertyOwner(PropertyDescriptor pd)
        {
            return null;
        }

        #endregion

    }
}
