﻿using Avalonia;
using Avalonia.Animation;
using Avalonia.Layout;
using Avalonia.Rendering.Composition;

namespace PanelExntension.Avalonia;

public class HorizontalGridLayout : NonVirtualizingLayout
{
    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<HorizontalGridLayout, double>(nameof(ColumnSpacing));

    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<HorizontalGridLayout, double>(nameof(RowSpacing));

    public static readonly StyledProperty<double> MinItemWidthProperty =
            AvaloniaProperty.Register<HorizontalGridLayout, double>(nameof(MinItemWidth));

    public static readonly StyledProperty<int> MaxColumnsProperty =
            AvaloniaProperty.Register<HorizontalGridLayout, int>(
                nameof(MaxColumns),
                defaultValue: int.MaxValue,
                coerce: (_, v) => Math.Max(v, 1));

    public double ColumnSpacing
    {
        get => GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    public int MaxColumns
    {
        get => GetValue(MaxColumnsProperty);
        set => SetValue(MaxColumnsProperty, value);
    }

    protected override Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
    {
        if (double.IsInfinity(availableSize.Width))
        {
            throw new InvalidOperationException();
        }

        var columns = (int)Math.Floor(availableSize.Width / MinItemWidth);
        columns = Math.Clamp(columns, 1, MaxColumns);

        var spacingWidth = (columns - 1) * ColumnSpacing;

        var itemWidth = (availableSize.Width - spacingWidth) / columns;

        foreach (var item in context.Children)
        {
            item.Measure(new Size(itemWidth, double.PositiveInfinity));
        }

        var curColumn = 0;
        var curRow = 0;
        var curMaxBottom = 0d;
        var prevMaxBottom = 0d;

        foreach (var item in context.Children)
        {
            var start = curColumn == 0;
            var last = curColumn == columns - 1;

            var margin = item.Margin;
            var left = curColumn * itemWidth + curColumn * ColumnSpacing;
            var top = prevMaxBottom + margin.Top;

            var rect = new Rect(left, top, itemWidth, item.DesiredSize.Height + margin.Bottom);
            item.Arrange(rect);

            curMaxBottom = Math.Max(item.Bounds.Bottom, curMaxBottom);

            curColumn++;
            if (last)
            {
                curColumn = 0;
                curRow++;

                prevMaxBottom = curMaxBottom + RowSpacing;
            }
        }

        return new Size(availableSize.Width, curMaxBottom);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MinItemWidthProperty
            || change.Property == MaxColumnsProperty
            || change.Property == ColumnSpacingProperty
            || change.Property == RowSpacingProperty)
        {
            InvalidateMeasure();
            InvalidateArrange();
        }
    }
}
