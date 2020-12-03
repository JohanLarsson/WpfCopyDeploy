namespace WpfCopyDeploy
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    public static class Scroll
    {
        public static readonly DependencyProperty GroupProperty = DependencyProperty.RegisterAttached(
            "Group",
            typeof(string),
            typeof(Scroll),
            new PropertyMetadata(OnGroupChanged));

        /// <summary>Helper for setting <see cref="GroupProperty"/> on <paramref name="scrollViewer"/>.</summary>
        /// <param name="scrollViewer"><see cref="ScrollViewer"/> to set <see cref="GroupProperty"/> on.</param>
        /// <param name="group">Group property value.</param>
        public static void SetGroup(ScrollViewer scrollViewer, string? @group) => scrollViewer.SetValue(GroupProperty, @group);

        /// <summary>Helper for getting <see cref="GroupProperty"/> from <paramref name="scrollViewer"/>.</summary>
        /// <param name="scrollViewer"><see cref="ScrollViewer"/> to read <see cref="GroupProperty"/> from.</param>
        /// <returns>Group property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(ScrollViewer))]
        public static string? GetGroup(ScrollViewer scrollViewer) => (string?)scrollViewer.GetValue(GroupProperty);

        /// <summary>
        /// Occurs, when the GroupProperty has changed.
        /// </summary>
        /// <param name="d">The DependencyObject on which the property has changed value.</param>
        /// <param name="e">Event data that is issued by any event that tracks changes to the effective value of this property.</param>
        private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                if (e.OldValue is string oldGroup &&
                    !string.IsNullOrEmpty(oldGroup))
                {
                    Synchronizer.GetOrCreate(oldGroup).Remove(scrollViewer);
                }

                if (e.NewValue is string newGroup &&
                    !string.IsNullOrEmpty(newGroup))
                {
                    Synchronizer.GetOrCreate(newGroup).Add(scrollViewer);
                }
            }
        }

        private class Synchronizer
        {
            private static readonly Dictionary<string, Synchronizer> Cache = new Dictionary<string, Synchronizer>();

            private readonly List<ScrollViewer> scrollViewers = new List<ScrollViewer>();

            private Synchronizer()
            {
            }

            internal static Synchronizer GetOrCreate(string group)
            {
                if (Cache.TryGetValue(group, out var synchronizer))
                {
                    return synchronizer;
                }

                synchronizer = new Synchronizer();
                Cache[group] = synchronizer;
                return synchronizer;
            }

            internal void Add(ScrollViewer scrollViewer)
            {
                if (this.scrollViewers.Count > 0)
                {
                    var first = this.scrollViewers[0];
                    scrollViewer.ScrollToHorizontalOffset(first.HorizontalOffset);
                    scrollViewer.ScrollToVerticalOffset(first.VerticalOffset);
                }

                this.scrollViewers.Add(scrollViewer);
                scrollViewer.ScrollChanged += this.OnScrollChanged;
            }

            internal void Remove(ScrollViewer scrollViewer)
            {
                this.scrollViewers.Remove(scrollViewer);
                scrollViewer.ScrollChanged -= this.OnScrollChanged;
            }

            private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                if (sender is ScrollViewer typedSender)
                {
                    if (e.HorizontalChange != 0)
                    {
                        foreach (var scrollViewer in this.scrollViewers)
                        {
                            if (!ReferenceEquals(scrollViewer, sender))
                            {
                                scrollViewer.ScrollToHorizontalOffset(typedSender.HorizontalOffset);
                            }
                        }
                    }

                    if (e.VerticalChange != 0)
                    {
                        foreach (var scrollViewer in this.scrollViewers)
                        {
                            if (!ReferenceEquals(scrollViewer, sender))
                            {
                                scrollViewer.ScrollToVerticalOffset(typedSender.VerticalOffset);
                            }
                        }
                    }
                }

            }
        }
    }
}