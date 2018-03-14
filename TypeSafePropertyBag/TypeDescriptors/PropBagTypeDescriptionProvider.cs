using System;
using System.Collections;
using System.ComponentModel;

/// <remarks>
/// This code was inspired by the code written by Nish Nishant described in a Code Project article
/// entitled: "Using a TypeDescriptionProvider to support dynamic run-time properties", which can be
/// found here: https://www.codeproject.com/Articles/26992/Using-a-TypeDescriptionProvider-to-support-dynamic
/// 
/// The code is covered by the Code Project Open License (CPOL).
/// </remarks>

namespace DRM.TypeSafePropertyBag.TypeDescriptors
{
    public class PropBagTypeDescriptionProvider<T> : TypeDescriptionProvider where T : IHaveACustomTypeDescriptor
    {
        #region Private Properties

        // Elected not to 'cache' this value because other agents may register a different provider at some later point.
        //private static TypeDescriptionProvider _defaultTypeDescriptionProvider = TypeDescriptor.GetProvider(typeof(T));

        #endregion

        #region Constuctors

        public PropBagTypeDescriptionProvider() : this(TypeDescriptor.GetProvider(typeof(T)))
        {
        }

        public PropBagTypeDescriptionProvider(TypeDescriptionProvider parent) : base(parent)
        {
        }

        #endregion

        #region Methods with Logic

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            if(objectType == null)
            {
                throw new InvalidOperationException($"The objectType is null. On call to {nameof(PropBagTypeDescriptionProvider<T>)}::GetTypeDescriptor.");
            }

            if (objectType != typeof(T))
            {
                //throw new InvalidOperationException($"The objectType is {objectType}, expected type: {typeof(T)}. On call to {nameof(PropBagTypeDescriptionProvider<T>)}::GetTypeDescriptor.");
                System.Diagnostics.Debug.WriteLine($"Calling GetTypeDesciptor with instance of type: {objectType}, expected type: {typeof(T)}. Returning Null.");
            }

            ICustomTypeDescriptor defaultTypeDescriptor = base.GetTypeDescriptor(typeof(T), null);

            if (instance == null)
            {
                return defaultTypeDescriptor;
            }
            else
            {
                if(instance is T ctdSource)
                {
                    //if(instance.GetType().Name == "MainWindowViewModel")
                    //{
                    //    System.Diagnostics.Debug.WriteLine("The PropBagTypeDescriptor is getting the CustomTypeDescriptor for the MainWindowViewModel.");
                    //}
                    ICustomTypeDescriptor result = ctdSource.GetCustomTypeDescriptor(defaultTypeDescriptor);
                    return result;
                }
                else
                {
                    Type actual = instance.GetType();
                    System.Diagnostics.Debug.WriteLine($"Calling GetTypeDesciptor with instance of type: {actual}, expected type: {typeof(T)} (The objectType is {objectType}.) Returning Null.");
                    return null;
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
}
