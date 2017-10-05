using AutoMapper;
using DRM.PropBag.ControlModel;
using System;

using System.Collections.Generic;
using System.Linq;

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

        #region Constructor

        public PropBagMapper(IPropBagMapperKey<TSource, TDestination> mapRequest)
        {
            //if (typeof(TSource) is IPropBag) throw new ApplicationException("The first type, TSource, is expected to be a regular, non-propbag-based type.");
            //if (typeof(TDestination) is IPropBag) throw new ApplicationException("The second type, TDestination, is expected to be a propbag-based type.");

            SourceTypeDef = mapRequest.SourceTypeDef;
            DestinationTypeDef = mapRequest.DestinationTypeDef;

            PropModel = mapRequest.DestinationTypeDef.PropModel;
            RunTimeType = mapRequest.DestinationTypeDef.BaseType;

            RegularInstanceCreator = mapRequest.ConstructSourceFunc;

            SupportsMapFrom = true;
        }

        #endregion

        #region Configure Method

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

        #endregion

        #region Type Mapper Function

        public TDestination MapToDestination(TSource s)
        {
            TDestination proxyViewModel = GetNewDestination(PropModel);

            return (TDestination)Mapper.Map(s, proxyViewModel, SourceType, RunTimeType);
        }

        public TDestination MapToDestination(TSource s, TDestination d)
        {
            return (TDestination) Mapper.Map(s, d, SourceType, RunTimeType);
        }

        public TSource MapToSource(TDestination d)
        {
            return (TSource)Mapper.Map(d, RunTimeType, SourceType);
        }

        public TSource MapToSource(TDestination d, TSource s)
        {
            return (TSource) Mapper.Map(d, s, RunTimeType, SourceType);
        }

        #endregion

        #region Mapper Functions for Lists

        public IEnumerable<TDestination> MapToDestination(IEnumerable<TSource> listOfSources)
        {
            //List<TDestination> result = new List<TDestination>();

            //foreach (TSource s in listOfSources)
            //{
            //    TDestination d = this.MapToDestination(s);
            //    result.Add(d);
            //}

            //return result;

            return listOfSources.Select(s => MapToDestination(s));
        }

        public IEnumerable<TSource> MapToSource(IEnumerable<TDestination> listOfDestinations)
        {
            //List<TSource> result = new List<TSource>();

            //foreach (TDestination d in listOfDestinations)
            //{
            //    TSource s = this.MapToSource(d);
            //    result.Add(s);
            //}

            //return result;
            return listOfDestinations.Select(d => MapToSource(d));
        }
        #endregion

        #region Create Instance of TDestination

        private TDestination GetNewDestination(PropModel propModel)
        {
            TDestination destination;
            try
            {
                destination = (TDestination)Activator.CreateInstance(RunTimeType, new object[] { propModel });
                return destination;
            }
            catch
            {
                throw new InvalidOperationException($"Cannot create an instance of {RunTimeType} that takes a PropModel parameter.");
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
