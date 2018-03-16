using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag.TypeDescriptors
{
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    //using PropItemSetInterface = IPropItemSet<String>;

    using PropModelType = IPropModel<String>;
    using PropItemSetKeyType = PropItemSetKey<String>;

    //using PropModelCacheInterface = ICachePropModels<String>;

    public class PropItemFixedPropertyDescriptor<T> : PropertyDescriptor where T : IPropBag
    {
        #region Private Members

        PropertyDescriptorValues<T> _tdConfig;
        public readonly ExKeyT _compKey;

        readonly PSAccessServiceInterface _propStoreAccessor;
        //readonly WeakRefKey<PropModelType> _propModel_wrKey;

        readonly PropItemSetKeyType _propItemSetKey;


        #endregion

        #region Public Properties

        public PropIdType PropertyId => _compKey.Level2Key;

        #endregion

        #region PropertyDescriptor Property Overrides

        public override Type ComponentType => _tdConfig.ComponentType;
        public override bool IsReadOnly => _tdConfig.IsReadOnly;
        public override Type PropertyType => _tdConfig.PropertyType;

        public override bool SupportsChangeEvents => _tdConfig.SupportsChangeEvents;

        #endregion

        #region Constructors

        public PropItemFixedPropertyDescriptor(PSAccessServiceInterface propStoreAccessor, PropModelType propModel, string propertyName, ExKeyT compKey, Type propertyType, Attribute[] attributes)
            : base(propertyName, attributes)
        {
            _propStoreAccessor = propStoreAccessor;

            //_propModel_wrKey = new WeakRefKey<PropModelType>(propModel);
            _propItemSetKey = new PropItemSetKeyType(propModel);

            _compKey = compKey;

            _tdConfig = new PropertyDescriptorValues<T>
                (
                attributes: attributes,
                isReadOnly: false,
                name: propertyName,
                propertyType: propertyType,
                supportsChangeEvents: true
                );
        }

        #endregion

        #region PropertyDescriptor Method Overrides

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            //ReportAccessCounter();


            //object x = ((T)component)[PropertyType, Name];

            T propBag = (T)component;
            object x = _propStoreAccessor.GetValueFast(propBag, _propItemSetKey, _compKey);
            return x;
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            //((T)component)[PropertyType, Name] = value;

            T propBag = (T)component;
            bool result = _propStoreAccessor.SetValueFast(propBag, _propItemSetKey, _compKey, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            // TODO: Consider adding ShouldSerializeValue to IProp<T> interface.
            return false;
        }

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
        public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
        {
            return base.GetChildProperties(instance, filter);
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
        protected override void OnValueChanged(object component, EventArgs e)
        {
            base.OnValueChanged(component, e);
        }

        protected override void FillAttributes(IList attributeList)
        {
            base.FillAttributes(attributeList);

            foreach(Attribute a in _tdConfig.Attributes)
            {
                attributeList.Add(a);
            }
        }

        #endregion

        #region Diagnostics

        long access_counter = 0;

        [System.Diagnostics.Conditional("DEBUG")]
        void ReportAccessCounter()
        {
            System.Diagnostics.Debug.WriteLine($"{ComponentType.Name}::{Name} has been accessed {++access_counter} times.");
        } 

        #endregion
    }
}
