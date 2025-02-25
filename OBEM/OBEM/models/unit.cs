using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{
    public class Unit : AComponent
    {
        private readonly List<Device> _devices = new List<Device>();
        public string Type { get; set; }

        public Unit(string name, string type, int id) : base (name,id) {
    
            Name = name; Type = type;
        }
    }
}
