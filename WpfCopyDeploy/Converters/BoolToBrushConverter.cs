namespace WpfCopyDeploy
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;

    [ValueConversion(typeof(bool), typeof(Brush))]
    [MarkupExtensionReturnType(typeof(BoolToBrushConverter))]
    public class BoolToBrushConverter : MarkupExtension, IValueConverter
    {
        public Brush WhenTrue { get; set; }

        public Brush WhenFalse { get; set; }

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
