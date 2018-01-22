
namespace MVVMApplication.Model
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CityOfResidence { get; set; }
        public Profession Profession { get; set; }
    }

    public enum Profession
    {
        Default = -1,
        Doctor,
        SoftwareEngineer,
        Student,
        SportsPerson,
        Other
    }


}
