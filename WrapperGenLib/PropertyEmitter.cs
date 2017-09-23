namespace DRM.WrapperGenLib
{
    using DRM.TypeSafePropertyBag;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public class PropertyEmitter<T> where T: IWrapperBase
    {
        private static readonly MethodInfo ProxBaseGetVal =
            typeof(T).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "GetVal");

        //private static readonly MethodInfo ProxBaseSetVal =
        //    typeof(T).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "SetItWithType");

        private static readonly MethodInfo ProxBaseSetValNT =
            typeof(T).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "SetVal");

        //private static readonly MethodInfo ChangeType =
        //    typeof(System.Convert).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "ChangeType");

        //private static readonly MethodInfo GetTypeFromHandle =
        //    typeof(System.Type).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "GetTypeFromHandle");

        private readonly FieldBuilder _fieldBuilder;
        private readonly MethodBuilder _getterBuilder;
        private readonly PropertyBuilder _propertyBuilder;
        private readonly MethodBuilder _setterBuilder;

        private Type pType;

        public PropertyEmitter(TypeBuilder owner, PropertyDescription property) //, FieldBuilder propertyChangedField)
        {
            var name = property.Name;
            var propertyType = property.Type;

            pType = propertyType;
            _fieldBuilder = owner.DefineField($"<{name}>", propertyType, FieldAttributes.Private);
            _propertyBuilder = owner.DefineProperty(name, PropertyAttributes.None, propertyType, null);

            _getterBuilder = owner.DefineMethod($"get_{name}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig |
                MethodAttributes.SpecialName, propertyType, new Type[0]);

            BuildPropGetter(_getterBuilder, name, propertyType);

            _propertyBuilder.SetGetMethod(_getterBuilder);
            if(!property.CanWrite)
            {
                return;
            }
            _setterBuilder = owner.DefineMethod($"set_{name}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig |
                MethodAttributes.SpecialName, typeof (void), new[] {propertyType});

            BuildPropSetter(_setterBuilder, name, propertyType);

            _propertyBuilder.SetSetMethod(_setterBuilder);
        }

        private void BuildPropGetter(MethodBuilder getter, string name, Type pType) 
        {
            ILGenerator getterIl = getter.GetILGenerator();

            getterIl.Emit(OpCodes.Ldarg_0); // Load reference to this.
            getterIl.Emit(OpCodes.Ldstr, name); // Load name
            getterIl.Emit(OpCodes.Call, ProxBaseGetVal);

            if (pType.IsValueType)
                getterIl.Emit(OpCodes.Unbox_Any, pType); // Unbox the value type
            else
                getterIl.Emit(OpCodes.Castclass, pType); // Cast to specified type

            getterIl.Emit(OpCodes.Ret);
        }

        private void BuildPropSetter(MethodBuilder setter, string name, Type pType)
        {
            ILGenerator setterIl = setter.GetILGenerator();
            setterIl.Emit(OpCodes.Ldarg_0); // Load reference to this.

            // Could not find a way to have .NET help resolve the type of this property
            // The code below loads a token from metadata available in the context of this builder,
            // but the actuall context of the PropertySet function in the object at runtime, of course
            // does not have access to this metadata.
            //setterIl.Emit(OpCodes.Ldarg_0); // Load reference to this for use by the LdToken call.
            //setterIl.Emit(OpCodes.Ldtoken, pType); // Load the RunTimeHandle for the referenced type: pType.
            //setterIl.Emit(OpCodes.Call, GetTypeFromHandle); // Convert to an actual instance of System.type

            setterIl.Emit(OpCodes.Ldstr, name); // Load name
            setterIl.Emit(OpCodes.Ldarg_1); // Load the value handed to the set method.

            if (pType.IsValueType)
                setterIl.Emit(OpCodes.Box, pType); // Box the value type

            setterIl.Emit(OpCodes.Call, ProxBaseSetValNT);
            setterIl.Emit(OpCodes.Pop); // Discard the value returned by SetValNT.
            setterIl.Emit(OpCodes.Ret);
        }

        public Type PropertyType => _propertyBuilder.PropertyType;

        //public MethodBuilder GetGetter(Type requiredType) 
        //    => !requiredType.IsAssignableFrom(PropertyType)
        //    ? throw new InvalidOperationException("Types are not compatible")
        //    : _getterBuilder;

        //public MethodBuilder GetSetter(Type requiredType) 
        //    => !PropertyType.IsAssignableFrom(requiredType)
        //    ? throw new InvalidOperationException("Types are not compatible")
        //    : _setterBuilder;
    }
}