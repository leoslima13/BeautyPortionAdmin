using System;
using BeautyPortionAdmin.Views.Dialogs.ActionSheet.Views;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Views.Dialogs.ActionSheet
{
    public class ActionSheetViewTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate _defaultActionSheetTemplate = new DefaultActionSheetView();
        private readonly DataTemplate _faIconActionSheetTemplate = new FaIconActionSheetView();
        private readonly DataTemplate _svgActionSheetTemplate = new SvgIconActionSheetView();

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (!(item is ActionSheetOptionViewModel actionSheetOptionViewModel))
                throw new ArgumentOutOfRangeException();

            return actionSheetOptionViewModel.ActionSheetType switch
            {
                ActionSheetType.Default => _defaultActionSheetTemplate,
                ActionSheetType.FaIcon => _faIconActionSheetTemplate,
                ActionSheetType.SvgIcon => _svgActionSheetTemplate,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
}
