using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{
    public abstract class AComponent
    {
        public string Name { get; protected set; }
        public int ID { get; protected set; }

        public AComponent(string name, int Id)
        {
            Name = name;
            ID = Id;
        }
    }
}
