using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Views.Dialogs.ActionSheet;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Rg.Plugins.Popup.Services;
using System.Reactive.Threading.Tasks;
using Xamarin.Forms;
using BeautyPortionAdmin.Services;

namespace BeautyPortionAdmin.Views.Dialogs
{
    public class DialogActionSheetPageViewModel : ViewModelBase
    {
        private readonly IPopupPageService _dialogPageService;

        public DialogActionSheetPageViewModel(IPopupPageService dialogPageService) : base(null)
        {
            _dialogPageService = dialogPageService;
            Options = new ReactiveProperty<IEnumerable<ActionSheetOptionViewModel>>().AddTo(Disposables);
        }

        public ReactiveProperty<IEnumerable<ActionSheetOptionViewModel>> Options { get; }

        public override void Initialize(INavigationParameters parameters)
        {
            var param = parameters.GetValue<DialogActionSheetPageParameter>(nameof(DialogActionSheetPageParameter));

            Title = param.Title;
            Options.Value = param.Options.Select(x => new ActionSheetOptionViewModel(x, param.OnOptionSelected, _dialogPageService).AddTo(Disposables));
        }
    }

    public class ActionSheetOptionViewModel : IDisposable
    {
        private readonly ActionSheetOption _actionSheetOption;
        private readonly Action<ActionSheetOption> _optionSelected;
        private readonly IPopupPageService _dialogPageService;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly BusyNotifier _actionBusyNotifier = new BusyNotifier();

        public ActionSheetOptionViewModel(ActionSheetOption actionSheetOption,
                                          Action<ActionSheetOption> optionSelected,
                                          IPopupPageService dialogPageService)
        {
            _actionSheetOption = actionSheetOption;
            _optionSelected = optionSelected;
            _dialogPageService = dialogPageService;

            HeightSize = new ReactiveProperty<double>().AddTo(_disposables);
            WidthSize = new ReactiveProperty<double>().AddTo(_disposables);
            Icon = new ReactiveProperty<string>().AddTo(_disposables);
            FontFamily = new ReactiveProperty<string>().AddTo(_disposables);
            IconColor = new ReactiveProperty<Color>().AddTo(_disposables);

            SetIconProperties();

            SelectCommand = _actionBusyNotifier.Inverse()
                .ToReactiveCommand()
                .WithSubscribe(OnSelectCommand)
                .AddTo(_disposables);
        }

        public string Option => _actionSheetOption.Option;
        public ActionSheetType ActionSheetType => _actionSheetOption.ActionSheetType;
        public ReactiveProperty<string> Icon { get; }
        public ReactiveProperty<string> FontFamily { get; }
        public ReactiveProperty<Color> IconColor { get; }
        public ReactiveProperty<double> WidthSize { get; }
        public ReactiveProperty<double> HeightSize { get; }

        public ReactiveCommand SelectCommand { get; }

        private void SetIconProperties()
        {
            if (_actionSheetOption is FaIconActionSheetOption faIconActionSheetOption)
            {
                Icon.Value = faIconActionSheetOption.FaIcon;
                IconColor.Value = faIconActionSheetOption.IconColor;
                FontFamily.Value = faIconActionSheetOption.FontFamily;
                return;
            }
            
            if (_actionSheetOption is SvgIconActionSheetOption svgIconActionSheetOption)
            {
                Icon.Value = svgIconActionSheetOption.SvgIcon;
                WidthSize.Value = svgIconActionSheetOption.WidthSize;
                HeightSize.Value = svgIconActionSheetOption.HeightSize;
            }

        }

        private async void OnSelectCommand()
        {
            var busy = _actionBusyNotifier.ProcessStart();

            await _dialogPageService.Close();
            _optionSelected.Invoke(_actionSheetOption);

            busy.Dispose();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }

    public class DialogActionSheetPageParameter
    {
        public DialogActionSheetPageParameter(string title, IEnumerable<ActionSheetOption> options, Action<ActionSheetOption> onOptionSelected)
        {
            Title = title;
            Options = options;
            OnOptionSelected = onOptionSelected;
        }

        public string Title { get; }
        public IEnumerable<ActionSheetOption> Options { get; }
        public Action<ActionSheetOption> OnOptionSelected { get; }
    }
}
