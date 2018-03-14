using MVVM_Sample1.Model;
using System.Data.Entity;

namespace MVVM_Sample1.Services
{
    public class PersonDB : DbContext
    {
        public PersonDB() : base("name=DefaultConnection")
        {
        }

        public DbSet<Person> Person { get; set; }
    }
}
