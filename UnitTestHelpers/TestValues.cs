using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestHelpers
{
    public class TestValues<T>
    {
        //T _initial;
        //T _second;
        //T _third;

        public T Initial { get; private set;}
        public T Second { get; private set; }
        public T Third { get; private set; }

        public TestValues(T initial, T second, T third)
        {
            Initial = initial;
            Second = second;
            Third = third;
        }
    }
}
