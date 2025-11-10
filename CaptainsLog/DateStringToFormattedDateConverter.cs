using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CaptainsLog.Converters
{
    // Converts a date stored as a string (e.g. ISO or other parseable format)
    // into "dd-MMM-yyyy" (e.g. 05-Nov-2025) for display in XAML bindings.
    public class DateStringToFormattedDateConverter : IValueConverter
    {
        // value: the incoming value from the binding (expected to be string or DateTime)
        // targetType: expected target type (usually string for Label.Text)
        // parameter: optional format override (e.g. "dd/MM/yyyy")
        // culture: the culture to use for parsing/formatting
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            // Allow overriding format via ConverterParameter
            var format = parameter as string ?? "dd-MMM-yyyy";

            // If already a DateTime, format directly
            if (value is DateTime dt)
            {
                return dt.ToString(format, culture ?? CultureInfo.InvariantCulture);
            }

            // If it's a string, attempt to parse
            if (value is string s)
            {
                // Try parse with current culture, then invariant, then ISO patterns
                if (DateTime.TryParse(s, culture, DateTimeStyles.None, out var parsed) ||
                    DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed) ||
                    DateTime.TryParseExact(s,
                        new[] {
                            "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", "yyyy-MM-ddTHH:mm:ssK",
                            "yyyy-MM-dd", "M/d/yyyy", "M/d/yy"
                        },
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal,
                        out parsed))
                {
                    return parsed.ToString(format, culture ?? CultureInfo.InvariantCulture);
                }

                // If parsing fails, return original string
                return s;
            }

            // Fallback: return ToString()
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                // Expecting input like "dd-MMM-yyyy" (or format provided as parameter)
                var format = parameter as string ?? "dd-MMM-yyyy";
                if (DateTime.TryParseExact(s, format, culture ?? CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    // Return a string in ISO date format (or a DateTime if the VM expects DateTime)
                    return parsed.ToString("s", CultureInfo.InvariantCulture); // e.g. "2025-11-05T13:45:30"
                }

                // Try generic parse
                if (DateTime.TryParse(s, culture ?? CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                {
                    return parsed.ToString("s", CultureInfo.InvariantCulture);
                }
            }

            return value;
        }
    }
}

/*
XAML: How to use the converter in your DieselLogPage.xaml

1) Add an xmlns mapping at the root ContentPage element:
   xmlns:converters="clr-namespace:CaptainsLog.Converters"

2) Add the converter to page resources (directly under ContentPage):
   <ContentPage.Resources>
       <ResourceDictionary>
           <converters:DateStringToFormattedDateConverter x:Key="DateStringConverter" />
       </ResourceDictionary>
   </ContentPage.Resources>

3) Update the Label binding to use the converter:
   <Label Grid.Column="0" Grid.Row="1"
          Text="{Binding EntryDate, Converter={StaticResource DateStringConverter}}"
          HorizontalTextAlignment="Center"/>

Optional: pass a custom format via ConverterParameter:
   Text="{Binding EntryDate, Converter={StaticResource DateStringConverter}, ConverterParameter='dd/MM/yyyy'}"

Notes:
- If you can change your view model, prefer exposing EntryDate as DateTime. Then you can use
  StringFormat: Text="{Binding EntryDate, StringFormat='{0:dd-MMM-yyyy}'}"
- Ensure the namespace 'CaptainsLog.Converters' matches the folder/namespace you place the C# file in.
*/