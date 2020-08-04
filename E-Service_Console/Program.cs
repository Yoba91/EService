using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EService.BL;
using EService.Data;
using EService.Data.Entity;

namespace E_Service_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = (System.IO.Path.GetDirectoryName(executable));
            AppDomain.CurrentDomain.SetData("DataDirectory", path);
            Console.WriteLine(AppDomain.CurrentDomain.GetData("DataDirectory"));            
            Console.ReadKey();
        }       
    }
}
