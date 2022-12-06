using BeautyPortionAdmin.Controls.Toast.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Controls.Toast
{
    public class ToastTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate _successTemplate = new DataTemplate(typeof(ToastSuccessView));
        private readonly DataTemplate _errorTemplate = new DataTemplate(typeof(ToastErrorView));

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (!(item is ToastViewModel viewModel))
                throw new ArgumentOutOfRangeException();

            switch (viewModel.Type)
            {
                case ToastType.Success:
                    return _successTemplate;
                case ToastType.Error:
                    return _errorTemplate;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
