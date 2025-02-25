using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{
    public class Floor : AComponent
    {
        private readonly List<Unit> units = new List<Unit>();

        public Floor(string name, int id, List<Unit> units) : base(name, id)
        {
            this.units = units;
        }
    }
}
