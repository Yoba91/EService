//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EService.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public partial class ServiceLog : INotifyPropertyChanged, IIdentifier
    {
        long rowid;
        long rowidDevice;
        long rowidRepairer;
        DateTime date;

        Device device;
        ICollection<ParameterValue> parametersValues;
        Repairer repairer;
        ICollection<ServiceDone> servicesDone;
        ICollection<SpareUsed> sparesUsed;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ServiceLog()
        {
            this.ParametersValues = new HashSet<ParameterValue>();
            this.ServicesDone = new HashSet<ServiceDone>();
            this.SparesUsed = new HashSet<SpareUsed>();
        }
        public long Rowid { get { return rowid; } set { rowid = value; OnPropertyChanged("Rowid"); } }
        public long RowidDevice { get { return rowidDevice; } set { rowidDevice = value; OnPropertyChanged("RowidDevice"); } }
        public long RowidRepairer { get { return rowidRepairer; } set { rowidRepairer = value; OnPropertyChanged("RowidRepairer"); } }
        public string Date { get { return date.ToShortDateString(); } set { date = new DateTime(int.Parse(value.Split('.')[2]), int.Parse(value.Split('.')[1]), int.Parse(value.Split('.')[0])); OnPropertyChanged("Date"); } }
        public DateTime DateTime { get { return date; } }
        public virtual Device Device { get { return device; } set { device = value; OnPropertyChanged("Device"); } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ParameterValue> ParametersValues { get { return parametersValues; } set { parametersValues = value; OnPropertyChanged("ParametersValues"); } }
        public virtual Repairer Repairer { get { return repairer; } set { repairer = value; OnPropertyChanged("Repairer"); } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServiceDone> ServicesDone { get { return servicesDone; } set { servicesDone = value; OnPropertyChanged("ServicesDone"); } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SpareUsed> SparesUsed { get { return sparesUsed; } set { sparesUsed = value; OnPropertyChanged("SparesUsed"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
