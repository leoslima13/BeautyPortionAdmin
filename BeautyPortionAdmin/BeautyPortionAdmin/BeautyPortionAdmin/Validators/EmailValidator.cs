using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BeautyPortionAdmin.Validators
{
    public class EmailValidator : ValidatorBase
    {
        public const string EMAIL_REGEX_PATTERN = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
                                                + "@"
                                                + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";

        public bool AllowsEmptyValue { get; set; }

        public override bool IsValid(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return AllowsEmptyValue;

            return Regex.IsMatch(input, EMAIL_REGEX_PATTERN);
        }
    }
}
