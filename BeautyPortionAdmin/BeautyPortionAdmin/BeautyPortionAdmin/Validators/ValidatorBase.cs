using BeautyPortionAdmin.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Validators
{
    public abstract class ValidatorBase : BindableObject
    {
        public static readonly BindableProperty FailMessageProperty =
            BindableProperty.Create(nameof(FailMessage), typeof(string), typeof(ValidatorBase), " ");

        public static readonly BindableProperty UseValidColorProperty =
            BindableProperty.Create(nameof(UseValidColor), typeof(bool), typeof(ValidatorBase), true);

        public static readonly BindableProperty InvalidColorProperty =
            BindableProperty.Create(nameof(InvalidColor), typeof(Color), typeof(ValidatorBase), Colors.ErrorColor);

        public static readonly BindableProperty ValidColorProperty =
            BindableProperty.Create(nameof(ValidColor), typeof(Color), typeof(ValidatorBase), Colors.PrimaryColor);

        public string FailMessage
        {
            get { return (string)GetValue(FailMessageProperty); }
            set { SetValue(FailMessageProperty, value); }
        }

        public bool UseValidColor
        {
            get { return (bool)GetValue(UseValidColorProperty); }
            set { SetValue(UseValidColorProperty, value); }
        }

        public Color InvalidColor
        {
            get { return (Color)GetValue(InvalidColorProperty); }
            set { SetValue(InvalidColorProperty, value); }
        }

        public Color ValidColor
        {
            get { return (Color)GetValue(ValidColorProperty); }
            set { SetValue(ValidColorProperty, value); }
        }

        public abstract bool IsValid(string input);
    }
}
