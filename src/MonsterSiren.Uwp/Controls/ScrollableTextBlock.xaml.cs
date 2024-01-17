﻿//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

using System.ComponentModel;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas;
using System.Runtime.CompilerServices;

namespace MonsterSiren.Uwp.Controls;

public sealed partial class ScrollableTextBlock : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private FrameworkElement parent;
    private bool isScrolling;

    public ScrollableTextBlock()
    {
        this.InitializeComponent();
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Unloaded -= OnUnloaded;
        parent.SizeChanged -= OnSizeChanged;
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set
        {
            SetValue(TextProperty, value);
            isScrolling = false;
            TryStartScrollAnimation();
        }
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(ScrollableTextBlock), new PropertyMetadata(string.Empty));

    private async void TryStartScrollAnimation()
    {
        if (isScrolling || parent is null)
        {
            return;
        }

        DefaultStoryboard.Begin();
        await Task.Delay(300);

        double textSize = MeasureTextSize();
        if (textSize > parent.ActualWidth)
        {
            Thickness padding = Separator.Margin;

            ScrollAnimation.To = -(textSize + (padding.Left + padding.Right));
            ScrollAnimation.Duration = TimeSpan.FromSeconds(textSize / FontSize / 2);

            ScrollStoryboard.Begin();
            isScrolling = true;
        }
        else
        {
            isScrolling = false;
        }
    }

    private double MeasureTextSize()
    {
        CanvasTextFormat textFormat = new()
        {
            FontSize = (float)FontSize,
            FontFamily = FontFamily.Source,
            Direction = CanvasTextDirection.LeftToRightThenTopToBottom,
            WordWrapping = CanvasWordWrapping.NoWrap
        };
        CanvasDevice device = CanvasDevice.GetSharedDevice();

        double width = (double.IsNaN(PrimaryText.ActualWidth) || PrimaryText.ActualWidth < 0) ? 0 : PrimaryText.ActualWidth;
        using CanvasTextLayout layout = new(device, Text, textFormat, (float)width, 0);
        return layout.LayoutBounds.Width;
    }

    /// <summary>
    /// 通知运行时属性已经发生更改
    /// </summary>
    /// <param name="propertyName">发生更改的属性名称,其填充是自动完成的</param>
    public void OnPropertiesChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        TryStartScrollAnimation();
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        TryStartScrollAnimation();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DependencyObject dpParent = VisualTreeHelper.GetParent(this);
        if (dpParent is FrameworkElement element)
        {
            parent = element;
            parent.SizeChanged += OnSizeChanged;
        }
    }

    private void OnScrollStoryboardCompleted(object sender, object e) => isScrolling = false;
}
