using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

/// <remarks>
/// This code is based on code written by Nish Nishant in a code project article
/// entitled: "Using a TypeDescriptionProvider to support dynamic run-time properties"
/// found here: https://www.codeproject.com/Articles/26992/Using-a-TypeDescriptionProvider-to-support-dynamic
/// 
/// The code is covered by the Code Project Open License (CPOL).
/// </remarks>

namespace DRM.TypeSafePropertyBag.TypeDescriptors
{
    public class PropBagTypeDescriptionProvider<T> : TypeDescriptionProvider where T : IPropBag
    {
        #region Private Properties

        private static TypeDescriptionProvider _defaultTypeProvider = TypeDescriptor.GetProvider(typeof(T));
        private static ICustomTypeDescriptor _defaultTypeDescriptor;

        #endregion

        #region Constuctors

        public PropBagTypeDescriptionProvider() : base(_defaultTypeProvider)
        {
            _defaultTypeDescriptor = base.GetTypeDescriptor(typeof(T), null);
        }

        #endregion

        #region Methods with Logic

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            if (instance == null)
            {
                //throw new InvalidOperationException($"The {nameof(PropBagTypeDescriptionProvider<T>)} does not support building a CustomTypeDescriptor for a type; It only builds CustomTypeDescriptors for a single instance.");

                ICustomTypeDescriptor defaultDescriptor = base.GetTypeDescriptor(typeof(T), instance);
                return defaultDescriptor;
            }
            else
            {
                if(instance is T propBag)
                {
                    System.Diagnostics.Debug.WriteLine($"Building a new PropBagCustomTypeDescriptor for Type: {typeof(T)} .");
                    PropBagCustomTypeDescriptor<T> result = new PropBagCustomTypeDescriptor<T>(_defaultTypeDescriptor, propBag);
                    return result;
                }
                else
                {
                    Type actual = instance.GetType();
                    throw new InvalidOperationException($"Instance must be of type: {typeof(T)}, instead it is of type: {actual}.");
                }
            }
        }

        #endregion

        #region Pass Through Methods

        public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
        {
            return base.CreateInstance(provider, objectType, argTypes, args);
        }

        public override IDictionary GetCache(object instance)
        {
            return base.GetCache(instance);
        }

        public override string GetFullComponentName(object component)
        {
            return base.GetFullComponentName(component);
        }

        public override Type GetReflectionType(Type objectType, object instance)
        {
            return base.GetReflectionType(objectType, instance);
        }

        public override Type GetRuntimeType(Type reflectionType)
        {
            return base.GetRuntimeType(reflectionType);
        }

        public override bool IsSupportedType(Type type)
        {
            return base.IsSupportedType(type);
        }

        #endregion
    }

    public class PropBagCustomTypeDescriptor<T> : CustomTypeDescriptor where T : IPropBag
    {
        #region Private Properties

        private readonly List<PropertyDescriptor> _customFields;

        #endregion

        #region Constructor

        public PropBagCustomTypeDescriptor(ICustomTypeDescriptor parent, T instance)
            : base(parent)
        {
            _customFields = new List<PropertyDescriptor>();
            _customFields.AddRange(GetCustomProps(instance).Cast<PropertyDescriptor>());
        }

        private IEnumerable<PropItemPropertyDescriptor<T>> GetCustomProps(T propBag)
        {
            //List<PropItemPropertyDescriptor<T>> piPropertyDescriptors = new List<PropItemPropertyDescriptor<T>>();

            if (propBag.HasPropModel)
            {
                foreach(IPropItemModel pi in propBag.GetPropItemModels())
                {
                    //PropItemPropertyDescriptor<T> propItemTypeDesc;

                    //if (pi.PropKind == PropKindEnum.CollectionView)
                    //{
                    //    propItemTypeDesc = new PropItemPropertyDescriptor<T>(pi.PropertyName, pi.CollectionType, new Attribute[] {});
                    //}
                    //else if(pi.PropKind == PropKindEnum.ObservableCollection)
                    //{
                    //    propItemTypeDesc = new PropItemPropertyDescriptor<T>(pi.PropertyName, pi.CollectionType, new Attribute[] {});
                    //}
                    //else if(pi.PropKind == PropKindEnum.Prop)
                    //{
                    //   propItemTypeDesc = new PropItemPropertyDescriptor<T>(pi.PropertyName, pi.PropertyType, new Attribute[] {});
                    //}
                    //else
                    //{
                    //    throw new InvalidOperationException($"The {nameof(PropBagTypeDescriptionProvider<T>)} does not recognized or does not support Props of Kind = {pi.PropKind}.");
                    //}

                    ////piPropertyDescriptors.Add(propItemTypeDesc);
                    //yield return propItemTypeDesc;

                    PropItemPropertyDescriptor<T> propItemTypeDesc;

                    if (pi.PropKind == PropKindEnum.CollectionView || pi.PropKind == PropKindEnum.ObservableCollection || pi.PropKind == PropKindEnum.Prop)
                    {
                        propItemTypeDesc = new PropItemPropertyDescriptor<T>(pi.PropertyName, pi.PropertyType, new Attribute[] { });
                    }
                    else
                    {
                        throw new InvalidOperationException($"The {nameof(PropBagTypeDescriptionProvider<T>)} does not recognized or does not support Props of Kind = {pi.PropKind}.");
                    }

                    //piPropertyDescriptors.Add(propItemTypeDesc);
                    yield return propItemTypeDesc;
                }
            }
            else
            {
                foreach(KeyValuePair<string, ValPlusType> kvp in propBag.GetAllPropNamesValuesAndTypes())
                {
                    PropItemPropertyDescriptor<T> propItemTypeDesc =
                        new PropItemPropertyDescriptor<T>(kvp.Key, kvp.Value.Type, new Attribute[] { });

                    //piPropertyDescriptors.Add(propItemTypeDesc);
                    yield return propItemTypeDesc;
                }
            }

            //return piPropertyDescriptors;
        }

        #endregion

        #region Methods with Logic

        public override PropertyDescriptorCollection GetProperties()
        {
            return new PropertyDescriptorCollection(base.GetProperties()
                .Cast<PropertyDescriptor>().Union(_customFields).ToArray());
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(base.GetProperties(attributes)
                .Cast<PropertyDescriptor>().Union(_customFields).ToArray());
        }

        #endregion

        #region Pass Through Methods

        public override AttributeCollection GetAttributes()
        {
            return base.GetAttributes();
        }

        public override string GetClassName()
        {
            return base.GetClassName();
        }

        public override string GetComponentName()
        {
            return base.GetComponentName();
        }

        public override TypeConverter GetConverter()
        {
            return base.GetConverter();
        }

        public override EventDescriptorCollection GetEvents()
        {
            return base.GetEvents();
        }

        public override EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return base.GetEvents(attributes);
        }

        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            return base.GetPropertyOwner(pd);
        }

        #endregion
    }
}
