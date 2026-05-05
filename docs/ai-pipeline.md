# AI Pipeline — Salubrity

This document describes the Gemini-powered AI pipeline used by the Salubrity backend to generate narrative content in clinical and corporate reports.

---

## 1. Overview

Salubrity uses **Google Gemini** (`gemini-2.5-flash` via the Generative Language REST API) for natural-language generation of clinical narratives, executive summaries, and recommendation drafts. The pipeline is deliberately constrained: every prompt is grounded in real, structured data pulled from the database and Gemini is instructed to use only the facts in the prompt.

| Concern | Where it lives |
|---|---|
| API key + endpoint | `appsettings.*.json → Gemini` section |
| HTTP client | `Salubrity.Infrastructure/AI/GeminiClient.cs` |
| Abstraction | `Salubrity.Application/Interfaces/AI/IGeminiClient.cs` |
| Options class | `Salubrity.Application/Options/GeminiOptions.cs` |
| DI registration | `Salubrity.Infrastructure/DependencyInjection.cs` |

**Configuration sample (Production):**
```json
"Gemini": {
  "ApiKey": "<set via env / appsettings — never commit>",
  "Model": "gemini-2.5-flash",
  "Endpoint": "https://generativelanguage.googleapis.com/v1beta/models"
}
```

The key is rotatable; restart the API service after changing it.

---

## 2. Core call pattern

Every consumer follows the same shape: a **system instruction** that locks the model into a JSON schema and tone, plus a **user prompt** that contains all the real data the model is allowed to reference.

```csharp
string raw = await _gemini.GenerateJsonAsync(
    systemInstruction,           // schema + tone constraints
    userPromptWithData,          // ONLY real data here
    temperature: 0.35,           // low, narrative-grade
    maxOutputTokens: 6000,
    ct: ct);

var json = ExtractJsonObject(raw);   // strips ``` fences if any
using var doc = JsonDocument.Parse(json);
var root = doc.RootElement;
string read(string key) => root.TryGetProperty(key, out var v)
    ? (v.GetString() ?? string.Empty).Trim()
    : string.Empty;
```

### Universal constraints

Every system instruction enforces:
1. **Use ONLY the facts in the user prompt** — no invented statistics or diagnoses.
2. **2–4 plain-text sentences per field** — no markdown, no bullets unless explicitly requested.
3. **Professional impersonal voice** — no first-person pronouns ("we", "I", "my").
4. **Reference the client by name** where natural.
5. **JSON-mode** — respond with a single JSON object containing exactly the listed string keys, no surrounding text.

On parse failure (network, JSON malformed, missing keys), services fall back to per-field "Narrative generation is currently unavailable. Please retry shortly or write this section manually." text. The error is logged with the raw model response for debugging.

---

## 3. Consumers

### 3.1 Preliminary Corporate Report
- **Service:** `Salubrity.Application/Services/Reporting/Reports/CorporateReportService.cs`
- **Trigger:** `BuildAsync(campId, filters, ct)` — called every time the JSON or PDF endpoint is hit (cached client-side for 60 s by react-query).
- **Inputs (from `ICorporateReportRepository.LoadAsync`):**
  - Camp metadata (name, client, package, dates)
  - Participation: expected, total attendees, gender split
  - Per-station completion percentages
  - Top 10 clinical findings (numeric vitals classified against `VitalThresholds`)
- **Output keys:** `overview`, `clinicalFindings`, `attendanceNotes`, `criticalFindingsNotes`, `stationSummary`, `topFindingsSummary`, `overallConclusion`, `whatHappensNext`.

### 3.2 Final Corporate Report
- **Service:** `Salubrity.Application/Services/Reporting/Reports/FinalCorporateReportService.cs`
- **Inputs:** everything Preliminary uses, plus age-bucket distribution from `IFinalCorporateReportRepository.GetAgeBucketsAsync`, plus the chart distributions actually shown on screen so the narrative matches.
- **Output keys (21):** `introduction`, `executiveOverview`, `executiveClinicalFindings`, `executiveRecommendation`, `overallHealthOfDisease`, `keyRiskClusters`, `objectivesAndMethods`, `participationByAgeNotes`, `lifestyleRiskOverallSummary`, `metabolicNcdNotes`, `mentalHealthSummary`, `eyeVisualHealthSummary`, `painAssessmentNotes`, `lifestyleRiskSecondarySummary`, `systemicOrganFunctionNotes`, `topFindingsSummary`, `analysisOutlookSummary`, `trendImprovements`, `trendDeclines`, `trendStableAreas`, `conclusion`.
- **Eye/Visual grounding:** if `IFinalCorporateReportRepository.GetEyeVisualHealthAsync(campId)` returns no responses, the prompt explicitly says "no eye exam responses recorded for this camp" so Gemini doesn't fabricate findings.

### 3.3 EXCO Corporate Report
- **Service:** `Salubrity.Application/Services/Reporting/Reports/ExcoCorporateReportService.cs`
- **Inputs:** participation rate, gender split, age buckets, top findings, plus six per-station attendance counts derived from `IFinalCorporateReportRepository.GetExcoCategoryCountsAsync` (counts distinct patients with completed forms for each category — Vision, BP/Triage, CDMP, Pre-diabetes, Mental Health).
- **Output keys (21):** six `kpi…Notes` + six `kpi…Trend` + four `headline*` + seven strategic-snapshot fields + two early-positive fields.
- **Trend constraint:** `kpi…Trend` must be exactly `Increase`, `Decrease`, or `Stable`. Defaults to `Stable` when no historical comparator is available.

### 3.4 Doctor Recommendation Draft (per-patient)
- **Service:** `Salubrity.Application/Services/Clinical/RecommendationDraftService.cs`
- **Trigger:** `POST /api/v1/doctor/recommendations/draft` with the doctor's existing draft text + clinical context.
- **Constraint:** impersonal/imperative voice ("It is recommended", "Follow up with"); first-person pronouns banned via system instruction.

---

## 4. Filtering propagates into prompts

When the admin applies Gender / Age / Day filters on a corporate report, the filters flow through the repository (`CorporateReportRepository.LoadAsync` and `FinalCorporateReportRepository.GetAgeBucketsFilteredAsync` / `GetExcoCategoryCountsAsync`) so that Gemini only sees the **filtered** numbers. Narrative output therefore describes the filtered cohort, not the whole camp.

---

## 5. Editing AI output

All Gemini-generated text fields in the Final and EXCO reports are wrapped in `<EditSaveBlock>` (`/components/admin/reports/EditSaveBlock.tsx`). Admins can:
1. Click **Edit** next to any narrative block.
2. Override the AI-generated text in a textarea.
3. Click **Save** to keep the edit for the rest of the session.

**Persistence is currently in-session only.** A page refresh re-fetches and Gemini regenerates fresh. To make edits persist, a follow-up `FinalReportEdits` (and `ExcoReportEdits`) overlay table is needed, with the service merging persisted overrides over Gemini's output before returning the DTO.

---

## 6. Performance and latency

| Operation | Typical latency |
|---|---|
| `gemini-2.5-flash` JSON-mode call (~6k token cap) | 3–6 s |
| Repo aggregation per report (≤500 participants) | 0.3–1 s |
| ScottPlot chart render (per chart) | 80–200 ms |
| QuestPDF render of a full Final report | 1–2 s |

react-query caches JSON responses for 60 s with a 30 s stale time, so consecutive views of the same report after the first generation are near-instant.

---

## 7. Failure modes and fallbacks

| Failure | Behaviour |
|---|---|
| Gemini API unreachable / 5xx | Service catches, logs raw response, returns per-field "Narrative generation is currently unavailable…" text. Report still renders with charts + KPIs. |
| Gemini returns malformed JSON | Same — `ExtractJsonObject` strips fences, `JsonDocument.Parse` throws, fallback kicks in. |
| Missing key in returned JSON | That field becomes empty string (`read()` helper returns `""`). Other fields still populate. |
| Camp has zero responses | Repo returns empty aggregates; Gemini prompt explicitly notes "(none recorded)" so the narrative doesn't fabricate. |
| Filter narrows to zero participants | Same as above — narrative will describe the empty filtered cohort honestly. |

---

## 8. Roadmap (open work)

1. **ICD-10 mapping for Top Critical Findings** — Phase 4 in the backlog. Plan: Gemini-based name → ICD-10 classifier with a `IcdCodeCache` table (finding name, code, classified_at). Avoids importing the full WHO list.
2. **Edit persistence** — overlay tables (`FinalReportEdits`, `ExcoReportEdits`) keyed by `(campId, fieldKey)`. Service merges overrides over Gemini output at fetch time.
3. **Cross-camp trend grounding** — populate the Final report's "Analysis & Outlook" line chart with real prior-camp data (currently fabricated 2024/2025 quarterly values) by querying historical camps for the same client/org.
4. **Recommendation persistence on EXCO/Final** — saved to a new `CorporateRecommendations` table, scoped per camp.

---

## 9. Operational checks

- **Verify the key works:** `curl -s -H "X-Goog-Api-Key: $KEY" "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent" -H "Content-Type: application/json" -d '{"contents":[{"parts":[{"text":"hello"}]}]}'`
- **Tail Gemini-related warnings:** `sudo journalctl -u salubrity-api.service -f | grep -i gemini`
- **Force a regeneration in the UI:** clear the report query cache (refresh page) — react-query reissues the request.
