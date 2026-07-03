// File: Application/Services/Reporting/Reports/CorporateReportDocument.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Salubrity.Application.DTOs.Reports;

namespace Salubrity.Application.Services.Reporting.Reports;

public sealed class CorporateReportDocument : IDocument
{
    private const string TealDark = "#134E4A";
    private const string LabelGray = "#6B7280";
    private const string BorderGray = "#E5E7EB";
    private const string AmberSoft = "#F5F1BF";
    private const string RedSoft = "#FEE2E2";

    private readonly CorporateReportDto _r;

    public CorporateReportDocument(CorporateReportDto report) => _r = report;

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Corporate Report - {_r.CampName}",
        Author = "Salubrity Centre",
        CreationDate = _r.GeneratedAt,
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(t => t.FontSize(9));

            page.Content().Column(col =>
            {
                col.Spacing(12);
                ComposeHeader(col);
                ComposeMeta(col);
                ComposeKPIs(col);
                ComposeAttendance(col);
                ComposeStationCompletion(col);
                ComposeTopFindings(col);
        ComposeCardiometabolic(col);
                ComposeNarratives(col);
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

    private void ComposeHeader(ColumnDescriptor col)
    {
        col.Item().Background(TealDark).Padding(16).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("SALUBRITY CENTRE").FontSize(16).Bold().FontColor(Colors.White);
                c.Item().Text("Preliminary Corporate Report").FontSize(10).FontColor("#A7F3D0");
            });
            row.ConstantItem(120).AlignRight().Column(c =>
            {
                c.Item().Text(_r.DateRange).FontSize(8).FontColor(Colors.White);
                c.Item().Text($"Generated: {_r.GeneratedAt:dd MMM yyyy}").FontSize(7).FontColor("#A7F3D0");
            });
        });
    }

    private void ComposeMeta(ColumnDescriptor col)
    {
        col.Item().Border(1).BorderColor(BorderGray).Padding(10).Row(row =>
        {
            MetaCell(row, "Camp", _r.CampName);
            MetaCell(row, "Client", _r.ClientName);
            MetaCell(row, "Package", _r.PackageName);
            MetaCell(row, "Venue", _r.Venue);
        });
    }

    private static void MetaCell(RowDescriptor row, string label, string value)
    {
        row.RelativeItem().Column(c =>
        {
            c.Item().Text(label).FontSize(7).FontColor(LabelGray);
            c.Item().Text(value.Length > 0 ? value : "-").FontSize(9).Bold();
        });
    }

    private void ComposeKPIs(ColumnDescriptor col)
    {
        col.Item().Text("Camp Summary").FontSize(11).Bold().FontColor(TealDark);
        col.Item().Row(row =>
        {
            KpiCard(row, $"{_r.ParticipationRate}%", "Participation Rate", "#FCE7F3", "#9D174D");
            KpiCard(row, _r.TotalAttendees.ToString(), "Total Attendees", "#F5F1BF", "#6B6213");
            KpiCard(row, _r.TotalServices.ToString(), "Total Services", "#F3F4F6", "#374151");
            KpiCard(row, $"{(_r.ParticipationTrend >= 0 ? "+" : "")}{_r.ParticipationTrend}%", "Trend", "#D1FAE5", "#065F46");
        });
    }

    private static void KpiCard(RowDescriptor row, string value, string label, string bg, string fg)
    {
        row.RelativeItem().Background(bg).Border(1).BorderColor("#E5E7EB").Padding(12).Column(c =>
        {
            c.Item().Text(value).FontSize(22).Bold().FontColor(fg).AlignCenter();
            c.Item().PaddingTop(2).Text(label).FontSize(7).FontColor(LabelGray).AlignCenter();
        });
    }

    private void ComposeAttendance(ColumnDescriptor col)
    {
        col.Item().Text("Attendance Split").FontSize(11).Bold().FontColor(TealDark);
        col.Item().Border(1).BorderColor(BorderGray).Padding(12).Column(c =>
        {
            // Stat number blocks
            c.Item().Row(row =>
            {
                AttendanceStat(row, "Female", _r.Attendance.Female, _r.Attendance.FemalePercent, "#7A6E1C");
                row.ConstantItem(8); // gap
                AttendanceStat(row, "Male", _r.Attendance.Male, _r.Attendance.MalePercent, TealDark);
                row.RelativeItem(); // spacer
            });

            // Full-width proportional stacked bar
            c.Item().PaddingTop(10).Column(inner =>
            {
                inner.Item().Text("Gender Split").FontSize(7).FontColor(LabelGray);
                inner.Item().PaddingTop(4).Row(bar =>
                {
                    int fp = Math.Max(1, Math.Min(99, _r.Attendance.FemalePercent));
                    int mp = Math.Max(1, 100 - fp);
                    bar.RelativeItem(fp).Height(22).Background("#D3C34A");
                    bar.RelativeItem(mp).Height(22).Background(TealDark);
                });
                inner.Item().PaddingTop(5).Row(legend =>
                {
                    legend.AutoItem().Width(10).Height(10).Background("#D3C34A");
                    legend.AutoItem().PaddingLeft(3).Text($"Female {_r.Attendance.FemalePercent}%").FontSize(7).FontColor(LabelGray);
                    legend.ConstantItem(16);
                    legend.AutoItem().Width(10).Height(10).Background(TealDark);
                    legend.AutoItem().PaddingLeft(3).Text($"Male {_r.Attendance.MalePercent}%").FontSize(7).FontColor(LabelGray);
                });
            });
        });
    }

    private static void AttendanceStat(RowDescriptor row, string label, int count, int pct, string color)
    {
        row.ConstantItem(110).Background("#F9FAFB").Border(1).BorderColor("#E5E7EB").Padding(8).Column(c =>
        {
            c.Item().Text(label).FontSize(7).FontColor(LabelGray);
            c.Item().Text(count.ToString()).FontSize(24).Bold().FontColor(color);
            c.Item().Text($"{pct}% of attendees").FontSize(7).FontColor(LabelGray);
        });
    }

    private void ComposeStationCompletion(ColumnDescriptor col)
    {
        if (_r.StationCompletion.Count == 0) return;
        col.Item().Text("Station Completion Rates").FontSize(11).Bold().FontColor(TealDark);
        col.Item().Border(1).BorderColor(BorderGray).Column(outer =>
        {
            // Header
            outer.Item().Background(TealDark).Padding(6).Row(h =>
            {
                h.RelativeItem(3).Text("Station").FontColor(Colors.White).Bold().FontSize(8);
                h.RelativeItem(4).PaddingLeft(4).Text("Completion (Female / Male)").FontColor(Colors.White).Bold().FontSize(8);
                h.ConstantItem(38).AlignRight().Text("F% / M%").FontColor(Colors.White).Bold().FontSize(8);
            });
            bool alt = false;
            foreach (var s in _r.StationCompletion)
            {
                var rowBg = alt ? "#F9FAFB" : "#FFFFFF";
                alt = !alt;
                int fp = Math.Max(0, Math.Min(100, s.Female));
                int mp = Math.Max(0, Math.Min(100, s.Male));
                outer.Item().Background(rowBg).BorderTop(1).BorderColor("#E5E7EB").Padding(6).Row(row =>
                {
                    row.RelativeItem(3).AlignMiddle().Text(s.Name).FontSize(8);
                    row.RelativeItem(4).PaddingLeft(4).AlignMiddle().Column(bars =>
                    {
                        if (fp == 0 && mp == 0)
                        {
                            bars.Item().PaddingTop(2).Text("No submissions recorded")
                                .FontSize(7).FontColor("#9CA3AF").Italic();
                        }
                        else
                        {
                            // Female bar with label
                            bars.Item().Row(bar =>
                            {
                                bar.ConstantItem(12).AlignMiddle()
                                    .Text("F").FontSize(6).Bold().FontColor("#7A6E1C");
                                bar.RelativeItem().AlignMiddle().Row(b =>
                                {
                                    if (fp > 0) b.RelativeItem(fp).Height(10).Background("#D3C34A");
                                    b.RelativeItem(Math.Max(1, 100 - fp)).Height(10).Background("#EEEAAA");
                                });
                            });
                            bars.Item().PaddingTop(3).Row(bar =>
                            {
                                bar.ConstantItem(12).AlignMiddle()
                                    .Text("M").FontSize(6).Bold().FontColor(TealDark);
                                bar.RelativeItem().AlignMiddle().Row(b =>
                                {
                                    if (mp > 0) b.RelativeItem(mp).Height(10).Background(TealDark);
                                    b.RelativeItem(Math.Max(1, 100 - mp)).Height(10).Background("#99D4CF");
                                });
                            });
                        }
                    });
                    row.ConstantItem(38).AlignMiddle().AlignRight().Column(c =>
                    {
                        c.Item().Text($"{fp}%").FontSize(9).Bold().FontColor(fp >= 80 ? "#16A34A" : fp >= 50 ? "#7A6E1C" : "#6B7280");
                        c.Item().Text($"{mp}%").FontSize(9).Bold().FontColor(mp >= 80 ? "#16A34A" : mp >= 50 ? TealDark : "#6B7280");
                    });
                });
            }
            // Legend
            outer.Item().Background("#F9FAFB").BorderTop(1).BorderColor("#E5E7EB").Padding(6).Row(leg =>
            {
                leg.AutoItem().Width(10).Height(7).Background("#D3C34A");
                leg.AutoItem().PaddingLeft(3).Text("Female completion").FontSize(6).FontColor(LabelGray);
                leg.ConstantItem(12);
                leg.AutoItem().Width(10).Height(7).Background(TealDark);
                leg.AutoItem().PaddingLeft(3).Text("Male completion").FontSize(6).FontColor(LabelGray);
            });
        });
    }

    private void ComposeTopFindings(ColumnDescriptor col)
    {
        if (_r.TopFindings.Count == 0) return;
        col.Item().Text("Top Clinical Findings").FontSize(11).Bold().FontColor(TealDark);
        col.Item().Border(1).BorderColor(BorderGray).Column(outer =>
        {
            outer.Item().Background(TealDark).Padding(6).Row(h =>
            {
                h.RelativeItem(3).Text("Finding").FontColor(Colors.White).Bold().FontSize(8);
                h.RelativeItem(3).PaddingLeft(4).Text("Prevalence").FontColor(Colors.White).Bold().FontSize(8);
                h.ConstantItem(36).AlignRight().Text("Count").FontColor(Colors.White).Bold().FontSize(8);
                h.ConstantItem(36).AlignRight().Text("%").FontColor(Colors.White).Bold().FontSize(8);
            });
            bool alt = false;
            foreach (var f in _r.TopFindings)
            {
                string rowBg = f.Level switch { "high" => "#FEF2F2", "med" => "#FFFBEB", _ => alt ? "#F9FAFB" : "#FFFFFF" };
                string barColor = f.Level switch { "high" => "#EF4444", "med" => "#F59E0B", _ => TealDark };
                string textColor = f.Level switch { "high" => "#B91C1C", "med" => "#7A6E1C", _ => TealDark };
                alt = !alt;
                int pct = Math.Max(0, Math.Min(100, f.Pct));
                outer.Item().Background(rowBg).BorderTop(1).BorderColor("#E5E7EB").Padding(6).Row(row =>
                {
                    row.RelativeItem(3).AlignMiddle().Text(f.Name).FontSize(8);
                    row.RelativeItem(3).PaddingLeft(4).AlignMiddle().Column(bar =>
                    {
                        bar.Item().Row(b =>
                        {
                            if (pct > 0) b.RelativeItem(pct).Height(10).Background(barColor);
                            b.RelativeItem(Math.Max(1, 100 - pct)).Height(10).Background("#E5E7EB");
                        });
                    });
                    row.ConstantItem(36).AlignMiddle().AlignRight().Text(f.N.ToString()).FontSize(8).FontColor(textColor);
                    row.ConstantItem(36).AlignMiddle().AlignRight().Text($"{f.Pct}%").FontSize(8).FontColor(textColor).Bold();
                });
            }
        });
    }

    private void ComposeNarratives(ColumnDescriptor col)
    {
        col.Item().Text("Narratives & Conclusions").FontSize(11).Bold().FontColor(TealDark);
        NarrativeBlock(col, "Overview", _r.Narratives.Overview);
        NarrativeBlock(col, "Clinical Findings", _r.Narratives.ClinicalFindings);
        NarrativeBlock(col, "Attendance Notes", _r.Narratives.AttendanceNotes);
        NarrativeBlock(col, "Station Summary", _r.Narratives.StationSummary);
        NarrativeBlock(col, "Top Findings Summary", _r.Narratives.TopFindingsSummary);
        NarrativeBlock(col, "Overall Conclusion", _r.Narratives.OverallConclusion);
        NarrativeBlock(col, "What Happens Next", _r.Narratives.WhatHappensNext);
    }

    private static void NarrativeBlock(ColumnDescriptor col, string title, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        col.Item().Column(c =>
        {
            c.Item().Text(title).FontSize(9).Bold().FontColor(TealDark);
            c.Item().Background("#F9FAFB").Border(1).BorderColor(BorderGray).Padding(8)
                .Text(text).FontSize(8).FontColor("#374151");
        });
    }

    private static void ComposeDisclaimer(ColumnDescriptor col)
    {
        col.Item().BorderTop(1).BorderColor(BorderGray).PaddingTop(6)
            .Text("Important Notice: This preliminary corporate report is generated for internal review purposes. Individual health data is anonymised in aggregate views. Results should be interpreted by qualified occupational health professionals. Data handled in accordance with the Kenya Data Protection Act 2019.")
            .FontSize(7).FontColor(LabelGray).Italic();
    }


    private void ComposeCardiometabolic(ColumnDescriptor col)
    {
        var c = _r.Cardiometabolic ?? new Salubrity.Application.DTOs.Reports.CardiometabolicSnapshotDto();
        col.Item().PaddingTop(8).Text("Cardiometabolic snapshot").FontSize(11).Bold();

        col.Item().PaddingTop(4).Row(row =>
        {
            // Average BP
            row.RelativeItem().Border(1).BorderColor("#E5E7EB").Padding(8).Column(cell =>
            {
                cell.Item().Text("Average Blood Pressure").FontSize(8).FontColor("#6B7280");
                cell.Item().PaddingTop(2).Text(
                    c.BloodPressureSamples > 0 ? $"{c.AverageSystolic} / {c.AverageDiastolic} mmHg" : "—"
                ).FontSize(14).Bold().FontColor("#7B1F2A");
                cell.Item().PaddingTop(2).Text($"Based on {c.BloodPressureSamples} readings").FontSize(7).FontColor("#6B7280");
            });

            row.ConstantItem(6);

            // BMI distribution
            row.RelativeItem().Border(1).BorderColor("#E5E7EB").Padding(8).Column(cell =>
            {
                cell.Item().Text("BMI distribution").FontSize(8).FontColor("#6B7280");
                cell.Item().PaddingTop(2).Element(e => RenderBands(e, c.BmiSamples, new[]
                {
                    ("Underweight", c.BmiUnderweight, "#60A5FA"),
                    ("Normal", c.BmiNormal, "#16A34A"),
                    ("Overweight", c.BmiOverweight, "#F59E0B"),
                    ("Obese", c.BmiObese, "#DC2626"),
                }));
                cell.Item().PaddingTop(2).Text($"{c.BmiSamples} samples").FontSize(7).FontColor("#6B7280");
            });

            row.ConstantItem(6);

            // RBS bands
            row.RelativeItem().Border(1).BorderColor("#E5E7EB").Padding(8).Column(cell =>
            {
                cell.Item().Text("RBS (mmol/L)").FontSize(8).FontColor("#6B7280");
                cell.Item().PaddingTop(2).Element(e => RenderBands(e, c.RbsSamples, new[]
                {
                    ("Normal (<7.8)", c.RbsNormal, "#16A34A"),
                    ("Impaired (7.8-11.0)", c.RbsImpaired, "#F59E0B"),
                    ("Diabetic (>=11.1)", c.RbsDiabetic, "#DC2626"),
                }));
                cell.Item().PaddingTop(2).Text($"{c.RbsSamples} samples").FontSize(7).FontColor("#6B7280");
            });
        });
    }

    private static void RenderBands(QuestPDF.Infrastructure.IContainer container, int total, (string label, int value, string color)[] items)
    {
        container.Column(stack =>
        {
            foreach (var item in items)
            {
                var pct = total > 0 ? (int)Math.Round(item.value * 100.0 / total) : 0;
                stack.Item().PaddingTop(2).Text(t =>
                {
                    t.Span(item.label).FontSize(7).FontColor("#374151");
                    t.Span($"  {item.value} ({pct}%)").FontSize(7).FontColor("#6B7280");
                });
                stack.Item().Row(bar =>
                {
                    if (pct > 0) bar.RelativeItem(pct).Height(5).Background(item.color);
                    bar.RelativeItem(Math.Max(1, 100 - pct)).Height(5).Background("#F3F4F6");
                });
            }
        });
    }
}
