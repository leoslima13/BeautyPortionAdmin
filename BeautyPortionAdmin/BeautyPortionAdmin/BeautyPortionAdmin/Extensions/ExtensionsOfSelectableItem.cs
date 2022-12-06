using System;
using System.Collections.Generic;
using BeautyPortionAdmin.Views.Dialogs.Picker;

namespace BeautyPortionAdmin.Extensions
{
    public static class ExtensionsOfSelectableItem
    {
        public static ISelectableItem ToSelectableItem(this object @this, string name, bool isSelected)
        {
            return new SelectableItem(name, @this, isSelected);
        }

        public static IDisposable SetSingleSelection(this IEnumerable<ISelectableItem> items)
        {
            return new SingleSelectManager(items);
        }
    }
}
