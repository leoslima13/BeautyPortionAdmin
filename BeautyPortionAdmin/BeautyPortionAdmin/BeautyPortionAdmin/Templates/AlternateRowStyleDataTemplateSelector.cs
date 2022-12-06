using System;
using System.Linq;
using BeautyPortionAdmin.Helpers;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Templates
{
    public class AlternateRowStyleDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Template { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (!(container is ItemsView collectionView))
                throw new Exception("This data template selector must be applied to a ItemsView");

            var items = collectionView.ItemsSource.Cast<IHasAlternateRowStyle>().ToList();
            var currentItem = item as IHasAlternateRowStyle;
            currentItem.HasAlternateRowStyle.Value = items.IndexOf(currentItem) % 2 == 0;

            return Template;
        }
    }
}
