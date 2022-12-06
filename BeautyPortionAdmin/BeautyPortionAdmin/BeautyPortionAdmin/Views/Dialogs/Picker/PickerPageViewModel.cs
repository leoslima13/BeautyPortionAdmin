using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using BeautyPortionAdmin.Controls;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Services;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Views.Dialogs.Picker
{
    public class PickerPageViewModel : ViewModelBase
    {
        private readonly IPopupPageService _popupPageService;
        private PickerPageParameter _param;

        public PickerPageViewModel(IPopupPageService popupPageService) : base(null)
        {
            _popupPageService = popupPageService;

            SearchCriteria = new ReactiveProperty<string>().AddTo(Disposables);
            SearchPlaceholder = new ReactiveProperty<string>().AddTo(Disposables);
            Items = new ReactiveProperty<FilteredCollection<IPickerItem>>().AddTo(Disposables);
            DataTemplate = new ReactiveProperty<DataTemplate>().AddTo(Disposables);
            IsSelectableItem = new ReactiveProperty<bool>().AddTo(Disposables);

            CanSelectCommand = Items
                    .Where(x => x != null && x.Any())
                    .Select(x => x.OfType<ISelectableItem>())
                    .Select(ObserveSelectedItems)
                    .Switch()
                    .Select(_ => Items.Value.OfType<ISelectableItem>().Any(x => x.IsSelected.Value));

            CanConfirmCommand = Items
                    .Where(x => x != null && x.Any())
                    .Select(x => x.OfType<IQuantifiableItem>())
                    .Select(ObserveItemsQuantity)
                    .Switch()
                    .Select(_ => Items.Value.OfType<IQuantifiableItem>().Any(x => x.Quantity.Value > 0));

            SearchCriteria.Throttle(TimeSpan.FromMilliseconds(250))
                .WhereNotNull()
                .Subscribe(_ => Items.Value.Filtered?.Refresh(_collectionFilter))
                .AddTo(Disposables);

            SelectCommand = new ReactiveCommand(CanSelectCommand)
                .WithSubscribe(OnSelectCommand, Disposables);

            ConfirmCommand = new ReactiveCommand(CanConfirmCommand)
                    .WithSubscribe(OnConfirmCommand, Disposables);

            CancelCommand = new ReactiveCommand()
                .WithSubscribe(OnCancelCommand, Disposables);

            ClearCriteriaCommand = new ReactiveCommand()
                .WithSubscribe(() => SearchCriteria.Value = string.Empty, Disposables);
        }

        private IObservable<bool> CanSelectCommand { get; }
        private IObservable<bool> CanConfirmCommand { get; }

        private Func<IPickerItem, bool> _collectionFilter => f => string.IsNullOrWhiteSpace(SearchCriteria.Value) ||
                                                                 f.Name.ToLower().StartsWith(SearchCriteria.Value.ToLower());

        public ReactiveProperty<bool> IsSelectableItem { get; set; }
        public ReactiveProperty<string> SearchCriteria { get; }
        public ReactiveProperty<string> SearchPlaceholder { get; }
        public ReactiveProperty<FilteredCollection<IPickerItem>> Items { get; }
        public ReactiveProperty<DataTemplate> DataTemplate { get; }
        public ReactiveCommand SelectCommand { get; }
        public ReactiveCommand ConfirmCommand { get; }
        public ReactiveCommand CancelCommand { get; }
        public ReactiveCommand ClearCriteriaCommand { get; }

        public override void Initialize(INavigationParameters parameters)
        {
            _param = parameters.GetValue<PickerPageParameter>(nameof(PickerPageParameter));
            Title = _param.Title;
            DataTemplate.Value = _param.DataTemplate;
            SearchPlaceholder.Value = _param.SearchPlaceholder;

            if (_param.Items == null)
                return;

            if (_param.Items.OfType<ISelectableItem>().Any())
            {
                IsSelectableItem.Value = true;
                var items = _param.Items.Cast<ISelectableItem>();

                if (_param.SelectionMode == PickerSelectionMode.Single)
                    items.SetSingleSelection();

                Items.Value = new FilteredCollection<IPickerItem>(items.ToList(), _collectionFilter);
            }
            else
            {
                var items = _param.Items.Cast<IQuantifiableItem>();

                Items.Value = new FilteredCollection<IPickerItem>(items.ToList(), _collectionFilter);
            }
        }

        IObservable<bool> ObserveSelectedItems(IEnumerable<ISelectableItem> items)
        {
            return items.Select(x => x.IsSelected).Merge();
        }

        IObservable<int> ObserveItemsQuantity(IEnumerable<IQuantifiableItem> items)
        {
            return items.Select(x => x.Quantity).Merge();
        }

        async void OnSelectCommand()
        {
            await _popupPageService.Close();
            if (Items.Value == null)
                return;
            _param.OnSelectedItems(Items.Value.OfType<ISelectableItem>().Where(x => x.IsSelected.Value).ToList());
        }

        async void OnConfirmCommand()
        {
            await _popupPageService.Close();
            if (Items.Value == null)
                return;
            _param.OnSelectedItems(Items.Value.OfType<IQuantifiableItem>().Where(x => x.Quantity.Value > 0).ToList());
        }

        private async void OnCancelCommand()
        {
            await _popupPageService.Close();
        }
    }

    public class PickerPageParameter
    {
        public PickerPageParameter(string title,
                                   IEnumerable<IPickerItem> items,
                                   DataTemplate dataTemplate,
                                   PickerSelectionMode selectionMode,
                                   Action<IEnumerable<IPickerItem>> onSelectedItems,
                                   string searchPlaceholder)
        {
            Title = title;
            Items = items;
            DataTemplate = dataTemplate;
            SelectionMode = selectionMode;
            OnSelectedItems = onSelectedItems;
            SearchPlaceholder = searchPlaceholder;
        }

        public string Title { get; }
        public IEnumerable<IPickerItem> Items { get; }
        public DataTemplate DataTemplate { get; }
        public PickerSelectionMode SelectionMode { get; }
        public Action<IEnumerable<IPickerItem>> OnSelectedItems { get; }
        public string SearchPlaceholder { get; }
    }
}
