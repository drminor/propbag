using DRM.TypeSafePropertyBag;
using System;
using System.Reflection;

namespace DRM.PropBag.ControlsWPF
{
    public class DoWhenChangedHelper
    {
        //public Func<object, EventHandler<PcGenEventArgs>> GetTheDoWhenChangedGenHandlerGetter(PropDoWhenChangedField pdwcf, Type propertyType)
        //{
        //    Type declaringType = pdwcf.DeclaringType ?? throw new ArgumentNullException(nameof(declaringType));

        //    string methodName = pdwcf.MethodName ?? throw new ArgumentNullException(nameof(methodName));

        //    GetGenHandlerRefDelegate GenHandlerRefGetter = GetTheGetGenHandlerDelegate(propertyType);

        //    // Return a function that takes a single input object and returns a reference to the clients EventHandler action.
        //    // The caller does not need to supply the declaringType, nor the methodName, since these are enclosed in the 
        //    // new custom function.
        //    return GetTheDoWhenChangedDelegate;

        //    // The declaring type and method name are being enclosed in this nested function,
        //    // which calls the custom delegate: 'GenHandlerRefGetter' just created (or read from the cache.)
        //    EventHandler<PcGenEventArgs> GetTheDoWhenChangedDelegate(object host)
        //    {
        //        EventHandler<PcGenEventArgs> result = GenHandlerRefGetter(host, declaringType, methodName);
        //        return result;
        //    }
        //}

        public MethodInfo GetMethodAndSubKind(PropDoWhenChangedField pdwcf, Type propertyType, string propertyName, out SubscriptionKind subscriptionKind)
        {
            MethodInfo mi = GetMethodInfo(pdwcf.DeclaringType, pdwcf.MethodName);

            if(HasPcGenEventHandlerSignature(mi))
            {
                subscriptionKind = SubscriptionKind.GenHandler;
                return mi;
            }
            else if(HasPcTypedEventHandlerSignature(mi))
            {
                subscriptionKind = SubscriptionKind.TypedHandler;
                return mi;
            }
            else
            {
                throw new ArgumentException($"The method: {mi.Name} on class: {mi.DeclaringType.FullName} doesn't match any of the supported signatures.");
            }
        }

        private MethodInfo GetMethodInfo(Type ownerType, string methodName)
        {
            MethodInfo mi = ownerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            return mi;
        }


        private bool HasPcGenEventHandlerSignature(MethodInfo mi)
        {
            if (mi.ReturnType != typeof(void))
            {
                // Must return void.
                return false;
            }

            ParameterInfo[] parms = mi.GetParameters();

            if ((parms.Length != 2) || (parms[0].ParameterType != typeof(object)) || (parms[1].ParameterType != typeof(PcGenEventArgs)))
            {
                // Must have signature of void X(object sender, PcGenEventArgs e)
                return false;
            }
            return true;
        }

        private bool HasPcTypedEventHandlerSignature(MethodInfo mi)
        {
            if (mi.ReturnType != typeof(void))
            {
                // Must return void.
                return false;
            }

            ParameterInfo[] parms = mi.GetParameters();

            if ((parms.Length != 2) || (parms[0].ParameterType != typeof(object))) // || (parms[1].ParameterType != typeof(PcGenEventArgs)))
            {
                // Must have signature of void X(object sender, PcTypedEventArgs<T>Has e)
                return false;
            }
            return true;
        }

        //#region Helper Methods for the Generic Method Templates

        //static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        //// Delegate declarations.
        //private delegate EventHandler<PcGenEventArgs> GetGenHandlerRefDelegate(object owningInstance, Type ownerType, string methodName);

        //private static GetGenHandlerRefDelegate GetTheGetGenHandlerDelegate(Type propertyType)
        //{
        //    MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetGenHandlerRefDelegate", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(propertyType);

        //    // TODO: Cache these delegates.
        //    GetGenHandlerRefDelegate result = (GetGenHandlerRefDelegate)Delegate.CreateDelegate(typeof(GetGenHandlerRefDelegate), methInfoGetProp);

        //    return result;
        //}

        //#endregion

        //#region Generic Method Templates

        //static class GenericMethodTemplates
        //{
        //    private static EventHandler<PcGenEventArgs> GetGenHandlerRefDelegate<T>(object owningInstance, Type ownerType, string methodName)
        //    {
        //        MethodInfo mi = ownerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

        //        if (!IsDuoAction<T>(mi)) return null;

        //        EventHandler<PcGenEventArgs> del = (EventHandler<PcGenEventArgs>)Delegate.CreateDelegate(typeof(EventHandler<PcGenEventArgs>), owningInstance, mi);

        //        return del;
        //    }

        //    /////private delegate EventHandler<PCGenEventArgs> GetGenHandlerRefDelegate(object owningInstance, Type ownerType, string methodName);


        //    //        // With No Value
        //    //        private static IProp<T> CreatePropWithNoValue<T>(IPropFactory propFactory,
        //    //            PropNameType propertyName, object extraInfo,
        //    //            bool hasStorage, bool isTypeSolid,
        //    //            EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        //    //        {
        //    //            EventHandler<PCTypedEventArgs<T>> doWhenChangedX = GetTypedDoWhenChanged<T>(doWhenChanged);

        //    //            return propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, isTypeSolid,
        //    //                doWhenChangedX, doAfterNotify, GetComparerForProps<T>(comparer, propFactory, useRefEquality));
        //    //        }

        //    //        public delegate object CreatePropWithNoValueDelegate(IPropFactory propFactory,
        //    //string propertyName, object extraInfo,
        //    //bool hasStorage, bool isTypeSolid,
        //    //EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality);


        //    static private bool IsDuoAction<T>(MethodInfo mi)
        //    {
        //        if (mi.ReturnType != typeof(void))
        //        {
        //            // Must return void.
        //            return false;
        //        }

        //        ParameterInfo[] parms = mi.GetParameters();

        //        if ((parms.Length != 2) || (parms[0].ParameterType != typeof(object)) || (parms[1].ParameterType != typeof(PcGenEventArgs)))
        //        {
        //            // Must have two parameters of the specified type.
        //            return false;
        //        }
        //        return true;
        //    }

        //}

        ////static class GenericMethodTemplates2
        ////{

        ////    private static Delegate GetDoWhenChangedXDelegate<T>(object owningInstance, Type ownerType, string methodName)
        ////    {
        ////        //EventHandler<PropertyChangedWithTValsEventArgs<T>> x = new EventHandler<PropertyChangedWithTValsEventArgs<T>>(Sa);
                
        ////        MethodInfo mi = ownerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

        ////        if (!IsDuoAction<T>(mi)) return null;

        ////        EventHandler<PCTypedEventArgs<T>> del
        ////            = (EventHandler<PCTypedEventArgs<T>>)
        ////            Delegate.CreateDelegate(typeof(EventHandler<PCTypedEventArgs<T>>), owningInstance, mi);

        ////        return del;
        ////    }


        ////    static private bool IsDuoAction<T>(MethodInfo mi)
        ////    {
        ////        if (mi.ReturnType != typeof(void))
        ////        {
        ////            // Must return void.
        ////            return false;
        ////        }

        ////        ParameterInfo[] parms = mi.GetParameters();

        ////        if ((parms.Length != 2) || (parms[0].ParameterType != typeof(T)) || (parms[1].ParameterType != typeof(T)))
        ////        {
        ////            // Must have two parameters of the specified type.
        ////            return false;
        ////        }
        ////        return true;
        ////    }

        ////}

        //#endregion

        //private EventHandler<PCTypedEventArgs<T>> GetDelegate<T>(string methodName)
        //{
        //    Type pp = this.GetType();
        //    MethodInfo mi = pp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        //    if (mi == null) return null;

        //    EventHandler<PCTypedEventArgs<T>> result = (EventHandler<PCTypedEventArgs<T>>)mi.CreateDelegate(typeof(EventHandler<PCTypedEventArgs<T>>), this);

        //    return result;
        //}
    }
}
