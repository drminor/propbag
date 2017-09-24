using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace DRM.TypeSafePropertyBag
{
    public static class TypeExtension
    {
        public static bool IsInterface(this Type type) => type.GetTypeInfo().IsInterface;

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type) => type.GetTypeInfo().DeclaredMethods;

        public static MethodInfo GetDeclaredMethod(this Type type, string name) => type.GetAllMethods().FirstOrDefault(mi => mi.Name == name);

        public static MethodInfo GetDeclaredMethod(this Type type, string name, Type[] parameters) =>
                type.GetAllMethods().Where(mi => mi.Name == name).MatchParameters(parameters);

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type) => type.GetTypeInfo().DeclaredConstructors;

        public static ConstructorInfo GetDeclaredConstructor(this Type type, Type[] parameters) =>
            type.GetDeclaredConstructors().MatchParameters(parameters);

        public static IEnumerable<MethodInfo> GetAllMethods(this Type type) => type.GetRuntimeMethods();

        private static TMethod MatchParameters<TMethod>(this IEnumerable<TMethod> methods, Type[] parameters) where TMethod : MethodBase =>
            methods.FirstOrDefault(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameters));

        public static Type BaseType(this Type type) => type.GetTypeInfo().BaseType;

        public static bool IsGenericType(this Type type) => type.GetTypeInfo().IsGenericType;

        //public static bool IsPublic(this MemberInfo memberInfo) => (memberInfo as FieldInfo)?.IsPublic ?? (memberInfo as PropertyInfo).IsPublic();

        public static bool IsValueType(this Type type) => type.GetTypeInfo().IsValueType;

        public static IEnumerable<Type> GetTypeInheritance(this Type type)
        {
            yield return type;

            var baseType = type.BaseType();
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType();
            }
        }

    }

}
