using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBEM.models
{
    public abstract class AComp
    {
        public string Naziv { get; protected set; }
        public string ID { get; protected set; }

        public AComp(string naziv, string Id)
        {
            Naziv = naziv;
            ID = Id;
        }
    }
}
