using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.TypeDescriptors
{
    public class PropBagTypeDescriptor<BagT> : PropertyDescriptor, IPropBagTypeDescriptor where BagT : IPropBag
    {
        #region Private Members

        IPropBag _propBag;
        PropModel _propModel;
        IPropFactory _propFactory;
        TypeDescriptorConfig _tdConfig;

        PropertyDescriptorCollection _children;

        TypeSafePropBagMetaData _metadata;

        #endregion

        #region PropertyDescriptor Property Overrides

        public override Type ComponentType => _tdConfig.ComponentType;
        public override bool IsReadOnly => _tdConfig.IsReadOnly;
        public override string Name => _tdConfig.Name;
        public override Type PropertyType => _tdConfig.PropertyType;

        public override bool SupportsChangeEvents => _tdConfig.SupportsChangeEvents;

        #endregion

        #region Constructors

        public PropBagTypeDescriptor(PropModel propModel, IPropFactory propFactory, string propertyName = null)
            : base(propertyName ?? propModel.ClassName, new Attribute[] { })
        {
            _propModel = propModel;
            _propFactory = propFactory;

            _tdConfig = new TypeDescriptorConfig(new Attribute[] { }, typeof(object), true, propertyName, typeof(PropBag), true);

            _children = this.GetChildProperties();

            PropertyDescriptor[] propDescriptors = BuildPropDescriptors(_propModel, _propFactory);

            foreach (PropertyDescriptor pDesc in propDescriptors)
            {
                _children.Add(pDesc);
            }
        }

        public PropBagTypeDescriptor(IPropBag propBag, string propertyName = null)
            //: base(propertyName ?? (propBag.GetMetaData()).ClassName, new Attribute[] { })
            : base(propertyName, new Attribute[] { })
        {
            _propBag = propBag;

            ITypeSafePropBagMetaData metadata = propBag.GetMetaData();

            propertyName = propertyName ?? metadata.ClassName;
            _tdConfig = new TypeDescriptorConfig(new Attribute[] { }, typeof(object), true, propertyName, typeof(PropBag), true);

            _children = this.GetChildProperties();
        }

        #endregion


        public PropertyDescriptor[] BuildPropDescriptors(PropModel pm, IPropFactory pf)
        {
            PropertyDescriptor[] descriptors = new PropertyDescriptor[pm.Props.Count];

            List<PropItem> propItems = pm.Props.ToList();

            for (int pdPtr = 0; pdPtr < propItems.Count; pdPtr++)
            {
                PropItem pItem = propItems[pdPtr];

                PropItemTypeDescriptor<IPropBag> propItemTypeDesc =
                    new PropItemTypeDescriptor<IPropBag>(pItem.PropertyName, pItem.PropertyType, new Attribute[] { });

                descriptors[pdPtr] = propItemTypeDesc;
            }

            return descriptors;
        }

        public PropertyDescriptor[] BuildPropDescriptors(IPropBag propBag)
        {
            List<KeyValuePair<string, ValPlusType>> propDefs = propBag.GetAllPropNamesAndTypes().ToList();

            PropertyDescriptor[] descriptors = new PropertyDescriptor[propDefs.Count];

            for (int pdPtr = 0; pdPtr < propDefs.Count; pdPtr++)
            {
                KeyValuePair<string, ValPlusType> kvp = propDefs[pdPtr];

                PropItemTypeDescriptor<IPropBag> propItemTypeDesc = 
                    new PropItemTypeDescriptor<IPropBag>(kvp.Key, kvp.Value.Type, new Attribute[] { });

                descriptors[pdPtr] = propItemTypeDesc;
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
