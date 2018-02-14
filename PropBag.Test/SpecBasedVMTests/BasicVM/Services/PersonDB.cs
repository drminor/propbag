using PropBagLib.Tests.BusinessModel;
using System;
using System.Data.Entity;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM.Services
{
    public class PersonDB : DbContext
    {
        public PersonDB() : base("name=DefaultConnection")
        {
            string dataDirPath = (string) AppDomain.CurrentDomain.GetData("DataDirectory");

            System.Diagnostics.Debug.WriteLine($"PersonDB is using DataDirectory: {dataDirPath}.");
        }

        public DbSet<Person> Person { get; set; }
    }
}
