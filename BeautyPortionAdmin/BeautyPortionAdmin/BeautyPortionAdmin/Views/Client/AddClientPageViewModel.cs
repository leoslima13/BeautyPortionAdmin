using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Models;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Reactive.Threading.Tasks;
using BeautyPortionAdmin.Services;
using Plugin.Media.Abstractions;
using System.IO;
using BeautyPortionAdmin.Views.Master;
using BeautyPortionAdmin.Views.Dialogs.ActionSheet;
using BeautyPortionAdmin.Fonts;
using BeautyPortionAdmin.Helpers;

namespace BeautyPortionAdmin.Views.Client
{
    public class AddClientPageViewModel : ViewModelBase, IHasFieldValidators, IHasMasterPage
    {
        private readonly IClientObservable _clientObservable;
        private readonly IMediaPickerService _mediaPickerService;
        private readonly IToastService _toastService;
        private readonly IPopupPageService _dialogPageService;
        private byte[] _fileBytes;
        private AddClientPageParameter _clientParam;

        public AddClientPageViewModel(INavigationService navigationService,
                                      IMasterPageViewModel masterPageViewModel,
                                      IFieldValidatorsObservable fieldValidatorsObservable,
                                      IClientObservable clientObservable,
                                      IMediaPickerService mediaPickerService,
                                      IToastService toastService,
                                      IPopupPageService dialogPageService) : base(navigationService)
        {
            MasterPageViewModel = masterPageViewModel;
            FieldValidatorsObservable = fieldValidatorsObservable;
            _clientObservable = clientObservable;
            _mediaPickerService = mediaPickerService;
            _toastService = toastService;
            _dialogPageService = dialogPageService;

            Title = "Adicionar Cliente";

            BusyNotifier = new BusyNotifier();
            Photo = new ReactiveProperty<ImageSource>().AddTo(Disposables);
            FirstName = new ReactiveProperty<string>().AddTo(Disposables);
            LastName = new ReactiveProperty<string>().AddTo(Disposables);
            Address = new ReactiveProperty<string>().AddTo(Disposables);
            Phone = new ReactiveProperty<string>().AddTo(Disposables);
            ClientIsFrom = new ReactiveProperty<ClientIsFrom?>().AddTo(Disposables);
            AddingClient = new ReactiveProperty<bool>(true).AddTo(Disposables);

            Initials = FirstName
                .WhereNotNull()
                .CombineLatest(LastName.WhereNotNull(), GetInitials)
                .StartWith("BP")
                .ToReactiveProperty()
                .AddTo(Disposables);

            AddClientCommand = FieldValidatorsObservable.ObserveFieldHasErrors
                .CombineLatest(ClientIsFrom, (hasErrors, clientFrom) => !hasErrors && clientFrom.HasValue)
                .ToReactiveCommand()
                .WithSubscribe(OnAddClientCommand, Disposables);

            EditClientCommand = FieldValidatorsObservable.ObserveFieldHasErrors
                .CombineLatest(ClientIsFrom, (hasErrors, clientFrom) => !hasErrors && clientFrom.HasValue)
                .ToReactiveCommand()
                .WithSubscribe(OnEditClientCommand, Disposables);

            AddPhotoCommand = new ReactiveCommand()
                .WithSubscribe(OnAddPhotoCommand, Disposables);
        }

        public IFieldValidatorsObservable FieldValidatorsObservable { get; }
        public IMasterPageViewModel MasterPageViewModel { get; }
        public BusyNotifier BusyNotifier { get; }

        public ReactiveProperty<bool> AddingClient { get; }
        public ReactiveProperty<string> Initials { get; }
        public ReactiveProperty<ImageSource> Photo { get; }
        public ReactiveProperty<string> FirstName { get; }
        public ReactiveProperty<string> LastName { get; }
        public ReactiveProperty<string> Address { get; }
        public ReactiveProperty<string> Phone { get; }
        public ReactiveProperty<ClientIsFrom?> ClientIsFrom { get; }
        public ReactiveCommand AddClientCommand { get; }
        public ReactiveCommand EditClientCommand { get; }
        public ReactiveCommand AddPhotoCommand { get; }

        public override void Initialize(INavigationParameters parameters)
        {
            _clientParam = parameters.GetValue<AddClientPageParameter>(nameof(AddClientPageParameter));

            if (_clientParam == null) return;

            AddingClient.Value = false;
            Title = "Editar Cliente";

            FirstName.Value = _clientParam.Client.FirstName;
            LastName.Value = _clientParam.Client.LastName;
            Address.Value = _clientParam.Client.Address;
            Phone.Value = _clientParam.Client.Phone;
            Photo.Value = _clientParam.Client.Photo?.ToImageSource();
            ClientIsFrom.Value = _clientParam.Client.ClientIsFrom;
        }

        private string GetInitials(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                return "BP";

            return $"{firstName[0].ToString().ToUpper()}{lastName[0].ToString().ToUpper()}";
        }

        private async void OnAddClientCommand()
        {
            var busy = BusyNotifier.ProcessStart();

            await Task.Delay(500); //Simulating api

            var client = new Models.Client
            {
                Address = Address.Value,
                FirstName = FirstName.Value,
                LastName = LastName.Value,
                Phone = Phone.Value,
                Photo = _fileBytes,
                ClientIsFrom = ClientIsFrom.Value.Value
            };
            _clientObservable.AddItem(client);

            busy.Dispose();
            _toastService.Success("Cliente Adicionado com sucesso!");
            var param = new NavigationParameters { { nameof(Models.Client), client } };
            await NavigationService.NavigateAsync($"../{Pages.ClientDetailPage}", param);
        }

        private async void OnEditClientCommand()
        {
            var busy = BusyNotifier.ProcessStart();

            await Task.Delay(500); //Simulating api

            var _client = _clientParam.Client;

            _client.FirstName = FirstName.Value;
            _client.LastName = LastName.Value;
            _client.Address = Address.Value;
            _client.Phone = Phone.Value;
            _client.Photo = _fileBytes ?? _client.Photo;
            _client.ClientIsFrom = ClientIsFrom.Value.Value;

            _clientObservable.UpdateItem(_client);

            busy.Dispose();
            _toastService.Success("Cliente Editado com sucesso!");
            NavigationService.GoBackAsync()
                .ToObservable()
                .Subscribe(_ => { _clientParam?.OnClientChanged(_client); });
        }

        private void OnAddPhotoCommand()
        {
            _dialogPageService.DisplayActionSheet("O que você deseja fazer", onOptionSelected,
                                                  new FaIconActionSheetOption("Tirar Foto", FaIcon.Camera),
                                                  new FaIconActionSheetOption("Escolher na Galeria", FaIcon.PhotoVideo));
        }

        private void onOptionSelected(ActionSheetOption selectedOption)
        {
            if(selectedOption.Option == "Tirar Foto")
            {
                TakePhoto();
                return;
            }

            PickPhoto();
        }

        void TakePhoto()
        {
            _mediaPickerService.TakePhoto()
                .ObserveOnUIDispatcher()
                .Subscribe(OnMediaPicked)
                .AddTo(Disposables);
        }

        void PickPhoto()
        {
            _mediaPickerService.PickPhoto()
               .ObserveOnUIDispatcher()
               .Subscribe(OnMediaPicked)
               .AddTo(Disposables);
        }


        void OnMediaPicked(MediaPickerResult mediaPickerResult)
        {
            if (mediaPickerResult.Status == MediaPickerStatus.Success)
            {
                Photo.Value = GetImageSource(mediaPickerResult.MediaFile);
            }

            ImageSource GetImageSource(MediaFile mediaFile)
            {
                var stream = mediaFile.GetStreamWithImageRotatedForExternalStorage();
                _fileBytes = CopyStreamToByte(stream);
                mediaFile.Dispose();
                return _fileBytes.ToImageSource();
            }

            byte[] CopyStreamToByte(Stream stream)
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray().ResizeImage();
                }
            }
        }
    }

    public class AddClientPageParameter
    {
        public AddClientPageParameter(Models.Client client, Action<Models.Client> onClientChanged)
        {
            Client = client;
            OnClientChanged = onClientChanged;
        }

        public Models.Client Client { get; }
        public Action<Models.Client> OnClientChanged { get; }
    }
}
