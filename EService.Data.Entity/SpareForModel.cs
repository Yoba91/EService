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

    public partial class SpareForModel : INotifyPropertyChanged, IIdentifier
    {
        long rowid;
        long rowidModel;
        long rowidSpare;
        Model model;
        Spare spare;
        ICollection<SpareUsed> sparesUsed;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SpareForModel()
        {
            this.SparesUsed = new HashSet<SpareUsed>();
        }

        public long Rowid { get { return rowid; } set { rowid = value; OnPropertyChanged("Rowid"); } }
        public long RowidModel { get { return rowidModel; } set { rowidModel = value; OnPropertyChanged("RowidModel"); } }
        public long RowidSpare { get { return rowidSpare; } set { rowidSpare = value; OnPropertyChanged("RowidSpare"); } }

        public virtual Model Model { get { return model; } set { model = value; OnPropertyChanged("Model"); } }
        public virtual Spare Spare { get { return spare; } set { spare = value; OnPropertyChanged("Spare"); } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SpareUsed> SparesUsed { get { return sparesUsed; } set { sparesUsed = value; OnPropertyChanged("SparesUsed"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
