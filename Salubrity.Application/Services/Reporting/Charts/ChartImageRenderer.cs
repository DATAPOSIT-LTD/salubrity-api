// File: Application/Services/Reporting/Charts/ChartImageRenderer.cs
using ScottPlot;

namespace Salubrity.Application.Services.Reporting.Charts;

public static class ChartImageRenderer
{
    public static byte[] PieAsPng(
        IEnumerable<(string Label, double Value, string HexColor)> slices,
        int width = 360,
        int height = 240)
    {
        var data = slices.Where(s => s.Value > 0).ToList();
        if (data.Count == 0)
            return Array.Empty<byte>();

        var plot = new Plot();
        var pieSlices = data.Select(s => new PieSlice
        {
            Value = s.Value,
            Label = s.Label,
            FillColor = ScottPlot.Color.FromHex(s.HexColor.TrimStart('#')),
        }).ToList();

        var pie = plot.Add.Pie(pieSlices);
        pie.ExplodeFraction = 0.0;
        // Slice-edge labels in ScottPlot 5.0.55 do not render reliably — relying on the
        // legend instead. Each slice's Label already carries the percentage ("Female 26%")
        // so the legend shows it.

        plot.HideGrid();
        plot.Axes.Frameless();

        var legend = plot.ShowLegend(Alignment.MiddleRight);
        legend.FontSize = 14;

        return plot.GetImageBytes(width, height, ImageFormat.Png);
    }

    public static byte[] LineAsPng(
        IList<string> xCategories,
        IEnumerable<(string SeriesName, double[] Values, string HexColor)> series,
        int width = 600,
        int height = 280,
        string yLabel = "")
    {
        var seriesList = series.ToList();
        if (xCategories.Count == 0 || seriesList.Count == 0)
            return Array.Empty<byte>();

        var plot = new Plot();
        var xs = Enumerable.Range(0, xCategories.Count).Select(i => (double)i).ToArray();

        foreach (var (name, values, hex) in seriesList)
        {
            var line = plot.Add.Scatter(xs, values);
            line.Color = ScottPlot.Color.FromHex(hex.TrimStart('#'));
            line.LineWidth = 2;
            line.MarkerSize = 6;
            line.LegendText = name;
        }

        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(xs, xCategories.ToArray());
        plot.Axes.Bottom.TickLabelStyle.Rotation = 0;
        if (!string.IsNullOrWhiteSpace(yLabel))
            plot.Axes.Left.Label.Text = yLabel;
        plot.ShowLegend(Alignment.UpperRight);

        return plot.GetImageBytes(width, height, ImageFormat.Png);
    }

    public static byte[] GroupedBarsAsPng(
        IList<string> categories,
        IEnumerable<(string SeriesName, double[] Values, string HexColor)> series,
        int width = 600,
        int height = 320,
        string yLabel = "")
    {
        var seriesList = series.ToList();
        if (categories.Count == 0 || seriesList.Count == 0)
            return Array.Empty<byte>();

        var plot = new Plot();
        int catCount = categories.Count;
        int seriesCount = seriesList.Count;
        double groupWidth = 0.8;
        double barWidth = groupWidth / seriesCount;

        for (int s = 0; s < seriesCount; s++)
        {
            var (name, values, hex) = seriesList[s];
            var positions = Enumerable.Range(0, catCount)
                .Select(i => i + (s - (seriesCount - 1) / 2.0) * barWidth)
                .ToArray();

            var bars = plot.Add.Bars(positions, values);
            bars.Color = ScottPlot.Color.FromHex(hex.TrimStart('#'));
            bars.LegendText = name;
            foreach (var bar in bars.Bars)
            {
                bar.Size = barWidth * 0.95;
            }
        }

        var tickPositions = Enumerable.Range(0, catCount).Select(i => (double)i).ToArray();
        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(tickPositions, categories.ToArray());
        plot.Axes.Bottom.TickLabelStyle.Rotation = -30;
        plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.UpperRight;
        if (!string.IsNullOrWhiteSpace(yLabel))
            plot.Axes.Left.Label.Text = yLabel;

        plot.ShowLegend(Alignment.UpperRight);
        plot.Axes.Margins(bottom: 0);

        return plot.GetImageBytes(width, height, ImageFormat.Png);
    }
}
