/*
 * Issues Agent runner.
 * Source: https://github.com/Veranzi/issues-agent (MIT)
 * License preserved in .github/workflows/issues-agent.LICENSE
 */

// .github/workflows/issues-agent-runner.js
// This is called automatically by the GitHub Action on every push.
// It uses the same smart logic: full scan on first run, resume from last commit after that.

import fetch from 'node-fetch';
import fs from 'fs';
import path from 'path';

const GITHUB_TOKEN = process.env.GITHUB_TOKEN;
const ANTHROPIC_API_KEY = process.env.ANTHROPIC_API_KEY;
const OWNER = process.env.REPO_OWNER;
const REPO = process.env.REPO_NAME;
const BRANCH = process.env.BRANCH || 'main';
const STATE_FILE = '.issues-agent-state.json';

const GH_HEADERS = {
  'Accept': 'application/vnd.github+json',
  'Authorization': `Bearer ${GITHUB_TOKEN}`,
  'Content-Type': 'application/json',
};

// ── helpers ──────────────────────────────────────────────────────────────────

function loadState() {
  try {
    return JSON.parse(fs.readFileSync(STATE_FILE, 'utf8'));
  } catch {
    return null;
  }
}

function saveState(state) {
  fs.writeFileSync(STATE_FILE, JSON.stringify(state, null, 2));
  console.log(`State saved → last SHA: ${state.lastSha?.slice(0, 7)}`);
}

async function ghGet(url) {
  const res = await fetch(url, { headers: GH_HEADERS });
  if (!res.ok) throw new Error(`GitHub API error ${res.status}: ${url}`);
  return res.json();
}

const sleep = ms => new Promise(r => setTimeout(r, ms));

async function claudeAnalyze(prompt) {
  if (!ANTHROPIC_API_KEY) {
    throw new Error(
      'ANTHROPIC_API_KEY env var is empty.\n' +
      'Fix: GitHub repo → Settings → Secrets and variables → Actions → New repository secret.\n' +
      '  Name:  ANTHROPIC_API_KEY\n' +
      '  Value: sk-ant-... (get one from https://console.anthropic.com/settings/keys)'
    );
  }
  const res = await fetch('https://api.anthropic.com/v1/messages', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'x-api-key': ANTHROPIC_API_KEY,
      'anthropic-version': '2023-06-01',
    },
    body: JSON.stringify({
      model: 'claude-sonnet-4-5',
      max_tokens: 16384,
      messages: [{ role: 'user', content: prompt }],
    }),
  });
  const data = await res.json();
  if (!res.ok) {
    throw new Error(`Anthropic API ${res.status}: ${data?.error?.message ?? JSON.stringify(data).slice(0, 300)}`);
  }
  const text = data.content?.find(b => b.type === 'text')?.text;
  if (!text) {
    throw new Error(`Anthropic returned no text content. Body: ${JSON.stringify(data).slice(0, 300)}`);
  }
  return text.replace(/```json|```/g, '').trim();
}

async function postIssue(title, body, labels) {
  const res = await fetch(`https://api.github.com/repos/${OWNER}/${REPO}/issues`, {
    method: 'POST',
    headers: GH_HEADERS,
    body: JSON.stringify({ title, body, labels }),
  });
  if (!res.ok) {
    const err = await res.json();
    throw new Error(`Failed to post issue: ${err.message}`);
  }
  const issue = await res.json();
  console.log(`  ✓ Posted: #${issue.number} — ${title}`);
  return issue;
}

// ── main ─────────────────────────────────────────────────────────────────────

async function run() {
  console.log(`\n🤖 Issues Agent starting for ${OWNER}/${REPO} on branch ${BRANCH}\n`);

  const savedState = loadState();
  const isFirstScan = !savedState;

  console.log(isFirstScan
    ? '📦 First scan — reading full codebase + all commits'
    : `▶️  Resuming from commit ${savedState.lastSha?.slice(0, 7)}`);

  // ── Step 1: Get commit history ────────────────────────────────────────────
  const allCommits = await ghGet(
    `https://api.github.com/repos/${OWNER}/${REPO}/commits?sha=${BRANCH}&per_page=50`
  );
  const latestSha = allCommits[0]?.sha;

  let commitsToProcess = allCommits;
  if (!isFirstScan && savedState.lastSha) {
    const idx = allCommits.findIndex(c => c.sha === savedState.lastSha);
    commitsToProcess = idx > 0 ? allCommits.slice(0, idx) : [];
  }

  const issues = [];

  // ── Step 2: Full codebase scan (first time only) ──────────────────────────
  if (isFirstScan) {
    console.log('\n🔍 Scanning codebase files...');
    const CODE_EXTS = ['.js','.ts','.jsx','.tsx','.py','.java','.kt','.go','.rb','.cs','.cpp','.c','.php','.swift'];
    const treeData = await ghGet(
      `https://api.github.com/repos/${OWNER}/${REPO}/git/trees/${BRANCH}?recursive=1`
    );
    const codeFiles = (treeData.tree || [])
      .filter(f => f.type === 'blob' && CODE_EXTS.some(e => f.path.endsWith(e)))
      .slice(0, 15);

    const fileContents = [];
    for (const f of codeFiles) {
      try {
        const data = await ghGet(
          `https://api.github.com/repos/${OWNER}/${REPO}/contents/${f.path}?ref=${BRANCH}`
        );
        const text = data.encoding === 'base64'
          ? Buffer.from(data.content, 'base64').toString('utf8')
          : data.content;
        fileContents.push({ path: f.path, content: text.slice(0, 1000) });
      } catch { /* skip unreadable files */ }
    }

    if (fileContents.length > 0) {
      console.log(`  Found ${fileContents.length} code files — asking Claude to analyze...`);
      const codeBlock = fileContents.map(f => `### ${f.path}\n\`\`\`\n${f.content}\n\`\`\``).join('\n\n');
      const prompt = `Analyze this codebase from "${OWNER}/${REPO}" and identify all features, fixes, and functionality already implemented.
Return a JSON array of GitHub issues. Each object must have:
- title: string (verb-first, e.g. "Add authentication middleware")
- body: string (bullet points in markdown, what was implemented and why it matters)
- labels: array of strings from: ["bug","enhancement","feature","documentation","refactor","performance","security","test"]
- files: array of filenames where this was found

Raw JSON array only — no markdown fences, no explanation.

${codeBlock}

JSON:`;
      const raw = await claudeAnalyze(prompt);
      let codeIssues;
      try {
        codeIssues = JSON.parse(raw);
      } catch (e) {
        console.error('  ⚠️  Failed to parse Claude response as JSON. Skipping codebase scan.');
        console.error('  Parse error:', e.message);
        console.error('  Raw response (first 1000 chars):', raw.slice(0, 1000));
        codeIssues = [];
      }
      codeIssues.forEach(i => { i._source = 'code'; });
      issues.push(...codeIssues);
      console.log(`  → ${codeIssues.length} items found in codebase`);
    }
  }

  // ── Step 3: New commits since last scan ───────────────────────────────────
  if (commitsToProcess.length === 0) {
    console.log('\n✅ No new commits since last scan — nothing to document');
  } else {
    console.log(`\n📝 Processing ${commitsToProcess.length} new commit(s)...`);

    // fetch diffs for each commit
    const groups = [];
    for (const c of commitsToProcess) {
      try {
        const detail = await ghGet(
          `https://api.github.com/repos/${OWNER}/${REPO}/commits/${c.sha}`
        );
        groups.push({
          sha: c.sha,
          message: c.commit.message.split('\n')[0],
          author: c.commit.author.name,
          date: c.commit.author.date.split('T')[0],
          files: (detail.files || []).map(f => ({
            filename: f.filename,
            status: f.status,
            additions: f.additions,
            deletions: f.deletions,
            patch: (f.patch || '').slice(0, 600),
          })),
        });
      } catch { /* skip */ }
    }

    // Process in batches of 5 to stay within API rate limits
    const BATCH_SIZE = 5;
    const BATCH_DELAY_MS = 20000; // 20s between batches
    let totalCommitIssues = 0;

    for (let batchStart = 0; batchStart < groups.length; batchStart += BATCH_SIZE) {
      const batch = groups.slice(batchStart, batchStart + BATCH_SIZE);
      const batchNum = Math.floor(batchStart / BATCH_SIZE) + 1;
      const totalBatches = Math.ceil(groups.length / BATCH_SIZE);
      console.log(`  Batch ${batchNum}/${totalBatches} (${batch.length} commits)...`);

      const sectText = batch.map((g, i) => {
        const fs = g.files.map(f => `  - ${f.filename} (+${f.additions}/-${f.deletions})`).join('\n');
        const ps = g.files.filter(f => f.patch).slice(0, 2).map(f => f.patch).join('\n');
        return `--- COMMIT ${batchStart + i + 1} ---\nMessage: ${g.message}\nAuthor: ${g.author}\nDate: ${g.date}\nSHA: ${g.sha.slice(0, 7)}\nFiles:\n${fs}\nPatch sample:\n${ps}`;
      }).join('\n\n');

      const prompt = `For each commit below from "${OWNER}/${REPO}", create ONE GitHub issue documenting what was solved/implemented.
Return a JSON array with one object per commit. Each object must have:
- title: string (verb-first)
- body: string (bullet points in markdown listing what was done)
- labels: array from ["bug","enhancement","feature","documentation","refactor","performance","security","test","chore"]
- files: array of filenames
- sha_short: string (7-char SHA)
- author: string
- date: string

Raw JSON array only — no markdown fences.

${sectText}

JSON:`;

      const raw = await claudeAnalyze(prompt);
      let batchIssues;
      try {
        batchIssues = JSON.parse(raw);
      } catch (e) {
        console.error(`  ⚠️  Failed to parse Claude response for batch ${batchNum}. Skipping.`);
        console.error('  Parse error:', e.message);
        console.error('  Raw response (first 500 chars):', raw.slice(0, 500));
        batchIssues = [];
      }
      batchIssues.forEach(i => { i._source = 'commit'; });
      issues.push(...batchIssues);
      totalCommitIssues += batchIssues.length;
      console.log(`    → ${batchIssues.length} issue(s) from batch ${batchNum}`);

      // Rate-limit delay between batches (skip after last batch)
      if (batchStart + BATCH_SIZE < groups.length) {
        console.log(`  ⏳ Waiting ${BATCH_DELAY_MS / 1000}s before next batch...`);
        await sleep(BATCH_DELAY_MS);
      }
    }

    console.log(`  → ${totalCommitIssues} total commit issue(s) documented`);
  }

  // ── Step 4: Post all issues to GitHub ─────────────────────────────────────
  if (issues.length === 0) {
    console.log('\n💤 Nothing new to post.');
  } else {
    console.log(`\n🚀 Posting ${issues.length} issue(s) to github.com/${OWNER}/${REPO}/issues...`);
    for (const issue of issues) {
      const sourceNote = issue._source === 'code'
        ? '> 🤖 Auto-documented from existing codebase'
        : `> 🤖 Auto-documented from commit \`${issue.sha_short || 'N/A'}\` by ${issue.author || 'N/A'} on ${issue.date || 'N/A'}`;
      const files = (issue.files || []).map(f => `\`${f}\``).join(', ');
      const body = `${issue.body}\n\n---\n${files ? `**Files:** ${files}\n\n` : ''}${sourceNote}`;
      await postIssue(issue.title, body, issue.labels || []);
    }
  }

  // ── Step 5: Save state ────────────────────────────────────────────────────
  saveState({
    lastSha: latestSha,
    lastScanDate: new Date().toISOString().split('T')[0],
    totalPosted: (savedState?.totalPosted || 0) + issues.length,
    repo: `${OWNER}/${REPO}`,
  });

  console.log(`\n✅ Done! ${issues.length} issue(s) posted.\n`);
}

run().catch(err => {
  console.error('❌ Agent error:', err.message);
  process.exit(1);
});
