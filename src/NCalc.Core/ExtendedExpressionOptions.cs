
namespace NCalc
{
    public class ExtendedExpressionOptions : IFormatProvider
    {
        public enum SeparatorType
        {
            BuiltIn,
            CurrentCulture,
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
                _dateSeparator = value;
            }
        }

        public string TimeSeparator
        {
            get => _timeSeparator;
            set
            {
                _timeSeparator = value;
            }
        }

        public string DecimalSeparator
        {
            get => _decimalSeparator;
            set
            {
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

        public ExtendedExpressionOptions() : this(CultureInfo.CurrentCulture)
        {
        }

        public ExtendedExpressionOptions(CultureInfo cultureInfo)
        {
            _cultureInfo = cultureInfo;
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
                _decimalSeparator = ".";
            _numberGroupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
        }

        internal string GetDateSeparator()
        {
            switch (DateSeparatorType)
            {
                case SeparatorType.BuiltIn:
                    return "/";
                case SeparatorType.CurrentCulture:
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
                case SeparatorType.CurrentCulture:
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
                    separatorString = ".";
                    break;
                case SeparatorType.CurrentCulture:
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
                    separatorString = ",";
                    break;
                case SeparatorType.CurrentCulture:
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
                if (DateSeparatorType == SeparatorType.BuiltIn)
                {
                    return CultureInfo.CurrentCulture.DateTimeFormat.Clone() as DateTimeFormatInfo; // The parser will just use built-in values, so this line never gets executed. But we need to have it just in case.
                }
                if (DateSeparatorType == SeparatorType.CurrentCulture)
                {
                    if (_cultureInfo != null)
                        return _cultureInfo.DateTimeFormat;
                    else
                        return CultureInfo.CurrentCulture.DateTimeFormat;
                }
                else
                if (DateSeparatorType == SeparatorType.Custom)
                {
                    DateTimeFormatInfo? result = null;
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
                    if (result is not null)
                    {
                        return result;
                    }
                    else
                        return null;
                }
            }
            throw new NotImplementedException();
        }
    }
}
