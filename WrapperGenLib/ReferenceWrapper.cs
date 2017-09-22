using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DRM.WrapperGenLib
{
    public class ReferenceWrapper : NamedValueWrapperBase
    {

        private static readonly MethodInfo ProxBaseGetVal =
            typeof(NamedValueWrapperBase).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "GetItWithNoType");

        private static readonly MethodInfo ProxBaseSetValNT =
            typeof(NamedValueWrapperBase).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "SetItWithNoType");

        Type sType;

        public ReferenceWrapper(Dictionary<string, Type> typeDefs) : base(typeDefs)
        {
            sType = typeof(string);
        }

        public string GetPropString(MethodBuilder getter, string name)
        {
            return (string)base.GetVal(name);
            //object val = base.GetVal(name);
            //string rVal = (string)val;
            //return rVal;
        }

        //public void SetPropString(MethodBuilder setter, string name, Type pType, object value)
        //{
        //    base.SetItWithType(value, pType, name);
        //}

        //public void SetPropString2(MethodBuilder setter, string name, object value)
        //{
        //    base.SetItWithType(value, sType, name);
        //}

        //string _propString = "dummy";
        public string PropString
        {
            get
            {
                string _ourName = "PropString";
                Type _ourType = this.PropString.GetType();

                //object val = ProxBaseGetVal.Invoke(null, new object[] { _ourName });

                //ValPlusType r = NamedValuesWithType[_ourName];
                //object val = r.Value;
                object val = base.GetVal(_ourName);
                string rVal = (string)val;
                return rVal;

                //string _ourName = "PropString";
                //return (string) base.NamedValuesWithType[_ourName].Value;
            }
            set
            {
                string _ourName = "PropString";
                //Type _ourType = this._propString.GetType();

                //base.SetItWithType(value, _ourType, _ourName);

                //ProxBaseSetVal.Invoke(null, new object[] { _ourName, value, _ourType });
                //ValPlusType n = new ValPlusType(value, _ourType);
                base.SetVal(_ourName, value);


            }
        }

        public string PropString2
        {
            get
            {
                return (string) base.GetVal("PropString");
            }
            set
            {
                Type tt = PropString2.GetType();
                base.SetVal("PropString2", value);
            }
        }

        public int PropInt
        {
            get
            {
                string _ourName = "PropInt";
                object val = base.GetVal(_ourName);
                int rVal = (int)val;
                return rVal;
            }
            set
            {
                string _ourName = "PropInt";
                Type _ourType = typeof(Int32);
                base.SetVal(_ourName, value);
            }
        }
    }
}
