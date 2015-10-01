using System;
using System.Text.RegularExpressions;
using io.Data;

namespace io.Strings
{
    public static class Conversion
    {
        public static string ToPhone(this string value)
        {
            value = value.Replace(" ", "");
            if (value.Length == 10 && value.IsNumeric())
                return String.Format("{0:(###) ###-####}", double.Parse(value));
            else
                return value;
        }
    }
}
