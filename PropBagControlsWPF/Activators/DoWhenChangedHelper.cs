using DRM.TypeSafePropertyBag;
using System;
using System.Reflection;



namespace DRM.PropBag.ControlsWPF
{
    public class DoWhenChangedHelper
    {
        public Func<object, EventHandler<PCGenEventArgs>> GetTheDoWhenChangedGenHandlerGetter(PropDoWhenChangedField pdwcf, Type propertyType)
        {
            Type declaringType = pdwcf.DeclaringType ?? throw new ArgumentNullException(nameof(declaringType));

            string methodName = pdwcf.MethodName ?? throw new ArgumentNullException(nameof(methodName));

            GetGenHandlerRefDelegate GenHandlerRefGetter = GetTheGetGenHandlerDelegate(propertyType);

            Func<object, EventHandler<PCGenEventArgs>> genHandlerGetter = GetTheDoWhenChangedDelegate;
            return genHandlerGetter;

            EventHandler<PCGenEventArgs> GetTheDoWhenChangedDelegate(object host)
            {
                EventHandler<PCGenEventArgs> result = GenHandlerRefGetter(host, declaringType, methodName);
                return result;
            }
        }

        #region Helper Methods for the Generic Method Templates

        static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        // Delegate declarations.
        private delegate EventHandler<PCGenEventArgs> GetGenHandlerRefDelegate(object owningInstance, Type ownerType, string methodName);

        private static GetGenHandlerRefDelegate GetTheGetGenHandlerDelegate(Type propertyType)
        {
            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetGenHandlerRefDelegate", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(propertyType);
            GetGenHandlerRefDelegate result = (GetGenHandlerRefDelegate)Delegate.CreateDelegate(typeof(GetGenHandlerRefDelegate), methInfoGetProp);

            return result;
        }

        #endregion

        #region Generic Method Templates

        static class GenericMethodTemplates
        {
            private static EventHandler<PCGenEventArgs> GetGenHandlerRefDelegate<T>(object owningInstance, Type ownerType, string methodName)
            {
                MethodInfo mi = ownerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

                if (!IsDuoAction<T>(mi)) return null;

                EventHandler<PCGenEventArgs> del = (EventHandler<PCGenEventArgs>)Delegate.CreateDelegate(typeof(EventHandler<PCGenEventArgs>), owningInstance, mi);

                return del;
            }

            /////private delegate EventHandler<PCGenEventArgs> GetGenHandlerRefDelegate(object owningInstance, Type ownerType, string methodName);


            //        // With No Value
            //        private static IProp<T> CreatePropWithNoValue<T>(IPropFactory propFactory,
            //            PropNameType propertyName, object extraInfo,
            //            bool hasStorage, bool isTypeSolid,
            //            EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
            //        {
            //            EventHandler<PCTypedEventArgs<T>> doWhenChangedX = GetTypedDoWhenChanged<T>(doWhenChanged);

            //            return propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, isTypeSolid,
            //                doWhenChangedX, doAfterNotify, GetComparerForProps<T>(comparer, propFactory, useRefEquality));
            //        }

            //        public delegate object CreatePropWithNoValueDelegate(IPropFactory propFactory,
            //string propertyName, object extraInfo,
            //bool hasStorage, bool isTypeSolid,
            //EventHandler<PCGenEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality);


            static private bool IsDuoAction<T>(MethodInfo mi)
            {
                if (mi.ReturnType != typeof(void))
                {
                    // Must return void.
                    return false;
                }

                ParameterInfo[] parms = mi.GetParameters();

                if ((parms.Length != 2) || (parms[0].ParameterType != typeof(object)) || (parms[1].ParameterType != typeof(PCGenEventArgs)))
                {
                    // Must have two parameters of the specified type.
                    return false;
                }
                return true;
            }

        }

        //static class GenericMethodTemplates2
        //{

        //    private static Delegate GetDoWhenChangedXDelegate<T>(object owningInstance, Type ownerType, string methodName)
        //    {
        //        //EventHandler<PropertyChangedWithTValsEventArgs<T>> x = new EventHandler<PropertyChangedWithTValsEventArgs<T>>(Sa);
                
        //        MethodInfo mi = ownerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

        //        if (!IsDuoAction<T>(mi)) return null;

        //        EventHandler<PCTypedEventArgs<T>> del
        //            = (EventHandler<PCTypedEventArgs<T>>)
        //            Delegate.CreateDelegate(typeof(EventHandler<PCTypedEventArgs<T>>), owningInstance, mi);

        //        return del;
        //    }


        //    static private bool IsDuoAction<T>(MethodInfo mi)
        //    {
        //        if (mi.ReturnType != typeof(void))
        //        {
        //            // Must return void.
        //            return false;
        //        }

        //        ParameterInfo[] parms = mi.GetParameters();

        //        if ((parms.Length != 2) || (parms[0].ParameterType != typeof(T)) || (parms[1].ParameterType != typeof(T)))
        //        {
        //            // Must have two parameters of the specified type.
        //            return false;
        //        }
        //        return true;
        //    }

        //}

        #endregion

        // TOOD: Update the .props.tt T4 Template with this.
        private EventHandler<PCTypedEventArgs<T>> GetDelegate<T>(string methodName)
        {
            Type pp = this.GetType();
            MethodInfo mi = pp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (mi == null) return null;

            EventHandler<PCTypedEventArgs<T>> result = (EventHandler<PCTypedEventArgs<T>>)mi.CreateDelegate(typeof(EventHandler<PCTypedEventArgs<T>>), this);

            return result;
        }
    }
}
