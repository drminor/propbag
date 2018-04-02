using DRM.TypeSafePropertyBag.TypeExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DRM.PropBag.TypeWrapper
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
            Type parentType = typeDescription.BaseType;
            Type[] typesToImplement = GetTypesToImplement(parentType);

            System.Diagnostics.Debug.WriteLine(name, $"Emitting new wrapper type: {name} based on: {parentType}.");

            TypeBuilder typeBuilder = moduleBuilderInfo.ModuleBuilder.DefineType
                (
                name,
                TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public,
                parentType,
                typesToImplement
                );

            CustomAttributeBuilder attributeBuilder = new AttributeEmitter().CreateAttributeBuilder(new WasEmittedAttribute(DateTime.Now.ToString()));
            typeBuilder.SetCustomAttribute(attributeBuilder);

            IEnumerable<ConstructorInfo> ctors = parentType.GetDeclaredConstructors();
            BuildConstructors(typeBuilder, ctors);

            //// Build the Clone method.  //override public object Clone() { return new <<ClassName>>(this);  }
            ////MethodAttributes attributesForCloneMethod = MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig;

            //MethodAttributes attributesForCloneMethod = MethodAttributes.Public | MethodAttributes.HideBySig;

            //MethodBuilder mb = typeBuilder.DefineMethod("Clone", attributesForCloneMethod, typeof(object), new Type[0]);
            //BuildCloneMethod(mb);

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
                            nameof(parentType));
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

        private Type[] GetTypesToImplement(Type baseType)
        {
            // Used to be just the next line.
            //Type[] implementedTypes = interfaceType.GetTypeInfo().ImplementedInterfaces.ToArray();

            IEnumerable<Type> typesImplementedByBase = baseType.GetTypeInfo().ImplementedInterfaces;
            Type[] result = typesImplementedByBase/*.Where(x => x.Name != "ICustomTypeDescriptor")*/.ToArray();

            return result;
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
            //if (ci.GetParameters().Length > 4) throw new NotSupportedException("Max parameter count for constructors is 4");

            ParameterInfo[] pars = ci.GetParameters();

            ILGenerator ctorIl = cb.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);

            for(int aPtr = 0; aPtr < pars.Length; aPtr++)
            {
                ctorIl.Emit(OpCodes.Ldarg_S, aPtr + 1);
            }

            //ctorIl.Emit(OpCodes.Ldarg_1);

            //if (ci.GetParameters().Length > 1)
            //    ctorIl.Emit(OpCodes.Ldarg_2);

            //if (ci.GetParameters().Length > 2)
            //    ctorIl.Emit(OpCodes.Ldarg_3);

            ////if (ci.GetParameters().Length > 3)
            ////    ctorIl.Emit(OpCodes.Ldarg_S, 4);

            ctorIl.Emit(OpCodes.Call, ci);
            ctorIl.Emit(OpCodes.Ret);
        }

  //      .method public hidebysig virtual instance object
  //      Clone() cil managed
  //      {
  //// Code size       12 (0xc)
  //.maxstack  1
  //.locals init ([0] object V_0)
  //IL_0000:  nop
  //IL_0001:  ldarg.0
  //IL_0002:  newobj instance void MVVMApplication.ViewModel.PersonVM::.ctor(class MVVMApplication.ViewModel.PersonVM)
  //IL_0007:  stloc.0
  //IL_0008:  br.s IL_000a
  //IL_000a:  ldloc.0
  //IL_000b:  ret

  //  } // end of method PersonVM::Clone

    //  //override public object Clone() { return new <<ClassName>>(this);  }
    //private void BuildCloneMethod(MethodBuilder mb)
    //    {
    //        ILGenerator mbIl = mb.GetILGenerator();
    //        //mbIl.Emit(OpCodes.Nop);
    //        mbIl.Emit(OpCodes.Ldarg_0); // Load reference to this.
    //        mbIl.Emit(OpCodes.Newobj);

    //        mbIl.Emit(OpCodes.Stloc_0);

    //        mbIl.Emit(OpCodes.Br_S);
    //        mbIl.Emit(OpCodes.Ldloc_0);

    //        mbIl.Emit(OpCodes.Ret);
    //    }

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
