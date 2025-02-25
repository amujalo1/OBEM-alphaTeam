using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{
    public class Device
    {

        public Device(int id, string name, int lowerBound, int upperBound, int numericValue, string unit, string apartmentName, string type, string floor)
        {
            Id = id;
            Name = name;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            NumericValue = numericValue;
            Unit = unit;
            ApartmentName = apartmentName;
            Type = type;
            Floor = floor;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public int LowerBound { get; set; }
        public int UpperBound { get; set; }

        public int NumericValue { get; set; }

        public string Unit { get; set; }

        public string ApartmentName { get; set; }

        public string Type { get; set; }

        public string Floor { get; set; }

        public bool IsActive { get; set; }

        public static Device FromDeviceInfo(DeviceInfo info) // for making a device out of deviceinfo
        {
            return new Device(
                id: int.TryParse(info.Id, out var parsedId) ? parsedId : 0, 
                name: info.Name,
                lowerBound: info.LowerBound,
                upperBound: info.UpperBound,
                numericValue: info.NumericValue,
                unit: info.Unit,
                apartmentName: info.Group2,  
                type: info.Group3,           
                floor: info.Group1           
            );
        }
    }
}
