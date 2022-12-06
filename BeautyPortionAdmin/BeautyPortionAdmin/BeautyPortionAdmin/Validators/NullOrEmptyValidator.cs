using System;
using System.Collections.Generic;
using System.Text;

namespace BeautyPortionAdmin.Validators
{
    public class NullOrEmptyValidator : ValidatorBase
    {
        public override bool IsValid(string input)
        {
            return !string.IsNullOrEmpty(input);
        }
    }
}
