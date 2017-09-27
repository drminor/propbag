using System;
using System.Threading;
using System.Reflection.Emit;
using System.Reflection;

using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.ViewModelBuilder
{

    public class DefaultModuleBuilderInfoProvider : IModuleBuilderInfoProvider
    {
        // This is executed once, upon first access to this class.
        private static Lazy<MyBuilderInfo> _myBuilderInfo = new Lazy<MyBuilderInfo>(() => new MyBuilderInfo(), LazyThreadSafetyMode.ExecutionAndPublication);

        // This returns the result of creating a new MyBuilderInfo, which occurs at most, 1 time.
        public IModuleBuilderInfo ModuleBuilderInfo => _myBuilderInfo.Value;

        private class MyBuilderInfo : IModuleBuilderInfo
        {
            //private static readonly byte[] PRIVATE_KEY =
            //    StringToByteArray(
            //        "002400000480000094000000060200000024000052534131000400000100010079dfef85ed6ba841717e154f13182c0a6029a40794a6ecd2886c7dc38825f6a4c05b0622723a01cd080f9879126708eef58f134accdc99627947425960ac2397162067507e3c627992aa6b92656ad3380999b30b5d5645ba46cc3fcc6a1de5de7afebcf896c65fb4f9547a6c0c6433045fceccb1fa15e960d519d0cd694b29a4");

            //private static readonly byte[] PRIVATE_KEY_TOKEN = StringToByteArray("be96cd2c38ef1005");

            static LockingConcurrentDictionary<TypeDescription, Type> _emiitedTypes;

            private static readonly string ASSEMBLY_NAME = "DRM.WrapperGenerator.Wrappers";
            private static readonly string MODULE_NAME = "Module_DRM.WrapperGenerator.Wrappers";

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
                IModuleBuilderInfo mbInfo = new DefaultModuleBuilderInfoProvider().ModuleBuilderInfo;
                Type emittedType = WrapperGenerator.EmitWrapper(td, mbInfo); 
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
#if NET40
            return AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#else
                return AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
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
