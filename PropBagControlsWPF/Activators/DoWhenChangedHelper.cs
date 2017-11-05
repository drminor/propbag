using System;
using System.Reflection;

namespace DRM.PropBag.ControlsWPF
{
    public class DoWhenChangedHelper
    {
        public Func<object, Delegate> GetTheDoWhenChangedActionGetter(PropDoWhenChangedField pdwcf, Type propertyType)
        {
            Type declaringType = pdwcf.DeclaringType ?? throw new ArgumentNullException(nameof(declaringType));

            string methodName = pdwcf.MethodName ?? throw new ArgumentNullException(nameof(methodName));

            GetActionRefDelegate ActionGetter = GetTheGetActionRefDelegate(propertyType);

            Func<object, Delegate> actionGetter = GetTheDoWhenChangedDelegate;
            return actionGetter;

            Delegate GetTheDoWhenChangedDelegate(object host)
            {
                Delegate result = ActionGetter(host, declaringType, methodName);
                return result;
            }
        }

        #region Helper Methods for the Generic Method Templates

        static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        // Delegate declarations.
        private delegate Delegate GetActionRefDelegate(object owningInstance, Type ownerType, string methodName);

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

        #endregion
    }
}
