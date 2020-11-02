using EService.BL;
using EService.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EService.VVM.ViewModels
{
    class AddDeviceVM : BaseVM
    {
        #region Поля
        private String _inventoryNumber, _serialNumber;
        private Model _selectedModel;
        private Dept _selectedDept;
        private Status _selectedStatus;
        private DbContext _dbContext;

        private IDelegateCommand _addDevice;
        #endregion
        #region Свойства
        public String InventoryNumber { get { return _inventoryNumber; } set { _inventoryNumber = value; this.AddDevice.RaiseCanExecuteChanged(); } }
        public String SerialNumber { get { return _serialNumber; } set { _serialNumber = value; this.AddDevice.RaiseCanExecuteChanged(); } }
        public Model Model { get { return _selectedModel; } set { _selectedModel = value; this.AddDevice.RaiseCanExecuteChanged(); } }
        public Dept Dept { get { return _selectedDept; } set { _selectedDept = value; this.AddDevice.RaiseCanExecuteChanged(); } }
        public Status Status { get { return _selectedStatus; } set { _selectedStatus = value; this.AddDevice.RaiseCanExecuteChanged(); } }
        public IList<Model> Models { get; set; }
        public IList<Dept> Depts { get; set; }
        public IList<Status> Statuses { get; set; }

        public IDelegateCommand AddDevice
        {
            get
            {
                if (_addDevice == null)
                {
                    _addDevice = new DelegateCommand(OpenDialog, CanExecuteAdd);
                }
                return _addDevice;
            }
        }

        #endregion
        #region Конструкторы
        public AddDeviceVM()
        {
            InventoryNumber = String.Empty;
            SerialNumber = String.Empty;
            Model = null;
            Dept = null;
            Status = null;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
            if (_dbContext is SQLiteContext)
            {
                var context = _dbContext as SQLiteContext;
                context.Model.Load();
                Models = context.Model.Local.ToBindingList();
                context.Dept.Load();
                Depts = context.Dept.Local.ToBindingList();
                context.Status.Load();
                Statuses = context.Status.Local.ToBindingList();
            }
        }
        #endregion
        #region Методы
        private void ExecuteAdd(object parameter)
        {
            Device device = new Device()
            {
                InventoryNumber = this.InventoryNumber,
                SerialNumber = this.SerialNumber,
                Model = this.Model,
                Dept = this.Dept,
                Status = this.Status
            };

            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Device.Add(device);
                context.SaveChanges();
                context.Configuration.LazyLoadingEnabled = true;
                context.Database.Initialize(true);
            }
        }

        private bool CanExecuteAdd(object parameter)
        {
            if ((InventoryNumber != String.Empty) && (Model != null) && (Dept != null) && (Status != null))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Новое устройство", "Вы действительно хотите добавить новое устройство?", ExecuteAdd);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
