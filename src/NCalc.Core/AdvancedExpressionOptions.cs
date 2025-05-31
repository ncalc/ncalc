
namespace NCalc
{
    [Flags]
    public enum AdvExpressionOptions
    {
        None = 0,

        /// <summary>
        /// When extended options are used, disables the use of the '/' character during parsing of dates.
        /// When the flag is not set and extended options are used, both the '/' and the date separator specified in the extended options ('.', usually) is used.
        /// </summary>
        SkipBuiltInDateSeparator = 1 << 0,

        /// <summary>
        /// When extended options are used, disables the use of the ':' character during parsing of times.
        /// When the flag is not set and extended options are used, both the ':' and the time separator specified in the extended options' CultureInfo is used.
        /// </summary>
        SkipBuiltInTimeSeparator = 1 << 1,

        /// <summary>
        /// When set, recognizes and skips underscore characters in numbers
        /// </summary>
        AcceptUnderscoresInNumbers = 1 << 2,

        /// <summary>
        /// When set, recognizes 0{digits} numeric literals as octals like C programming language does
        /// </summary>
        AcceptCStyleOctals = 1 << 3,

        /// <summary>
        /// When set, the `%` character (per cent) is used to calculate percent rather than modulo (default in ncalc)
        /// </summary>
        CalculatePercent = 1 << 4,

        /// <summary>
        /// When set, the `@` character (at) is used to reference a previous evaluation's result. For this, the EvaluateFunction event is fired with `@` as the function name.
        /// </summary>
        UseResultReference = 1 << 5,
    }

    public class AdvancedExpressionOptions : IFormatProvider
    {
        public enum SeparatorType
        {
            BuiltIn,
            FromCulture,
            Custom
        }

        public enum DateOrderKind
        {
            YMD,
            DMY,
            MDY
        }

        string _dateSeparator = "/";
        string _timeSeparator = ":";
        string _decimalSeparator = ".";
        string _numberGroupSeparator = "";

        CultureInfo? _cultureInfo;

        public AdvExpressionOptions Flags { get; set; }

        public SeparatorType DateSeparatorType { get; set; }
        public SeparatorType TimeSeparatorType { get; set; }
        public SeparatorType DecimalSeparatorType { get; set; }
        public SeparatorType NumberGroupSeparatorType { get; set; }

        public DateOrderKind DateOrder { get; set; }

        public CultureInfo? CultureInfo
        {
            get => (_cultureInfo != null) ? _cultureInfo : CultureInfo.CurrentCulture;
            set { _cultureInfo = value; }
        }

        public string DateSeparator
        {
            get => _dateSeparator;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _dateSeparator = value;
            }
        }

        public string TimeSeparator
        {
            get => _timeSeparator;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _timeSeparator = value;
            }
        }

        public string DecimalSeparator
        {
            get => _decimalSeparator;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _decimalSeparator = value;
            }
        }

        public string NumberGroupSeparator
        {
            get => _numberGroupSeparator;
            set
            {
                _numberGroupSeparator = value;
            }
        }

        public static AdvancedExpressionOptions DefaultOptions = new();

        public AdvancedExpressionOptions() : this(CultureInfo.CurrentCulture)
        {
        }

        public AdvancedExpressionOptions(AdvExpressionOptions advOptions) : this(CultureInfo.CurrentCulture, advOptions)
        {
        }

        public AdvancedExpressionOptions(CultureInfo cultureInfo) : this (cultureInfo, AdvExpressionOptions.SkipBuiltInDateSeparator | AdvExpressionOptions.SkipBuiltInTimeSeparator)
        {
            _cultureInfo = cultureInfo;
            InitFieldsFromCulture();
        }

        public AdvancedExpressionOptions(CultureInfo cultureInfo, AdvExpressionOptions advOptions)
        {
            _cultureInfo = cultureInfo;
            Flags = advOptions;
            InitFieldsFromCulture();
        }

        void InitFieldsFromCulture()
        {
            DateSeparatorType = SeparatorType.BuiltIn;
            TimeSeparatorType = SeparatorType.BuiltIn;
            NumberGroupSeparatorType = SeparatorType.BuiltIn;
            DecimalSeparatorType = SeparatorType.BuiltIn;

            var shortDatePattern = ((_cultureInfo is not null) ? _cultureInfo : CultureInfo.CurrentCulture).DateTimeFormat.ShortDatePattern;
            if (!string.IsNullOrEmpty(shortDatePattern))
            {
                switch (shortDatePattern.Trim()[0])
                {
                    case 'M':
                        DateOrder = DateOrderKind.MDY;
                        break;
                    case 'Y':
                        DateOrder = DateOrderKind.YMD;
                        break;
                    default:
                        DateOrder = DateOrderKind.DMY;
                        break;
                }
            }
            else
                DateOrder = DateOrderKind.DMY;

            if (_cultureInfo is not null)
                _dateSeparator = _cultureInfo.DateTimeFormat.DateSeparator;
            else
                _dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
            if (string.IsNullOrEmpty(_dateSeparator))
                _dateSeparator = "/";
            _decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (string.IsNullOrEmpty(_decimalSeparator))
                _decimalSeparator = Parlot.Fluent.NumberLiterals.DefaultDecimalSeparator.ToString();
            _numberGroupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
        }

        internal string GetDateSeparator()
        {
            switch (DateSeparatorType)
            {
                case SeparatorType.BuiltIn:
                    return "/";
                case SeparatorType.FromCulture:
                    return ((_cultureInfo is not null) ? _cultureInfo : CultureInfo.CurrentCulture).DateTimeFormat.DateSeparator;
                case SeparatorType.Custom:
                    return _dateSeparator;
                default:
                    return "/";
            }
        }

        internal string GetTimeSeparator()
        {
            switch (TimeSeparatorType)
            {
                case SeparatorType.BuiltIn:
                    return ":";
                case SeparatorType.FromCulture:
                    return ((_cultureInfo is not null) ? _cultureInfo : CultureInfo.CurrentCulture).DateTimeFormat.TimeSeparator;
                case SeparatorType.Custom:
                    return _timeSeparator;
                default:
                    return ":";
            }
        }

        internal char GetDecimalSeparatorChar()
        {
            string? separatorString = "";
            switch (DecimalSeparatorType)
            {
                case SeparatorType.BuiltIn:
                    separatorString = Parlot.Fluent.NumberLiterals.DefaultDecimalSeparator.ToString();
                    break;
                case SeparatorType.FromCulture:
                    separatorString = ((_cultureInfo is not null) ? _cultureInfo : CultureInfo.CurrentCulture).NumberFormat.NumberDecimalSeparator;
                    break;
                case SeparatorType.Custom:
                    separatorString = _decimalSeparator;
                    break;
            }
            if (string.IsNullOrEmpty(separatorString))
                return '\0';
            else
                return separatorString[0];
        }

        internal char GetNumberGroupSeparatorChar()
        {
            string? separatorString = "";
            switch (NumberGroupSeparatorType)
            {
                case SeparatorType.BuiltIn:
                    separatorString = Parlot.Fluent.NumberLiterals.DefaultGroupSeparator.ToString();
                    break;
                case SeparatorType.FromCulture:
                    separatorString = ((_cultureInfo is not null) ? _cultureInfo : CultureInfo.CurrentCulture).NumberFormat.NumberGroupSeparator;
                    break;
                case SeparatorType.Custom:
                    separatorString = _numberGroupSeparator;
                    break;
            }
            if (string.IsNullOrEmpty(separatorString))
                return '\0';
            else
                return separatorString[0];
        }

        internal IFormatProvider GetDateFormatProvider()
        {
            return this;
        }

        public object? GetFormat(Type? formatType)
        {
            if (formatType == typeof(DateTimeFormatInfo))
            {
                if (DateSeparatorType == TimeSeparatorType)
                {
                    if (DateSeparatorType == SeparatorType.BuiltIn)
                    {
                        return CultureInfo.CurrentCulture.DateTimeFormat.Clone() as DateTimeFormatInfo; // The parser will just use built-in values, so this line never gets executed. But we need to have it just in case.
                    }
                    if (DateSeparatorType == SeparatorType.FromCulture)
                    {
                        return (_cultureInfo is not null) ? _cultureInfo.DateTimeFormat : CultureInfo.CurrentCulture.DateTimeFormat;
                    }
                    else
                    if (DateSeparatorType == SeparatorType.Custom)
                    {
                        return CombineDateTimeFormat();
                    }
                }
                else
                    return CombineDateTimeFormat();
            }
            return null;
        }

        private DateTimeFormatInfo? CombineDateTimeFormat()
        {
            DateTimeFormatInfo? result = null;
            if (DateSeparatorType == SeparatorType.BuiltIn)
            {
                result = CultureInfo.CurrentCulture.DateTimeFormat.Clone() as DateTimeFormatInfo; // The parser will just use built-in values, so this line never gets executed. But we need to have it just in case.
            }
            if (DateSeparatorType == SeparatorType.FromCulture)
            {
                result = (_cultureInfo is not null) ? _cultureInfo.DateTimeFormat : CultureInfo.CurrentCulture.DateTimeFormat;
            }
            else
            if (DateSeparatorType == SeparatorType.Custom)
            {
                switch (DateOrder)
                {
                    case DateOrderKind.DMY:
                        result = CultureInfo.GetCultureInfo("de-DE").DateTimeFormat.Clone() as DateTimeFormatInfo;
                        if (result is not null)
                            result.DateSeparator = _dateSeparator;
                        break;
                    case DateOrderKind.MDY:
                        result = CultureInfo.GetCultureInfo("en-US").DateTimeFormat.Clone() as DateTimeFormatInfo;
                        if (result is not null)
                            result.DateSeparator = _dateSeparator;
                        break;
                    case DateOrderKind.YMD:
                        result = CultureInfo.CurrentCulture.DateTimeFormat.Clone() as DateTimeFormatInfo;
                        if (result is not null && result.DateSeparator != _dateSeparator)
                        {
                            result.FullDateTimePattern = result.FullDateTimePattern.Replace(result.DateSeparator, _dateSeparator);
                            result.LongDatePattern = result.LongDatePattern.Replace(result.DateSeparator, _dateSeparator);
                            result.ShortDatePattern = string.Join(_dateSeparator, "yyyy", "M", "d");
                            result.MonthDayPattern = string.Join(_dateSeparator, "M", "d");
                            result.YearMonthPattern = string.Join(_dateSeparator, "yyyy", "M");

                            result.DateSeparator = _dateSeparator;
                        }
                        break;
                }
            }

            if (result == null)
                return result;

            DateTimeFormatInfo timeInfo = ((TimeSeparatorType == SeparatorType.FromCulture) && _cultureInfo is not null) ? _cultureInfo.DateTimeFormat : CultureInfo.CurrentCulture.DateTimeFormat;

            result.AMDesignator = timeInfo.AMDesignator;
            result.PMDesignator = timeInfo.PMDesignator;
            result.LongTimePattern = timeInfo.LongTimePattern;
            result.ShortTimePattern = timeInfo.ShortTimePattern;
            result.TimeSeparator = timeInfo.TimeSeparator;

            if (TimeSeparatorType == SeparatorType.Custom)
            {
                result.TimeSeparator = _timeSeparator;
            }

            return result;
        }
    }
}
