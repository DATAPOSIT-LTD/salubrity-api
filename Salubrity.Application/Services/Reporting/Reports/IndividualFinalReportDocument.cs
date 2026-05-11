using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Salubrity.Application.DTOs.Reports;

namespace Salubrity.Application.Services.Reporting.Reports;

/// <summary>
/// QuestPDF document for the Individual Final Report. Same spine as the Preliminary
/// doc (header, demographics, KPIs, findings, services, health score, risk bars) plus:
/// body map with silhouette image, doctor recommendation block, referrals table,
/// and signature footer.
/// </summary>
public sealed class IndividualFinalReportDocument : IDocument
{
    private const string TealDark = "#134E4A";
    private const string Yellow = "#CA8A04";
    private const string GreenSoft = "#DCFCE7";
    private const string GreenText = "#166534";
    private const string AmberSoft = "#FEF3C7";
    private const string AmberText = "#B45309";
    private const string RedSoft = "#FEE2E2";
    private const string RedText = "#B91C1C";
    private const string BorderGray = "#E5E7EB";
    private const string LabelGray = "#6B7280";
    private const string Burgundy = "#7B1F2A";

    private readonly IndividualFinalReportDto _r;

    public IndividualFinalReportDocument(IndividualFinalReportDto report)
    {
        _r = report;
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title = "Individual Final Report",
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
            page.DefaultTextStyle(t => t.FontSize(10));

            page.Content().Column(col =>
            {
                col.Spacing(15);

                ComposeHeader(col);
                ComposeDemographics(col);
                ComposeResultsAtGlance(col);
                ComposeFindingsLists(col);
                ComposeServiceSections(col);
                ComposeHealthScore(col);
                ComposeRiskBars(col);
                ComposeBodyMap(col);
                ComposeRecommendation(col);
                ComposeReferrals(col);
                ComposeSignature(col);
                ComposeDisclaimer(col, "Important Notice: This is the finalised occupational & wellness report issued after clinical review. Results should be interpreted alongside your healthcare provider. For emergencies, contact your nearest healthcare facility immediately. Data handled in accordance with the Kenya Data Protection Act 2019.");
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
                c.Item().Text("Individual Final Report: Occupation & Wellness Report")
                    .FontSize(16).FontColor(Colors.White).Bold();
                c.Item().PaddingTop(3)
                    .Text("Conclusive findings with risk analysis and personalised recommendations.")
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
            DemoCell(g, "Camp", _r.CampName);
            DemoCell(g, "Camp Date", string.IsNullOrWhiteSpace(_r.CampDate) ? "—" : _r.CampDate);
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
        if (_r.AbnormalFindings.Count == 0 && _r.BorderlineFindings.Count == 0) return;

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
            else row.RelativeItem();

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
            else row.RelativeItem();
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
                            g.Item().Row(mr =>
                            {
                                mr.RelativeItem().Text(t =>
                                {
                                    t.Span($"{m.Label}: ").SemiBold().FontSize(8);
                                    t.Span(m.Value).FontSize(8);
                                });
                                mr.AutoItem().Background(BadgeBg(m.Status))
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

    private void ComposeHealthScore(ColumnDescriptor col)
    {
        col.Item().Border(1).BorderColor(BorderGray).Padding(12).Column(c =>
        {
            c.Item().Row(row =>
            {
                row.ConstantItem(70).AlignCenter().AlignMiddle().Column(sc =>
                {
                    sc.Item().AlignCenter().Text(_r.GeneralHealthScore.Score + "%")
                        .FontSize(18).Bold().FontColor(GreenText);
                    sc.Item().AlignCenter().Text("Score").FontSize(7).FontColor(LabelGray);
                });
                row.RelativeItem().PaddingLeft(10).Column(tc =>
                {
                    tc.Item().Text("General Health Score").Bold().FontSize(11);
                    tc.Item().PaddingTop(2).Text(_r.GeneralHealthScore.Message).FontSize(9);
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

    private void ComposeBodyMap(ColumnDescriptor col)
    {
        if (_r.BodyMap.Count == 0) return;

        col.Item().PaddingTop(5).Text("C. General findings at a glance").FontSize(12).Bold();

        // Split entries left/right by index parity so they flank the silhouette.
        var leftEntries = new List<BodyMapEntryDto>();
        var rightEntries = new List<BodyMapEntryDto>();
        for (var i = 0; i < _r.BodyMap.Count; i++)
        {
            if (i % 2 == 0) leftEntries.Add(_r.BodyMap[i]);
            else rightEntries.Add(_r.BodyMap[i]);
        }

        var silhouetteBytes = TryLoadAsset("report-assets/body/silhouette.png");

        col.Item().Border(1).BorderColor(BorderGray).Padding(10).Row(row =>
        {
            row.RelativeItem().AlignMiddle().Column(lc =>
            {
                foreach (var e in leftEntries)
                    RenderBodyMapLabel(lc, e, alignRight: true);
            });
            row.ConstantItem(130).AlignCenter().AlignMiddle().Element(e =>
            {
                if (silhouetteBytes is null)
                    e.AlignCenter().Text("(silhouette)").FontSize(9).FontColor(LabelGray);
                else
                    e.Height(320).Image(silhouetteBytes).FitArea();
            });
            row.RelativeItem().AlignMiddle().Column(rc =>
            {
                foreach (var e in rightEntries)
                    RenderBodyMapLabel(rc, e, alignRight: false);
            });
        });
    }

    private static void RenderBodyMapLabel(ColumnDescriptor c, BodyMapEntryDto entry, bool alignRight)
    {
        // Status color stays for the small status caption beside the service name (red/amber/green dot),
        // but the leader line itself is burgundy to match the on-screen preview.
        var statusColor = StatusColor(entry.Status);

        c.Item().PaddingVertical(4).Row(row =>
        {
            if (alignRight)
            {
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text(entry.ServiceName).FontSize(9).Bold();
                    col.Item().AlignRight().Text(entry.Status).FontSize(8).FontColor(statusColor);
                });
                // Leader pointing toward the silhouette: thin burgundy line ending in a small circle.
                row.ConstantItem(34).AlignMiddle().Row(p =>
                {
                    p.RelativeItem().Height(2).Background(Burgundy).AlignMiddle();
                    p.ConstantItem(6).Height(6).Background(Burgundy).AlignMiddle();
                });
            }
            else
            {
                // Leader pointing away from the silhouette: small circle + thin burgundy line.
                row.ConstantItem(34).AlignMiddle().Row(p =>
                {
                    p.ConstantItem(6).Height(6).Background(Burgundy).AlignMiddle();
                    p.RelativeItem().Height(2).Background(Burgundy).AlignMiddle();
                });
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(entry.ServiceName).FontSize(9).Bold();
                    col.Item().Text(entry.Status).FontSize(8).FontColor(statusColor);
                });
            }
        });
    }

    private void ComposeRecommendation(ColumnDescriptor col)
    {
        col.Item().PaddingTop(5).Text("D. Recommendation").FontSize(12).Bold();

        var text = _r.DoctorRecommendation?.Instructions?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            col.Item().Background(AmberSoft).Padding(8)
                .Text("Pending doctor review — no recommendation has been recorded for this patient in this camp yet.")
                .FontSize(9).FontColor(AmberText);
            return;
        }

        col.Item().Border(1).BorderColor(BorderGray).Padding(10)
            .Text(text).FontSize(9);
    }

    private static void RenderLabelValue(ColumnDescriptor c, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        c.Item().PaddingTop(6).Text(label).FontSize(8).FontColor(LabelGray);
        c.Item().PaddingTop(2).Text(value).FontSize(9);
    }

    private void ComposeReferrals(ColumnDescriptor col)
    {
        if (_r.Referrals.Count == 0) return;

        col.Item().PaddingTop(5).Text("Referrals based on issues").FontSize(12).Bold();

        col.Item().Border(1).BorderColor(BorderGray).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(2);
                cols.RelativeColumn(3);
                cols.RelativeColumn(1);
                cols.RelativeColumn(2);
            });

            table.Header(h =>
            {
                h.Cell().Background("#F9FAFB").Padding(6).Text("Specialist").FontSize(8).Bold().FontColor(LabelGray);
                h.Cell().Background("#F9FAFB").Padding(6).Text("Reason").FontSize(8).Bold().FontColor(LabelGray);
                h.Cell().Background("#F9FAFB").Padding(6).Text("Urgency").FontSize(8).Bold().FontColor(LabelGray);
                h.Cell().Background("#F9FAFB").Padding(6).Text("Follow-Up Schedule").FontSize(8).Bold().FontColor(LabelGray);
            });

            foreach (var r in _r.Referrals)
            {
                table.Cell().BorderTop(1).BorderColor(BorderGray).Padding(6).Column(specCol =>
                {
                    specCol.Item().Text(
                        !string.IsNullOrWhiteSpace(r.Speciality) ? r.Speciality :
                        !string.IsNullOrWhiteSpace(r.ServiceName) ? r.ServiceName :
                        !string.IsNullOrWhiteSpace(r.ServiceProviderName) ? r.ServiceProviderName :
                        "—").FontSize(9).Bold();
                    if (!string.IsNullOrWhiteSpace(r.Speciality) && !string.IsNullOrWhiteSpace(r.ServiceName))
                        specCol.Item().Text(r.ServiceName).FontSize(7).FontColor(LabelGray);
                });
                table.Cell().BorderTop(1).BorderColor(BorderGray).Padding(6)
                    .Text(r.Reason).FontSize(9);
                table.Cell().BorderTop(1).BorderColor(BorderGray).Padding(6)
                    .Text(t => t.Span(r.Urgency?.Name ?? "—").FontSize(9).FontColor(UrgencyColor(r.Urgency?.Name)));
                table.Cell().BorderTop(1).BorderColor(BorderGray).Padding(6)
                    .Text(r.FollowUpSchedule?.Name ?? "—").FontSize(9);
            }
        });
    }

    private void ComposeSignature(ColumnDescriptor col)
    {
        col.Item().PaddingTop(10).Row(row =>
        {
            row.Spacing(10);
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("Prepared by").FontSize(8).FontColor(LabelGray);
                c.Item().Border(1).BorderColor(BorderGray).Padding(6)
                    .Text(string.IsNullOrWhiteSpace(_r.Signature.PreparedByName) ? "—" : _r.Signature.PreparedByName)
                    .FontSize(9);
                c.Item().PaddingTop(4).Text("Signature").FontSize(8).FontColor(LabelGray);
                c.Item().Border(1).BorderColor(BorderGray).Padding(10).Height(30)
                    .AlignCenter().AlignMiddle()
                    .Text("(Doctor's Signature)").FontSize(8).Italic().FontColor(LabelGray);
            });
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("Prepared at").FontSize(8).FontColor(LabelGray);
                c.Item().Border(1).BorderColor(BorderGray).Padding(6)
                    .Text(FormatDate(_r.Signature.PreparedAt)).FontSize(9);
                c.Item().PaddingTop(4).Text("Official Stamp").FontSize(8).FontColor(LabelGray);
                c.Item().Border(1).BorderColor(BorderGray).Padding(10).Height(30)
                    .AlignCenter().AlignMiddle()
                    .Text("Stamp appears here").FontSize(8).Italic().FontColor(LabelGray);
            });
        });
    }

    private void ComposeDisclaimer(ColumnDescriptor col, string text)
    {
        col.Item().Background(TealDark).Padding(8).AlignCenter()
            .Text(text).FontSize(8).Italic().FontColor(Colors.White);
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private static byte[]? TryLoadAsset(string relativePath)
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", relativePath);
            if (File.Exists(path)) return File.ReadAllBytes(path);
        }
        catch { }
        return null;
    }

    private static string FormatDate(DateTime? d) =>
        d.HasValue ? d.Value.ToString("dd/MM/yyyy") : "—";

    private static string BadgeBg(string status) => status switch
    {
        "Normal" => GreenSoft,
        "Borderline" => AmberSoft,
        "Abnormal" => RedSoft,
        _ => GreenSoft,
    };

    private static string BadgeFg(string status) => status switch
    {
        "Normal" => GreenText,
        "Borderline" => AmberText,
        "Abnormal" => RedText,
        _ => GreenText,
    };

    private static string StatusColor(string status) => status switch
    {
        "Abnormal" => "#DC2626",   // red-600
        "Borderline" => "#D97706", // amber-600
        _ => "#16A34A",            // green-600
    };

    private static string UrgencyColor(string? name) => (name ?? "").ToLowerInvariant() switch
    {
        "high" => "#DC2626",
        "medium" => "#D97706",
        _ => "#16A34A",
    };

    private static string IconLabel(string key) => key switch
    {
        "heart" => "♥",
        "eye" => "E",
        "brain" => "B",
        "tooth" => "T",
        "apple" => "A",
        _ => "•",
    };

    private static int LevelIndex(string level) => level switch
    {
        "VeryLow" => 0,
        "Low" => 1,
        "Medium" => 2,
        "High" => 3,
        "VeryHigh" => 4,
        _ => 2,
    };

    private static string LevelColor(string level) => level switch
    {
        "VeryLow" => "#F87171",
        "Low" => "#FBBF24",
        "Medium" => "#FACC15",
        "High" => "#2DD4BF",
        "VeryHigh" => "#EF4444",
        _ => "#FACC15",
    };
}
