using AutoMapper;
using DRM.PropBag.ControlModel;
using System;
using System.Collections.Generic;
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
            //if (typeof(TSource) is IPropBag) throw new ApplicationException("The first type, TSource, is expected to be a regular, non-propbag-based type.");
            //if (typeof(TDestination) is IPropBag) throw new ApplicationException("The second type, TDestination, is expected to be a propbag-based type.");

            SourceTypeDef = mapRequest.SourceTypeDef;
            DestinationTypeDef = mapRequest.DestinationTypeDef;

            PropModel = DestinationTypeDef.PropModel;
            _extraMembers = PropModel.GetExtraMembers(); //GetExtraMembers(PropModel);

            RegularInstanceCreator = mapRequest.ConstructSourceFunc;

            SupportsMapFrom = true;
        }

        #endregion

        #region Configure Method

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
            //cfg.IncludeExtraMembersForType(typeof(Destination), extraMembers);

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

        #region Type Mapper Function

        public TDestination MapToDestination(TSource s)
        {
            TDestination d = GetNewDestination(PropModel);
            return MapToDestination(s, d);
        }

        public TDestination MapToDestination(TSource s, TDestination d)
        {
            return (TDestination)Mapper.Map<TSource, TDestination>(s, d);
        }

        public TSource MapToSource(TDestination d)
        {
            return (TSource)Mapper.Map<TDestination, TSource>(d);
        }

        public TSource MapToSource(TDestination d, TSource s)
        {
            return (TSource)Mapper.Map<TDestination, TSource>(d, s);
        }

        private TDestination GetNewDestination(PropModel propModel)
        {
            TDestination destination;
            try
            {
                destination = (TDestination)Activator.CreateInstance(typeof(TDestination), new object[] { propModel });
                return destination;
            }
            catch
            {
                throw new InvalidOperationException($"Cannot create an instance of {typeof(TDestination)} that takes a PropModel parameter.");
            }
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
