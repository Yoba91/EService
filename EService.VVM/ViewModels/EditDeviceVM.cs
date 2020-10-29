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
    public class EditDeviceVM : BaseVM
    {
        #region Поля
        private String _inventoryNumber, _serialNumber;
        private Model _selectedModel;
        private Dept _selectedDept;
        private Status _selectedStatus;
        private DbContext _dbContext;
        private Device _device;

        private IDelegateCommand _editDevice;
        #endregion
        #region Свойства
        public String InventoryNumber { get { return _inventoryNumber; } set { _inventoryNumber = value; this.EditDevice.RaiseCanExecuteChanged(); } }
        public String SerialNumber { get { return _serialNumber; } set { _serialNumber = value; this.EditDevice.RaiseCanExecuteChanged(); } }
        public Model Model { get { return _selectedModel; } set { _selectedModel = value; this.EditDevice.RaiseCanExecuteChanged(); } }
        public Dept Dept { get { return _selectedDept; } set { _selectedDept = value; this.EditDevice.RaiseCanExecuteChanged(); } }
        public Status Status { get { return _selectedStatus; } set { _selectedStatus = value; this.EditDevice.RaiseCanExecuteChanged(); } }
        public IList<Model> Models { get; set; }
        public IList<Dept> Depts { get; set; }
        public IList<Status> Statuses { get; set; }
        public IDelegateCommand EditDevice
        {
            get
            {
                if (_editDevice == null)
                {
                    _editDevice = new DelegateCommand(OpenDialog, CanExecuteEdit);
                }
                return _editDevice;
            }
        }
        #endregion
        #region Конструкторы
        public EditDeviceVM(long rowid)
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
                _device = context.Device.Where(d => d.Rowid == rowid).SingleOrDefault();
                InventoryNumber = _device.InventoryNumber;
                SerialNumber = _device.SerialNumber;
                Model = _device.Model;
                Dept = _device.Dept;
                Status = _device.Status;
            }
        }
        #endregion
        #region Методы
        private void ExecuteEdit(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                Device device = context.Device.Where(d => d.Rowid == _device.Rowid).SingleOrDefault();
                device.InventoryNumber = InventoryNumber;
                device.SerialNumber = SerialNumber;
                device.Model = Model;
                device.Dept = Dept;
                device.Status = Status;
                context.SaveChanges();
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            if ((InventoryNumber != String.Empty) && (Model != null) && (Dept != null) && (Status != null))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM(String.Format("Изменить устройство с номером {0}({1})", _device.InventoryNumber, _device.SerialNumber), "Вы действительно хотите изменить выбранное устройство?", ExecuteEdit);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
