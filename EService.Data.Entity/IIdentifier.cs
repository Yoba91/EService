using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EService.Data.Entity
{

    public interface IIdentifier
    {
        long Rowid { get; set; }
    }
}
