using BeautyPortionAdmin.Controls;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Behaviors
{
    public class CustomEntryValidatorBehavior : Behavior<CustomEntry>
    {
        public readonly BindableProperty ValidatorsProperty =
            BindableProperty.Create(nameof(Validators), typeof(List<ValidatorBase>), typeof(CustomEntryValidatorBehavior), new List<ValidatorBase>());

        public List<ValidatorBase> Validators
        {
            get { return (List<ValidatorBase>)GetValue(ValidatorsProperty); }
            set { SetValue(ValidatorsProperty, value); }
        }

        private CustomEntry _associatedObject;

        protected override void OnAttachedTo(CustomEntry bindable)
        {
            _associatedObject = bindable;

            _associatedObject.MyEntry.TextChanged += CustomEntry_TextChanged;
            _associatedObject.MyEntry.Unfocused += CustomEntry_Unfocused;
            _associatedObject.MyEntry.Focused += CustomEntry_Focused;

            _associatedObject.BindingContextChanged += CustomEntry_BindingContextChanged;

            base.OnAttachedTo(bindable);
        }

        protected override void OnDetachingFrom(CustomEntry bindable)
        {
            _associatedObject.MyEntry.TextChanged -= CustomEntry_TextChanged;
            _associatedObject.MyEntry.Unfocused -= CustomEntry_Unfocused;
            _associatedObject.MyEntry.Focused -= CustomEntry_Focused;

            _associatedObject.BindingContextChanged -= CustomEntry_BindingContextChanged;

            _associatedObject = null;

            base.OnDetachingFrom(bindable);
        }

        private bool HasErrors(string input)
        {
            if (Validators == null) return false;

            return Validators.Any(v => !v.IsValid(input));
        }

        private void ValidateAndShowErrors()
        {
            if (Validators == null) return;

            foreach (var item in Validators)
            {
                if(item.IsValid(_associatedObject.MyEntry.Text))
                {
                    _associatedObject.MessageLabel.Text = " ";

                    if (item.UseValidColor)
                        SetColors(item.ValidColor);

                    (_associatedObject.BindingContext as IHasFieldValidators)?.FieldValidatorsObservable.SetFieldError(_associatedObject.Id, false);
                }
                else
                {
                    _associatedObject.MessageLabel.Text = item.FailMessage;

                    if (item.UseValidColor)
                        SetColors(item.InvalidColor);

                    (_associatedObject.BindingContext as IHasFieldValidators)?.FieldValidatorsObservable.SetFieldError(_associatedObject.Id, true);
                }
            }
        }

        private void CustomEntry_BindingContextChanged(object sender, EventArgs e)
        {
            if(_associatedObject.BindingContext is IHasFieldValidators fieldValidators)
            {
                fieldValidators.FieldValidatorsObservable.SetFieldError(_associatedObject.Id, HasErrors(_associatedObject.Text));
            }
        }

        private void CustomEntry_Focused(object sender, FocusEventArgs e)
        {
            if (_associatedObject.MessageLabel.Text != " ")
                ValidateAndShowErrors();
        }

        private void CustomEntry_Unfocused(object sender, FocusEventArgs e) => ValidateAndShowErrors();

        private void CustomEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            (_associatedObject.BindingContext as IHasFieldValidators)?.FieldValidatorsObservable.SetFieldError(_associatedObject.Id, HasErrors(e.NewTextValue));

            if (_associatedObject.MyEntry.IsFocused)
                SetColors(Colors.PrimaryColor);

            _associatedObject.MessageLabel.Text = " ";
        }

        void SetColors(Color color)
        {
            _associatedObject.MessageLabel.TextColor = color;
            _associatedObject.UnderlineEntry.Color = color;
            _associatedObject.StackEntry.BackgroundColor = Color.FromRgba(color.R, color.G,
                                                                          color.B, .16);
        }
    }
}
