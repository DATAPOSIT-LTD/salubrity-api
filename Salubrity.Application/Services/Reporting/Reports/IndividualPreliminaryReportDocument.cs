using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Salubrity.Application.DTOs.Reports;

namespace Salubrity.Application.Services.Reporting.Reports;

/// <summary>
/// QuestPDF document that renders the Individual Preliminary Report PDF.
/// Mirrors the on-screen layout (header banner, demographics, results-at-a-glance,
/// findings, per-service sections, disclaimer banners, health score).
/// </summary>
public sealed class IndividualPreliminaryReportDocument : IDocument
{
    // Brand colors (kept in sync with the frontend Tailwind palette)
    private const string TealDark = "#134E4A";   // teal-900
    private const string Yellow = "#CA8A04";     // yellow-600
    private const string GreenSoft = "#DCFCE7";  // green-100
    private const string GreenText = "#166534";  // green-700
    private const string AmberSoft = "#FEF3C7";  // amber-100
    private const string AmberText = "#B45309";  // amber-700
    private const string RedSoft = "#FEE2E2";    // red-100
    private const string RedText = "#B91C1C";    // red-700
    private const string BorderGray = "#E5E7EB"; // gray-200
    private const string LabelGray = "#6B7280";  // gray-500

    private readonly IndividualPreliminaryReportDto _r;
    private readonly string _reportTitle;

    public IndividualPreliminaryReportDocument(IndividualPreliminaryReportDto report, string reportTitle)
    {
        _r = report;
        _reportTitle = reportTitle;
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title = _reportTitle,
        Author = "Salubrity Centre",
        CreationDate = _r.GeneratedAt
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(t => t.FontSize(10));

            page.Content().Column(col =>
            {
                col.Spacing(15);

                ComposeHeader(col);
                ComposeDemographics(col);
                ComposeResultsAtGlance(col);
                ComposeFindingsLists(col);
                ComposeServiceSections(col);
                ComposeDisclaimer(col, "This is a preliminary report and is subject to change upon further analysis or receipt of additional information.");
                ComposeHealthScore(col);
                ComposeRiskBars(col);
                ComposeDisclaimer(col, "Important Notice: This is a preliminary report based on screening data collected at the wellness camp. It is not a clinical diagnosis. Results are subject to verification and should be interpreted by a qualified healthcare professional. Final findings, risk scoring, and recommendations will be provided in your Individual Final Report. Data handled in accordance with the Kenya Data Protection Act 2019.");
            });

            page.Footer().AlignCenter().Text(t =>
            {
                t.Span($"Generated {_r.GeneratedAt:yyyy-MM-dd HH:mm} UTC  •  ").FontSize(8).FontColor(LabelGray);
                t.Span("Page ").FontSize(8).FontColor(LabelGray);
                t.CurrentPageNumber().FontSize(8).FontColor(LabelGray);
                t.Span(" of ").FontSize(8).FontColor(LabelGray);
                t.TotalPages().FontSize(8).FontColor(LabelGray);
            });
        });
    }

    // ── Sections ────────────────────────────────────────────────────────

    private void ComposeHeader(ColumnDescriptor col)
    {
        col.Item().Row(row =>
        {
            row.ConstantItem(120).Background(Yellow).Padding(15).AlignCenter().AlignMiddle()
                .Text("SALUBRITY\nCENTRE").FontSize(11).FontColor(Colors.White).Bold();

            row.RelativeItem().Background(TealDark).Padding(15).Column(c =>
            {
                c.Item().Text(_reportTitle).FontSize(16).FontColor(Colors.White).Bold();
                c.Item().PaddingTop(3).Text("This report presents draft findings based on the data obtained from the test results.")
                    .FontSize(9).FontColor("#A7F3D0");
            });
        });
    }

    private void ComposeDemographics(ColumnDescriptor col)
    {
        col.Item().Text("Patient Demographics").FontSize(12).Bold();

        col.Item().Grid(g =>
        {
            g.Columns(4);
            g.Spacing(8);

            DemoCell(g, "Full Name", _r.Demographics.FullName);
            DemoCell(g, "Gender", _r.Demographics.Gender);
            DemoCell(g, "Branch", _r.Demographics.Branch);
            DemoCell(g, "Email Address", _r.Demographics.Email);
            DemoCell(g, "Phone No.", _r.Demographics.Phone);
            DemoCell(g, "Date of birth", FormatDate(_r.Demographics.DateOfBirth));
            DemoCell(g, "Nationality", _r.Demographics.Nationality);
        });
    }

    private static void DemoCell(GridDescriptor g, string label, string value)
    {
        g.Item().Column(c =>
        {
            c.Item().Text(label + ":").FontSize(8).FontColor(LabelGray);
            c.Item().Border(1).BorderColor(BorderGray).Padding(6).Text(value).FontSize(9);
        });
    }

    private void ComposeResultsAtGlance(ColumnDescriptor col)
    {
        col.Item().Text("Results at a glance").FontSize(12).Bold();

        col.Item().Row(row =>
        {
            row.Spacing(8);
            StatCard(row, _r.ResultsAtGlance.ParametersTested, "Parameters Tested", "#E5E7EB", "#374151");
            StatCard(row, _r.ResultsAtGlance.NormalCount, "Normal Findings", GreenSoft, "#14532D");
            StatCard(row, _r.ResultsAtGlance.BorderlineCount, "Borderline", AmberSoft, "#78350F");
            StatCard(row, _r.ResultsAtGlance.AbnormalCount, "Abnormal Findings", RedSoft, "#7F1D1D");
        });
    }

    private static void StatCard(RowDescriptor row, int value, string label, string bg, string textColor)
    {
        row.RelativeItem().Background(bg).Padding(10).Column(c =>
        {
            c.Item().AlignCenter().Text(value.ToString()).FontSize(18).Bold().FontColor(textColor);
            c.Item().AlignCenter().PaddingTop(2).Text(label).FontSize(8).FontColor(textColor);
        });
    }

    private void ComposeFindingsLists(ColumnDescriptor col)
    {
        if (_r.AbnormalFindings.Count == 0 && _r.BorderlineFindings.Count == 0)
            return;

        col.Item().Row(row =>
        {
            row.Spacing(15);

            if (_r.AbnormalFindings.Count > 0)
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Abnormal Findings:").Bold().FontSize(10);
                    var i = 1;
                    foreach (var f in _r.AbnormalFindings)
                        c.Item().Text($"{i++}. {f}").FontSize(9);
                });
            }
            else
            {
                row.RelativeItem();
            }

            if (_r.BorderlineFindings.Count > 0)
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Borderline Findings:").Bold().FontSize(10);
                    var i = 1;
                    foreach (var f in _r.BorderlineFindings)
                        c.Item().Text($"{i++}. {f}").FontSize(9);
                });
            }
            else
            {
                row.RelativeItem();
            }
        });
    }

    private void ComposeServiceSections(ColumnDescriptor col)
    {
        if (_r.ServiceSections.Count == 0) return;

        col.Item().PaddingTop(5).Text("Test results:").FontSize(12).Bold();

        foreach (var s in _r.ServiceSections)
        {
            col.Item().Border(1).BorderColor(BorderGray).Row(row =>
            {
                row.ConstantItem(110).Padding(10).AlignMiddle().Column(c =>
                {
                    c.Item().AlignCenter().Text(IconLabel(s.IconKey)).FontSize(20);
                    c.Item().AlignCenter().PaddingTop(4).Text(s.ServiceName).FontSize(8).FontColor(TealDark).Bold();
                });

                row.RelativeItem().Background("#F9FAFB").Padding(10).Column(c =>
                {
                    c.Item().Text("Test Results:").Bold().FontSize(9);

                    c.Item().PaddingTop(4).Grid(g =>
                    {
                        g.Columns(2);
                        g.Spacing(4);
                        foreach (var m in s.Metrics)
                        {
                            g.Item().Row(metricRow =>
                            {
                                metricRow.RelativeItem().Text(t =>
                                {
                                    t.Span($"{m.Label}: ").SemiBold().FontSize(8);
                                    t.Span(m.Value).FontSize(8);
                                });
                                metricRow.AutoItem().Background(BadgeBg(m.Status))
                                    .PaddingHorizontal(4).PaddingVertical(1)
                                    .Text(m.Status).FontSize(7).FontColor(BadgeFg(m.Status));
                            });
                        }
                    });

                    c.Item().PaddingTop(8).BorderTop(1).BorderColor(BorderGray)
                        .PaddingTop(4).Text("Summary:").Bold().FontSize(9);
                    c.Item().PaddingTop(2).MinHeight(20)
                        .Text(string.IsNullOrWhiteSpace(s.Summary) ? " " : s.Summary)
                        .FontSize(8).FontColor(LabelGray);
                });
            });
        }
    }

    private void ComposeDisclaimer(ColumnDescriptor col, string text)
    {
        col.Item().Background(TealDark).Padding(8).AlignCenter()
            .Text(text).FontSize(8).Italic().FontColor(Colors.White);
    }

    private void ComposeHealthScore(ColumnDescriptor col)
    {
        col.Item().Border(1).BorderColor(BorderGray).Padding(12).Column(c =>
        {
            c.Item().Row(row =>
            {
                row.ConstantItem(70).AlignCenter().AlignMiddle().Column(scoreCol =>
                {
                    scoreCol.Item().AlignCenter().Text(_r.GeneralHealthScore.Score + "%")
                        .FontSize(18).Bold().FontColor(GreenText);
                    scoreCol.Item().AlignCenter().Text("Score").FontSize(7).FontColor(LabelGray);
                });
                row.RelativeItem().PaddingLeft(10).Column(textCol =>
                {
                    textCol.Item().Text("General Health Score").Bold().FontSize(11);
                    textCol.Item().PaddingTop(2).Text(_r.GeneralHealthScore.Message).FontSize(9);
                });
            });
        });
    }

    private void ComposeRiskBars(ColumnDescriptor col)
    {
        if (_r.RiskBars.Count == 0) return;

        col.Item().Border(1).BorderColor(BorderGray).Padding(12).Column(c =>
        {
            c.Item().Row(row =>
            {
                row.ConstantItem(120);
                row.RelativeItem().Row(headerRow =>
                {
                    foreach (var label in new[] { "Very Low", "Low", "Medium", "High", "Very High" })
                        headerRow.RelativeItem().Text(label).FontSize(7).FontColor(LabelGray);
                });
            });

            foreach (var bar in _r.RiskBars)
            {
                c.Item().PaddingTop(4).Row(row =>
                {
                    row.ConstantItem(120).AlignMiddle().Text(bar.Name).FontSize(8);
                    row.RelativeItem().Height(8).Row(barRow =>
                    {
                        var idx = LevelIndex(bar.Level);
                        for (var i = 0; i < 5; i++)
                            barRow.RelativeItem().Background(i == idx ? LevelColor(bar.Level) : "#F3F4F6");
                    });
                });
            }
        });
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private static string FormatDate(DateTime? d) =>
        d.HasValue ? d.Value.ToString("dd/MM/yyyy") : "—";

    private static string BadgeBg(string status) => status switch
    {
        "Normal" => GreenSoft,
        "Borderline" => AmberSoft,
        "Abnormal" => RedSoft,
        _ => GreenSoft
    };

    private static string BadgeFg(string status) => status switch
    {
        "Normal" => GreenText,
        "Borderline" => AmberText,
        "Abnormal" => RedText,
        _ => GreenText
    };

    private static string IconLabel(string key) => key switch
    {
        "heart" => "♥",
        "eye" => "👁",
        "brain" => "🧠",
        "tooth" => "🦷",
        "apple" => "🍎",
        _ => "•"
    };

    private static int LevelIndex(string level) => level switch
    {
        "VeryLow" => 0,
        "Low" => 1,
        "Medium" => 2,
        "High" => 3,
        "VeryHigh" => 4,
        _ => 2
    };

    private static string LevelColor(string level) => level switch
    {
        "VeryLow" => "#F87171",   // red-400
        "Low" => "#FBBF24",       // amber-400
        "Medium" => "#FACC15",    // yellow-400
        "High" => "#2DD4BF",      // teal-400
        "VeryHigh" => "#EF4444",  // red-500
        _ => "#FACC15"
    };
}
