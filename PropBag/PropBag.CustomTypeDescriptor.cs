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
        #region ICustomTypeDescriptor Support

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return AttributeCollection.Empty;
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return this.OurMetaData.ClassName;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return this.OurMetaData.ClassName;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            throw new NotImplementedException();
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            throw new NotImplementedException();
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            throw new NotImplementedException();
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            throw new NotImplementedException();
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            throw new NotImplementedException();
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            throw new NotImplementedException();
        }

        //PropBagTypeDescriptor<PropBag> pbTypeDescriptor;
        PropertyDescriptorCollection _properties;
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            if (_properties == null)
            {
                PropBagTypeDescriptor<PropBag> pbTypeDescriptor = new PropBagTypeDescriptor<PropBag>(this, "PropBag - Base-Test");
                PropertyDescriptor[] propDescriptors = pbTypeDescriptor.BuildPropDescriptors(this);

                _properties = new PropertyDescriptorCollection(propDescriptors);
            }
            return _properties;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            System.Diagnostics.Debug.WriteLine("GetProperties was called with attributes filter.");
            return ((ICustomTypeDescriptor)this).GetProperties();
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
