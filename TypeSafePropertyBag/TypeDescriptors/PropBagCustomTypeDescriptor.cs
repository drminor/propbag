using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    public class PropBagCustomTypeDescriptor : CustomTypeDescriptor
    {
        #region Private Properties

        private readonly Func<IList<PropertyDescriptor>> _customPropsGetter;

        //private readonly object _funcTarget;

        #endregion

        #region Constructor

        public PropBagCustomTypeDescriptor(ICustomTypeDescriptor parent, Func<IList<PropertyDescriptor>> customPropsGetter)
            : base(parent)
        {
            _customPropsGetter = customPropsGetter;
            //_funcTarget = customPropsGetter?.Target;
        }

        #endregion

        #region Methods with Logic

        public override PropertyDescriptorCollection GetProperties()
        {
            //return new PropertyDescriptorCollection(base.GetProperties()
            //    .Cast<PropertyDescriptor>().Union(_customFields).ToArray());

            PropertyDescriptor[] temp = MergePropDescriptors(base.GetProperties(), _customPropsGetter());
            return new PropertyDescriptorCollection(temp);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            //return new PropertyDescriptorCollection(base.GetProperties(attributes)
            //    .Cast<PropertyDescriptor>().Union(_customFields).ToArray());

            PropertyDescriptor[] temp = MergePropDescriptors(base.GetProperties(attributes), _customPropsGetter());
            return new PropertyDescriptorCollection(temp);
        }

        //public IList<PropertyDescriptor> GetPropertyDescriptors()
        //{
        //    return base.GetProperties().Cast<PropertyDescriptor>().Union(_customFields).ToList();
        //}

        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            object result = base.GetPropertyOwner(pd);
            return result;
        }

        public override EventDescriptorCollection GetEvents()
        {
            EventDescriptorCollection result = base.GetEvents();
            return result;
        }

        public override EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            EventDescriptorCollection result = base.GetEvents(attributes);
            return result;
        }

        public override string GetComponentName()
        {
            string result = base.GetComponentName();
            return result;
        }

        #endregion

        #region Private Methods

        private PropertyDescriptor[] MergePropDescriptors(PropertyDescriptorCollection a, IList<PropertyDescriptor> b)
        {
            int aCnt = a.Count;
            int bCnt = b.Count;

            PropertyDescriptor[] descriptors = new PropertyDescriptor[aCnt + bCnt];

            int mainPdPtr;
            for (mainPdPtr = 0; mainPdPtr < aCnt; mainPdPtr++)
            {
                descriptors[mainPdPtr] = a[mainPdPtr];
            }

            for (int pdPtr = 0; pdPtr < bCnt; pdPtr++)
            {
                descriptors[mainPdPtr++] = b[pdPtr];
            }

            return descriptors;
        }

        #endregion

        //#region Pass Through Methods

        //public override AttributeCollection GetAttributes()
        //{
        //    return base.GetAttributes();
        //}

        //public override string GetClassName()
        //{
        //    return base.GetClassName();
        //}

        //public override string GetComponentName()
        //{
        //    return base.GetComponentName();
        //}

        //public override TypeConverter GetConverter()
        //{
        //    return base.GetConverter();
        //}

        //#endregion
    }
}
