using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Validators
{
    public class MinLengthValidator : ValidatorBase
    {
        public static readonly BindableProperty MinLengthProperty =
            BindableProperty.Create(nameof(MinLength), typeof(int), typeof(MinLengthValidator));

        public int MinLength
        {
            get => (int)GetValue(MinLengthProperty);
            set => SetValue(MinLengthProperty, value);
        }

        public override bool IsValid(string input)
        {
            return input?.Length >= MinLength;
        }
    }
}
