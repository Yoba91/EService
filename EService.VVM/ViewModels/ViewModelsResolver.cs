using EService.VVM.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EService.VVM.ViewModels
{
    public class ViewModelsResolver : IViewModelsResolver
    {
        private readonly Dictionary<string, Func<INotifyPropertyChanged>> _vmResolvers = new Dictionary<string, Func<INotifyPropertyChanged>>();

        public ViewModelsResolver()
        {
            _vmResolvers.Add(MainVM.ServiceLogVMAlias, () => new ServiceLogVM());
            _vmResolvers.Add(MainVM.TypeModelVMAlias, () => new TypeModelVM());
            _vmResolvers.Add(MainVM.ModelVMAlias, () => new ModelVM());
            _vmResolvers.Add(MainVM.DeviceVMAlias, () => new DeviceVM());
            _vmResolvers.Add(MainVM.ParameterVMAlias, () => new ParameterVM());
            _vmResolvers.Add(MainVM.SpareVMAlias, () => new SpareVM());
            _vmResolvers.Add(MainVM.ServiceVMAlias, () => new ServiceVM());
            _vmResolvers.Add(MainVM.DeptVMAlias, () => new DeptVM());
            _vmResolvers.Add(MainVM.StatusVMAlias, () => new StatusVM());
            _vmResolvers.Add(MainVM.NotFoundPageVMAlias, () => new Page404VM());
        }

        public INotifyPropertyChanged GetViewModelInstance(string alias)
        {
            if (_vmResolvers.ContainsKey(alias))
            {
                return _vmResolvers[alias]();
            }

            return _vmResolvers[MainVM.NotFoundPageVMAlias]();
        }
    }
}
