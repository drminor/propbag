using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public class TypeInspectorUtility
    {
        public static IList<string> GetPropertyNames(Type objectType, object instance)
        {
            ICustomTypeDescriptor ictd = GetCustomTypeDescriptor(objectType, instance);

            IList<string> result = GetPropertyNames(ictd);
            return result;
        }

        public static IList<string> GetPropertyNames(Type t)
        {
            ICustomTypeDescriptor ictd = GetCustomTypeDescriptor(t);

            IList<string> result = GetPropertyNames(ictd);
            return result;
        }

        public static IList<string> GetPropertyNames(object instance)
        {
            ICustomTypeDescriptor ictd = GetCustomTypeDescriptor(instance);

            IList<string> result = GetPropertyNames(ictd);
            return result;
        }

        public static IList<string> GetPropertyNames(ICustomTypeDescriptor ictd)
        {
            if (ictd == null)
            {
                return new List<string>();
            }
            else
            {
                IList<string> result = ictd.GetProperties().Cast<PropertyDescriptor>().Select(p => p.Name).ToList();
                return result;
            }
        }

        public static ICustomTypeDescriptor GetCustomTypeDescriptor(Type t, object instance)
        {
            if (instance is ICustomTypeDescriptor ictd)
            {
                return ictd;
            }
            else
            {
                ICustomTypeDescriptor result = new MyTypeDescriptionProvider(t).GetTypeDescriptor(t, instance);
                return result;
            }
        }

        public static ICustomTypeDescriptor GetCustomTypeDescriptor(Type t)
        {
            ICustomTypeDescriptor result = new MyTypeDescriptionProvider(t).GetTypeDescriptor(t);
            return result;
        }

        public static ICustomTypeDescriptor GetCustomTypeDescriptor(object instance)
        {
            if(instance is ICustomTypeDescriptor ictd)
            {
                return ictd;
            }
            else
            {
                Type t = instance.GetType();
                ICustomTypeDescriptor result = new MyTypeDescriptionProvider(t).GetTypeDescriptor(instance);
                return result;
            }
        }

        class MyTypeDescriptionProvider : TypeDescriptionProvider
        {
            #region Constuctors

            public MyTypeDescriptionProvider(Type objectType) : this(TypeDescriptor.GetProvider(objectType))
            {
            }

            public MyTypeDescriptionProvider(TypeDescriptionProvider parent) : base(parent)
            {
            }

            #endregion

            #region Methods with Logic

            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                ICustomTypeDescriptor result = base.GetTypeDescriptor(objectType, instance);
                return result;
            }

            #endregion

            #region Pass Through Methods

            public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
            {
                return base.CreateInstance(provider, objectType, argTypes, args);
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
    }
}
