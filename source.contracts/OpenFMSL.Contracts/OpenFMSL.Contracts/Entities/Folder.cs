using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Entities
{
    public class Folder : Entity
    {
        public Folder(string name) : base()
        {
            Name = name;
            IconName = "Folder";
        }
    }
}
