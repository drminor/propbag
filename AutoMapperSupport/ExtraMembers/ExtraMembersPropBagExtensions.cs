using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport
{
    public static class ExtraMembersPropBagExtensions
    {
        /// <summary>
        /// Creates property get and set accessors for the specified Type using the 
        /// information from the registered properties of this IPropBag instance.
        /// The type must derive from IPropBag
        /// </summary>
        /// <typeparam name="T">The Type that will receive the new get and set accessors.</typeparam>
        /// <param name="propBag">The PropBag intance from which to get the property information.</param>
        /// <returns>List of System.Reflection.MethodInfo records.</returns>
        //public static IEnumerable<MemberInfo> BuildPropertyInfoList<T>(this IPropBag propBag) where T: IPropBag
        //{
        //    ICollection<MemberInfo> result = new List<MemberInfo>();

        //    Type targetType = typeof(T);

        //    foreach (KeyValuePair<string, ValPlusType> kvp in propBag.GetAllPropNamesAndTypes())
        //    {
        //        string propertyName = kvp.Key;
        //        Type propertyType = kvp.Value.Type;

        //        //Func<object, object> getter =
        //        //    new Func<object, object>((host) => ((IPropBag)host).GetIt(propertyName, propertyType));

        //        //MethodInfo getterMethodInfo = typeof(IPropBag).GetMethod("GetIt", BindingFlags.Public | BindingFlags.Instance);

        //        //MethodInfo getter = typeof(PBDispatcher).GetMethod("GetValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        //        //Action<object,object> setter =
        //        //    new Action<object, object>((host, value) => ((IPropBag)host).SetItWithType(value, propertyType, propertyName));

        //        //MethodInfo setterMethodInfo = typeof(IPropBag).GetMethod("SetItWithType", BindingFlags.Public | BindingFlags.Instance);

        //        //MethodInfo setter = typeof(PBDispatcher).GetMethod("SetValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        //        //AutoMapper.ProxyPropertyInfo pi = new AutoMapper.ProxyPropertyInfo(propertyName, propertyType, targetType,
        //        //    AutoMapper.ProxyPropHelpers.GetGetter(getterMethodInfo), AutoMapper.ProxyPropHelpers.GetSetter(setterMethodInfo));


        //        MethodInfo getter = typeof(IPropBag).GetMethod("GetValueGen");
        //        MethodInfo setter = typeof(IPropBag).GetMethod("SetValueGen");

        //        // TODO: Revive Custom Automapper Form
        //        //ProxyPropertyInfo pi = new ProxyPropertyInfo(propertyName, propertyType, typeof(T),
        //        //    propBag, getter, setter);


        //        //ProxyPropertyInfo pi = new ProxyPropertyInfo(propertyName, propertyType, targetType,
        //        //    getter, setter);

        //        //result.Add(pi);

        //   }

        //    return result;
        //}

        //public static IEnumerable<ProxyPropertyInfo> BuildPropertyInfoList2<T>(this IPropBag propBag) where T : IPropBag
        //{
        //    ICollection<ProxyPropertyInfo> result = new List<ProxyPropertyInfo>();

        //    Type targetType = typeof(T);

        //    foreach (KeyValuePair<string, ValPlusType> kvp in propBag.GetAllPropNamesAndTypes())
        //    {
        //        string propertyName = kvp.Key;
        //        Type propertyType = kvp.Value.Type;

        //        Func<object, object> getter =
        //            new Func<object, object>((host) => ((IPropBag)host).GetIt(propertyName, propertyType));

        //        Action<object, object> setter =
        //            new Action<object, object>((host, value) => ((IPropBag)host).SetItWithType(value, propertyType, propertyName));

        //        ProxyPropertyInfo pi = new ProxyPropertyInfo(propertyName, propertyType, targetType,
        //            getter, setter);

        //        result.Add(pi);
        //    }

        //    return result;
        //}
    }
}
