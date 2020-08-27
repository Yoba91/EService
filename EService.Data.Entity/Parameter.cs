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

    public partial class Parameter : INotifyPropertyChanged, IIdentifier
    {
        long rowid;
        string name;
        string unit;
        string defaultValue;
        ICollection<ParameterForModel> parametersForModels;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Parameter()
        {
            this.ParametersForModels = new HashSet<ParameterForModel>();
        }

        public long Rowid { get { return rowid; } set { rowid = value; OnPropertyChanged("Rowid"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public string Unit { get { return unit; } set { unit = value; OnPropertyChanged("Unit"); } }
        public string Default { get { return defaultValue; } set { defaultValue = value; OnPropertyChanged("Default"); } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ParameterForModel> ParametersForModels { get { return parametersForModels; } set { parametersForModels = value; OnPropertyChanged("ParametersForModels"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
