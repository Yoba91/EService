using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EService.VVM.ViewModels
{
    public class DialogVM : BaseVM
    {
        string title;
        string message;
        IDelegateCommand action, command, exit;


        public string Title { get { return title; } set { title = value; OnPropertyChanged("Title"); } }
        public string Message { get { return message; } set { message = value; OnPropertyChanged("Message"); } }

        public IDelegateCommand Command 
        { 
            get 
            {
                if (command == null)
                {
                    command = new DelegateCommand(ExecuteCommand);
                }
                return command; 
            } 
        }
        public IDelegateCommand Exit { get { return exit; } }

        private void ExecuteCommand(object parameter)
        {
            action.Execute(new object());
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            displayRootRegistry.MainVM.Refresh();
            Exit.Execute(parameter);
        }
        private void ExecuteExit(object parameter)
        {
            Window win = parameter as Window;
            win.Close();
        }

        private DialogVM()
        {

        }

        public DialogVM(string title, string message, Action<object> action)
        {
            Title = title;
            Message = message;
            this.action = new DelegateCommand(action);
            this.exit = new DelegateCommand(ExecuteExit);
        }
    }
}
