using System;
using System.Reactive.Threading.Tasks;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Fonts;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Models;
using BeautyPortionAdmin.Services;
using BeautyPortionAdmin.Views.Dialogs.ActionSheet;
using BeautyPortionAdmin.Views.Master;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Views.Client
{
    public class ClientDetailPageViewModel : ViewModelBase, IHasMasterPage
    {
        private readonly IPopupPageService _popupPageService;
        private readonly IOrderObservable _orderObservable;
        private Models.Client _client;

        public ClientDetailPageViewModel(INavigationService navigationService,
                                         IMasterPageViewModel masterPageViewModel,
                                         IPopupPageService popupPageService,
                                         IOrderObservable orderObservable) : base(navigationService)
        {
            Title = "Detalhes do Cliente";

            MasterPageViewModel = masterPageViewModel;
            _popupPageService = popupPageService;
            _orderObservable = orderObservable;

            Initials = new ReactiveProperty<string>().AddTo(Disposables);
            Photo = new ReactiveProperty<ImageSource>().AddTo(Disposables);
            ClientName = new ReactiveProperty<string>().AddTo(Disposables);
            Phone = new ReactiveProperty<string>().AddTo(Disposables);
            Address = new ReactiveProperty<string>().AddTo(Disposables);

            ActionMenuCommand = new ReactiveCommand()
                .WithSubscribe(OnActionMenuCommand, Disposables);

            AddOrderCommand = new ReactiveCommand()
                .WithSubscribe(OnAddOrderCommand, Disposables);
        }

        public IMasterPageViewModel MasterPageViewModel { get; }

        public ReactiveProperty<string> Initials { get; }
        public ReactiveProperty<ImageSource> Photo { get; }
        public ReactiveProperty<string> ClientName { get; }
        public ReactiveProperty<string> Phone { get; }
        public ReactiveProperty<string> Address { get; }

        public ReactiveCommand ActionMenuCommand { get; }
        public ReactiveCommand AddOrderCommand { get; }

        public override void Initialize(INavigationParameters parameters)
        {
            _client = parameters.GetValue<Models.Client>(nameof(Models.Client));

            if (_client == null)
                throw new ArgumentNullException("Client can't be null");

            ClientName.Value = $"{_client.FirstName} {_client.LastName}";
            Initials.Value = $"{_client.FirstName.ToUpper()[0]}{_client.LastName.ToUpper()[0]}";
            Photo.Value = _client.Photo?.ToImageSource();
            Phone.Value = _client.Phone;
            Address.Value = _client.Address;
        }

        void OnActionMenuCommand()
        {
            _popupPageService.DisplayActionSheet("Ações para o cliente", OnOptionSelected,
                                                new FaIconActionSheetOption("Adicionar Pedido", FaIcon.Gift),
                                                new FaIconActionSheetOption("Editar", FaIcon.Edit),
                                                new FaIconActionSheetOption("Excluir", FaIcon.Trash));

            void OnOptionSelected(ActionSheetOption selectedOption)
            {
                if(selectedOption.Option == "Editar")
                {
                    var addClientPage = new AddClientPageParameter(_client, OnClientChanged);
                    var navParam = new NavigationParameters
                    {
                        { nameof(AddClientPageParameter), addClientPage }
                    };

                    NavigationService.NavigateAsync(Pages.AddClientPage, navParam)
                        .ToObservable()
                        .Subscribe();
                    return;
                }
            }
        }

        void OnClientChanged(Models.Client client)
        {
            _client = client;

            ClientName.Value = $"{client.FirstName} {client.LastName}";
            Initials.Value = $"{client.FirstName.ToUpper()[0]}{client.LastName.ToUpper()[0]}";
            Photo.Value = client.Photo?.ToImageSource();
            Phone.Value = client.Phone;
            Address.Value = client.Address;
        }

        void OnAddOrderCommand()
        {
            _orderObservable.SetClient(_client);
            NavigationService.NavigateAsync(Pages.OrderDetailPage)
                .ToObservable()
                .Subscribe()
                .AddTo(Disposables);
        }
    }
}
