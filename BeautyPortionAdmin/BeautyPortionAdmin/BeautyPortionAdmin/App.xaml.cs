using Prism;
using Prism.Ioc;
using Xamarin.Essentials.Interfaces;
using Xamarin.Essentials.Implementation;
using Xamarin.Forms;
using Prism.DryIoc;
using BeautyPortionAdmin.Views.Home;
using BeautyPortionAdmin.Views.Login;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Views.Home.Tabs;
using BeautyPortionAdmin.Bootstraping;
using DryIoc;
using BeautyPortionAdmin.Views.Client;
using BeautyPortionAdmin.Views.Products;
using BeautyPortionAdmin.Views.Dialogs;
using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Services;
using BeautyPortionAdmin.Views.Orders;

namespace BeautyPortionAdmin
{
    public partial class App : PrismApplication
    {
        public App(IPlatformInitializer initializer)
            : base(initializer)
        {
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            Sharpnado.Tabs.Initializer.Initialize(loggerEnable: false, false);
            Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);

            await NavigationService.NavigateAsync(Pages.LoginPage.AsNavigation());
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.GetContainer().BootstrapTypes(new[] { typeof(App).Assembly });
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();
            containerRegistry.RegisterSingleton<IPopupNavigation>(() => PopupNavigation.Instance);

            containerRegistry.Register<ClientsTabViewModel>();
            containerRegistry.Register<OrdersTabViewModel>();
            containerRegistry.Register<ProductsTabViewModel>();
            containerRegistry.Register<ReportsTabViewModel>();
            containerRegistry.Register<DialogPage>();
            containerRegistry.Register<DialogPageViewModel>();
            containerRegistry.Register<DialogActionSheetPage>();
            containerRegistry.Register<DialogActionSheetPageViewModel>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigation<HomePage, HomePageViewModel>();
            containerRegistry.RegisterForNavigation<AddClientPage, AddClientPageViewModel>();
            containerRegistry.RegisterForNavigation<AddProductPage, AddProductPageViewModel>();
            containerRegistry.RegisterForNavigation<ProductDetailPage, ProductDetailPageViewModel>();
            containerRegistry.RegisterForNavigation<ClientDetailPage, ClientDetailPageViewModel>();
            containerRegistry.RegisterForNavigation<OrderDetailPage, OrderDetailPageViewModel>();
            containerRegistry.RegisterForNavigation<OrderFractionStepPage, OrderFractionStepPageViewModel>();
            containerRegistry.RegisterForNavigation<OrderDetailStepPage, OrderDetailStepPageViewModel>();
            containerRegistry.RegisterForNavigation<OrderSummaryStepPage, OrderSummaryStepPageViewModel>();
        }

    }
}
