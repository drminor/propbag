using AutoMapper;
using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;
using DRM.PropBag.ViewModelBuilder;
using System;
using System.Collections.Generic;


namespace DRM.PropBag.AutoMapperSupport
{
    public class PropBagMapper<TSource, TDestination>
        : IPropBagMapper<TSource, TDestination>
    {
        #region Public and Private Members

        private IMapTypeDefinitionGen SourceTypeDef;
        private IMapTypeDefinitionGen DestinationTypeDef;

        public Type SourceType { get => SourceTypeDef.Type; }
        public Type DestinationType { get => DestinationTypeDef.Type; }

        public PropModel PropModel { get; }
        public Type RunTimeType { get; }
        public IMapper Mapper { get; set; }

        public Func<TDestination, TSource> RegularInstanceCreator { get; }

        public bool SupportsMapFrom { get; }

        #endregion

        #region Constructors

        public PropBagMapper(IPropBagMapperKey<TSource, TDestination> mapRequest)
        {
            if (typeof(TSource) is IPropBag) throw new ApplicationException("The first type, TSource, is expected to be a regular, non-propbag-based type.");
            if (typeof(TDestination) is IPropBag) throw new ApplicationException("The second type, TDestination, is expected to be a propbag-based type.");

            SourceTypeDef = mapRequest.SourceTypeDef;
            DestinationTypeDef = mapRequest.DestinationTypeDef;

            PropModel = mapRequest.DestinationTypeDef.PropModel;
            RunTimeType = mapRequest.DestinationTypeDef.BaseType;

            RegularInstanceCreator = mapRequest.ConstructSourceFunc;

            SupportsMapFrom = true;
        }

        #endregion

        #region Construction Support

        //public static TypeDescription BuildTypeDesc(string className, PropModel pm)
        //{
        //    //string assemblyName = WRAPPER_MODULE_ASSEMBLY_NAME;
        //    string namespaceName = pm.NamespaceName ?? DEFAULT_NAMESPACE_NAME;

        //    TypeName tn = new TypeName(className, namespaceName); //, assemblyName);

        //    IEnumerable<PropertyDescription> propDescs = pm.GetPropertyDescriptions();

        //    TypeDescription td = new TypeDescription(tn, typeof(PbT), propDescs);
        //    return td;
        //}

        //public static Type CreateWrapper(TypeDescription td, IModuleBuilderInfo modBuilderInfo = null)
        //{
        //    // If the caller did not supply a ModuleBuilderInfo object, then use the default one 
        //    // provided by the WrapperGenLib
        //    IModuleBuilderInfo builderInfoToUse = modBuilderInfo ?? new DefaultModuleBuilderInfoProvider().ModuleBuilderInfo;

        //    Type emittedType = builderInfoToUse.GetWrapperType(td);

        //    System.Diagnostics.Debug.WriteLine($"Created Type: {emittedType.FullName}");

        //    return emittedType;
        //}



        #endregion

        public IMapperConfigurationExpression Configure(IMapperConfigurationExpression cfg)
        {
            ConfigIt(cfg, RegularInstanceCreator);
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
        public void ConfigIt(IMapperConfigurationExpression cfg, Func<TDestination, TSource> regularInstanceCreator) 
        {
            cfg.CreateMap(typeof(TSource), RunTimeType);

            if (regularInstanceCreator != null)
            {
                cfg.CreateMap(RunTimeType, typeof(TSource)); //.ConstructUsing(RegularInstanceCreator);
            }
            else
            {
                cfg.CreateMap(RunTimeType, typeof(TSource));
            }
        }


        #region IPropBagMapper implementation

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

        public TDestination MapFrom(TSource s)
        {
            TDestination wrappedObject = (TDestination) Activator.CreateInstance(RunTimeType, new object[] { PropModel });

            return (TDestination)Mapper.Map(s, wrappedObject, SourceType, RunTimeType);
        }

        public TDestination MapFrom(TSource s, TDestination d)
        {
            //object proxyObject = Activator.CreateInstance(_wrapperType, new object[] { _propModel });

            //// Next Line may be helpful when debugging.
            ////var rr = Mapper.Map(source, wrappedObject, typeof(RegT), _wrapperType);

            // Mapper.Map(source, proxyObject, typeof(RegT), _wrapperType);

            //return (PbT)proxyObject;
            return (TDestination) Mapper.Map(s, d, SourceType, RunTimeType);
        }

        public TSource MapTo(TDestination d)
        {
            return (TSource)Mapper.Map(d, RunTimeType, SourceType);
        }

        public TSource MapTo(TDestination d, TSource s)
        {
            return (TSource) Mapper.Map(d, s, RunTimeType, SourceType);
        }

        //public object MapFrom(object source)
        //{
        //    //object wrappedObject = Activator.CreateInstance(_wrapperType, new object[] { _propModel });
        //    //return (RegT)Mapper.Map(wrappedObject, _wrapperType, typeof(RegT));
        //    return Mapper.Map(source, TypePair.DestinationType, TypePair.SourceType);
        //}

        //public object MapTo(object source, object destination)
        //{
        //    //object proxyObject = Activator.CreateInstance(_wrapperType, new object[] { _propModel });

        //    //// Next Line may be helpful when debugging.
        //    ////var rr = Mapper.Map(source, wrappedObject, typeof(RegT), _wrapperType);

        //    // Mapper.Map(source, proxyObject, typeof(RegT), _wrapperType);

        //    //return (PbT)proxyObject;
        //    return Mapper.Map(source, destination, TypePair.SourceType, TypePair.DestinationType);
        //}

        //public object MapTo(object source)
        //{
        //    //if (_propModel == null) throw new InvalidOperationException($"The version of MapTo that doesn't take a {typeof(PbT)} is not supported. This instance was not created from a BoundPropBagTemplate.");

        //    //PbT proxyObject;
        //    //try
        //    //{
        //    //    proxyObject = (PbT)Activator.CreateInstance(_wrapperType, new object[] { _propModel });
        //    //}
        //    //catch
        //    //{
        //    //    throw new InvalidOperationException($"Cannot create an instance of {typeof(PbT)} that takes a PropModel parameter.");
        //    //}

        //    ////object wrappedObject = Activator.CreateInstance(_wrapperType, new object[] { destination, _propModel });

        //    //var rr = Mapper.Map(source, proxyObject, typeof(RegT), _wrapperType);
        //    //return proxyObject;

        //    object newInstance;
        //    try
        //    {
        //        newInstance = Activator.CreateInstance(TypePair.DestinationType, new object[] { PropModel });
        //    }
        //    catch
        //    {
        //        throw new InvalidOperationException($"Cannot create an instance of {TypePair.DestinationType} that takes a PropModel parameter.");
        //    }

        //    return Mapper.Map(source, newInstance, TypePair.SourceType, TypePair.DestinationType);
        //}

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
