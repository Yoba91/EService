using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EService.VVM.ViewModels
{
    public class BaseVM : INotifyPropertyChanged
    {
        public virtual void Refresh()
        { }
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        protected class OpenWindowCommand : DelegateCommand
        {
            public OpenWindowCommand(Action<object> execute, BaseVM main) : base(execute)
            {
            }

            public OpenWindowCommand(Action<object> execute, Func<object, bool> canExecute, BaseVM main) : base(execute, canExecute)
            {
            }

            public override void Execute(object parameter)
            {
                base.Execute(parameter);
            }

            public override bool CanExecute(object parameter)
            {
                return base.CanExecute(parameter);
            }
        }
    }
}
