using EService.VVM.Views.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EService.VVM.Navigation
{
    public class PagesResolver : IPageResolver
    {

        private readonly Dictionary<string, Func<Page>> _pagesResolvers = new Dictionary<string, Func<Page>>();

        public PagesResolver()
        {
            _pagesResolvers.Add(Navigation.ServiceLogAlias, () => new ServiceLogPage());
            _pagesResolvers.Add(Navigation.TypeModelAlias, () => new TypeModelPage());
            _pagesResolvers.Add(Navigation.ModelAlias, () => new ModelPage());
            _pagesResolvers.Add(Navigation.DeviceAlias, () => new DevicePage());
            _pagesResolvers.Add(Navigation.ParameterAlias, () => new ParameterPage());
            _pagesResolvers.Add(Navigation.SpareAlias, () => new SparePage());
            _pagesResolvers.Add(Navigation.ServiceAlias, () => new ServicePage());
            _pagesResolvers.Add(Navigation.DeptAlias, () => new DeptPage());
            _pagesResolvers.Add(Navigation.StatusAlias, () => new StatusPage());
            _pagesResolvers.Add(Navigation.CategoryAlias, () => new CategoryPage());
            _pagesResolvers.Add(Navigation.UserAlias, () => new UserPage());
            _pagesResolvers.Add(Navigation.NotFoundPageAlias, () => new Page404());
        }

        public Page GetPageInstance(string alias)
        {
            if (_pagesResolvers.ContainsKey(alias))
            {
                return _pagesResolvers[alias]();
            }

            return _pagesResolvers[Navigation.NotFoundPageAlias]();
        }        
    }
}
