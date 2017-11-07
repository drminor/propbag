using DRM.TypeSafePropertyBag;
using System;
using System.Reflection;

namespace DRM.PropBag.ControlsWPF
{
    public class DoWhenChangedHelper
    {
        public Func<object, EventHandler<PropertyChangedWithValsEventArgs>> GetTheDoWhenChangedActionGetter(PropDoWhenChangedField pdwcf, Type propertyType)
        {
            Type declaringType = pdwcf.DeclaringType ?? throw new ArgumentNullException(nameof(declaringType));

            string methodName = pdwcf.MethodName ?? throw new ArgumentNullException(nameof(methodName));

            GetActionRefDelegate ActionGetter = GetTheGetActionRefDelegate(propertyType);

            Func<object, EventHandler<PropertyChangedWithValsEventArgs>> actionGetter = GetTheDoWhenChangedDelegate;
            return actionGetter;

            EventHandler<PropertyChangedWithValsEventArgs> GetTheDoWhenChangedDelegate(object host)
            {
                EventHandler<PropertyChangedWithValsEventArgs> result = ActionGetter(host, declaringType, methodName);
                return result;
            }
        }

        #region Helper Methods for the Generic Method Templates

        static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        // Delegate declarations.
        private delegate EventHandler<PropertyChangedWithValsEventArgs> GetActionRefDelegate(object owningInstance, Type ownerType, string methodName);

        private static GetActionRefDelegate GetTheGetActionRefDelegate(Type propertyType)
        {
            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetActionRefDelegate", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(propertyType);
            GetActionRefDelegate result = (GetActionRefDelegate)Delegate.CreateDelegate(typeof(GetActionRefDelegate), methInfoGetProp);

            return result;
        }

        #endregion

        #region Generic Method Templates

        static class GenericMethodTemplates
        {
            private static Delegate GetActionRefDelegate<T>(object owningInstance, Type ownerType, string methodName)
            {
                MethodInfo mi = ownerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

                if (!IsDuoAction<T>(mi)) return null;

                Action<T, T> del = (Action<T, T>)Delegate.CreateDelegate(typeof(Action<T, T>), owningInstance, mi);

                return del;
            }


            static private bool IsDuoAction<T>(MethodInfo mi)
            {
                if (mi.ReturnType != typeof(void))
                {
                    // Must return void.
                    return false;
                }

                ParameterInfo[] parms = mi.GetParameters();

                if ((parms.Length != 2) || (parms[0].ParameterType != typeof(T)) || (parms[1].ParameterType != typeof(T)))
                {
                    // Must have two parameters of the specified type.
                    return false;
                }
                return true;
            }

        }

        static class GenericMethodTemplates2
        {

            private static Delegate GetDoWhenChangedXDelegate<T>(object owningInstance, Type ownerType, string methodName)
            {
                //EventHandler<PropertyChangedWithTValsEventArgs<T>> x = new EventHandler<PropertyChangedWithTValsEventArgs<T>>(Sa);
                
                MethodInfo mi = ownerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

                if (!IsDuoAction<T>(mi)) return null;

                EventHandler<PropertyChangedWithTValsEventArgs<T>> del
                    = (EventHandler<PropertyChangedWithTValsEventArgs<T>>)
                    Delegate.CreateDelegate(typeof(EventHandler<PropertyChangedWithTValsEventArgs<T>>), owningInstance, mi);

                return del;
            }


            static private bool IsDuoAction<T>(MethodInfo mi)
            {
                if (mi.ReturnType != typeof(void))
                {
                    // Must return void.
                    return false;
                }

                ParameterInfo[] parms = mi.GetParameters();

                if ((parms.Length != 2) || (parms[0].ParameterType != typeof(T)) || (parms[1].ParameterType != typeof(T)))
                {
                    // Must have two parameters of the specified type.
                    return false;
                }
                return true;
            }

        }

        #endregion

        // TOOD: Update the .props.tt T4 Template with this.
        private EventHandler<PropertyChangedWithTValsEventArgs<T>> GetDelegate<T>(string methodName)
        {
            Type pp = this.GetType();
            MethodInfo mi = pp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (mi == null) return null;

            EventHandler<PropertyChangedWithTValsEventArgs<T>> result = (EventHandler<PropertyChangedWithTValsEventArgs<T>>)mi.CreateDelegate(typeof(EventHandler<PropertyChangedWithTValsEventArgs<T>>), this);

            return result;
        }
    }
}
