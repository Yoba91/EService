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

    public partial class ServiceCategory : INotifyPropertyChanged, IIdentifier
    {
        long rowid;
        string name;
        string range;
        int minValue, maxValue;

        public long Rowid { get { return rowid; } set { rowid = value; OnPropertyChanged("Rowid"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public string Range { get { return range; } set { range = value; OnPropertyChanged("Range"); } }
        public int MinValue { get { int.TryParse(range.Split('_')[0], out minValue); return minValue; } private set { minValue = value; } }
        public int MaxValue { get { int.TryParse(range.Split('_')[1], out maxValue); return maxValue; } private set { maxValue = value; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
