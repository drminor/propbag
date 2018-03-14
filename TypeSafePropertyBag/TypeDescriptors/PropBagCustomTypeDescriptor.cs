using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    public class PropBagCustomTypeDescriptor : CustomTypeDescriptor
    {
        #region Private Properties

        //private readonly PropBag _instance;
        private readonly Func<IList<PropertyDescriptor>> _customPropsGetter;
        //private readonly List<PropertyDescriptor> _customFields;
        //private PropertyDescriptorCollection _propDescCollection;

        #endregion

        #region Constructor

        public PropBagCustomTypeDescriptor(ICustomTypeDescriptor parent, Func<IList<PropertyDescriptor>> customPropsGetter)
            : base(parent)
        {
            //_instance = instance;

            _customPropsGetter = customPropsGetter;

            // Get the PropertyDescriptors for our custom fields 
            // by calling a private method on the class instance in which we are nested.
            //_customFields = instance.GetCustomProps().ToList();

            //_propDescCollection = new PropertyDescriptorCollection
            //    (
            //    base.GetProperties()
            //    .Cast<PropertyDescriptor>()
            //    .Union(_customFields)
            //    .ToArray()
            //    );
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
            return base.GetPropertyOwner(pd);
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

        //public override EventDescriptorCollection GetEvents()
        //{
        //    return base.GetEvents();
        //}

        //public override EventDescriptorCollection GetEvents(Attribute[] attributes)
        //{
        //    return base.GetEvents(attributes);
        //}

        //public override object GetPropertyOwner(PropertyDescriptor pd)
        //{
        //    return base.GetPropertyOwner(pd);
        //}

        //#endregion
    }
}
