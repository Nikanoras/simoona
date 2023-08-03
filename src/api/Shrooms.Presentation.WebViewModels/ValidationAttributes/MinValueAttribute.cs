using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MinValueAttribute : ValidationAttribute
    {
        private readonly int _minValue;

        public MinValueAttribute(int minValue)
        {
            _minValue = minValue;
        }

        public override bool IsValid(object value)
        {
            return _minValue <= (int)value;
        }

        public override string FormatErrorMessage(string name)
        {
            return "Incorrect value";
        }
    }
}