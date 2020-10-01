using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EService.VVM.Navigation
{
    public interface IPageResolver
    {
        Page GetPageInstance(string alias);
    }
}
