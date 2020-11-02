using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace EService.VVM.Navigation
{
    public sealed class Navigation
    {
        #region Константы
        public static readonly string ServiceLogAlias = "ServiceLog";
        public static readonly string TypeModelAlias = "TypeModel";
        public static readonly string ModelAlias = "Model";
        public static readonly string DeviceAlias = "Device";
        public static readonly string ParameterAlias = "Parameter";
        public static readonly string SpareAlias = "Spare";
        public static readonly string ServiceAlias = "Service";
        public static readonly string DeptAlias = "Dept";
        public static readonly string StatusAlias = "Status";
        public static readonly string CategoryAlias = "Category";
        public static readonly string UserAlias = "User";
        public static readonly string NotFoundPageAlias = "Page404";
        #endregion

        #region Поля

        private NavigationService _navService;
        private readonly IPageResolver _resolver;

        #endregion

        #region Свойства

        public static NavigationService Service
        {
            get { return Instance._navService; }
            set
            {
                if (Instance._navService != null)
                {
                    Instance._navService.Navigated -= Instance._navService_Navigated;
                }

                Instance._navService = value;
                Instance._navService.Navigated += Instance._navService_Navigated;
            }
        }

        #endregion


        #region Открытые методы

        public static void Navigate(Page page, object context)
        {
            if (Instance._navService == null || page == null)
            {
                return;
            }

            Instance._navService.Navigate(page, context);
            Navigation._instance.ClearHistory();
        }

        public static void Navigate(Page page)
        {
            Navigate(page, null);
            Navigation._instance.ClearHistory();
        }

        public static void Navigate(string uri, object context)
        {
            if (Instance._navService == null || uri == null)
            {
                return;
            }

            var page = Instance._resolver.GetPageInstance(uri);

            Navigate(page, context);
            Navigation._instance.ClearHistory();
        }

        public static void Navigate(string uri)
        {
            Navigate(uri, null);
            Navigation._instance.ClearHistory();
        }
        #endregion


        #region Закрытые методы

        void _navService_Navigated(object sender, NavigationEventArgs e)
        {
            var page = e.Content as Page;

            if (page == null)
            {
                return;
            }

            page.DataContext = e.ExtraData;
        }

        private void ClearHistory()
        {
            while (Navigation._instance._navService.CanGoBack)
            {
                Navigation._instance._navService.RemoveBackEntry();
            }            
        }

        #endregion


        #region Singleton

        private static volatile Navigation _instance;
        private static readonly object SyncRoot = new Object();

        private Navigation()
        {
            _resolver = new PagesResolver();
        }

        private static Navigation Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = new Navigation();
                    }
                }

                return _instance;
            }
        }
        #endregion
    }
}
