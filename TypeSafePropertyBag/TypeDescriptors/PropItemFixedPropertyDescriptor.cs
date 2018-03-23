using System;
using System.Collections;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag.TypeDescriptors
{
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using PropNameType = String;
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PropModelType = IPropModel<String>;
    using PropItemSetKeyType = PropItemSetKey<String>;

    using PSFastAccessServiceInterface = IPropStoreFastAccess<UInt32, String>;

    //using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    public class PropItemFixedPropertyDescriptor<T> : PropertyDescriptor where T : IPropBag
    {
        #region Private Members

        PropertyDescriptorValues<T> _tdConfig;
        public readonly ExKeyT _compKey;
        public readonly PropIdType PropId; // This is the Level2 Key.

        readonly PSFastAccessServiceInterface _psFastAccessService;
        readonly PropItemSetKeyType _propItemSetKey;

        #endregion

        #region Public Properties

        //public PropIdType PropertyId => _compKey.Level2Key;

        #endregion

        #region PropertyDescriptor Property Overrides

        public override Type ComponentType => _tdConfig.ComponentType;
        public override bool IsReadOnly => _tdConfig.IsReadOnly;
        public override Type PropertyType => _tdConfig.PropertyType;

        public override bool SupportsChangeEvents => _tdConfig.SupportsChangeEvents;

        public override string DisplayName {
            get
            {
                string result = base.DisplayName;
                return result;
            }
        }

        #endregion

        #region Constructors

        public PropItemFixedPropertyDescriptor
        (
            PSFastAccessServiceInterface psFastAccessService,
            PropModelType propModel,
            PropIdType propId,
            PropNameType propertyName,
            Type propertyType,
            Attribute[] attributes
        )
            : this
            (
                GetTdConfig(propModel.ClassName, attributes, propertyName, propertyType),
                psFastAccessService,
                propModel,
                propId
            )
        {
        }

        public PropItemFixedPropertyDescriptor
        (
            PropertyDescriptorValues<T> tdConfig,
            PSFastAccessServiceInterface psFastAccessService,
            PropModelType propModel,
            PropIdType propId
        )
            : base(tdConfig.Name, tdConfig.Attributes)
        {
            _tdConfig = tdConfig;
            _psFastAccessService = psFastAccessService;
            _propItemSetKey = new PropItemSetKeyType(propModel);
            PropId = propId;

            _compKey = new SimpleExKey(10, propId);
        }

        #endregion

        private static PropertyDescriptorValues<T> GetTdConfig(string componentName, Attribute[] attributes, PropNameType propertyName, Type propertyType)
        {
            bool isReadOnly = false;
            bool supportsChangeEvents = true;

            //if(componentName == "PersonVM")
            //{
            //    supportsChangeEvents = false;
            //}

            //if(propertyName == "PersonListView")
            //{
            //    supportsChangeEvents = false;
            //}

            PropertyDescriptorValues<T> result = new PropertyDescriptorValues<T>(attributes, isReadOnly, propertyName, propertyType, supportsChangeEvents);
            return result;
        }

        #region PropertyDescriptor Method Overrides

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            //ReportAccessCounter();

            object result;
            if (component is IPropBagInternal ipbi)
            {
                ExKeyT compKey = new SimpleExKey(ipbi.ObjectId, PropId);
                result = _psFastAccessService.GetValueFast(compKey, _propItemSetKey);
            }
            else
            {
                result = _psFastAccessService.GetValueFast((T)component, PropId, _propItemSetKey);
            }

            return result;
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            bool result;
            if (component is IPropBagInternal ipbi)
            {
                ExKeyT compKey = new SimpleExKey(ipbi.ObjectId, PropId);
                result = _psFastAccessService.SetValueFast(compKey, _propItemSetKey, value);
            }
            else
            {
                result = _psFastAccessService.SetValueFast((T)component, PropId, _propItemSetKey, value);
            }
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

        protected override void FillAttributes(IList attributeList)
        {
            base.FillAttributes(attributeList);

            foreach(Attribute a in _tdConfig.Attributes)
            {
                attributeList.Add(a);
            }
        }

        #endregion

        #region Event Support

        public override void AddValueChanged(object component, EventHandler handler)
        {
            base.AddValueChanged(component, handler);
        }

        protected override object GetInvocationTarget(Type type, object instance)
        {
            object target = base.GetInvocationTarget(type, instance);
            return target;
        }

        public override void RemoveValueChanged(object component, EventHandler handler)
        {
            base.RemoveValueChanged(component, handler);
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
