using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Entities
{
    public interface IEntityManagerViewModel
    {
        Entity GetModelTree();        
        void New();
    }
    public interface IEntityManagerView
    {
    }
}
