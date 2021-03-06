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

    public partial class Device : INotifyPropertyChanged, IIdentifier
    {
        private long rowid;
        private long rowidModel;
        private long rowidDept;
        private long rowidStatus;
        private string serialNumber;
        private string inventoryNumber;

        private Dept dept;
        private Status status;
        private Model model;

        public ICollection<ServiceLog> serviceLogs;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Device()
        {
            this.ServiceLogs = new HashSet<ServiceLog>();
        }

        public long Rowid { get { return rowid; } set { rowid = value; OnPropertyChanged("Rowid"); } }
        public long RowidModel { get { return rowidModel; } set { rowidModel = value; OnPropertyChanged("RowidModel"); } }
        public long RowidDept { get { return rowidDept; } set { rowidDept = value; OnPropertyChanged("RowidDept"); } }
        public long RowidStatus { get { return rowidStatus; } set { rowidStatus = value; OnPropertyChanged("RowidStatus"); } }
        public string SerialNumber { get { return serialNumber; } set { serialNumber = value; OnPropertyChanged("SerialNumber"); } }
        public string InventoryNumber { get { return inventoryNumber; } set { inventoryNumber = value; OnPropertyChanged("InventoryNumber"); } }

        public virtual Dept Dept { get { return dept; } set { dept = value; OnPropertyChanged("Dept"); } }
        public virtual Status Status { get { return status; } set { status = value; OnPropertyChanged("Status"); } }
        public virtual Model Model { get { return model; } set { model = value; OnPropertyChanged("Model"); } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServiceLog> ServiceLogs { get { return serviceLogs; } set { serviceLogs = value; OnPropertyChanged("ServiceLogs"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
