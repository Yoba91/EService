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

    public partial class SpareUsed : INotifyPropertyChanged, IIdentifier
    {
        long rowid;
        long rowidSpareForModel;
        long rowidServiceLog;
        ServiceLog serviceLog;
        SpareForModel spareForModel;

        public long Rowid { get { return rowid; } set { rowid = value; OnPropertyChanged("Rowid"); } }
        public long RowidSpareForModel { get { return rowidSpareForModel; } set { rowidSpareForModel = value; OnPropertyChanged("RowidSpareForModel"); } }
        public long RowidServiceLog { get { return rowidServiceLog; } set { rowidServiceLog = value; OnPropertyChanged("RowidServiceLog"); } }

        public virtual ServiceLog ServiceLog { get { return serviceLog; } set { serviceLog = value; OnPropertyChanged("ServiceLog"); } }
        public virtual SpareForModel SpareForModel { get { return spareForModel; } set { spareForModel = value; OnPropertyChanged("SpareForModel"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
