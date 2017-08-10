using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Serialization;

using DRM.PropBag;
using DRM.PropBagModel;


namespace DRM.PropBagClassGenerator
{
    public class T4Support
    {

        static public string GetBaseClassName(PropModel propModel)
        {
            return propModel.DeriveFromPubPropBag ? "PubPropBag" : "PropBag";
        }

        static public string GetSafetyModeString(PropModel propModel)
        {
            return propModel.TypeSafetyMode.ToString();
        }

        static public string GetNamespaces(PropModel propModel)
        {
            StringBuilder r = new StringBuilder();

            IList<string> requiredNamespaces = propModel.GetRequiredNamespaces();

            foreach (string s in requiredNamespaces)
            {
                r.AppendLine(string.Format("using {0};", s));
            }

            foreach (string s in propModel.Namespaces)
            {
                r.AppendLine(string.Format("using {0};", s));
            }

            return r.AppendLine().ToString();
        }

        static public string GetAddPropMethodCallText(PropModel propModel, PropItem pi)
        {
            PropDoWhenChanged doWhenPrepped = propModel.PrepareDoWhenChangedField(pi);

            PropComparerField comparerPrepped = propModel.PrepareComparerField(pi.ComparerField);

            // Prepare the AddProp method call
            string formatString;
            object[] vals = new object[] {
                comparerPrepped.UseRefEquality ? "ObjComp" : null,
                null, // will eventually be null, "NoValue" or "NoStore"
                pi.Type,
                pi.Name,
                doWhenPrepped.DoWhenChanged,
                doWhenPrepped.DoAfterNotify ? "true" : "false",
                comparerPrepped.Comparer, "null",
                "null", // Extra Info -- if we ever use it.
                null // Initial Value
            };


            if (pi.HasStore)
            {
                PropIniialValueField initialValPrepped = propModel.PrepareInitialField(pi);

                if (!initialValPrepped.SetToUndefined)
                {
                    // AddProp or AddPropObjComp

                    //  public IProp<T> AddProp<T>(string propertyName, 
                    //      Action<T, T> doIfChanged = null,
                    //      bool doAfterNotify = false,
                    //      IEqualityComparer<T> comparer = null,
                    //      object extraInfo = null,
                    //      T initalValue = default(T))

                    if (initialValPrepped.SetToDefault) // (pi.InitalValueField.SetToDefault)
                    {
                        if (comparerPrepped.UseRefEquality)
                        {
                            formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5})";
                        }
                        else
                        {
                            formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})";
                        }
                    }
                    else
                    {
                        vals[8] = propModel.GetStringRepForValue(initialValPrepped.InitialValue, pi.Type);

                        if (comparerPrepped.UseRefEquality)
                        {
                            formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {7}, {8})";
                        }
                        else
                        {
                            formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6}, {7}, {8})";
                        }
                    }
                }
                else
                {
                    //AddPropNoValue or AddPropObjCompNoValue

                    //  public IProp<T> AddProp<T>(string propertyName, 
                    //      Action<T, T> doIfChanged = null,
                    //      bool doAfterNotify = false,
                    //      IEqualityComparer<T> comparer = null,
                    //      object extraInfo = null,

                    vals[1] = "NoValue";
                    if (comparerPrepped.UseRefEquality)
                    {
                        formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5})";
                    }
                    else
                    {
                        formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})";
                    }
                }
            }
            else
            {
                // AddPropNoStore or AddPropNoStoreObjComp

                //  public void AddPropNoStore<T>(string propertyName,
                //      Action<T, T> doIfChanged,
                //      bool doAfterNotify = false,
                //      IEqualityComparer<T> comparer = null)

                vals[1] = "NoStore";
                if (comparerPrepped.UseRefEquality)
                {
                    formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5})";
                }
                else
                {
                    formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})";
                }
            }

            return string.Format(formatString, vals);
        }


    }
}
