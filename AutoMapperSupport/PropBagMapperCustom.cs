using AutoMapper;
using AutoMapper.ExtraMembers;
using DRM.PropBag;
using DRM.PropBag.ControlModel;

//using DRM.PropBag.ViewModelBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace DRM.PropBag.AutoMapperSupport
{
    public class PropBagMapperCustom<TSource, TDestination>
        : IPropBagMapper<TSource, TDestination>
    {
        #region Public and Private Members

        IEnumerable<MemberInfo> _extraMembers = new List<MemberInfo>();

        private IMapTypeDefinitionGen SourceTypeDef;
        private IMapTypeDefinitionGen DestinationTypeDef;

        public Type SourceType { get => SourceTypeDef.Type; }
        public Type DestinationType { get => DestinationTypeDef.Type; }

        public PropModel PropModel { get; }
        public IMapper Mapper { get; set; }

        public Func<TDestination, TSource> RegularInstanceCreator { get; }

        public bool SupportsMapFrom { get; }

        #endregion

        #region Constructors

        public PropBagMapperCustom(IPropBagMapperKey<TSource, TDestination> mapRequest)
        {
            if (typeof(TSource) is IPropBag) throw new ApplicationException("The first type, TSource, is expected to be a regular, non-propbag-based type.");
            if (typeof(TDestination) is IPropBag) throw new ApplicationException("The second type, TDestination, is expected to be a propbag-based type.");

            SourceTypeDef = mapRequest.SourceTypeDef;
            DestinationTypeDef = mapRequest.DestinationTypeDef;

            PropModel = mapRequest.DestinationTypeDef.PropModel;
            _extraMembers = GetExtraMembers(PropModel);

            RegularInstanceCreator = mapRequest.ConstructSourceFunc;

            SupportsMapFrom = true;
        }

    #endregion

        #region Construction Support

        private List<MemberInfo> GetExtraMembers(PropModel pm)
        {
            List<MemberInfo> result = new List<MemberInfo>();

            foreach (PropItem propItem in pm.Props)
            {
                string propertyName = propItem.PropertyName;
                Type propertyType = propItem.PropertyType;

                // TOOD: Use a Static Bridge, to avoid boxing the host variable.
                //Func<object, string, Type, object> getter =
                //    new Func<object, string, Type, object>((host, pn, pt) => ((PbT)host).GetValWithType(pn, pt));

                //Action<object, string, Type, object> setter =
                //    new Action<object, string, Type, object>((host, pn, pt, value) => ((PbT)host).SetValWithType(pn, pt, value));

                //PropertyInfoWT pi = new PropertyInfoWT(propertyName, propertyType, typeof(PbT),
                //    getter, setter);

                //result.Add(pi);

            }

            return result;
        }

        public IMapperConfigurationExpression Configure(IMapperConfigurationExpression cfg)
        {
            ConfigIt(cfg, _extraMembers, RegularInstanceCreator);
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
        public void ConfigIt(IMapperConfigurationExpression cfg,
            IEnumerable<MemberInfo> extraMembers,
           Func<TDestination, TSource> regularInstanceCreator) 
        {

            cfg.ShouldMapField = ShouldMap;
            cfg.ShouldMapProperty = ShouldMap;

            //cfg.IncludeExtraMembersForType(typeof(Destination), extraMembers);

            //if(null != cfg.GetExtraGetterStrategy(PropertyInfoWT.STRATEGEY_KEY))
            //{
            //    cfg.DefineExtraMemberGetterBuilder(PropertyInfoWT.STRATEGEY_KEY, GetGetterStrategy);
            //}

            //if (null != cfg.GetExtraSetterSrategry(PropertyInfoWT.STRATEGEY_KEY))
            //{
            //    cfg.DefineExtraMemberSetterBuilder(PropertyInfoWT.STRATEGEY_KEY, GetSetterStrategy);
            //}


            cfg
                .CreateMap<TSource, TDestination>()
                //.RegisterExtraMembers(cfg)
                .AddExtraDestintionMembers(extraMembers)
                //.ForMember("Item1", opt => opt.Condition(srcA => srcA.Item1 != "AA"))
            ;

            //Func<Destination, bool> cond = (s => "x" != (string)s.GetIt("Item1", typeof(string)));

            
            cfg
                .CreateMap<TDestination, TSource>()
                .AddExtraSourceMembers(extraMembers)
                //.ForMember("Item1", opt => opt.Condition(cond))
            ;

        }

        #endregion

        #region Extra Member Support



        public bool ShouldMap(MemberInfo mi)
        {
            if(IsPublic(mi)) return true;

            Attribute[] atts = mi.GetCustomAttributes(true) as Attribute[];
            if (atts == null) return false;
            Attribute test = atts.FirstOrDefault(a => a is ExtraMemberAttribute);

            return test != null;
        }

        private bool IsPublic(MemberInfo mi)
        {
            if (mi is MethodInfo methInfo) return methInfo.IsPublic;
            if (mi is PropertyInfo pi) return pi.GetMethod.IsPublic;
            return false;
        }

        #endregion

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

        public TDestination MapFrom(TSource s)
        {
            return (TDestination)Mapper.Map<TSource, TDestination>(s);
        }
        public TDestination MapFrom(TSource s, TDestination d)
        {
            return (TDestination)Mapper.Map<TSource, TDestination>(s, d);
        }

        public TSource MapTo(TDestination d)
        {
            return (TSource)Mapper.Map<TDestination, TSource>(d);
        }

        public TSource MapTo(TDestination d, TSource s)
        {
            return (TSource)Mapper.Map<TDestination, TSource>(d, s);
        }

        //public RegT MapFrom(PbT source)
        //{
        //    return (RegT)Mapper.Map<PbT, RegT>(source);
        //}

        //public RegT MapFrom(PbT source, RegT destination)
        //{
        //    return (RegT)Mapper.Map<PbT, RegT>(source, destination);
        //}

        //public PbT MapTo(RegT source, PbT destination)
        //{
        //    PbT result = Mapper.Map<RegT, PbT>(source, destination);
        //    return result;
        //}

        //public PbT MapTo(RegT source)
        //{
        //    if (_propModel == null) throw new InvalidOperationException($"The version of MapTo that doesn't take a {typeof(PbT)} is not supported. This instance was not created from a BoundPropBagTemplate.");

        //    PbT destination = GetNewDestination();
        //    return MapTo(source, destination);
        //}

        //private PbT GetNewDestination()
        //{
        //    PbT destination;
        //    try
        //    {
        //        destination = (PbT)Activator.CreateInstance(typeof(PbT), new object[] { _propModel });
        //        return destination;
        //    }
        //    catch
        //    {
        //        throw new InvalidOperationException($"Cannot create an instance of {typeof(PbT)} that takes a PropModel parameter.");
        //    }
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
