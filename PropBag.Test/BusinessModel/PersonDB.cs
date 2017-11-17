using System.Data.Entity;

namespace PropBagLib.Tests.BusinessModel
{
    public class PersonDB:DbContext
    {
        public PersonDB():base("name=DefaultConnection")
        {

        }

        public DbSet<Person> Person { get; set; }
    }
}
