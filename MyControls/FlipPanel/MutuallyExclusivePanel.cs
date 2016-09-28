using System.Windows;
using System.Windows.Controls;

namespace MyControlLibrary.FlipPanel
{
    public class MutuallyExclusivePanel : Panel
    {
        public static readonly DependencyProperty CurrentIndexProperty = DependencyProperty.Register("CurrentIndex", typeof(int), typeof(MutuallyExclusivePanel), new PropertyMetadata(0, OnCurrentIndexChanged));
        public static readonly DependencyProperty IndexProperty = DependencyProperty.RegisterAttached("Index", typeof(int), typeof(MutuallyExclusivePanel), new FrameworkPropertyMetadata(0));

        public MutuallyExclusivePanel()
        {
            SizeChanged += (s, e) => InvalidateMeasure();
        }

        private static void OnCurrentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            foreach (UIElement child in ((MutuallyExclusivePanel)d).Children)
                child.Visibility = GetIndex(child) == ((MutuallyExclusivePanel)d).CurrentIndex ? Visibility.Visible : Visibility.Collapsed;
        }

        public static void SetIndex(UIElement element, int value)
        {
            element.SetValue(MutuallyExclusivePanel.IndexProperty, value);
        }

        public static int GetIndex(UIElement element)
        {
            return (int)element.GetValue(MutuallyExclusivePanel.IndexProperty);
        }

        public int CurrentIndex
        {
            get { return (int)GetValue(MutuallyExclusivePanel.CurrentIndexProperty); }
            set { SetValue(MutuallyExclusivePanel.CurrentIndexProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in Children)
                child.Measure(availableSize);
            return base.MeasureOverride(availableSize);
        }

        private double GetHorizontalPosition(FrameworkElement element)
        {
            switch (element.HorizontalAlignment)
            {
                case System.Windows.HorizontalAlignment.Right:
                    return RenderSize.Width - element.DesiredSize.Width;
                case System.Windows.HorizontalAlignment.Center:
                    return (RenderSize.Width - element.DesiredSize.Width) / 2.0;
                default: // Left and Stretch
                    return 0;
            }
        }
        
        private double GetVerticalPosition(FrameworkElement element)
        {
            switch (element.VerticalAlignment)
            {
                case System.Windows.VerticalAlignment.Bottom:
                    return RenderSize.Height - element.DesiredSize.Height;
                case System.Windows.VerticalAlignment.Center:
                    return (RenderSize.Height - element.DesiredSize.Height) / 2.0;
                default: // Top and Stretch
                    return 0;
            }


        }

        private double GetWidth(FrameworkElement element)
        {
            if (element.HorizontalAlignment == System.Windows.HorizontalAlignment.Stretch)
                return RenderSize.Width;

            return element.DesiredSize.Width;
        }

        private double GetHeight(FrameworkElement element)
        {
            if (element.VerticalAlignment == System.Windows.VerticalAlignment.Stretch)
                return RenderSize.Height;

            return element.DesiredSize.Height;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in Children)
            {
                Point position = new Point();
                Size size = child.DesiredSize;
                if (child is FrameworkElement)
                {
                    FrameworkElement element = (FrameworkElement)child;
                    position.Offset(GetHorizontalPosition(element), GetVerticalPosition(element));
                    size = new Size(GetWidth(element), GetHeight(element));
                }

                child.Arrange(new Rect(position, size));
                child.Visibility = GetIndex(child) == CurrentIndex ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return base.ArrangeOverride(finalSize);
        }
    }
}
