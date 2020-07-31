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

    public partial class ParameterForModel : INotifyPropertyChanged, IIdentifier
    {
        long rowid;
        long rowidModel;
        long rowidParameters;
        Model model;
        Parameter parameter;
        ICollection<ParameterValue> parameterValues;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ParameterForModel()
        {
            this.ParameterValues = new HashSet<ParameterValue>();
        }
    
        public long Rowid { get { return rowid; } set { rowid = value; OnPropertyChanged("Rowid"); } }
        public long RowidModel { get { return rowidModel; } set { rowidModel = value; OnPropertyChanged("RowidModel"); } }
        public long RowidParameters { get { return rowidParameters; } set { rowidParameters = value; OnPropertyChanged("RowidParameters"); } }

        public virtual Model Model { get { return model; } set { model = value; OnPropertyChanged("Model"); } }
        public virtual Parameter Parameter { get { return parameter; } set { parameter = value; OnPropertyChanged("Parameter"); } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ParameterValue> ParameterValues { get { return parameterValues; } set { parameterValues = value; OnPropertyChanged("ParameterValues"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
