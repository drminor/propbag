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
using System.Linq;
using AutoMapper.ExtraMembers;
using System.Linq.Expressions;

namespace DRM.AutoMapperSupport
{
    public class PropBagMapperCustom<RegT, PbT> : IPropBagMapperGen where PbT : IPropBag, new()
    {
        #region Public and Private Members


        PropModel _propModel;
        //Dictionary<string, Type> _propDefs;

        IEnumerable<MemberInfo> _extraMembers = new List<MemberInfo>();

        Func<object, object> _regularInstanceCreator;

        public IMapper Mapper { get; set; }

        TypePair _typePair;
        public TypePair TypePair => _typePair;

        #endregion

        #region Constructors

        public PropBagMapperCustom(string newTypeName, Dictionary<string, Type> propDefs) : this(newTypeName, propDefs, null) { }

        public PropBagMapperCustom(string newTypeName, Dictionary<string, Type> propDefs, Func<object, object> regularInstanceCreator)
        {
            _propModel = null;

            //PbT destination = GetNewDestination();

            //IPropBag ip = (IPropBag)destination;
            //IEnumerable<MemberInfo> extraMembers = ip.BuildPropertyInfoList<PbT>();

            _extraMembers = GetExtraMembers(propDefs);

            _regularInstanceCreator = regularInstanceCreator;

            _typePair = new TypePair(typeof(RegT), typeof(PbT));
        }

        public PropBagMapperCustom(BoundPropBag boundPB) : this(boundPB, null, null) { }

        public PropBagMapperCustom(BoundPropBag boundPB, IModuleBuilderInfo moduleBuilderInfo) : this(boundPB, moduleBuilderInfo, null) { }

        public PropBagMapperCustom(BoundPropBag boundPB, IModuleBuilderInfo moduleBuilderInfo, Func<object, object> regularInstanceCreator)
        {
            _propModel = boundPB.PropModel;

            Dictionary<string, Type> propDefs = boundPB.PropModel.GetPropertyDefs();

            _extraMembers = GetExtraMembers(propDefs);

            _regularInstanceCreator = regularInstanceCreator;

            _typePair = new TypePair(typeof(RegT), typeof(PbT));
        }

        #endregion

        #region Construction Support

        private List<MemberInfo> GetExtraMembers(IDictionary<string, Type> props)
        {
            List<MemberInfo> result = new List<MemberInfo>();

            foreach (KeyValuePair<string, Type> kvp in props)
            {
                string propertyName = kvp.Key;
                Type propertyType = kvp.Value;

                Func<object, string, Type, object> getter =
                    new Func<object, string, Type, object>((host, pn, pt) => ((PbT)host).GetValWithType(pn, pt));

                Action<object, string, Type, object> setter =
                    new Action<object, string, Type, object>((host, pn, pt, value) => ((PbT)host).SetValWithType(pn, pt, value));

                PropertyInfoWT pi = new PropertyInfoWT(propertyName, propertyType, typeof(PbT),
                    getter, setter);

                result.Add(pi);

            }

            return result;
        }

        public IMapperConfigurationExpression Configure(IMapperConfigurationExpression cfg)
        {
            ConfigIt(cfg, _extraMembers, _regularInstanceCreator);
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
        public void ConfigIt(IMapperConfigurationExpression cfg, IEnumerable<MemberInfo> extraMembers, Func<object, object> regularInstanceCreator) 
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
                .CreateMap<RegT, PbT>()
                //.RegisterExtraMembers(cfg)
                .AddExtraDestintionMembers(extraMembers)
                //.ForMember("Item1", opt => opt.Condition(srcA => srcA.Item1 != "AA"))
            ;

            //Func<Destination, bool> cond = (s => "x" != (string)s.GetIt("Item1", typeof(string)));

            cfg
                .CreateMap<PbT, RegT>()
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

            return (RegT)Mapper.Map<PbT, RegT>(source);
        }

        public PbT MapTo(RegT source, PbT destination)
        {
            PbT result = Mapper.Map<RegT, PbT>(source, destination);
            return result;
        }

        public PbT MapTo(RegT source)
        {
            if (_propModel == null) throw new InvalidOperationException($"The version of MapTo that doesn't take a {typeof(PbT)} is not supported. This instance was not created from a BoundPropBagTemplate.");

            PbT destination = GetNewDestination();
            return MapTo(source, destination);
        }

        private PbT GetNewDestination()
        {
            PbT destination;
            try
            {
                destination = (PbT)Activator.CreateInstance(typeof(PbT), new object[] { _propModel });
                return destination;
            }
            catch
            {
                throw new InvalidOperationException($"Cannot create an instance of {typeof(PbT)} that takes a PropModel parameter.");
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
