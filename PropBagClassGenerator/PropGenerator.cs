using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using DRM.PropBag;
using DRM.PropBagModel;

namespace DRM.PropBagClassGenerator
{
    public class PropGenerator<T>
    {
        static public IProp Create(PropDefRaw def, AbstractPropFactory factory, Type derivedCaller)
        {
            IEqualityComparer<T> comparer = GetComp(def.Comparer, def.UseRefEquality, factory);
            Action<T,T> doWhen = GetDoWhen(def.DoWhenChanged, derivedCaller);

            if (def.HasStore)
            {
                if (def.CreateType == PropCreateMethodEnum.noValue)
                {
                    factory.CreateWithNoValue<T>(def.PropName, def.ExtraInfo, def.HasStore, def.TypeIsSolid, doWhen, def.DoAfterNotify, comparer);
                }
                else
                {
                    T initVal = Convert(def.InitialValue);
                    factory.Create<T>(initVal, def.PropName, def.ExtraInfo, def.HasStore, def.TypeIsSolid, doWhen, def.DoAfterNotify, comparer);
                }
            }
            else
            {
                factory.CreateWithNoValue<T>(def.PropName, def.ExtraInfo, def.HasStore, def.TypeIsSolid, doWhen, def.DoAfterNotify, comparer);
            }
            return null;
        }

        static IEqualityComparer<T> GetComp(string x, bool useRefEquality, AbstractPropFactory factory)
        {
            if (useRefEquality)
                return factory.GetRefEqualityComparer<T>();

            return null;
        }

        static Action<T, T> GetDoWhen(string x, Type d)
        {
            MethodInfo mi = d.GetMethod(x, BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(typeof(T));
            //Type delType = typeof(Action<T,T>)

            Type dd = mi.GetRuntimeBaseDefinition().GetType();

            Action<T, T> result = (Action<T,T>)mi.CreateDelegate(dd);

            return result;
        }

        static T Convert(string x)
        {
            return default(T);
        }



        static public PropDefRaw GetPropDef(PropModel propModel, PropItem pi, bool typeIsSolid = true)
        {
            PropDoWhenChanged doWhenPrepped = propModel.PrepareDoWhenChangedField(pi);

            PropComparerField comparerPrepped = propModel.PrepareComparerField(pi.ComparerField);

            PropCreateMethodEnum creationStyle;
            string initVal = null;

            if (pi.HasStore)
            {
                PropIniialValueField initialValPrepped = propModel.PrepareInitialField(pi);

                if (!initialValPrepped.SetToUndefined)
                {
                    if (initialValPrepped.SetToDefault) // (pi.InitalValueField.SetToDefault)
                    {
                        // Use default value for "we provide storage" implementation.
                        creationStyle = PropCreateMethodEnum.useDefault;
                    }
                    else
                    {
                        // Use the value indicated for "we provide storage" implentation.
                        creationStyle = PropCreateMethodEnum.value;
                        initVal = pi.InitalValueField.InitialValue;
                    }
                }
                else
                {
                    // No value for "we provide storage" implementation: value will be undefined.
                    creationStyle = PropCreateMethodEnum.noValue;
                }
            }
            else
            {
                // No value, for no store implementation.
                creationStyle = PropCreateMethodEnum.noValue;
            }

            return new PropDefRaw(creationStyle, pi.HasStore, typeIsSolid,
                comparerPrepped.UseRefEquality, pi.Type, pi.Name,
                doWhenPrepped.DoWhenChanged, doWhenPrepped.DoAfterNotify,
                comparerPrepped.Comparer, pi.ExtraInfo, initVal);

        }
    }
}
