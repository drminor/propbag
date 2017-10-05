using DRM.TypeSafePropertyBag;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DRM.PropBag.ViewModelBuilder
{


    public class PropertyEmitter
    {
        private static readonly MethodInfo BaseGetItGen = 
            typeof(ITypeSafePropBag).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "GetIt");

        private static readonly MethodInfo BaseSetItGen =
            typeof(ITypeSafePropBag).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "SetIt" && m.IsGenericMethod && m.GetParameters().Length == 2);

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

            CustomAttributeBuilder attributeBuilder = new AttributeEmitter().CreateAttributeBuilder(new WasEmittedAttribute(DateTime.Now.ToString()));
            _propertyBuilder.SetCustomAttribute(attributeBuilder);

        }

        private void BuildPropGetter(MethodBuilder getter, string name, Type pType)
        {
            MethodInfo typedGetIt = BaseGetItGen.MakeGenericMethod(new Type[] { pType });
            ILGenerator getterIl = getter.GetILGenerator();

            getterIl.Emit(OpCodes.Ldarg_0); // Load reference to this.
            getterIl.Emit(OpCodes.Ldstr, name); // Load name
            getterIl.Emit(OpCodes.Call, typedGetIt);
            getterIl.Emit(OpCodes.Ret);
        }

        private void BuildPropSetter(MethodBuilder setter, string name, Type pType)
        {
            MethodInfo typedSetIt = BaseSetItGen.MakeGenericMethod(new Type[] { pType });

            ILGenerator setterIl = setter.GetILGenerator();
            setterIl.Emit(OpCodes.Ldarg_0); // Load reference to this.
            setterIl.Emit(OpCodes.Ldarg_1); // Load the value handed to the set method.
            setterIl.Emit(OpCodes.Ldstr, name); // Load name

            setterIl.Emit(OpCodes.Call, typedSetIt);
            setterIl.Emit(OpCodes.Pop); // Discard the value returned by SetIt<T>
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