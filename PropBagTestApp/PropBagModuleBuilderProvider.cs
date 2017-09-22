using System;
using System.Threading;
using System.Reflection.Emit;
using System.Reflection;
using DRM.TypeSafePropertyBag;

namespace DRM.WrapperGenLib
{

    public class PropBagModuleBuilderProvider : IModuleBuilderInfoProvider
    {
        // This is executed once, upon first access to this class.
        private static Lazy<MyBuilderInfo> _myBuilderInfo = new Lazy<MyBuilderInfo>(() => new MyBuilderInfo(), LazyThreadSafetyMode.ExecutionAndPublication);

        // This returns the result of creating a new MyBuilderInfo, which occurs at most, 1 time.
        public IModuleBuilderInfo ModuleBuilderInfo => _myBuilderInfo.Value;

        private class MyBuilderInfo : IModuleBuilderInfo
        {
            //private static readonly byte[] PRIVATE_KEY =
            //    StringToByteArray(
            //        "123456789ABCDEF");

            //private static readonly byte[] PRIVATE_KEY_TOKEN = StringToByteArray("123");

            static LockingConcurrentDictionary<TypeDescription, Type> _emiitedTypes;

            private static readonly string ASSEMBLY_NAME = "PropBagWrappers";
            private static readonly string MODULE_NAME = "Module_PropBagWrappers";

            private static Lazy<int> _assemblyId;
            public int AssemblyId => _assemblyId.Value;

            private static Lazy<AssemblyName> _assemblyName;
            public AssemblyName AssemblyName => _assemblyName.Value;

            private static Lazy<AssemblyBuilder> _assemblyBuilder;
            public AssemblyBuilder AssemblyBuilder => _assemblyBuilder.Value;

            private static Lazy<ModuleBuilder> _moduleBuilder;
            public ModuleBuilder ModuleBuilder => _moduleBuilder.Value;

            public Type GetWrapperType(TypeDescription td)
            {
                Type tt = _emiitedTypes.GetOrAdd(td);
                return tt;
            }

            public MyBuilderInfo()
            {
            }

            static MyBuilderInfo()
            {
                _assemblyId = new Lazy<int>(() => AssemblyIdIssuer.NextModuleId, LazyThreadSafetyMode.ExecutionAndPublication);
                _assemblyName = new Lazy<AssemblyName>(() => GetAssemblyName(ASSEMBLY_NAME), LazyThreadSafetyMode.ExecutionAndPublication);
                _assemblyBuilder = new Lazy<AssemblyBuilder>(() => GetAssemblyBuilder(_assemblyName.Value));
                _moduleBuilder = new Lazy<ModuleBuilder>(() => GetModuleBuilder(_assemblyBuilder.Value, MODULE_NAME), LazyThreadSafetyMode.ExecutionAndPublication);
                _emiitedTypes = new LockingConcurrentDictionary<TypeDescription, Type>(BuildWrapperType);
            }

            private static Type BuildWrapperType(TypeDescription td)
            {
                IModuleBuilderInfo mbInfo = new PropBagModuleBuilderProvider().ModuleBuilderInfo;
                Type emittedType = WrapperGenerator<InstanceWrapperBase>.EmitWrapper(td, mbInfo);
                return emittedType;
            }

            private static AssemblyName GetAssemblyName(string assemblyName)
            {
                AssemblyName result = new AssemblyName(assemblyName);
                //result.SetPublicKey(privateKey);
                //result.SetPublicKeyToken(privateKeyToken);
                return result;
            }

            private static AssemblyBuilder GetAssemblyBuilder(AssemblyName assemblyName)
            {
                return AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            }

            private static ModuleBuilder GetModuleBuilder(AssemblyBuilder assemblyBuilder, string moduleName)
            {
                return assemblyBuilder.DefineDynamicModule(moduleName);
            }

            private static byte[] StringToByteArray(string hex)
            {
                int numberChars = hex.Length;
                byte[] bytes = new byte[numberChars / 2];
                for (int i = 0; i < numberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            }
        }
    }

}
