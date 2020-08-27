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

    public partial class Model : INotifyPropertyChanged, IIdentifier
    {
        private long rowid;
        private long rowidTypes;
        private string fullName;
        private string shortName;
        private ICollection<Device> devices;
        private TypeModel typeModel;
        private ICollection<ParameterForModel> parametersForModels;
        private ICollection<ServiceForModel> servicesForModels;
        private ICollection<SpareForModel> sparesForModels;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Model()
        {
            this.Devices = new HashSet<Device>();
            this.ParametersForModels = new HashSet<ParameterForModel>();
            this.ServicesForModels = new HashSet<ServiceForModel>();
            this.SparesForModels = new HashSet<SpareForModel>();
        }

        public long Rowid { get { return rowid; } set { rowid = value; OnPropertyChanged("Rowid"); } }
        public long RowidTypes { get { return rowidTypes; } set { rowidTypes = value; OnPropertyChanged("RowidTypes"); } }
        public string FullName { get { return fullName; } set { fullName = value; OnPropertyChanged("FullName"); } }
        public string ShortName { get { return shortName; } set { shortName = value; OnPropertyChanged("ShortName"); } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Device> Devices { get { return devices; } set { devices = value; OnPropertyChanged("Devices"); } }
        public virtual TypeModel TypeModel { get { return typeModel; } set { typeModel = value; OnPropertyChanged("TypeModel"); } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ParameterForModel> ParametersForModels { get { return parametersForModels; } set { parametersForModels = value; OnPropertyChanged("ParametersForModels"); } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServiceForModel> ServicesForModels { get { return servicesForModels; } set { servicesForModels = value; OnPropertyChanged("ServicesForModels"); } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SpareForModel> SparesForModels { get { return sparesForModels; } set { sparesForModels = value; OnPropertyChanged("SparesForModels"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
