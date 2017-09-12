using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;

using PropBagTestApp.Models;
using AutoMapper;
using AutoMapper.Configuration.Conventions;
using AutoMapper.Collection;
using AutoMapper.EquivalencyExpression;
using System.Runtime.Serialization;

namespace PropBagTestApp
{
    /// <summary>
    /// Interaction logic for DtoTest.xaml
    /// </summary>
    public partial class DtoTest : Window
    {

    //    cfg.CreateMap<Source, Destination>()
    //.ForMember(dest => dest.Total,
    //    opt => opt.ResolveUsing<CustomResolver, decimal>(src => src.SubTotal));

        private void ReadWithMap(MyModel mm, DtoTestViewModel vm)
        {
            var config = new AutoMapper.MapperConfiguration
                (
                    //cfg => cfg.CreateMap<MyModel, DtoTestViewModel>().ForMember
                    //(
                    //    dest => dest.Amount,
                    //    opt => opt.ResolveUsing<CustomResolver, int>(src => src.Amount)
                    //)

                    cfg => cfg.CreateMap<MyModel, DtoTestViewModel>().ForAllMembers
                    (opt => opt.ResolveUsing<PropBagDestResolver>()
                        
                    )
                );

            var mapper = config.CreateMapper();

            mapper.Map<MyModel, DtoTestViewModel>(mm, vm);
        }

        private MyModel GetTestInstance()
        {
            MyModel m1 = new MyModel
            {
                ProductId = Guid.NewGuid(),
                Amount = 10,
                Size = 32.44
            };

            return m1;
        }

        public void MapUsingDict()
        {
            var config = new AutoMapper.MapperConfiguration
            (
                cfg => { cfg.AddCollectionMappers(); }
            );

            //var cn = new AutoMapper.MapperConfiguration(
            //    cfg => cfg.CreateMap<MyModel, AList>().EqualityComparison((om, oa) => om.

            

            var mapper = config.CreateMapper();

            MyModel mm = GetTestInstance();
            AList al2 = mapper.Map<AList>(mm);

            AList al = AList.Create();
            mapper.Map<MyModel, AList>(mm, al);
        }

    }

    //public class CustomResolver : IMemberValueResolver<object, object, object>
    //{
    //    public int Resolve(object source, object destination, int sourceMember, int destinationMember, ResolutionContext context)
    //    {
    //        var x = (IMemberConfiguration)context;

    //        throw new NotImplementedException();
    //    }

    //}

    public class PropBagDestResolver : IValueResolver<MyModel, DtoTestViewModel, object>
    {

        public object Resolve(MyModel source, DtoTestViewModel destination, object destMember, ResolutionContext context)
        {
            var x = (IMemberConfiguration)context;
            throw new NotImplementedException();
        }
    }

   [Serializable]
    public class AList : Dictionary<string, object>
    {
        public AList() { }

        public static AList Create()
        {
            AList result = new AList();

            Guid g = Guid.NewGuid();
            result.Add("ProductId", g);
            int a = 32;
            result.Add("Amount", a);

            double d = 21.34;
            result.Add("Size", d);


            return result;
        }

        protected AList(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

