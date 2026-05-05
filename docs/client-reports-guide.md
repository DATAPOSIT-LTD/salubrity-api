# Client Reports — User Guide

This guide explains the three corporate reports that Salubrity produces after a health camp, who each one is for, what's in it, and how to generate, edit, download, and email them.

---

## At a glance

| Report | Audience | Length | When to use it |
|---|---|---|---|
| **Preliminary Corporate Report** | HR / operations | 1 page | Right after the camp closes, as a quick read on participation, gender split, station completion, top findings. |
| **Final Corporate Report** | HR / management / clinical leads | ~10 pages | Once doctor sign-offs are in. Adds risk stratification, mental health, eye/visual health, recommendations, conclusion. |
| **EXCO Corporate Report** | Executive committee / leadership | 2–3 pages | For board-level review. KPI-focused with business-risk implications and headlines. |

All three are accessed from **Admin → Reports → [select camp] → Generate Report**, then choose the report type via the radio at the top.

---

## How to generate any report

1. From the admin sidebar, click **Reports**.
2. Pick the camp you want to report on. You'll land on the *Generate Report* page.
3. Choose **Preliminary**, **Final**, or **EXCO** in the *Type of report* row.
4. Apply filters (optional, see §5).
5. The report renders on screen. Use the **Download Report** button (top right) for a PDF, or **Send to mail** to email it as an attachment.

The first load takes 3–6 seconds (Gemini is generating the narratives). Subsequent views within a minute are cached.

---

## 1. Preliminary Corporate Report

### What's in it
- **Camp Summary KPIs** — Participation Rate, Total Attendees, Total Services, Participation Trend
- **Overview** — narrative paragraph
- **Clinical Findings** — narrative paragraph
- **Visualization** — Attendance pie (Female / Male) + Critical Findings pie (sample · phase 3) with Notes for each
- **Station Completion Rates** — bar chart per station + per-row Female/Male breakdown
- **Top Critical Clinical Findings** — horizontal bars ranked by prevalence, color-coded by risk level
- **Overall Summary / Conclusion** and **What happens Next** — collapsible narrative blocks
- **Disclaimer footer**

### Where the data comes from
- Participation, gender split, station completion, top findings: **real**, computed from form responses + `VitalThresholds`.
- All narrative text: **AI-generated** by Gemini, grounded in the real numbers.
- Critical Findings pie: **placeholder sample data** (40/60) while per-metric Normal/Abnormal classification is still in development.

### Editing
Every narrative block has an **Edit** button → textarea → Save / Cancel. Edits live for the rest of the session; refreshing the page regenerates from Gemini.

### Download / Email
- **Download Report** → `corporate-report-{camp-slug}.pdf`
- **Send to mail** → opens a modal with prefilled Contact Person (from the camp's organization), recipient chip-list, message textarea, and an attachment indicator showing the PDF filename + size

---

## 2. Final Corporate Report

### What's in it
The Final report extends the Preliminary with a much more detailed clinical narrative, broken into ~20 collapsible sections:

1. **Introduction**
2. **Executive Summary (General Overview)** — Overview, Clinical Findings, Recommendation
3. **Executive Summary** — Participation & Coverage, Overall health of disease, Key risk clusters
4. **Objectives and Methods**
5. **Result at a glance** — 4 KPI tiles + 9 sub-charts:
   - Participation Demographics by age (bar)
   - Lifestyle Risk Stratification (donut)
   - Metabolic & NCD Risk (bar by gender)
   - Mental Health & Wellness (donut + score)
   - Eye & Visual Health (Left/Right acuity tiles)
   - Pain Assessment (donut by gender)
   - Lifestyle Risk Stratification (binary view)
   - Systemic Organ Function (KPI tiles)
   - Top Critical Clinical Findings (bars with ICD-10 codes)
6. **Analysis & Outlook** — line chart comparing prior cycles with summary
7. **Outlook & Predictions** — three trend cards (Improving / Stable / Needs Attention)
8. **Trend Analysis Summary** — Improvements, Declines, Stable Areas
9. **Recommendations Risks Based** — list with priority dropdowns
10. **Conclusion**

### What's real vs sample (current state)

| Section | Status |
|---|---|
| Participation Rate | Real |
| Participation Demographics by age | Real (`Users.DateOfBirth` × `Genders.Name`) |
| Top Critical Clinical Findings | Real (reused from Preliminary) |
| Eye & Visual Health acuity (left/right) | Real if intake captured `VA` / `Vision` fields under Left Eye / Right Eye sections; otherwise shows "—" with a "no eye exam recorded for this camp" note |
| Recommendations Risks Based | Real (queries `HealthAssessmentRecommendations`); empty until that table is populated |
| Lifestyle Risk, Metabolic/NCD bars, Mental Health donut, Pain Assessment, Systemic Organ Function | Sample data (Pass B in roadmap — needs classification rules) |
| Analysis & Outlook line chart, Outlook Predictions cards | Sample data (needs cross-camp historical query) |
| Every text narrative | AI-generated by Gemini |

### Editing
Same pattern as Preliminary — every text block has an Edit button. Recommendation rows have textareas for the recommendation + implementation note + a priority dropdown.

### Download / Email
- **Download Report** → `final-corporate-report-{camp-slug}.pdf` — full ~20-section QuestPDF document with embedded ScottPlot charts.
- **Send to mail** → same modal as Preliminary, posts to the Final endpoint.

---

## 3. EXCO Corporate Report

The EXCO report is the leadership view: short, KPI-driven, business-impact framed.

### What's in it
- **Header** — camp name, client, date range
- **Key KPIs** (6 cards in a 2-column grid):
  - Camp Engagement & Turnout
  - Vision & productivity Visual issues
  - Cardiometabolic (High BP)
  - Care navigation CDMP recommended
  - Cardiometabolic (Pre-diabetes)
  - Mental health (MH Flags)

  Each card has Percentage, Attendance (e.g. `41/225`), Trend (Increase / Decrease / Stable), and AI-generated Notes.
- **1. One-Page Wellness Headlines for EXCO** — Engagement, Health-risk burden, Risk identification and triage
- **2. Strategic Risk Snapshot** — Workforce Health Profile (Participation + Age profile), Highest-burden domains (bullet list), Business Risk Implications (4 cards: Future medical costs, Presenteeism and output, Safety and quality risk, Employer-brand risk)
- **3. Early positives and strengths to build on**

### How the KPIs are calculated
Each KPI's **attendance count** is the number of distinct patients who completed at least one form matching that station's keywords:

| KPI | Form-name keywords matched |
|---|---|
| Camp Engagement & Turnout | (special — total attendees / expected) |
| Vision & productivity Visual issues | `eye`, `optomet`, `vision` (preferred: distinct patients with a `Left Eye` / `Right Eye` field response) |
| Cardiometabolic (High BP) | `triage`, `physical exam` |
| Care navigation CDMP recommended | `chronic`, `referral`, `cdmp` |
| Cardiometabolic (Pre-diabetes) | `triage service diagnosis`, `glucose`, `rbs`, `blood sugar`, `random blood sugar` |
| Mental health (MH Flags) | `mental` |

The **percentage** is `attendance / max(totalAttendees, expected)`. KPIs without matching forms (often CDMP, Vision when no eye station was set up) will show 0% — Gemini's notes will explicitly call out the data gap.

### Section / KPI checkboxes
Each section header and each KPI card has a **green checkbox** to its left. Unchecking one fades it on screen and **excludes it from the downloaded PDF and email**. The selection survives the page session but resets on refresh.

### Editing
Every narrative block (KPI Notes, headlines, business risk implications, early positives) has an Edit button.

### Download / Email
- **Download Report** → `exco-corporate-report-{camp-slug}.pdf` — QuestPDF document with the 6 KPI cards + collapsible sections, respecting any unchecked exclusions from the on-screen view.
- **Send to mail** → same modal pattern; posts to `/api/v1/reports/corporate/{campId}/exco/send-email` with the exclusion list as a query param.

---

## 4. Title bar (above each report's content)

Above the rendered content of any report, a small bar shows the selected report's full label and the camp name + date range — a quick orientation cue when scrolling.

---

## 5. Filters

Three filters apply to all three report types (Preliminary / Final / EXCO):

| Filter | Effect |
|---|---|
| **Gender** (Female / Male / Any) | Restricts participants and form responses to that gender |
| **Age** (e.g. `18–30`, `31–45`) | Restricts to participants whose age falls in that bucket (computed from `Users.DateOfBirth`) |
| **Day** (1, 2, 3, …) | Restricts form responses to those submitted on the Nth day of the camp (Day 1 = `camp.StartDate`) |

Filters propagate end-to-end:
- The **Total denominator** shrinks to the filtered population, so percentages remain meaningful.
- The **Top Findings** list re-ranks against the filtered cohort.
- The **AI narratives** are regenerated with the filtered numbers as input, so the prose describes the filtered cohort.

---

## 6. Send-to-mail blocker (operations note)

The current SMTP relay is **SMTP2GO** (`mail-eu.smtp2go.com:2525`, STARTTLS), with `info@app.salubritycentre.com` as the FromEmail. Sender-domain verification at SMTP2GO is **pending** — until the DNS records (CNAME / SPF / DKIM) for `app.salubritycentre.com` are added and verified in the SMTP2GO dashboard, mail will be rejected with `550 From header sender domain not verified`. The PDFs are still generated correctly; the failure is at the relay step.

---

## 7. Individual final reports — separate flow

This guide covers **corporate** reports. The **individual** final report (one per participant) lives behind the **Final Individual Report** tab inside each camp's detail page (`Admin → Health Camps → [camp] → Final Individual Report`). The admin must click **Publish** there before participants can see their own final report. The dashboard shows a yellow "Individual final reports pending release" reminder card listing camps that are completed but not yet published.

---

## 8. Common questions

**Q: A KPI shows 0% / —. Is the report broken?**
No — that means the underlying data (a specific form submission, a specific lab panel, a specific section response) wasn't captured for the filtered cohort. The narrative will say so explicitly. Check the camp's intake forms.

**Q: I edited a narrative and refreshed — my edit is gone.**
Edits are session-only today. Persistence is in the roadmap.

**Q: The download is slower than the on-screen view.**
The download triggers a fresh server-side regeneration (PDF + Gemini), so 5–8 s on first call is normal.

**Q: Why does the EXCO report's "CDMP" KPI always show 0?**
Because no form name in the current intake catalogue matches `chronic` / `referral` / `cdmp`. CDMP referrals are tracked in a different table that isn't yet wired into the EXCO calculation; that's on the roadmap.

**Q: Can I include only certain sections in the PDF?**
Yes — on the EXCO report, every section and every KPI has a checkbox. Uncheck what you don't need; the downloaded PDF and emailed PDF both honour the selection. (Preliminary and Final don't yet have section-level exclusions.)
