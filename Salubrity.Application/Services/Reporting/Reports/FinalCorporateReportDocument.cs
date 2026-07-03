// File: Application/Services/Reporting/Reports/FinalCorporateReportDocument.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Salubrity.Application.DTOs.Reports;
using Salubrity.Application.Services.Reporting.Charts;

namespace Salubrity.Application.Services.Reporting.Reports;

public sealed class FinalCorporateReportDocument : IDocument
{
    private const string TealDark = "#134E4A";
    private const string LabelGray = "#6B7280";
    private const string BorderGray = "#E5E7EB";
    private const string TextGray = "#374151";

    private readonly FinalCorporateReportDto _r;
    public FinalCorporateReportDocument(FinalCorporateReportDto r) => _r = r;

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Final Corporate Report - {_r.CampName}",
        Author = "Salubrity Centre",
        CreationDate = _r.GeneratedAt,
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(28);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(t => t.FontSize(9).FontColor(TextGray));
            page.Content().Column(col =>
            {
                col.Spacing(10);
                ComposeHeader(col);
                ComposeMeta(col);
                TextSection(col, "Introduction", _r.Introduction);
                ComposeExecutiveGeneral(col);
                ComposeExecutiveDetail(col);
                TextSection(col, "Objectives and Methods", _r.ObjectivesAndMethods);
                ComposeResultAtAGlance(col);
                ComposeAgeDemographics(col);
                ComposeLifestyleRisk(col, "Lifestyle Risk Stratification", _r.LifestyleRiskOverall);
                ComposeMetabolic(col);
                ComposeMentalHealth(col);
                ComposeEyeHealth(col);
                ComposePainAssessment(col);
                ComposeLifestyleRisk(col, "Lifestyle Risk Stratification (binary)", _r.LifestyleRiskSecondary);
                ComposeSystemicOrgan(col);
                ComposeTopFindings(col);
                ComposeAnalysisOutlook(col);
                ComposeOutlookPredictions(col);
                ComposeTrendAnalysisSummary(col);
                ComposeRecommendations(col);
                TextSection(col, "Conclusion", _r.Conclusion);
                ComposeDisclaimer(col);
            });
            page.Footer().AlignCenter().Text(t =>
            {
                t.Span($"Generated {_r.GeneratedAt:yyyy-MM-dd HH:mm} UTC  -  Page ").FontSize(7).FontColor(LabelGray);
                t.CurrentPageNumber().FontSize(7).FontColor(LabelGray);
                t.Span(" of ").FontSize(7).FontColor(LabelGray);
                t.TotalPages().FontSize(7).FontColor(LabelGray);
            });
        });
    }

    private void ComposeHeader(QuestPDF.Infrastructure.IContainer _) { }
    private void ComposeHeader(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().PaddingBottom(4).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("Final Corporate Health Report").FontSize(15).Bold().FontColor(TealDark);
                c.Item().Text(_r.CampName).FontSize(11).FontColor(TealDark);
            });
            row.ConstantItem(140).AlignRight().Column(c =>
            {
                c.Item().Text(_r.ClientName).FontSize(9).Bold().FontColor(TealDark);
                c.Item().Text(_r.DateRange).FontSize(8).FontColor(LabelGray);
            });
        });
    }

    private void ComposeMeta(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Background("#F9FAFB").Border(1).BorderColor(BorderGray).Padding(8).Row(row =>
        {
            row.RelativeItem().Text($"Client: {_r.ClientName}").FontSize(8);
            row.RelativeItem().AlignCenter().Text($"Camp: {_r.CampName}").FontSize(8);
            row.RelativeItem().AlignRight().Text($"Date: {_r.DateRange}").FontSize(8);
        });
    }

    private void TextSection(QuestPDF.Fluent.ColumnDescriptor col, string title, string body)
    {
        col.Item().Text(title).FontSize(11).Bold().FontColor(TealDark);
        col.Item().Border(1).BorderColor(BorderGray).Padding(10).Text(body ?? string.Empty).FontSize(9).FontColor(TextGray);
    }

    private void ComposeExecutiveGeneral(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Executive Summary (General Overview)").FontSize(11).Bold().FontColor(TealDark);
        col.Item().Border(1).BorderColor(BorderGray).Padding(10).Column(c =>
        {
            c.Item().Row(row =>
            {
                row.RelativeItem().Column(cc =>
                {
                    cc.Item().Text("Overview").FontSize(8).Bold().FontColor(LabelGray);
                    cc.Item().PaddingTop(2).Text(_r.ExecutiveSummaryGeneral.Overview).FontSize(8);
                });
                row.ConstantItem(8);
                row.RelativeItem().Column(cc =>
                {
                    cc.Item().Text("Clinical Findings").FontSize(8).Bold().FontColor(LabelGray);
                    cc.Item().PaddingTop(2).Text(_r.ExecutiveSummaryGeneral.ClinicalFindings).FontSize(8);
                });
            });
            c.Item().PaddingTop(8).Text("Recommendation").FontSize(8).Bold().FontColor(LabelGray);
            c.Item().PaddingTop(2).Text(_r.ExecutiveSummaryGeneral.Recommendation).FontSize(8);
        });
    }

    private void ComposeExecutiveDetail(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Executive Summary").FontSize(11).Bold().FontColor(TealDark);
        col.Item().Border(1).BorderColor(BorderGray).Padding(10).Column(c =>
        {
            c.Item().Row(row =>
            {
                row.RelativeItem().Column(cc =>
                {
                    cc.Item().Text("Participation and Coverage").FontSize(8).Bold().FontColor(LabelGray);
                    cc.Item().PaddingTop(2).Text(_r.ExecutiveSummaryDetail.ParticipationCoverage).FontSize(8);
                });
                row.ConstantItem(8);
                row.RelativeItem().Column(cc =>
                {
                    cc.Item().Text("Overall health of disease").FontSize(8).Bold().FontColor(LabelGray);
                    cc.Item().PaddingTop(2).Text(_r.ExecutiveSummaryDetail.OverallHealthOfDisease).FontSize(8);
                });
            });
            c.Item().PaddingTop(8).Text("Key risk clusters").FontSize(8).Bold().FontColor(LabelGray);
            c.Item().PaddingTop(2).Text(_r.ExecutiveSummaryDetail.KeyRiskClusters).FontSize(8);
        });
    }

    private void Kpi(QuestPDF.Fluent.RowDescriptor row, string value, string label, string bg, string fg)
    {
        row.RelativeItem().Background(bg).Padding(10).Column(c =>
        {
            c.Item().AlignCenter().Text(value).FontSize(16).Bold().FontColor(fg);
            c.Item().AlignCenter().Text(label).FontSize(7).FontColor(fg);
        });
    }

    private void ComposeResultAtAGlance(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Result at a glance").FontSize(11).Bold().FontColor(TealDark);
        col.Item().Row(row =>
        {
            Kpi(row, $"{_r.ResultAtAGlance.ParticipationRate}%", "Participation Rate", "#D1FADF", "#065F46");
            row.ConstantItem(6);
            Kpi(row, $"{_r.ResultAtAGlance.AbnormalFindingsPercent}%", "Abnormal Findings", "#FFEDD5", "#9A3412");
            row.ConstantItem(6);
            Kpi(row, $"{_r.ResultAtAGlance.FollowUpPercent}%", "Follow Up", "#E5E7EB", "#1F2937");
            row.ConstantItem(6);
            Kpi(row, $"{_r.ResultAtAGlance.ParticipationRateSecondary}%", "Participation Rate", "#D1FADF", "#065F46");
        });
    }

    private void ComposeAgeDemographics(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Participation Demographics by age").FontSize(10).Bold().FontColor(TealDark);
        var labels = _r.ParticipationByAge.Buckets.Select(b => b.Label).ToList();
        var f = _r.ParticipationByAge.Buckets.Select(b => (double)b.Female).ToArray();
        var m = _r.ParticipationByAge.Buckets.Select(b => (double)b.Male).ToArray();
        if (labels.Count > 0 && (f.Sum() + m.Sum()) > 0)
        {
            var bytes = ChartImageRenderer.GroupedBarsAsPng(labels,
                new[] { ("Female", f, "#1F4E4D"), ("Male", m, "#D4C553") },
                width: 720, height: 280, yLabel: "Participants");
            if (bytes.Length > 0)
                col.Item().Border(1).BorderColor(BorderGray).Padding(8).AlignCenter().MaxWidth(540).Image(bytes).FitWidth();
        }
        if (!string.IsNullOrWhiteSpace(_r.ParticipationByAge.Notes))
            col.Item().Background("#F9FAFB").Border(1).BorderColor(BorderGray).Padding(8).Text(_r.ParticipationByAge.Notes).FontSize(8).FontColor(TextGray);
    }

    private void ComposeLifestyleRisk(QuestPDF.Fluent.ColumnDescriptor col, string title, LifestyleRiskDto data)
    {
        col.Item().Text(title).FontSize(10).Bold().FontColor(TealDark);
        var pieData = data.Slices
            .Where(s => s.Value > 0)
            .Select((s, i) => (s.Label + $" {s.Value}%", (double)s.Value, PieColor(i)))
            .ToList();
        var bytes = ChartImageRenderer.PieAsPng(pieData, width: 520, height: 240);
        col.Item().Border(1).BorderColor(BorderGray).Padding(8).Row(row =>
        {
            if (bytes.Length > 0) row.RelativeItem().AlignCenter().Image(bytes).FitWidth();
            row.RelativeItem(2).PaddingLeft(10).Text(data.Summary ?? string.Empty).FontSize(8).FontColor(TextGray);
        });
    }

    private void ComposeMetabolic(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Metabolic & NCD Risk").FontSize(10).Bold().FontColor(TealDark);
        var labels = _r.MetabolicNcdRiskBars.Buckets.Select(b => b.Label).ToList();
        var f = _r.MetabolicNcdRiskBars.Buckets.Select(b => (double)b.Female).ToArray();
        var m = _r.MetabolicNcdRiskBars.Buckets.Select(b => (double)b.Male).ToArray();
        var bytes = ChartImageRenderer.GroupedBarsAsPng(labels,
            new[] { ("Female", f, "#1F4E4D"), ("Male", m, "#D4C553") },
            width: 600, height: 240);
        col.Item().Border(1).BorderColor(BorderGray).Padding(8).Row(row =>
        {
            if (bytes.Length > 0) row.RelativeItem(2).AlignCenter().Image(bytes).FitWidth();
            row.RelativeItem().PaddingLeft(10).Text(_r.MetabolicNcdRiskBars.Notes ?? string.Empty).FontSize(8).FontColor(TextGray);
        });
    }

    private void ComposeMentalHealth(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Mental Health & Wellness").FontSize(10).Bold().FontColor(TealDark);
        var pieData = _r.MentalHealth.Distribution
            .Where(s => s.Value > 0)
            .Select((s, i) => (s.Label + $" {s.Value}%", (double)s.Value, PieColor(i)))
            .ToList();
        var bytes = ChartImageRenderer.PieAsPng(pieData, width: 520, height: 240);
        col.Item().Border(1).BorderColor(BorderGray).Padding(8).Row(row =>
        {
            if (bytes.Length > 0) row.RelativeItem().AlignCenter().Image(bytes).FitWidth();
            row.RelativeItem().PaddingLeft(10).Column(c =>
            {
                c.Item().Text($"Score: {_r.MentalHealth.OverallScoreOutOfTen}/10 — {_r.MentalHealth.Band}").FontSize(11).Bold().FontColor(TealDark);
                c.Item().PaddingTop(4).Text(_r.MentalHealth.Summary ?? string.Empty).FontSize(8).FontColor(TextGray);
            });
        });
    }

    private void ComposeEyeHealth(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Eye & Visual Health").FontSize(10).Bold().FontColor(TealDark);
        col.Item().Border(1).BorderColor(BorderGray).Padding(10).Row(row =>
        {
            row.ConstantItem(120).Background("#F9FAFB").Border(1).BorderColor(BorderGray).Padding(10).Column(c =>
            {
                c.Item().AlignCenter().Text("Left Eye").FontSize(7).FontColor(LabelGray);
                c.Item().AlignCenter().Text(_r.EyeVisualHealth.LeftEye).FontSize(16).Bold();
            });
            row.ConstantItem(8);
            row.ConstantItem(120).Background("#F9FAFB").Border(1).BorderColor(BorderGray).Padding(10).Column(c =>
            {
                c.Item().AlignCenter().Text("Right Eye").FontSize(7).FontColor(LabelGray);
                c.Item().AlignCenter().Text(_r.EyeVisualHealth.RightEye).FontSize(16).Bold();
            });
            row.RelativeItem().PaddingLeft(10).Text(_r.EyeVisualHealth.Summary ?? string.Empty).FontSize(8).FontColor(TextGray);
        });
    }

    private void ComposePainAssessment(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Pain Assessment").FontSize(10).Bold().FontColor(TealDark);
        var malePie = _r.PainAssessment.Male.Where(s => s.Value > 0).Select((s, i) => (s.Label + $" {s.Value}%", (double)s.Value, PieColor(i))).ToList();
        var femalePie = _r.PainAssessment.Female.Where(s => s.Value > 0).Select((s, i) => (s.Label + $" {s.Value}%", (double)s.Value, PieColor(i))).ToList();
        var hasPieData = malePie.Count > 0 || femalePie.Count > 0;

        if (!hasPieData)
        {
            col.Item().Border(1).BorderColor(BorderGray).Padding(12).Column(c =>
            {
                c.Item().AlignCenter().Text(_r.PainAssessment.Notes ?? "Pain assessment data not available.").FontSize(8).Italic().FontColor(LabelGray);
            });
            return;
        }

        var maleBytes = ChartImageRenderer.PieAsPng(malePie, width: 360, height: 220);
        var femaleBytes = ChartImageRenderer.PieAsPng(femalePie, width: 360, height: 220);
        col.Item().Border(1).BorderColor(BorderGray).Padding(8).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().AlignCenter().Text("Male").FontSize(8).Bold().FontColor(LabelGray);
                if (maleBytes.Length > 0) c.Item().AlignCenter().Image(maleBytes).FitWidth();
            });
            row.RelativeItem().Column(c =>
            {
                c.Item().AlignCenter().Text("Female").FontSize(8).Bold().FontColor(LabelGray);
                if (femaleBytes.Length > 0) c.Item().AlignCenter().Image(femaleBytes).FitWidth();
            });
            row.RelativeItem().PaddingLeft(10).Text(_r.PainAssessment.Notes ?? string.Empty).FontSize(8).FontColor(TextGray);
        });
    }

    private void ComposeSystemicOrgan(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Systemic Organ Function").FontSize(10).Bold().FontColor(TealDark);
        col.Item().Border(1).BorderColor(BorderGray).Padding(10).Row(row =>
        {
            row.RelativeItem(2).Row(r =>
            {
                Kpi(r, $"{_r.SystemicOrganFunction.NormalFunctionPercent}%", "Normal Function", "#D1FADF", "#065F46");
                r.ConstantItem(6);
                Kpi(r, $"{_r.SystemicOrganFunction.RequiresMonitoringPercent}%", "Requires Monitoring", "#FFF3C7", "#854D0E");
                r.ConstantItem(6);
                Kpi(r, $"{_r.SystemicOrganFunction.AtRiskPercent}%", "At Risk", "#FEE2E2", "#991B1B");
            });
            row.RelativeItem().PaddingLeft(10).Text(_r.SystemicOrganFunction.Notes ?? string.Empty).FontSize(8).FontColor(TextGray);
        });
    }

    private void ComposeTopFindings(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Top Critical Clinical Findings").FontSize(10).Bold().FontColor(TealDark);
        if (_r.TopCriticalClinicalFindings.Count == 0)
        {
            col.Item().Border(1).BorderColor(BorderGray).Padding(10).Text("No critical findings recorded for this camp yet.").FontSize(8).Italic().FontColor(LabelGray);
        }
        else
        {
            col.Item().Border(1).BorderColor(BorderGray).Column(outer =>
            {
                int i = 1;
                foreach (var f in _r.TopCriticalClinicalFindings)
                {
                    var color = f.Level == "high" ? "#dc2626" : f.Level == "med" ? "#d97706" : "#16a34a";
                    var pct = Math.Max(1, Math.Min(100, f.Pct));
                    var bg = i % 2 == 0 ? "#F9FAFB" : "#FFFFFF";
                    outer.Item().Background(bg).BorderTop(1).BorderColor(BorderGray).Padding(6).Row(row =>
                    {
                        row.ConstantItem(20).AlignMiddle().Text($"{i}.").FontSize(8).FontColor(LabelGray);
                        row.RelativeItem(3).AlignMiddle().Text(f.Name).FontSize(8);
                        row.RelativeItem(3).PaddingLeft(4).AlignMiddle().Column(bar =>
                        {
                            bar.Item().Row(b =>
                            {
                                b.RelativeItem(pct).Height(7).Background(color);
                                b.RelativeItem(Math.Max(1, 100 - pct)).Height(7).Background("#FEF3C7");
                            });
                        });
                        row.ConstantItem(70).AlignMiddle().AlignRight().Text($"n={f.N} · {f.Pct}%").FontSize(8).FontColor(LabelGray);
                    });
                    i++;
                }
            });
        }
        if (!string.IsNullOrWhiteSpace(_r.TopCriticalFindingsSummary))
            col.Item().Background("#F9FAFB").Border(1).BorderColor(BorderGray).Padding(8).Text(_r.TopCriticalFindingsSummary).FontSize(8).FontColor(TextGray);
    }

    private void ComposeAnalysisOutlook(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Analysis & Outlook").FontSize(11).Bold().FontColor(TealDark);
        var xs = _r.AnalysisOutlook.Series.SelectMany(s => s.Points.Select(p => p.XLabel)).Distinct().ToList();
        var seriesArr = _r.AnalysisOutlook.Series.Select((s, i) =>
        (
            s.Year,
            xs.Select(x => s.Points.FirstOrDefault(p => p.XLabel == x)?.Value ?? 0).ToArray(),
            i == 0 ? "#1F4E4D" : "#D4C553"
        )).ToList();
        var bytes = ChartImageRenderer.LineAsPng(xs, seriesArr, width: 720, height: 280);
        col.Item().Border(1).BorderColor(BorderGray).Padding(8).Column(c =>
        {
            c.Item().Text(_r.AnalysisOutlook.Comparator).FontSize(9).Bold().FontColor(TealDark);
            if (bytes.Length > 0) c.Item().PaddingTop(6).AlignCenter().MaxWidth(540).Image(bytes).FitWidth();
            c.Item().PaddingTop(6).Text(_r.AnalysisOutlook.Summary ?? string.Empty).FontSize(8).FontColor(TextGray);
        });
    }

    private void ComposeOutlookPredictions(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Outlook & Predictions for 2026").FontSize(10).Bold().FontColor(TealDark);
        col.Item().Row(row =>
        {
            PredictionCard(row, _r.OutlookPredictions.OverallHealth);
            row.ConstantItem(6);
            PredictionCard(row, _r.OutlookPredictions.AbnormalFindings);
            row.ConstantItem(6);
            PredictionCard(row, _r.OutlookPredictions.FollowUpRate);
        });
    }

    private void PredictionCard(QuestPDF.Fluent.RowDescriptor row, PredictionCardDto card)
    {
        var (bg, fg) = card.Direction switch
        {
            "improving" => ("#D1FADF", "#065F46"),
            "declining" => ("#FEE2E2", "#991B1B"),
            _ => ("#FFF3C7", "#854D0E"),
        };
        row.RelativeItem().Background(bg).Padding(10).Column(c =>
        {
            c.Item().Text(card.Label).FontSize(7).FontColor(fg);
            c.Item().PaddingTop(4).Text(card.Delta).FontSize(18).Bold().FontColor(fg);
            c.Item().PaddingTop(2).Text(card.Caption).FontSize(7).FontColor(fg);
        });
    }

    private void ComposeTrendAnalysisSummary(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Trend Analysis Summary").FontSize(10).Bold().FontColor(TealDark);
        col.Item().Row(row =>
        {
            row.RelativeItem().Border(1).BorderColor(BorderGray).Padding(8).Column(c =>
            {
                c.Item().Text("Improvements").FontSize(8).Bold().FontColor("#16a34a");
                c.Item().PaddingTop(2).Text(_r.TrendAnalysisSummary.Improvements).FontSize(8);
            });
            row.ConstantItem(6);
            row.RelativeItem().Border(1).BorderColor(BorderGray).Padding(8).Column(c =>
            {
                c.Item().Text("Declines").FontSize(8).Bold().FontColor("#dc2626");
                c.Item().PaddingTop(2).Text(_r.TrendAnalysisSummary.Declines).FontSize(8);
            });
            row.ConstantItem(6);
            row.RelativeItem().Border(1).BorderColor(BorderGray).Padding(8).Column(c =>
            {
                c.Item().Text("Stable Areas").FontSize(8).Bold().FontColor("#854D0E");
                c.Item().PaddingTop(2).Text(_r.TrendAnalysisSummary.StableAreas).FontSize(8);
            });
        });
    }

    private void ComposeRecommendations(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Recommendations Risks Based").FontSize(11).Bold().FontColor(TealDark);
        if (_r.Recommendations.Count == 0)
        {
            col.Item().Border(1).BorderColor(BorderGray).Padding(10).Text("No clinical recommendations recorded for this camp yet.").FontSize(8).Italic().FontColor(LabelGray);
            return;
        }
        col.Item().Border(1).BorderColor(BorderGray).Column(outer =>
        {
            int i = 1;
            foreach (var rec in _r.Recommendations)
            {
                var bg = i % 2 == 0 ? "#F9FAFB" : "#FFFFFF";
                outer.Item().Background(bg).BorderTop(1).BorderColor(BorderGray).Padding(8).Column(c =>
                {
                    c.Item().Row(row =>
                    {
                        row.RelativeItem().Text(rec.Title).FontSize(9).Bold().FontColor(TealDark);
                        row.ConstantItem(80).AlignRight().Text($"Priority: {rec.Priority}").FontSize(7).FontColor(LabelGray);
                    });
                    if (!string.IsNullOrWhiteSpace(rec.Recommendation))
                        c.Item().PaddingTop(2).Text(rec.Recommendation).FontSize(8);
                    if (!string.IsNullOrWhiteSpace(rec.ImplementationNote))
                        c.Item().PaddingTop(2).Text($"Note: {rec.ImplementationNote}").FontSize(7).Italic().FontColor(LabelGray);
                });
                i++;
            }
        });
    }

    private void ComposeDisclaimer(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().PaddingTop(6).Background("#0E3D3B").Padding(10).AlignCenter().Text($"Disclaimer: This report is prepared for the exclusive use of {_r.ClientName} management. It summarises population-level screening findings and does not constitute individual clinical diagnoses. Individual reviews receive separate confidential reports. Data is anonymised and aggregated in compliance with the Kenya Data Protection Act 2019.").FontSize(7).FontColor("#FFFFFF");
    }

    private static readonly string[] _palette = { "#134E4A", "#D4C553", "#D97706", "#D4A53D", "#dc2626", "#9CA3AF" };
    private static string PieColor(int i) => _palette[i % _palette.Length];
}
