using EService.Data.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EService.ViewModel
{
    public class DeptViewModel : INotifyPropertyChanged
    {
        private Dept selectedDept;
        public ObservableCollection<Dept> Depts { get; set; }

        public Dept SelectedDept { get { return selectedDept; } set { selectedDept = value; OnPropertyChanged("SelectedDept"); } }

        public DeptViewModel()
        {
            Depts = new ObservableCollection<Dept>();
            DbContext dbContext = new SQLiteContext();
            if(dbContext is SQLiteContext)
            {
                SQLiteContext context = new SQLiteContext();
                context = dbContext as SQLiteContext;
                foreach (var item in context.Dept)
                {
                    Depts.Add(item);
                }
            }
        }

        public void AddDept()
        {
            Dept dept = new Dept();
            Depts.Insert(0, dept);
            SelectedDept = dept;
        }

        public void DeleteDept()
        {
            if(selectedDept != null)
            {
                Depts.Remove(SelectedDept);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
