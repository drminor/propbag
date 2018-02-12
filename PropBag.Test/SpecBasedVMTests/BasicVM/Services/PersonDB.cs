using PropBagLib.Tests.BusinessModel;
using System.Data.Entity;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM.Services
{
    public class PersonDB : DbContext
    {
        public PersonDB() : base("name=DefaultConnection")
        {
        }

        public DbSet<Person> Person { get; set; }
    }
}
