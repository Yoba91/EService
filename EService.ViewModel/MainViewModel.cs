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
        public ObservableCollection<ServiceLog> ServiceLogs { get; set; }

        public ServiceLog SelectedServiceLog { get { return selectedServiceLog; } set { selectedServiceLog = value; OnPropertyChanged("SelectedServiceLog"); } }

        public MainViewModel()
        {
            ServiceLogs = new ObservableCollection<ServiceLog>();
            DbContext dbContext = new SQLiteContext();
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = new SQLiteContext();
                context = dbContext as SQLiteContext;
                foreach (var item in context.ServiceLog)
                {
                    ServiceLogs.Add(item);
                    item.Index = ServiceLogs.Count;
                }
            }
        }

        public void AddDept()
        {
            ServiceLog serviceLog = new ServiceLog();
            ServiceLogs.Insert(0, serviceLog);
            SelectedServiceLog = serviceLog;
        }

        public void DeleteDept()
        {
            if (SelectedServiceLog != null)
            {
                ServiceLogs.Remove(SelectedServiceLog);
            }
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
