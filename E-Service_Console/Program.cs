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
using EService.Data.POCO;

namespace E_Service_Console
{
    class Program
    {
        //static void Main(string[] args)
        //{
        //    string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
        //    string path = (System.IO.Path.GetDirectoryName(executable));
        //    AppDomain.CurrentDomain.SetData("DataDirectory", path);
        //    //Console.WriteLine(AppDomain.CurrentDomain.GetData("DataDirectory"));
        //    SQLiteContext context = new SQLiteContext();
        //    //context.Database.Connection.ConnectionString = "D:\\Projects\\E-Service\\EService.Data.Entity\\service_db.db";
        //    long time = 0;
            
            
        //    var list = context.serviceLog.ToList();
        //    foreach (var log in list)
        //    {
        //        Console.WriteLine(log.rowid);
        //        Console.WriteLine(log.devices.inventoryNumber);
        //        Console.WriteLine(log.devices.serialNumber);
        //        Console.WriteLine(log.devices.model.shortName);
        //        Console.WriteLine(log.devices.model.typeModel.shortName);
        //        Console.WriteLine(log.devices.status.name);
        //        Console.WriteLine(log.devices.dept.name);
        //        Console.WriteLine("{0} {1} {2}", log.repairer.surname, log.repairer.name, log.repairer.midname);
        //        Console.WriteLine(log.date);
        //        Console.WriteLine("--------------------");
        //    }
        //    for (int i = 0; i < 1; i++)
        //    {
        //        time += GetServiceLog(context);
        //    }
        //    Console.WriteLine(time);
        //    Console.ReadKey();
        //}

        //static long GetServiceLog(SQLiteContext context)
        //{
        //    long time = 0;
        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();
        //    context.Configuration.LazyLoadingEnabled = false;
        //    var list = context.serviceLog.ToList();
        //    List<EService.Data.POCO.ServiceLog> serviceLog = new List<EService.Data.POCO.ServiceLog>();
        //    foreach (var item in list)
        //    {
        //        EService.Data.IData temp = new EService.Data.POCO.ServiceLog();
        //        var isExist = temp.ConvertData(out temp, item);
        //        if (isExist)
        //            serviceLog.Add((EService.Data.POCO.ServiceLog)temp);
        //    }
        //    sw.Stop();

        //    foreach (var log in serviceLog)
        //    {
        //        Console.WriteLine(log.rowid);
        //        Console.WriteLine(log.devices.inventoryNumber);
        //        Console.WriteLine(log.devices.serialNumber);
        //        Console.WriteLine(log.devices.model.shortName);
        //        Console.WriteLine(log.devices.model.typeModel.shortName);
        //        Console.WriteLine(log.devices.status.name);
        //        Console.WriteLine(log.devices.dept.name);
        //        Console.WriteLine("{0} {1} {2}",log.repairer.surname, log.repairer.name, log.repairer.midname);
        //        Console.WriteLine(log.date);
        //        Console.WriteLine("--------------------");
        //    }

        //    time = sw.ElapsedMilliseconds;
        //    return time;
        //}
        //static long GetTypeModel(SQLiteContext context)
        //{
        //    long time = 0;
        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();
        //    var list = context.types.ToList();
        //    List<EService.Data.POCO.TypeModel> typeModels = new List<EService.Data.POCO.TypeModel>();
        //    foreach (var item in list)
        //    {
        //        EService.Data.IData temp = new EService.Data.POCO.TypeModel();
        //        var isExist = temp.ConvertData(out temp, item);
        //        if (isExist)
        //            typeModels.Add((EService.Data.POCO.TypeModel)temp);
        //    }
        //    sw.Stop();

        //    foreach (var item in typeModels)
        //    {
        //        Console.WriteLine(item.rowid);
        //        Console.WriteLine(item.fullName);
        //        Console.WriteLine(item.shortName);                
        //        Console.WriteLine("--------------------");
        //    }

        //    time = sw.ElapsedMilliseconds;
        //    return time;
        //}
        //static long GetDevices(SQLiteContext context)
        //{
        //    long time = 0;
        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();
        //    var list = context.devices.ToList();
        //    List<EService.Data.POCO.Device> devices = new List<EService.Data.POCO.Device>();
        //    foreach (var item in list)
        //    {
        //        EService.Data.IData temp = new EService.Data.POCO.Device();
        //        var isExist = temp.ConvertData(out temp, item);
        //        if (isExist)
        //            devices.Add((EService.Data.POCO.Device)temp);
        //    }
        //    sw.Stop();
            
        //    foreach (var item in devices)
        //    {
        //        Console.WriteLine(item.rowid);
        //        Console.WriteLine(item.rowidDept);
        //        Console.WriteLine(item.rowidModel);
        //        Console.WriteLine(item.rowidStatus);
        //        Console.WriteLine(item.inventoryNumber);
        //        Console.WriteLine(item.serialNumber);
        //        Console.WriteLine("--------------------");
        //    }
            
        //    time = sw.ElapsedMilliseconds;
        //    return time;
        //}
        //static long GetDepts(SQLiteContext context)
        //{
        //    long time = 0;
        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();
        //    var list = context.dept.ToList();
        //    List<EService.Data.POCO.Dept> depts = new List<EService.Data.POCO.Dept>();
        //    foreach (var item in list)
        //    {
        //        EService.Data.IData temp = new EService.Data.POCO.Dept();
        //        var isExist = temp.ConvertData(out temp, item);
        //        if (isExist)
        //            depts.Add((EService.Data.POCO.Dept)temp);
        //    }
        //    sw.Stop();
        //    foreach (var item in depts)
        //    {
        //        Console.WriteLine(item.rowid);
        //        Console.WriteLine(item.name);
        //        Console.WriteLine(item.code);
        //        Console.WriteLine(item.description);
        //        Console.WriteLine("--------------------");
        //    }
        //    time = sw.ElapsedMilliseconds;
        //    return time;
        //}
    }
}
