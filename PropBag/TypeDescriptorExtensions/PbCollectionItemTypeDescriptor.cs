using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.TypeDescriptorExtensions
{
    public class PbCollectionItemTypeDescriptor : PropertyDescriptor, IPbCollectionTypeDescriptor
    {
        #region PropertyDescriptor Property Overrides
        Type _componentType;
        public override Type ComponentType { get { return _componentType; } }
        public void SetComponentType(Type value) { _componentType = value; }

        public override bool IsReadOnly => false;

        Type _propertyType;
        public override Type PropertyType { get { return _propertyType; } }
        public void SetPropertyType(Type value) { _propertyType = value; }
        #endregion

        #region Constructors
        protected PbCollectionItemTypeDescriptor(MemberDescriptor descr) : base(descr)
        {
        }

        protected PbCollectionItemTypeDescriptor(string name, Attribute[] attrs) : base(name, attrs)
        {
        }

        protected PbCollectionItemTypeDescriptor(MemberDescriptor descr, Attribute[] attrs) : base(descr, attrs)
        {
        }

        protected PbCollectionItemTypeDescriptor(string name) : base(name, new Attribute[] { })
        {
        }

        #endregion

        #region PropertyDescriptor Method Overrides

        public override bool CanResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldSerializeValue(object component)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
