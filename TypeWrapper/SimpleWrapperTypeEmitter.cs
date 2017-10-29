using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DRM.TypeWrapper
{
    public class SimpleWrapperTypeEmitter : IEmitWrapperType
    {
        IModuleBuilderInfo _mbInfo;

        public SimpleWrapperTypeEmitter(IModuleBuilderInfo mbInfo)
        {
            _mbInfo = mbInfo;
        }

        public Type EmitWrapperType(TypeDescription td)
        {
            return EmitWrapper(td, _mbInfo);
        }

        private Type EmitWrapper(TypeDescription typeDescription, IModuleBuilderInfo moduleBuilderInfo)
        {
            string name = BuildTypeName(typeDescription.TypeName, moduleBuilderInfo.AssemblyId);
            Type interfaceType = typeDescription.BaseType;
            Type[] implementedTypes = interfaceType.GetTypeInfo().ImplementedInterfaces.ToArray();

            System.Diagnostics.Debug.WriteLine(name, "Emitting Class Wrapper for type");

            TypeBuilder typeBuilder = moduleBuilderInfo.ModuleBuilder.DefineType
                (
                name,
                TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public,
                interfaceType,
                implementedTypes
                );

            IEnumerable<ConstructorInfo> ctors = interfaceType.GetDeclaredConstructors();
            BuildConstructors(typeBuilder, ctors);

            var propertiesToImplement = new List<PropertyDescription>();

            // First we collect all properties, those with setters before getters in order to enable less specific redundant getters
            foreach (var property in typeDescription.PropertyDescriptions)
            {
                if (property.CanWrite)
                {
                    propertiesToImplement.Insert(0, property);
                }
                else
                {
                    propertiesToImplement.Add(property);
                }
            }

            var fieldBuilders = new Dictionary<string, PropertyEmitter>();
            foreach (var property in propertiesToImplement)
            {
                if (fieldBuilders.TryGetValue(property.Name, out PropertyEmitter propertyEmitter))
                {
                    if ((propertyEmitter.PropertyType != property.Type) &&
                        ((property.CanWrite) || (!property.Type.IsAssignableFrom(propertyEmitter.PropertyType))))
                    {
                        throw new ArgumentException(
                            $"The interface has a conflicting property {property.Name}",
                            nameof(interfaceType));
                    }
                }
                else
                {
                    propertyEmitter = new PropertyEmitter(typeBuilder, property); //, propertyChangedField));
                    fieldBuilders.Add(property.Name, propertyEmitter);

                }
            }
            Type result = typeBuilder.CreateType();
            return result;
        }

        private string BuildTypeName(TypeName typeName, int moduleId)
        {
            return $"{moduleId}.{typeName.Name}";
        }

        //private static string BuildTypeName_OLD(TypeName typeName, Type interfaceType)
        //{
        //    string y = interfaceType.AssemblyQualifiedName;
        //    if (typeName.AssemblyName != null)
        //        return $"{typeName.Name}_Wrapper_{Regex.Replace(typeName.AssemblyName, @"[\s,]+", "_")}";
        //    else
        //        return $"{typeName.Name}_Wrapper_{Regex.Replace(interfaceType.AssemblyQualifiedName ?? interfaceType.FullName ?? interfaceType.Name, @"[\s,]+", "_")}";
        //}

        private void BuildConstructors(TypeBuilder tb, IEnumerable<ConstructorInfo> cis)
        {
            foreach (ConstructorInfo ci in cis)
            {
                List<Type> parameterTypes = new List<Type>();
                foreach (ParameterInfo pi in ci.GetParameters())
                {
                    parameterTypes.Add(pi.ParameterType);
                }

                ConstructorBuilder cb = tb.DefineConstructor
                    (
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    parameterTypes.ToArray()
                    );

                BuildConstructor(cb, ci);
            }
        }

        private void BuildConstructor(ConstructorBuilder cb, ConstructorInfo ci)
        {
            if (ci == null) throw new ArgumentNullException(string.Format("Cannot find contructor method for {0}", nameof(ci)));
            if (ci.GetParameters().Length > 3) throw new NotSupportedException("Max parameter count for constructors is 3");

            ILGenerator ctorIl = cb.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldarg_1);

            if (ci.GetParameters().Length > 1)
                ctorIl.Emit(OpCodes.Ldarg_2);

            if (ci.GetParameters().Length > 2)
                ctorIl.Emit(OpCodes.Ldarg_3);

            ctorIl.Emit(OpCodes.Call, ci);
            ctorIl.Emit(OpCodes.Ret);
        }

        #region DevelopementWork Get Key Pair

        //private static void GetKeys()
        //{
        //    FileStream fs = new FileStream(@"C:\Users\david_000\Source\keyPair.snk", FileMode.Open);
        //    StrongNameKeyPair kp = new StrongNameKeyPair(fs);
        //    fs.Close();
        //}

        #endregion
    }
}
