using AutoMapper;
using DRM.PropBag.ControlModel;
using DRM.WrapperGenLib;
using System;
using System.Collections.Generic;

using DRM.PropBag.LiveClassGenerator;
using System.Reflection.Emit;
using System.Reflection;
using System.IO;
using DRM.PropBag.ControlsWPF;
using DRM.TypeSafePropertyBag;
using DRM.PropBag;

namespace DRM.AutoMapperSupport
{
    public class PropBagMapper<RegT, PbT> where PbT : IPropBag, new()
    {
        #region Public and Private Members

        // Used to collect all wrapper types for classes that define no value for their outputNamespace,
        // so that they are kept separate from all those classes that do define a namespace.
        public const string DEFAULT_NAMESPACE_NAME = "PropBagWrappers";

        PropModel _propModel;
        Dictionary<string, Type> _propDefs;
        Type _wrapperType;
        IMapper _mapper = null;

        #endregion

        #region Constructors

        public PropBagMapper(string newTypeName, Dictionary<string, Type> propDefs) : this(newTypeName, propDefs, null) { }

        public PropBagMapper(string newTypeName, Dictionary<string, Type> propDefs, Func<object, object> regularInstanceCreator)
        {
            _propModel = null;
            // TODO: NOTE: No special handling is performed here for new Type names that 
            // have no namespace as do the other constructor.
            TypeName typeName = new TypeName(newTypeName);
            IEnumerable<PropertyDescription> propDescs = GetPropertyDescriptions(propDefs);
            TypeDescription typeDescription = new TypeDescription(typeName, propDescs);

            _wrapperType = CreateWrapper(typeDescription);

            var mc = CreateMapperConfig(_wrapperType, regularInstanceCreator);
            _mapper = mc.CreateMapper();
        }

        public PropBagMapper(BoundPropBag boundPB) : this(boundPB, null, null) { }

        public PropBagMapper(BoundPropBag boundPB, IModuleBuilderInfo moduleBuilderInfo) : this(boundPB, moduleBuilderInfo, null) { }

        public PropBagMapper(BoundPropBag boundPB, IModuleBuilderInfo moduleBuilderInfo, Func<object, object> regularInstanceCreator)
        {
            _propModel = boundPB.PropModel;
            // Use the name of the class that dervives from IPropBag
            // as the basis of the name of the new wrapper type.
            TypeDescription typeDescription = BuildTypeDesc(boundPB.DerivingType.Name, boundPB.PropModel, out _propDefs);

            boundPB.WrapperType = CreateWrapper(typeDescription, moduleBuilderInfo);
            _wrapperType = boundPB.WrapperType;

            var mc = CreateMapperConfig(_wrapperType, regularInstanceCreator);
            _mapper = mc.CreateMapper();
        }

        #endregion

        #region Construction Support

        private TypeDescription BuildTypeDesc(string className, PropModel pm, out Dictionary<string, Type> propDefs)
        {
            //string assemblyName = WRAPPER_MODULE_ASSEMBLY_NAME;
            string namespaceName = pm.NamespaceName ?? DEFAULT_NAMESPACE_NAME;

            TypeName tn = new TypeName(className, namespaceName); //, assemblyName);

            propDefs = pm.GetPropertyDefs();
            IEnumerable<PropertyDescription> propDescs = GetPropertyDescriptions(propDefs);

            TypeDescription td = new TypeDescription(tn, propDescs);
            return td;
        }

        public Type CreateWrapper(TypeDescription td, IModuleBuilderInfo modBuilderInfo = null)
        {
            // If the caller did not supply a ModuleBuilderInfo object, then use the default one 
            // provided by the WrapperGenLib
            IModuleBuilderInfo builderInfoToUse = modBuilderInfo ?? new DefaultModuleBuilderInfoProvider().ModuleBuilderInfo;

            Type emittedType = builderInfoToUse.GetWrapperType(td);

            System.Diagnostics.Debug.WriteLine($"Created Type: {emittedType.FullName}");

            return emittedType;
        }

        public static IEnumerable<PropertyDescription> GetPropertyDescriptions(IEnumerable<KeyValuePair<string, Type>> propDefs)
        {
            List<PropertyDescription> result = new List<PropertyDescription>();

            foreach (KeyValuePair<string, Type> kvp in propDefs)
            {
                string propertyName = kvp.Key;
                Type propertyType = kvp.Value;

                result.Add(new PropertyDescription(propertyName, propertyType));
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">The base type to use, must Implement ITypeSafePropBag</typeparam>
        /// <param name="newTypeName"></param>
        /// <param name="regularType"></param>
        /// <param name="propDefs"></param>
        /// <returns></returns>
        public MapperConfiguration CreateMapperConfig(Type wrapperType, Func<object, object> regularInstanceCreator) 
        {
            //byte dummy = 0xf;
            //DRM.PropBag.PropBag dummyPropBag = new DRM.PropBag.PropBag(dummy);
            //object generatedObject = Activator.CreateInstance(emittedType, new object[] { dummyPropBag, propDefs });

            //Type wType = generatedObject.GetType();

            //wrapperType = wType;


            //// Loop over all the generated properties, and assign the values from our XML:
            //PropertyInfo[] properties = newProxyType.GetProperties();

            ////properties[1].SetMethod.Invoke(generatedObject, new object[] { "Can we do it?" });

            //ITypeSafePropBag tspb = (ITypeSafePropBag)generatedObject;
            //Dictionary<string, ValPlusType> namedValue = ((TypeSafePropBagBase)tspb).NamedValuesWithType;
            //tspb.SetItWithNoType("A", "Item1");

            //MethodInfo getM = properties[1].GetGetMethod();

            //string item1Val = (string)getM.Invoke(generatedObject, new object[] { });

            //// Create a fresh instance.
            //generatedObject = Activator.CreateInstance(newProxyType, new object[] { propDefs });

            //MethodInfo setM = properties[1].GetSetMethod();
            //setM.Invoke(generatedObject, new object[] { "B" });

            //// Create a fresh instance.
            //generatedObject = Activator.CreateInstance(newProxyType, new object[] { propDefs });


            var config = new MapperConfiguration(cfg =>
            {
                cfg
                    .CreateMap(typeof(RegT), wrapperType)
                    //.ForMember("ThePropFactory", (mc => mc.Ignore()))
                    //.ForMember("TypeSafetyMode", (mc => mc.Ignore())))
                    //.ForMember("NamedValuesWithType", (mc => mc.Ignore()))
                    //.ForMember("TypeDefs", (mc => mc.Ignore()))


                    //.ConstructUsing(src => Activator.CreateInstance(newProxyType, new object[] { propDefs }))
                    ;

                if (regularInstanceCreator != null)
                {
                    cfg.CreateMap(wrapperType, typeof(RegT)).ConstructUsing(regularInstanceCreator);
                }
                else
                {
                    cfg.CreateMap(wrapperType, typeof(RegT));
                }
            });

            config.AssertConfigurationIsValid();

            return config;
            //var mapper = config.CreateMapper();

            //Source src = new Source
            //{
            //    Item1 = "This is it",
            //    MyInteger = 2
            //};

            ////src.SetItem1("This is it");

            //var newDest = mapper.Map(src, generatedObject, typeof(Source), wrapperType);

            //ITypeSafePropBag tDest = (ITypeSafePropBag)newDest;
            //tDest.GetItWithNoType("Item1").ShouldBe("This is it");
            //tDest.GetItWithNoType("MyInteger").ShouldBe(2);

            ////var newDest2 = mapper.Map<Source, Destination>(src);

            ////Destination destinationUsedAsSource = new Destination();
            ////destinationUsedAsSource.SetItWithType("Item1", typeof(string), "This is the initial value of src2.Item1");
            ////destinationUsedAsSource.SetItWithType("MyInteger", typeof(int), -1);

            //Source newSource = (Source)mapper.Map(generatedObject, src, wrapperType, typeof(Source));
            //newSource.Item1.ShouldBe("This is it");
            //newSource.MyInteger.ShouldBe(2);
        }

        #endregion

        public RegT MapFrom(PbT source)
        {
            object wrappedObject = Activator.CreateInstance(_wrapperType, new object[] { source, _propDefs });
            return (RegT)_mapper.Map(wrappedObject, _wrapperType, typeof(RegT));
        }




        public void MapTo(RegT source, PbT destination)
        {
            object wrappedObject = Activator.CreateInstance(_wrapperType, new object[] { destination, _propDefs });

            // Used for debugging only.
            var rr = _mapper.Map(source, wrappedObject, typeof(RegT), _wrapperType);
        }

        public PbT MapTo(RegT source)
        {
            if (_propModel == null) throw new InvalidOperationException($"The version of MapTo that doesn't take a {typeof(PbT)} is not supported. This instance was not created from a BoundPropBagTemplate.");

            PbT destination;
            try
            {
                destination = (PbT)Activator.CreateInstance(typeof(PbT), new object[] { _propModel });
            }
            catch
            {
                throw new InvalidOperationException($"Cannot create an instance of {typeof(PbT)} that takes a PropModel parameter.");
            }

            object wrappedObject = Activator.CreateInstance(_wrapperType, new object[] { destination, _propDefs });

            var rr = _mapper.Map(source, wrappedObject, typeof(RegT), _wrapperType);
            return destination;
        }

        #region DevelopementWork Get Key Pair

        //private static void GetKeys()
        //{
        //    FileStream fs = new FileStream(@"C:\Users\david_000\Source\keyPair.snk", FileMode.Open);
        //    StrongNameKeyPair kp = new StrongNameKeyPair(fs);
        //    fs.Close();
        //}

        #endregion

    }
}
