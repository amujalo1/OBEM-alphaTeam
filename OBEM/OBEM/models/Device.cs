using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{
{
    class Device
    {

        public Device(string name, int lowerBound, int upperBound, int numericValue, string unit, string apartmetName, string type, string floor)
        {
            Name = name;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            NumericValue = numericValue;
            Unit = unit;
            ApartmetName = apartmetName;
            Type = type;
            Floor = floor;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public int LowerBound { get; set; }
        public int UpperBound { get; set; }

        public int NumericValue { get; set; }

        public string Unit { get; set; }

        public string ApartmetName { get; set; }

        public string Type { get; set; }

        public string Floor { get; set; }

    }
}
}
