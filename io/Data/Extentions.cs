using System;
using System.Text.RegularExpressions;

namespace io.Data
{
    public static class Conversion
    {
        public static Int32 ToInt32(this object value)
        {
            if (value != null)
                return value.ToString().ToInt32();
            else
                return 0;
        }

        public static Int32 ToInt32(this string value)
        {
            if (Validation.IsNumeric(value))
                return Convert.ToInt32(Math.Round(Convert.ToDouble(value)));
            else
                return Convert.ToInt32(Math.Round(Convert.ToDouble(0)));
        }

        public static Int64 ToInt64(this object value)
        {
            if (value != null)
                return value.ToString().ToInt64();
            else
                return 0;
        }

        public static Int64 ToInt64(this string value)
        {
            if (Validation.IsNumeric(value))
                return Convert.ToInt64(Math.Round(Convert.ToDouble(value)));
            else
                return Convert.ToInt64(Math.Round(Convert.ToDouble(0)));
        }

        public static string ToString(string value)
        {
            try
            {
                return value == null ? "" : value;
            }
            catch
            {
                return "";
            }
        }

        public static Double ToDouble(this object value)
        {
            if (value != null)
                return value.ToString().ToDouble();
            else
                return 0;
        }

        public static Double ToDouble(this string value)
        {
            if (Validation.IsNumeric(value))
                return Convert.ToDouble(value);
            else
                return Convert.ToDouble(0);
        }

        public static Decimal ToDecimal(this object value, int decimals)
        {
            if (value != null)
                return value.ToString().ToDecimal(decimals);
            else
                return 0M;
        }

        public static Decimal ToDecimal(this string value, int decimals)
        {
            if (Validation.IsNumeric(value))
                return Convert.ToDecimal(Math.Round(Convert.ToDouble(value), decimals));
            else
                return Convert.ToDecimal(Math.Round(Convert.ToDouble(0), decimals));
        }

        public static DateTime ToDate(this string value)
        {
            if (value.IsDate())
                return Convert.ToDateTime(Convert.ToDateTime(value).ToString("MM/dd/yyyy"));
            else
                return DateTime.MinValue;
        }

        public static DateTime ToDate(this object value)
        {
            if (value != null)
            {
                if (value.ToString().IsDate())
                    return Convert.ToDateTime(Convert.ToDateTime(value.ToString()).ToString("MM/dd/yyyy"));
                else
                    return DateTime.MinValue;
            }
            else
                return DateTime.MinValue;
        }


        public static DateTime ToDateTime(this string value)
        {
            if (value.IsDate())
                return Convert.ToDateTime(value);
            else
                return DateTime.MinValue;
        }

        public static DateTime ToDateTime(this object value)
        {
            if (value != null)
            {
                if (value.ToString().IsDate())
                    return Convert.ToDateTime(value.ToString());
                else
                    return DateTime.MinValue;
            }
            else
                return DateTime.MinValue;
        }
    }

    public static class Validation
    {
        public enum StringFormat
        {
            Plain = 0,
            Email = 1,
            PhoneNumber = 2,
            SSN = 3,
            Date = 4,
            DateTime = 5,
            TitleCase = 6,
            ZipCode = 9,
            TrueTypeDecimalPlain = 7,
            TrueTypeDecimalMoney = 8,
            TrueTypeInteger = 10,
            TrueTypeDouble = 11
        }

        public enum DecimalFormat
        {
            Plain = 0,
            Money = 1
        }

        public enum NumberFormat
        {
            Decimal = 0,
            Money = 1,
            Integer = 2,
            Double = 3,
            Percent = 4
        }

        public static bool RegexMatches(string value, string exp)
        {
            Regex regex = new Regex(exp);
            return (regex.Match(value).Success);
        }

        public static bool RegexMatches(string value, StringFormat format)
        {
            switch (format)
            {
                case StringFormat.Plain:
                    return true;
                case StringFormat.Email:
                    return RegexMatches(value, @"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}");
                case StringFormat.PhoneNumber:
                    return RegexMatches(value, @"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}");
                case StringFormat.SSN:
                    return RegexMatches(value, @"^(?!(000|666|9))\d{3}-(?!00)\d{2}-(?!0000)\d{4}$");
                case StringFormat.Date:
                    return IsDate(value);
                case StringFormat.DateTime:
                    return IsDate(value);
                case StringFormat.TitleCase:
                    return false;
                case StringFormat.TrueTypeDecimalPlain:
                    return IsNumeric(value);
                case StringFormat.TrueTypeDecimalMoney:
                    return IsNumeric(value);
                case StringFormat.ZipCode:
                    return RegexMatches(value, @"(^(?!0{5})(\d{5})(?!-?0{4})(|-\d{4})?$)");
                default:
                    return false;
            }
        }

        public static string ToString(this string value)
        {
            return value == null ? "" : value;
        }

        public static bool IsDate(this string value)
        {
            DateTime aDate;
            return (DateTime.TryParse(value, out aDate));
        }

        public static bool IsNumeric(string value)
        {
            Double aNum;
            try
            {
                return Double.TryParse(value.ToString(), out aNum);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsNumeric(this object value)
        {
            Double aNum;
            try
            {
                return Double.TryParse(value.ToString(), out aNum);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsGUID(string id)
        {
            Guid newGUID = default(Guid);
            return Guid.TryParse(id, out newGUID);
        }

        public static UIData<System.DateTime> Validate(this UIData<System.DateTime> item)
        {
            item.SubmittedValue = item.Value;
            item.IsValid = Constants.YES;
            item.Message = string.Empty;
            return item;
        }

        public static UIData<bool> Validate(this UIData<bool> item)
        {
            if (item == null) { item = new UIData<bool>(false, Types.Boolean); }

            item.SubmittedValue = item.Value;
            item.IsValid = Constants.YES;
            item.Message = string.Empty;
            return item;
        }

        public static UIData<int> Validate(this UIData<int> item, string requiredMessage = "")
        {
            if (item == null) { item = new UIData<int>(0, Types.Integer); }

            bool isGood = Constants.YES;
            string message = string.Empty;

            bool required = (requiredMessage.Length != 0);

            if (required && item.Value == 0)
            {
                isGood = Constants.NO;
                message = (requiredMessage.Length == 0 ? "Value required" : requiredMessage);
            }

            item.IsValid = isGood;
            item.Message = message;

            return item;
        }

        public static UIData<double> Validate(this UIData<double> item, string requiredMessage = "")
        {
            if (item == null) { item = new UIData<double>(0, Types.Double); }

            bool isGood = Constants.YES;
            string message = string.Empty;

            bool required = (requiredMessage.Length != 0);

            if (required && item.Value == 0)
            {
                isGood = Constants.NO;
                message = (requiredMessage.Length == 0 ? "Value required" : requiredMessage);
            }

            item.IsValid = isGood;
            item.Message = message;

            return item;
        }

        public static UIData<decimal> Validate(this UIData<decimal> item, DecimalFormat format = DecimalFormat.Plain, string requiredMessage = "")
        {
            if (item == null) { item = new UIData<decimal>(0, Types.Decimal); }

            bool isGood = Constants.YES;
            string message = string.Empty;

            bool required = (requiredMessage.Length != 0);

            if (required && item.Value == 0)
            {
                isGood = Constants.NO;
                message = (requiredMessage.Length == 0 ? "Value required" : requiredMessage);
            }

            item.IsValid = isGood;
            item.Message = message;

            return item;
        }

        public static UIData<string> Validate(this UIData<string> item, StringFormat format = StringFormat.Plain, string requiredMessage = "", int maxLength = 0, bool trim = true, int roundingDecimals = 0)
        {
            if (item == null) { item = new UIData<string>("", Types.String); }

            item.Value = item.Value ?? "";

            bool isRequired = (requiredMessage.Length != 0);
            item.SubmittedValue = item.Value;

            if (item.TrueType == Types.Date)
                if (format != StringFormat.Date) { format = StringFormat.Date; }

            if (item.TrueType == Types.DateTime)
                if (format != StringFormat.DateTime) { format = StringFormat.DateTime; }

            if (format == StringFormat.TrueTypeDecimalPlain || format == StringFormat.TrueTypeDecimalMoney)
                item.TrueType = Types.Decimal;

            if (format == StringFormat.TrueTypeDouble)
                item.TrueType = Types.Double;

            if (format == StringFormat.TrueTypeInteger)
                item.TrueType = Types.Integer;

            switch (item.TrueType)
            {
                case Types.Integer:
                    item.Value = item.Value.Replace("$", "");
                    if (item.Value.Length == 0) { item.Value = "0"; }
                    if (IsNumeric(item.Value))
                    {
                        UIData<int> intData = new UIData<int>(Conversion.ToInt32(item.Value), Types.Integer);
                        intData.Validate(requiredMessage);
                        intData.CopyValuesTo(item);
                    }
                    else
                    {
                        item.IsValid = Constants.NO;
                        item.Message = (requiredMessage.Length == 0 ? "Invalid number" : requiredMessage);
                    }
                    break;
                case Types.Double:
                    item.Value = item.Value.Replace("$", "");
                    if (item.Value.Length == 0) { item.Value = "0"; }
                    if (IsNumeric(item.Value))
                    {
                        UIData<double> doubleData = new UIData<double>(Conversion.ToDouble(item.Value), Types.Double);
                        doubleData.Validate(requiredMessage);
                        doubleData.CopyValuesTo(item);
                    }
                    else
                    {
                        item.IsValid = Constants.NO;
                        item.Message = (requiredMessage.Length == 0 ? "Invalid number" : requiredMessage);
                    }
                    break;
                case Types.Decimal:
                    item.Value = item.Value.Replace("$", "");
                    if (item.Value.Length == 0) { item.Value = "0"; }
                    if (IsNumeric(item.Value))
                    {
                        UIData<decimal> decData = new UIData<decimal>(Conversion.ToDecimal(item.Value, roundingDecimals), Types.Decimal);
                        if (format == StringFormat.TrueTypeDecimalMoney)
                        {
                            decData.Validate(DecimalFormat.Money, requiredMessage);
                            decData.CopyValuesTo(item);
                            item.Value = decData.Value.ToString();
                        }
                        else
                        {
                            decData.Validate(DecimalFormat.Plain, requiredMessage);
                            decData.CopyValuesTo(item);
                        }
                    }
                    else
                    {
                        item.IsValid = Constants.NO;
                        item.Message = (requiredMessage.Length == 0 ? "Invalid number" : requiredMessage);
                    }
                    break;
                case Types.Boolean:
                    try
                    {
                        if (Convert.ToBoolean(item.Value) == true | Convert.ToBoolean(item.Value) == false)
                        {
                            UIData<bool> boolData = new UIData<bool>(Convert.ToBoolean(item.Value), Types.Decimal);
                            boolData.Validate();
                            boolData.CopyValuesTo(item);
                        }
                        else
                        {
                            item.IsValid = Constants.NO;
                            item.Message = (requiredMessage.Length == 0 ? "Invalid boolean value" : requiredMessage);
                        }
                    }
                    catch
                    {
                        item.IsValid = Constants.NO;
                        item.Message = (requiredMessage.Length == 0 ? "Invalid boolean value" : requiredMessage);
                    }
                    break;
                default:
                    if (trim) item.Value = item.Value.Trim();
                    ValidatedData result = new ValidatedData();

                    switch (format)
                    {
                        case StringFormat.Plain:
                            result = ValidatePlain(item, maxLength, requiredMessage);
                            break;
                        case StringFormat.TitleCase:
                            result = ValidateTitleCase(item, maxLength, requiredMessage);
                            break;
                        case StringFormat.Email:
                            result = ValidateEmail(item, maxLength, requiredMessage);
                            break;
                        case StringFormat.PhoneNumber:
                            result = ValidatePhoneNumber(item, requiredMessage);
                            break;
                        case StringFormat.SSN:
                            result = ValidateSSN(item, requiredMessage);
                            break;
                        case StringFormat.Date:
                            result = ValidateDate(item, requiredMessage);
                            break;
                        case StringFormat.DateTime:
                            result = ValidateDateTime(item, requiredMessage);
                            break;
                        case StringFormat.ZipCode:
                            result = ValidateZipcode(item, requiredMessage);
                            break;
                    }

                    item.IsValid = result.IsGood;
                    item.Message = result.Message;
                    item.Value = result.NewValue;
                    break;
            }

            return item;
        }

        private struct ValidatedData
        {
            public string NewValue;
            public string Message;
            public string Warning;
            public bool IsGood;
            public bool ContinueEvaluate;
        }

        private static ValidatedData ValidateGeneral(UIData<string> item, int maxLength, string requiredMessage)
        {
            ValidatedData result = default(ValidatedData);
            result.NewValue = item.Value;
            result.Message = "";
            result.Warning = "";
            result.IsGood = true;
            result.ContinueEvaluate = true;

            bool isRequired = requiredMessage.Length > 0;

            if (item.Value.Length == 0 & isRequired)
            {
                result.Message = requiredMessage;
                result.Warning = "Value was empty";
                result.IsGood = false;
                result.ContinueEvaluate = false;
            }
            else
            {
                if (maxLength > 0 & item.Value.Length > maxLength)
                {
                    result.Message = "Max. " + maxLength + " characters";
                    result.Warning = "Value was " + item.Value.Length + " characters";
                    result.IsGood = false;
                    result.ContinueEvaluate = false;
                }
                else
                {
                    if (item.Value == string.Empty)
                    {
                        result.IsGood = true;
                        result.ContinueEvaluate = false;
                    }
                }
            }

            if (SQLInjection(item.Value))
            {
                result.Message = "Invalid string";
                result.Warning = "String contains sql injection characters.";
                result.IsGood = false;
                result.ContinueEvaluate = false;
            }

            return result;
        }

        private static ValidatedData ValidatePlain(UIData<string> item, int maxLength, string requiredMessage)
        {
            return ValidateGeneral(item, maxLength, requiredMessage);
        }

        private static ValidatedData ValidateTitleCase(UIData<string> item, int maxLength, string requiredMessage)
        {
            ValidatedData result = ValidateGeneral(item, maxLength, requiredMessage);
            if (result.ContinueEvaluate)
            {
                System.Globalization.CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
                System.Globalization.TextInfo textInfo = cultureInfo.TextInfo;
                result.NewValue = textInfo.ToTitleCase(item.Value);
            }
            return result;
        }

        private static ValidatedData ValidateEmail(UIData<string> item, int maxLength, string requiredMessage)
        {
            ValidatedData result = ValidateGeneral(item, maxLength, requiredMessage);
            if (result.ContinueEvaluate)
            {
                if (!RegexMatches(result.NewValue.ToLower(), StringFormat.Email))
                {
                    result.Message = "Invalid email";
                    result.Warning = item.Value + " is invalid email";
                    result.IsGood = Constants.NO;
                }
            }
            return result;
        }

        private static ValidatedData ValidatePhoneNumber(UIData<string> item, string requiredMessage)
        {
            Int16 maxLength = 10;
            ValidatedData result = ValidateGeneral(item, maxLength, requiredMessage);
            if (result.ContinueEvaluate)
            {
                if (!RegexMatches(result.NewValue, StringFormat.PhoneNumber))
                {
                    result.Message = "Invalid phone number";
                    result.Warning = item.Value + " is invalid phone number";
                    result.IsGood = Constants.NO;
                }
            }
            return result;
        }

        private static ValidatedData ValidateSSN(UIData<string> item, string requiredMessage)
        {
            Int16 maxLength = 9;
            ValidatedData result = ValidateGeneral(item, maxLength, requiredMessage);
            if (result.ContinueEvaluate)
            {
                if (!RegexMatches(result.NewValue, StringFormat.SSN))
                {
                    result.Message = "Invalid ssn";
                    result.Warning = item.Value + " is invalid ssn";
                    result.IsGood = Constants.NO;
                }
            }
            return result;
        }

        private static ValidatedData ValidateZipcode(UIData<string> item, string requiredMessage)
        {
            Int16 maxLength = 5;
            ValidatedData result = ValidateGeneral(item, maxLength, requiredMessage);
            if (result.ContinueEvaluate)
            {
                if (!RegexMatches(result.NewValue, StringFormat.ZipCode))
                {
                    result.Message = "Invalid Zipcode";
                    result.Warning = item.Value + " is invalid Zipcode";
                    result.IsGood = Constants.NO;
                }
            }
            return result;
        }

        private static ValidatedData ValidateDate(UIData<string> item, string requiredMessage)
        {
            string newValue = Regex.Replace(item.Value, "[^\x0d\x0a\x20-\x7e\t]", "");

            ValidatedData result = default(ValidatedData);
            result.NewValue = newValue;
            result.Message = "";
            result.Warning = "";
            result.IsGood = true;
            result.ContinueEvaluate = true;

            if (requiredMessage.Length == 0 && item.Value.Length == 0)
                return result;

            DateTime aDate;
            bool isADate = (DateTime.TryParse(newValue, out aDate));

            if (isADate)
            {
                result.NewValue = Convert.ToDateTime(newValue).ToString("MM/dd/yyyy");
            }
            else
            {
                result.Message = "Invalid Date";
                result.Warning = item.Value + " is invalid Date";
                result.IsGood = Constants.NO;
            }

            return result;
        }

        private static ValidatedData ValidateDateTime(UIData<string> item, string requiredMessage)
        {
            string newValue = Regex.Replace(item.Value, "[^\x0d\x0a\x20-\x7e\t]", "");

            ValidatedData result = default(ValidatedData);
            result.NewValue = newValue;
            result.Message = "";
            result.Warning = "";
            result.IsGood = true;
            result.ContinueEvaluate = true;

            if (requiredMessage.Length == 0 && item.Value.Length == 0)
                return result;

            DateTime aDate;
            bool isADate = (DateTime.TryParse(newValue, out aDate));

            if (isADate)
            {
                result.NewValue = Convert.ToDateTime(newValue).ToString();
            }
            else
            {
                result.Message = "Invalid Date";
                result.Warning = item.Value + " is invalid Date";
                result.IsGood = Constants.NO;
            }

            return result;
        }

        public static bool SQLInjection(string value)
        {
            if (value.Trim().Contains("--"))
                return true;
            if (value.Trim().EndsWith(";--"))
                return true;
            if (value.ToUpper().Contains("UNION"))
                return true;
            if (value.ToUpper().Contains("DROP"))
                return true;
            if (value.ToUpper().Contains("SELECT"))
                return true;
            return false;
        }
    }
}