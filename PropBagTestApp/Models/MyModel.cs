using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropBagTestApp.Models
{
    public class MyModel
    {
        public Guid ProductId { get; set; }
        public int Amount { get; set; }

        public double Size { get; set; }
    }

    public class MyModel2
    {
        public Guid ProductId { get; set; }
        public int Amount { get; set; }

        public double Size { get; set; }
    }

    public class MyModel3
    {
        public Guid ProductId { get; set; }
        public int Amount { get; set; }

        public double Size { get; set; }

        public MyModel4 Deep { get; set; }
    }

    public class MyModel4
    {
        public string MyString { get; set; }
    }
}
