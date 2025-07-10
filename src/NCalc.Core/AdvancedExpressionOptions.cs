namespace NCalc
{
    [Flags]
    public enum AdvExpressionOptions
    {
        None = 0,

        /// <summary>
        /// When extended options are used, disables the use of the date separator character of the current culture during parsing of dates.
        /// When the flag is not set and extended options are used, both the date separator character of the current culture and the date separator specified in the extended options ('.', usually) is used.
        /// </summary>
        SkipBuiltInDateSeparator = 1 << 0,

        /// <summary>
        /// When extended options are used, disables the use of the time separator character of the current culture during parsing of times.
        /// When the flag is not set and extended options are used, both the time separator character of the current culture and the time separator specified in the extended options is used.
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

        /// <summary>
        /// When set, the current locale's currency symbol is accepted to indicate a decimal number (the symbol itself is skipped).
        /// </summary>
        AcceptCurrencySymbol = 1 << 6,

        /// <summary>
        /// When enabled, supports parsing of periods expressed in a humane form like 3y7mo5w7d3h2min18s
        /// </summary>
        ParseHumanePeriods = 1 << 7
    }

    public class AdvancedExpressionOptions : IFormatProvider
    {
        public enum SeparatorType
        {
            BuiltIn,
            FromCulture,
            Custom
        }

        public enum GroupSeparatorType
        {
            Skip,
            BuiltIn,
            FromCulture,
            Custom
        }

        public enum HoursFormatKind
        {
            BuiltIn, // From current culture
            FromCulture, // from specified culture or current culture if nothing is specified
            Always12Hour, // use 12-hour format
            Always24Hour  // use 24-hour format
        }

        public enum CurrencySymbolType
        {
            CurrentCulture,
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
        string _currencyDecimalSeparator = ".";
        string _currencyNumberGroupSeparator = string.Empty;
        string _currencySymbol = string.Empty;
        string _currencySymbol2 = string.Empty;
        string _currencySymbol3 = string.Empty;

        readonly List<string> _periodYearIndicators = ["years", "year", "yrs", "yr", "y"];
        readonly List<string> _periodMonthIndicators = ["months", "month", "mon", "mos", "mo"];
        readonly List<string> _periodWeekIndicators = ["weeks", "week", "wks", "wk", "w"];
        readonly List<string> _periodDayIndicators = ["days", "day", "d"];
        readonly List<string> _periodHourIndicators = ["hours", "hour", "hrs", "hr", "h"];
        readonly List<string> _periodMinuteIndicators = ["minutes", "minute", "mins", "min", "m"];
        readonly List<string> _periodSecondIndicators = ["seconds", "second", "secs", "sec", "s"];
        readonly List<string> _periodMSecIndicators = ["msec", "ms"];

        readonly List<string> _periodNowIndicators = ["now"];
        readonly List<string> _periodTodayIndicators = ["today"];
        readonly List<string> _periodPastIndicators = ["ago", "before", "earlier"];
        readonly List<string> _periodFutureIndicators = ["after", "in", "later"];

        //
        // Period*Indicators contain the lists of literals recognized as a period indicator for a certain type of period
        // When adding custom / localized indicators to the lists, add them in all lowercase -
        // the comparer in the parser compares a lowercase user-provided value with the indicators case-sensitively (for performance)
        //

        public List<string> PeriodYearIndicators => _periodYearIndicators;
        public List<string> PeriodMonthIndicators => _periodMonthIndicators;
        public List<string> PeriodWeekIndicators => _periodWeekIndicators;
        public List<string> PeriodDayIndicators => _periodDayIndicators;
        public List<string> PeriodHourIndicators => _periodHourIndicators;
        public List<string> PeriodMinuteIndicators => _periodMinuteIndicators;
        public List<string> PeriodSecondIndicators => _periodSecondIndicators;
        public List<string> PeriodMSecIndicators => _periodMSecIndicators;

        public List<string> PeriodNowIndicators => _periodNowIndicators;
        public List<string> PeriodTodayIndicators => _periodTodayIndicators;
        public List<string> PeriodPastIndicators => _periodPastIndicators;
        public List<string> PeriodFutureIndicators => _periodFutureIndicators;

        CultureInfo? _cultureInfo;

        public AdvExpressionOptions Flags { get; set; }

        public SeparatorType DateSeparatorType { get; set; }
        public SeparatorType TimeSeparatorType { get; set; }
        public SeparatorType DecimalSeparatorType { get; set; }
        public GroupSeparatorType NumberGroupSeparatorType { get; set; }
        public SeparatorType CurrencyDecimalSeparatorType { get; set; }
        public GroupSeparatorType CurrencyNumberGroupSeparatorType { get; set; }
        public CurrencySymbolType CurrencySymbolsType { get; set; }

        public DateOrderKind DateOrder { get; set; }
        public HoursFormatKind HoursFormat { get; set; }

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

        public string CurrencyDecimalSeparator
        {
            get => _currencyDecimalSeparator;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _currencyDecimalSeparator = value;
            }
        }

        public string CurrencyNumberGroupSeparator
        {
            get => _currencyNumberGroupSeparator;
            set
            {
                _currencyNumberGroupSeparator = value;
            }
        }

        public string CurrencySymbol
        {
            get => _currencySymbol;
            set
            {
                _currencySymbol = value;
            }
        }

        public string CurrencySymbol2
        {
            get => _currencySymbol2;
            set
            {
                _currencySymbol2 = value;
            }
        }

        public string CurrencySymbol3
        {
            get => _currencySymbol3;
            set
            {
                _currencySymbol3 = value;
            }
        }

        public AdvancedExpressionOptions() : this(CultureInfo.CurrentCulture)
        {
        }

        public AdvancedExpressionOptions(AdvExpressionOptions advOptions) : this(CultureInfo.CurrentCulture, advOptions)
        {
        }

        public AdvancedExpressionOptions(CultureInfo cultureInfo) : this (cultureInfo, AdvExpressionOptions.None)
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
            NumberGroupSeparatorType = GroupSeparatorType.Skip;
            CurrencyNumberGroupSeparatorType = GroupSeparatorType.Skip;
            DecimalSeparatorType = SeparatorType.BuiltIn;
            CurrencyDecimalSeparatorType = SeparatorType.BuiltIn;

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
            if (string.IsNullOrEmpty(_numberGroupSeparator))
                _numberGroupSeparator = Parlot.Fluent.NumberLiterals.DefaultGroupSeparator.ToString();

            _currencyDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
            _currencyNumberGroupSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator;
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

        internal char GetCurrencyDecimalSeparatorChar()
        {
            string? separatorString = "";
            switch (CurrencyDecimalSeparatorType)
            {
                case SeparatorType.BuiltIn:
                    separatorString = Parlot.Fluent.NumberLiterals.DefaultDecimalSeparator.ToString();
                    break;
                case SeparatorType.FromCulture:
                    separatorString = ((_cultureInfo is not null) ? _cultureInfo : CultureInfo.CurrentCulture).NumberFormat.CurrencyDecimalSeparator;
                    break;
                case SeparatorType.Custom:
                    separatorString = _currencyDecimalSeparator;
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
                case GroupSeparatorType.Skip:
                    return '\0';
                case GroupSeparatorType.BuiltIn:
                    separatorString = Parlot.Fluent.NumberLiterals.DefaultGroupSeparator.ToString();
                    break;
                case GroupSeparatorType.FromCulture:
                    separatorString = ((_cultureInfo is not null) ? _cultureInfo : CultureInfo.CurrentCulture).NumberFormat.NumberGroupSeparator;
                    break;
                case GroupSeparatorType.Custom:
                    separatorString = _numberGroupSeparator;
                    break;
            }
            if (string.IsNullOrEmpty(separatorString))
                return '\0';
            else
                return separatorString[0];
        }

        internal char GetCurrencyNumberGroupSeparatorChar()
        {
            string? separatorString = "";
            switch (CurrencyNumberGroupSeparatorType)
            {
                case GroupSeparatorType.Skip:
                    return '\0';
                case GroupSeparatorType.BuiltIn:
                    separatorString = Parlot.Fluent.NumberLiterals.DefaultGroupSeparator.ToString();
                    break;
                case GroupSeparatorType.FromCulture:
                    separatorString = ((_cultureInfo is not null) ? _cultureInfo : CultureInfo.CurrentCulture).NumberFormat.CurrencyGroupSeparator;
                    break;
                case GroupSeparatorType.Custom:
                    separatorString = _currencyNumberGroupSeparator;
                    break;
            }
            if (string.IsNullOrEmpty(separatorString))
                return '\0';
            else
                return separatorString[0];
        }

        internal void GetCurrencySymbols(out string currencySymbol, out string currencySymbol2, out string currencySymbol3)
        {
            currencySymbol = string.Empty;
            currencySymbol2 = string.Empty;
            currencySymbol3 = string.Empty;
            if ((CurrencySymbolsType == CurrencySymbolType.CurrentCulture) || (CurrencySymbolsType == CurrencySymbolType.FromCulture))
            {
                CultureInfo culture;
                if ((CurrencySymbolsType == CurrencySymbolType.FromCulture) && (_cultureInfo != null))
                    culture = _cultureInfo;
                else
                    culture = CultureInfo.CurrentCulture;

                currencySymbol = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;

                try
                {
                    RegionInfo ri;
                    int lcid = culture.LCID;
                    if (!culture.IsNeutralCulture && !string.IsNullOrEmpty(culture.Name))
                    {
                        ri = new RegionInfo(culture.LCID);
                        currencySymbol2 = ri.ISOCurrencySymbol;
                    }
                    if ((currencySymbol.Length > 0) && (currencySymbol[0] == '\x20ac') && !"EUR".Equals(currencySymbol2)) // Euro character
                        currencySymbol3 = "EUR";
                }
                catch (ArgumentException)
                {
                    // ignore the exception related to the culture being CultureInfo.LOCALE_INVARIANT, CultureInfo.LOCALE_NEUTRAL, CultureInfo.LOCALE_CUSTOM_DEFAULT, or CultureInfo.LOCALE_CUSTOM_UNSPECIFIED
                }
            }
            else
            if (CurrencySymbolsType == CurrencySymbolType.Custom)
            {
                currencySymbol = _currencySymbol;
                currencySymbol2 = _currencySymbol2;
                currencySymbol3 = _currencySymbol3;
            }
        }

        internal bool Use12HourTime()
        {
            switch (HoursFormat)
            {
                case HoursFormatKind.BuiltIn:
                    return CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("t");
                case HoursFormatKind.FromCulture:
                    return ((_cultureInfo is not null) ? _cultureInfo.DateTimeFormat : CultureInfo.CurrentCulture.DateTimeFormat).ShortTimePattern.Contains("t");
                case HoursFormatKind.Always12Hour:
                    return true;
                case HoursFormatKind.Always24Hour:
                    return false;
                default:
                    return CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("t");
            }
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
                result = ((_cultureInfo is not null) ? _cultureInfo.DateTimeFormat : CultureInfo.CurrentCulture.DateTimeFormat).Clone() as DateTimeFormatInfo;
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
