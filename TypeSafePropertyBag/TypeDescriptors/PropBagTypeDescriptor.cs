using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DRM.TypeSafePropertyBag.TypeDescriptors
{
    using PropModelType = IPropModel<String>;

    public class PropBagTypeDescriptor<BagT> : PropertyDescriptor, IPropBagTypeDescriptor where BagT : IPropBag
    {

        #region Static Members

        static PropertyDescriptorCollection _basePropertyDescriptors;

        static PropBagTypeDescriptor()
        {
            _basePropertyDescriptors = GetBasePropertyCollection();
        }

        #endregion

        #region Private Members

        TypeDescriptorConfig _tdConfig;
        PropertyDescriptorCollection _children;

        #endregion

        #region PropertyDescriptor Property Overrides

        public override Type ComponentType => _tdConfig.ComponentType;
        public override bool IsReadOnly => _tdConfig.IsReadOnly;
        public override string Name => _tdConfig.Name;
        public override Type PropertyType => _tdConfig.PropertyType;

        public override bool SupportsChangeEvents //=> _tdConfig.SupportsChangeEvents;
        {
            get
            {
                bool result = _tdConfig.SupportsChangeEvents;
                return result;
            }
        }

        #endregion

        #region Constructors

        public PropBagTypeDescriptor(PropModelType propModel, string propertyName = null)
            : base(GetPropertyName(propertyName, propModel), new Attribute[] { })
        {
            string propertyNameToUse = base.Name;

            _tdConfig = new TypeDescriptorConfig(new Attribute[] { }, typeof(object), true, propertyNameToUse, typeof(BagT), true);

            _children = GetBasePropertyCollection();

            PropertyDescriptor[] propDescriptors = BuildPropDescriptors(propModel);

            foreach (PropertyDescriptor pDesc in propDescriptors)
            {
                _children.Add(pDesc);
            }
        }

        public PropBagTypeDescriptor(IPropBag propBag, string propertyName)
            : base(GetPropertyName(propertyName, propBag), new Attribute[] { })
        {
            string propertyNameToUse = base.Name;

            _tdConfig = new TypeDescriptorConfig(new Attribute[] { }, typeof(object), true, propertyNameToUse, typeof(BagT), true);

            _children = GetBasePropertyCollection();
        }

        #endregion

        static private PropertyDescriptorCollection GetBasePropertyCollection()
        {
            PropertyDescriptorCollection result;
            result = TypeDescriptor.GetProperties(typeof(BagT));
            return result;
        }

        static private string GetPropertyName(string className, PropModelType propModel)
        {
            return className ?? propModel.ClassName;
        }

        static private string GetPropertyName(string className, IPropBag propBag)
        {
            return className ?? propBag.GetMetaData().ClassName;
        }

        public PropertyDescriptor[] BuildPropDescriptors(PropModelType pm/*, IPropFactory pf*/)
        {
            PropertyDescriptor[] descriptors = new PropertyDescriptor[pm.Count];

            List<IPropItemModel> propItems = pm.GetPropItems().ToList();

            for (int pdPtr = 0; pdPtr < propItems.Count; pdPtr++)
            {
                IPropItemModel pItem = propItems[pdPtr];

                PropItemTypeDescriptor<IPropBag> propItemTypeDesc =
                    new PropItemTypeDescriptor<IPropBag>(pItem.PropertyName, pItem.PropertyType, new Attribute[] { });

                descriptors[pdPtr] = propItemTypeDesc;
            }

            return descriptors;
        }

        public PropertyDescriptor[] BuildPropDescriptors(IPropBag propBag)
        {
            List<KeyValuePair<string, ValPlusType>> propDefs = propBag.GetAllPropNamesAndTypes().ToList();

            PropertyDescriptor[] descriptors = new PropertyDescriptor[propDefs.Count + _basePropertyDescriptors.Count];

            int mainPdPtr;
            for(mainPdPtr = 0; mainPdPtr < _basePropertyDescriptors.Count; mainPdPtr++)
            {
                descriptors[mainPdPtr] = _basePropertyDescriptors[mainPdPtr];
            }

            for (int pdPtr = 0; pdPtr < propDefs.Count; pdPtr++)
            {
                KeyValuePair<string, ValPlusType> kvp = propDefs[pdPtr];

                PropItemTypeDescriptor<IPropBag> propItemTypeDesc = 
                    new PropItemTypeDescriptor<IPropBag>(kvp.Key, kvp.Value.Type, new Attribute[] { });

                descriptors[mainPdPtr++] = propItemTypeDesc;
            }

            return descriptors;
        }

        #region PropertyDescriptor Method Overrides

        public override bool CanResetValue(object component)
        {
            return false;
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
            // TODO: Consider adding ShouldSerializeValue to IProp<T> interface.
            return false;
        }

        #endregion

        #region PropertyDescriptor Method Replacements

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
        override public PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
        {
            PropertyDescriptorCollection result;
            result =  base.GetChildProperties(instance, filter);
            return result;
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
        //new public virtual object GetEditor(Type editorBaseType)
        //{
        //    throw new NotImplementedException();
        //}

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
        override protected void OnValueChanged(object component, EventArgs e)
        {
            base.OnValueChanged(component, e);
        }

        #endregion
    }
}
