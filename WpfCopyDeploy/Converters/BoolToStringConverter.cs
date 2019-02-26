namespace WpfCopyDeploy
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ValueConversion(typeof(bool), typeof(string))]
    [MarkupExtensionReturnType(typeof(BoolToStringConverter))]
    public class BoolToStringConverter : MarkupExtension, IValueConverter
    {
        public string WhenTrue { get; set; }

        public string WhenFalse { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? this.WhenTrue : this.WhenFalse;
            }

            throw new ArgumentException("Expected bool", nameof(value));
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
