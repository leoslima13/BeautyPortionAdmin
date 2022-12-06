using BeautyPortionAdmin.Framework;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeautyPortionAdmin.Views.Home.Tabs
{
    public class ReportsTabViewModel : ViewModelBase
    {
        public ReportsTabViewModel(INavigationService navigationService) : base(navigationService)
        {
            Title = "Relatórios";
        }
    }
}
