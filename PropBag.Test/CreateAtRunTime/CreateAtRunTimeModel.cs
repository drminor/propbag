using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using System.Threading;
//using System.Runtime.CompilerServices;
using System.Reflection;

using DRM.Inpcwv;
using DRM.PropBag;
using DRM.PropBag.ClassGenerator;
using DRM.PropBag.ControlModel;

namespace PropBagLib.Tests
{
    public class CreateAtRunTimeModel : PropBag
    {

        public CreateAtRunTimeModel() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered, null) { }

		public CreateAtRunTimeModel(PropBagTypeSafetyMode typeSafetyMode) : this(typeSafetyMode, null) { }

        public CreateAtRunTimeModel(PropBagTypeSafetyMode typeSafetyMode, AbstractPropFactory factory)
            : base(typeSafetyMode, factory) { }

        public CreateAtRunTimeModel(PropModel pm) : base(pm)
        {

        }

        //public void RegisterProps(PropModel pm)
        //{
        //    IProp<string> p = ThePropFactory.Create<string>("First string.", "First");
        //    AddProp<string>("PropName", p);
        //}

        /// <summary>
        /// If the delegate exists, the original name is returned,
        /// otherwise null is returned.
        /// </summary>
        /// <param name="methodName">Some public or non-public instance method in this class.</param>
        /// <returns>The name, unchanged, if the method exists, otherwise null.</returns>
        private Action<T, T> GetDelegate<T>(string methodName)
        {
            Type pp = this.GetType();
            MethodInfo mi = pp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (mi == null) return null;

            Action<T, T> result = (Action<T, T>)mi.CreateDelegate(typeof(Action<T, T>), this);

            return result;
        }
    }
}
