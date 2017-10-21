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

        public static bool IsPropBagBased(this Type type)
        {
            Type[] interfaces = type.GetInterfaces();

            bool result = null != type.GetInterfaces().FirstOrDefault
                (
                x => x.Name == "IPropBag"
                );

            return result;
        }

        public static bool HasDeclaredProperty(this Type type, string propertyName)
        {
            return null != type.GetTypeInfo().GetDeclaredProperty(propertyName);
        }

        public static PropertyInfo GetDeclaredProperty(this Type type, string propertyName)
        {
            return type.GetTypeInfo().GetDeclaredProperty(propertyName);
        }

        public static MemberInfo FindMember(this Type type, string propertyName)
        {
            MemberInfo[] members = type.GetTypeInfo().GetMembers();

            MemberInfo result = members.FirstOrDefault((x => x.Name == propertyName));

            return result;
        }

        public static FieldInfo GetDeclaredField(this Type type, string fieldName)
        {
            return type.GetTypeInfo().GetDeclaredField(fieldName);
        }

        // TODO: Create a Class Atribute for our WrapperGenerator
        // so that we can more accurately determine this.
        public static bool IsEmittedProxy(this Type type)
        {
            return type.FullName.Contains("1.") || type.FullName.Contains("2.");
        }


    }

}
