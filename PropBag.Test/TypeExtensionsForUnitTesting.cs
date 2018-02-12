using NUnit.Framework;
using System;
using System.Reflection;

namespace PropBagLib.Tests
{
    public static class TypeExtensionsForUnitTesting
    {
        public static string GetTestName(this Type tt)
        {
            //IEnumerable<Attribute> attributes = tt.GetCustomAttributes();
            //TestFixtureAttribute tfa = tt.GetCustomAttribute<TestFixtureAttribute>(true);
            //string name = tfa.TestName;

            string name = tt.GetCustomAttribute<TestFixtureAttribute>(true)?.TestName ?? "TestFixtureName was not provided";
            return name;
        }
    }
}
