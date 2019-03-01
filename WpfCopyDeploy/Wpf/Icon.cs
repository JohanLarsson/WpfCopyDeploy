namespace WpfCopyDeploy
{
    using System.Windows;
    using System.Windows.Media;

    public static class Icon
    {
        public static readonly DependencyProperty GeometryProperty = DependencyProperty.RegisterAttached(
            "Geometry",
            typeof(Geometry),
            typeof(Icon),
            new PropertyMetadata(default(Geometry)));

        /// <summary>Helper for setting <see cref="GeometryProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="FrameworkElement"/> to set <see cref="GeometryProperty"/> on.</param>
        /// <param name="value">Geometry property value.</param>
        public static void SetGeometry(this FrameworkElement element, Geometry value)
        {
            element.SetValue(GeometryProperty, value);
        }

        /// <summary>Helper for getting <see cref="GeometryProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="FrameworkElement"/> to read <see cref="GeometryProperty"/> from.</param>
        /// <returns>Geometry property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static Geometry GetGeometry(this FrameworkElement element)
        {
            return (Geometry)element.GetValue(GeometryProperty);
        }

    }
}