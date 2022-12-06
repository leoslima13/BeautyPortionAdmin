using System;
using BeautyPortionAdmin.Views.Dialogs.Picker;

namespace BeautyPortionAdmin.Extensions
{
    public static class ExtensionsOfQuantifiableItem
    {
        public static IQuantifiableItem ToQuantifiableItem(this object @this, string name, int quantity)
        {
            return new QuantifiableItem(name, @this, quantity);
        }
    }
}
