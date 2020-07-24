using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EService.Data.Entity;

namespace EService.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ServiceLog selectedServiceLog;

        private Status selectedStatus;
        public DateTime FirstDate { get; set; }
        public DateTime SecondDate { get; set; }
        public ObservableCollection<ParameterValue> ParametersValues { get; set; }
        public ObservableCollection<ServiceDone> ServicesDone { get; set; }
        public ObservableCollection<SpareUsed> SparesUsed { get; set; }
        public IList<ServiceLog> ServiceLogs { get; set; }
        public IList<Status> Statuses { get; set; }
        public IList<Repairer> Repairers { get; set; }

        public ServiceLog SelectedServiceLog { get { return selectedServiceLog; } set 
            { 
                selectedServiceLog = value;
                ParametersValues.Clear();
                ServicesDone.Clear();
                SparesUsed.Clear();
                foreach (var item in selectedServiceLog.ParametersValues)
                {
                    ParametersValues.Add(item);
                }
                foreach (var item in selectedServiceLog.ServicesDone)
                {
                    ServicesDone.Add(item);
                }
                foreach (var item in selectedServiceLog.SparesUsed)
                {
                    SparesUsed.Add(item);
                }
                OnPropertyChanged("SelectedServiceLog"); 
            } 
        }

        public MainViewModel()
        {
            FirstDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            SecondDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            Repairers = new ObservableCollection<Repairer>();
            Statuses = new ObservableCollection<Status>();
            ServiceLogs = new ObservableCollection<ServiceLog>();
            ParametersValues = new ObservableCollection<ParameterValue>();
            ServicesDone = new ObservableCollection<ServiceDone>();
            SparesUsed = new ObservableCollection<SpareUsed>();
            DbContext dbContext = new SQLiteContext();
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
                context.ServiceLog.Load();
                ServiceLogs = context.ServiceLog.Local.ToBindingList();
                context.Status.Load();
                Statuses = context.Status.Local.ToBindingList();
                context.Repairer.Load();
                Repairers = context.Repairer.Local.ToBindingList();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
