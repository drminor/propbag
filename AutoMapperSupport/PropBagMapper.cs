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
    public class PropBagMapper<RegT, PbT> : IPropBagMapperGen where PbT : IPropBag, new()
    {
        #region Public and Private Members

        // Used to collect all wrapper types for classes that define no value for their outputNamespace,
        // so that they are kept separate from all those classes that do define a namespace.
        public const string DEFAULT_NAMESPACE_NAME = "PropBagWrappers";

        PropModel _propModel;
        Dictionary<string, Type> _propDefs;
        Type _wrapperType;
        Func<object, object> _regularInstanceCreator;

        public IMapper Mapper { get; set; }

        TypePair _typePair;
        public TypePair TypePair => _typePair;

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

            _regularInstanceCreator = regularInstanceCreator;

            _typePair = new TypePair(typeof(RegT), typeof(PbT));
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

            _regularInstanceCreator = regularInstanceCreator;

            _typePair = new TypePair(typeof(RegT), typeof(PbT));
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

        #endregion

        public IMapperConfigurationExpression Configure(IMapperConfigurationExpression cfg)
        {
            ConfigIt(cfg, _wrapperType, _regularInstanceCreator);
            return cfg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">The base type to use, must Implement ITypeSafePropBag</typeparam>
        /// <param name="newTypeName"></param>
        /// <param name="regularType"></param>
        /// <param name="propDefs"></param>
        /// <returns></returns>
        public void ConfigIt(IMapperConfigurationExpression cfg, Type wrapperType, Func<object, object> regularInstanceCreator) 
        {
            cfg.CreateMap(typeof(RegT), wrapperType);

            if (regularInstanceCreator != null)
            {
                cfg.CreateMap(wrapperType, typeof(RegT)).ConstructUsing(regularInstanceCreator);
            }
            else
            {
                cfg.CreateMap(wrapperType, typeof(RegT));
            }
        }


        #region IPropBagMapperGen implementation

        //public delegate object MapFromX(IPropBag source);
        //public delegate IPropBag MapToX(object source, IPropBag destination);
        //public delegate IPropBag MapToNewX(object source);

        //MapFromX MapFrom
        //{
        //    get
        //    {
        //        return MapFromX;
        //    }
        //}

        //MapToX MapTo
        //{
        //    get
        //    {
        //        return MapToX;
        //    }
        //}

        //public MapToNewX MapToNew
        //{
        //    get
        //    {
        //        return MapToNewX;
        //    }
        //}

        #endregion

        #region Type Mapper Function

        //public object MapFromX(IPropBag source)
        //{
        //    return (RegT)MapFrom((PbT)source);
        //}

        //public IPropBag MapToX(object source, IPropBag destination)
        //{
        //    return MapTo((RegT)source, (PbT)destination);
        //}

        //public IPropBag MapToNewX(object source)
        //{
        //    return (PbT)MapTo((RegT)source);
        //}

        public RegT MapFrom(PbT source)
        {
            object wrappedObject = Activator.CreateInstance(_wrapperType, new object[] { source, _propDefs });
            return (RegT)Mapper.Map(wrappedObject, _wrapperType, typeof(RegT));
        }

        public PbT MapTo(RegT source, PbT destination)
        {
            object wrappedObject = Activator.CreateInstance(_wrapperType, new object[] { destination, _propDefs });

            // Next Line may be helpful when debugging.
            //var rr = Mapper.Map(source, wrappedObject, typeof(RegT), _wrapperType);

            Mapper.Map(source, wrappedObject, typeof(RegT), _wrapperType);

            return destination;
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

            var rr = Mapper.Map(source, wrappedObject, typeof(RegT), _wrapperType);
            return destination;
        }

        #endregion

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
