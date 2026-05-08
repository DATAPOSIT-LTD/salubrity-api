// File: Application/Services/Reporting/Reports/ExcoCorporateReportDocument.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Salubrity.Application.DTOs.Reports;

namespace Salubrity.Application.Services.Reporting.Reports;

public sealed class ExcoCorporateReportDocument : IDocument
{
    private const string TealDark = "#0E3D3B";
    private const string LabelGray = "#6B7280";
    private const string BorderGray = "#E5E7EB";
    private const string TextGray = "#374151";

    private readonly ExcoCorporateReportDto _r;
    private readonly HashSet<string> _excluded;

    public ExcoCorporateReportDocument(ExcoCorporateReportDto r, IEnumerable<string>? excluded = null)
    {
        _r = r;
        _excluded = new HashSet<string>(excluded ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
    }

    private bool Skip(string key) => _excluded.Contains(key);

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"EXCO Corporate Report - {_r.CampName}",
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
                if (!Skip("keyKpis")) ComposeKpis(col);
                if (!Skip("wellnessHeadlines")) ComposeWellnessHeadlines(col);
                if (!Skip("strategicRiskSnapshot")) ComposeStrategicRiskSnapshot(col);
                if (!Skip("earlyPositives")) ComposeEarlyPositives(col);
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

    private void ComposeHeader(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().PaddingBottom(4).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("EXCO Corporate Health Report").FontSize(15).Bold().FontColor(TealDark);
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

    private void ComposeKpis(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("Key KPIs").FontSize(11).Bold().FontColor(TealDark);

        var kpis = new (string Key, ExcoKpiDto Dto)[]
        {
            ("kpi.campEngagement", _r.CampEngagementTurnout),
            ("kpi.vision",         _r.VisionProductivity),
            ("kpi.highBp",         _r.CardiometabolicHighBp),
            ("kpi.cdmp",           _r.CareNavigationCdmp),
            ("kpi.preDiabetes",    _r.CardiometabolicPreDiabetes),
            ("kpi.mentalHealth",   _r.MentalHealthFlags),
        };

        var visible = kpis.Where(k => !Skip(k.Key)).Select(k => k.Dto).ToList();
        if (visible.Count == 0) return;

        // 2-column layout
        for (int i = 0; i < visible.Count; i += 2)
        {
            var left = visible[i];
            var right = i + 1 < visible.Count ? visible[i + 1] : null;
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(e => RenderKpi(e, left));
                row.ConstantItem(8);
                if (right != null)
                    row.RelativeItem().Element(e => RenderKpi(e, right));
                else
                    row.RelativeItem();
            });
        }
    }

    private void RenderKpi(QuestPDF.Infrastructure.IContainer container, ExcoKpiDto kpi)
    {
        var trendBg = kpi.Trend == "Increase" ? "#FEE2E2" : kpi.Trend == "Decrease" ? "#D1FADF" : "#F3F4F6";
        var trendFg = kpi.Trend == "Increase" ? "#991B1B" : kpi.Trend == "Decrease" ? "#065F46" : "#1F2937";

        container.Border(1).BorderColor(BorderGray).Padding(10).Column(c =>
        {
            c.Item().Text(kpi.Title).FontSize(9).Bold().FontColor(TealDark);
            c.Item().PaddingTop(6).Row(r =>
            {
                r.RelativeItem().Column(cc =>
                {
                    cc.Item().Text("Percentage").FontSize(7).FontColor(LabelGray);
                    cc.Item().Text(kpi.Percentage).FontSize(11).Bold();
                });
                r.RelativeItem().Column(cc =>
                {
                    cc.Item().Text("Attendance").FontSize(7).FontColor(LabelGray);
                    cc.Item().Text(kpi.Attendance).FontSize(11).Bold();
                });
                r.RelativeItem().Column(cc =>
                {
                    cc.Item().Text("Trend").FontSize(7).FontColor(LabelGray);
                    cc.Item().Background(trendBg).Padding(4).Text(kpi.Trend).FontSize(9).Bold().FontColor(trendFg);
                });
            });
            if (!string.IsNullOrWhiteSpace(kpi.Notes))
            {
                c.Item().PaddingTop(6).Text("Notes (Auto generated):").FontSize(7).Bold().FontColor(LabelGray);
                c.Item().PaddingTop(2).Background("#F9FAFB").Padding(6).Text(kpi.Notes).FontSize(8).FontColor(TextGray);
            }
        });
    }

    private void ComposeWellnessHeadlines(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("1. One-Page Wellness Headlines for EXCO").FontSize(11).Bold().FontColor(TealDark);
        col.Item().Border(1).BorderColor(BorderGray).Padding(10).Column(c =>
        {
            c.Item().Row(row =>
            {
                row.RelativeItem().Element(e => HeadlineBlock(e, "Engagement", _r.WellnessHeadlines.Engagement));
                row.ConstantItem(8);
                row.RelativeItem().Element(e => HeadlineBlock(e, "Health-risk burden", _r.WellnessHeadlines.HealthRiskBurden));
            });
            c.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Element(e => HeadlineBlock(e, "Risk identification and triage", _r.WellnessHeadlines.RiskIdentificationAndTriage));
                row.ConstantItem(8);
                row.RelativeItem().Element(e => HeadlineBlock(e, "Health-risk burden", _r.WellnessHeadlines.HealthRiskBurdenSecondary));
            });
        });
    }

    private void HeadlineBlock(QuestPDF.Infrastructure.IContainer container, string title, string body)
    {
        container.Column(c =>
        {
            c.Item().Text(title).FontSize(8).Bold().FontColor(LabelGray);
            c.Item().PaddingTop(2).Text(body ?? string.Empty).FontSize(8);
        });
    }

    private void ComposeStrategicRiskSnapshot(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("2. Strategic Risk Snapshot").FontSize(11).Bold().FontColor(TealDark);
        col.Item().Border(1).BorderColor(BorderGray).Padding(10).Column(c =>
        {
            c.Item().Text("Workforce Health Profile (2025)").FontSize(9).Bold().FontColor(TealDark);
            c.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Element(e => HeadlineBlock(e, "Participation", _r.StrategicRiskSnapshot.ParticipationSummary));
                row.ConstantItem(8);
                row.RelativeItem().Element(e => HeadlineBlock(e, "Age profile", _r.StrategicRiskSnapshot.AgeProfileSummary));
            });
            c.Item().PaddingTop(10).Text("Highest-burden domains").FontSize(8).Bold().FontColor(LabelGray);
            c.Item().PaddingTop(2).Background("#F9FAFB").Padding(6).Text(_r.StrategicRiskSnapshot.HighestBurdenDomains ?? string.Empty).FontSize(8);
            c.Item().PaddingTop(10).Text("Business Risk Implications").FontSize(9).Bold().FontColor(TealDark);
            c.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Element(e => HeadlineBlock(e, "Future medical and insurance costs", _r.StrategicRiskSnapshot.FutureMedicalCosts));
                row.ConstantItem(8);
                row.RelativeItem().Element(e => HeadlineBlock(e, "Presenteeism and output", _r.StrategicRiskSnapshot.PresenteeismAndOutput));
            });
            c.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Element(e => HeadlineBlock(e, "Safety and quality risk", _r.StrategicRiskSnapshot.SafetyAndQualityRisk));
                row.ConstantItem(8);
                row.RelativeItem().Element(e => HeadlineBlock(e, "Employer-brand, retention and engagement risk", _r.StrategicRiskSnapshot.EmployerBrandRisk));
            });
        });
    }

    private void ComposeEarlyPositives(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().Text("3. Early positives and strengths to build on").FontSize(11).Bold().FontColor(TealDark);
        col.Item().Border(1).BorderColor(BorderGray).Padding(10).Row(row =>
        {
            row.RelativeItem().Element(e => HeadlineBlock(e, "Higher future medical and insurance costs", _r.EarlyPositives.HigherFutureMedicalCosts));
            row.ConstantItem(8);
            row.RelativeItem().Element(e => HeadlineBlock(e, "Age profile", _r.EarlyPositives.AgeProfile));
        });
    }

    private void ComposeDisclaimer(QuestPDF.Fluent.ColumnDescriptor col)
    {
        col.Item().PaddingTop(6).Background(TealDark).Padding(10).AlignCenter()
            .Text($"Disclaimer: This EXCO report is prepared for the exclusive use of {_r.ClientName} executive leadership. It summarises population-level screening findings and does not constitute individual clinical diagnoses. Data is anonymised and aggregated in compliance with the Kenya Data Protection Act 2019.")
            .FontSize(7).FontColor("#FFFFFF");
    }
}
