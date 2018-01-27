using MVVMApplication.Model;
using System.Data.Entity;

namespace MVVMApplication.Services
{
    public class PersonDB : DbContext
    {
        public PersonDB() : base("name=DefaultConnection")
        {
        }

        public DbSet<Person> Person { get; set; }
    }
}
