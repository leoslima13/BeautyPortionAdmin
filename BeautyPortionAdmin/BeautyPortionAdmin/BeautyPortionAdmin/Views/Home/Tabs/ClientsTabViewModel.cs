using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Models;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Xamarin.Forms;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Reactive.Disposables;
using Reactive.Bindings.Notifiers;

namespace BeautyPortionAdmin.Views.Home.Tabs
{
    public class ClientsTabViewModel : ViewModelBase
    {
        private readonly IClientObservable _clientObservable;
        private readonly BusyNotifier _busyNotifier = new BusyNotifier();

        public ClientsTabViewModel(INavigationService navigationService,
                                   IClientObservable clientObservable) : base(navigationService)
        {
            _clientObservable = clientObservable;

            Title = "Clientes";

            SearchCriteria = new ReactiveProperty<string>().AddTo(Disposables);

            Clients = _clientObservable.ObserveItems
                .Select(CreateClientViewModel)
                .ToReactiveProperty()
                .AddTo(Disposables);

            SearchCriteria.Throttle(TimeSpan.FromMilliseconds(250))
                .WhereNotNull()
                .Subscribe(_ => Clients.Value.Filtered?.Refresh(_collectionFilter))
                .AddTo(Disposables);

            ClearCriteriaCommand = new ReactiveCommand()
                .WithSubscribe(() => SearchCriteria.Value = string.Empty, Disposables);

            AddNewClientCommand = _busyNotifier.Inverse()
                .ToReactiveCommand()
                .WithSubscribe(OnAddNewClientCommand, Disposables);
            
        }

        public ReactiveProperty<bool> IsBusy { get; }
        public ReactiveProperty<FilteredCollection<ClientViewModel>> Clients { get; }
        public ReactiveProperty<string> SearchCriteria { get; }
        public ReactiveCommand ClearCriteriaCommand { get; }
        public ReactiveCommand AddNewClientCommand { get; }

        private Func<ClientViewModel, bool> _collectionFilter => f => string.IsNullOrWhiteSpace(SearchCriteria.Value) ||
                                                                 f.FullName.ToLower().StartsWith(SearchCriteria.Value.ToLower());

        private FilteredCollection<ClientViewModel> CreateClientViewModel(IEnumerable<Models.Client> clients)
        {
            return new FilteredCollection<ClientViewModel>(clients.Select(x => new ClientViewModel(x, NavigationService).AddTo(Disposables)).ToList(), _collectionFilter);
        }

        private void OnAddNewClientCommand()
        {
            var busy = _busyNotifier.ProcessStart();

            NavigationService.NavigateAsync(Pages.AddClientPage)
                .ToObservable()
                .Subscribe(_ => { busy.Dispose(); })
                .AddTo(Disposables);
        }
    }

    public class ClientViewModel : BindableBase, IDisposable
    {
        private readonly Models.Client _client;
        private readonly INavigationService _navigationService;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly BusyNotifier _busyNotifier = new BusyNotifier();

        public ClientViewModel(Models.Client client, INavigationService navigationService)
        {
            _client = client;
            _navigationService = navigationService;
            GoToDetailsCommand = _busyNotifier.Inverse()
                .ToReactiveCommand()
                .WithSubscribe(OnGoToDetailsCommand, _disposables);
        }

        public string FullName => $"{_client.FirstName} {_client.LastName}";
        public string Initials => $"{_client.FirstName[0].ToString().ToUpper()}{_client.LastName[0].ToString().ToUpper()}";
        public ImageSource Photo => _client.Photo?.ToImageSource();
        public string Phone => _client.Phone;
        public string Address => _client.Address;
        public ClientIsFrom ClientIsFrom => _client.ClientIsFrom;

        public ReactiveCommand GoToDetailsCommand { get; }

        private void OnGoToDetailsCommand()
        {
            var busy = _busyNotifier.ProcessStart();

            var parameters = new NavigationParameters
            {
                { nameof(Models.Client), _client }
            };

            _navigationService.NavigateAsync(Pages.ClientDetailPage, parameters)
                .ToObservable()
                .Subscribe(_ => { busy.Dispose(); })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
