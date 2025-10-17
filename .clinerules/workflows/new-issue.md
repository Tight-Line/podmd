# 🧩 Issue → Branch (Confirm each step)

> Workflow purpose: fetch a GitHub issue, summarize it, propose branch + PR metadata, and perform each action **only after explicit user confirmation**. Every actionable step asks for confirmation.

---

## Inputs / Environment

- `ISSUE_NUMBER` — the GitHub issue number to operate on. If not set, the workflow should prompt the user for it.
- `REPO_OWNER` and `REPO_NAME` — optional if working in the repo; `gh` usually infers the repo from local git.
- `BASE_REF` — the branch to fork from and PR into. If not set, defaults to the current branch (detected as `$(git branch --show-current)`). This allows PRs to target feature branches rather than always the default branch.

---

## 1️⃣ Prompt for issue number (if not provided)

```xml
<ask_followup_question>
<question>Please provide the GitHub issue number to work on (or confirm $ISSUE_NUMBER if already set).</question>
<options>["Use $ISSUE_NUMBER", "Provide a different issue number"]</options>
</ask_followup_question>
```

If the user chooses "Provide a different issue number", the workflow should ask for the number and set `ISSUE_NUMBER` appropriately before proceeding.

---

## 2️⃣ Fetch the GitHub issue (ask before running)

```xml
<ask_followup_question>
<question>I will fetch the issue details from GitHub for issue #${ISSUE_NUMBER}. Proceed?</question>
<options>["Yes, fetch issue", "No, cancel"]</options>
</ask_followup_question>
```

```bash
# runs only after user confirmation
gh issue view ${ISSUE_NUMBER} --json number,title,body,author,labels,url > issue.json
```

---

## 3️⃣ Summarize the issue (ask before running AI summary)

```xml
<ask_followup_question>
<question>I will read `issue.json` and produce a short summary (3–5 bullets) and propose a branch name + PR title/description. Proceed?</question>
<options>["Yes, summarize", "No, skip summarizing"]</options>
</ask_followup_question>
```

```ai
Read the file `issue.json`. Produce:
- A 3–5 bullet summary of what's being asked
- A suggested kebab-case branch name (e.g. `feature/<short>-<issue-number>`)
- A suggested PR title and concise PR body
Only output these values in a clearly labeled block so they can be copied.
```

---

## 4️⃣ Confirm branch name and PR metadata (required before creating branch)

```xml
<ask_followup_question>
<question>Review the proposed branch name and PR metadata. If you want to change anything, pick the option to edit. Otherwise confirm to create the branch.</question>
<options>["Confirm branch & PR metadata", "Edit branch name", "Edit PR title/body", "Cancel workflow"]
</options>
</ask_followup_question>
```

- If the user selects "Edit branch name" or "Edit PR title/body" ask for the new value(s) and re-confirm.

---

## 5️⃣ Create the git branch (ask before running)

```xml
<ask_followup_question>
<question>I will create a new git branch named `{{branch_name}}` from the current branch we're on (detected as the base branch for this PR). Proceed?</question>
<options>["Yes, create branch", "No, don't create branch"]</options>
</ask_followup_question>
```

```bash
# run only after confirmation
git fetch origin
# use current branch as the base for branching and PR
BASE_REF=$(git branch --show-current)
if [ "$BASE_REF" = "HEAD" ]; then
  echo "Detached HEAD state detected. Using origin main as base."
  BASE_REF=main
fi
git checkout -b {{branch_name}}
```

---

## 6️⃣ Create an initial draft PR (ask before running)

```xml
<ask_followup_question>
<question>I can create an initial **draft** PR with the proposed title and description. This will push the empty branch and open a draft PR. Proceed?</question>
<options>["Yes, create draft PR", "No, skip PR creation"]</options>
</ask_followup_question>
```

```bash
# run only after confirmation
# push the branch
git push -u origin HEAD
# create draft PR using gh; uses placeholders from AI step
gh pr create --base ${BASE_REF} --title "{{pr_title}}" --body "{{pr_body}}\n\nResolves #${ISSUE_NUMBER}" --draft
```

---

## 7️⃣ Summarize status and ask for next action (discussion/implementation)

```xml
<ask_followup_question>
<question>Branch and draft PR are ready (if you created them). What would you like to do next?</question>
<options>["Discuss implementation plan here", "Start implementing now (AI-assisted)", "Exit and do this later"]</options>
</ask_followup_question>
```

- If user selects **Discuss implementation plan here**, the AI should produce a short checklist of implementation tasks and ask which task to start on.

- If user selects **Start implementing now (AI-assisted)**, the workflow must **ask for confirmation before each code-edit step** below.

---

## 8️⃣ Implementation — each edit step must be confirmed

Example flow for a single-edit cycle (repeatable):

1. AI proposes a concrete file to edit and the change summary.

```xml
<ask_followup_question>
<question>AI proposes to edit `src/path/to/file.ts` to do: "Add null check for X and update Y behavior". Proceed with this single edit?</question>
<options>["Yes, apply this edit", "No, skip this edit", "Show diff preview first"]</options>
</ask_followup_question>
```

- If the user asks for a diff preview, the AI should produce a unified-diff text block and then re-ask for confirmation.

- If the user confirms, the workflow applies the change (via file edits), then shows the changed files and asks whether to commit.

```bash
# after applying edits
git add <modified-files>
```

```xml
<ask_followup_question>
<question>I have staged the changes to <modified-files>. Would you like me to commit them with the message: "Implement part of #${ISSUE_NUMBER}: {{short-summary}}"?</question>
<options>["Yes, commit and push", "Yes, commit but don't push", "No, keep changes unstaged", "Edit commit message"]</options>
</ask_followup_question>
```

- If user confirms commit & push:

```bash
git commit -m "Implement part of #${ISSUE_NUMBER}: {{short-summary}}"
git push
```

- After push, optionally add an inline review comment to the draft PR or update the PR description — **ask first**.

---

## 8️⃣.x Create PR if skipped (after implementation complete)

If the initial draft PR creation was skipped in step 6, create it now after code implementation and commits are pushed:

```xml
<ask_followup_question>
<question>Initial PR creation was skipped, but implementation is complete and pushed. Do you want to create the PR now with the current branch state?</question>
<options>["Yes, create PR now", "No, continue without PR", "Review implementation first"]</options>
</ask_followup_question>
```

If confirmed:

```xml
<ask_followup_question>
<question>Create PR with title '{{pr_title}}' and description '{{pr_body}}\n\nResolves #${ISSUE_NUMBER}'?</question>
<options>["Yes, create PR", "Edit title/body first", "Cancel PR creation"]</options>
</ask_followup_question>
```

```bash
# create PR using gh (using the same BASE_REF detected earlier)
if [ -z "$BASE_REF" ]; then
  BASE_REF=$(git branch --show-current)
  if [ "$BASE_REF" = "HEAD" ]; then
    BASE_REF=main
  fi
fi
gh pr create --base ${BASE_REF} --title "{{pr_title}}" --body "{{pr_body}}\n\nResolves #${ISSUE_NUMBER}" --draft
```

---

## 9️⃣ Finalize PR (ask before merging)

### Update Memory Bank (before merging)

Before finalizing the PR, update the Memory Bank to document all implemented changes for continuity:

```xml
<ask_followup_question>
<question>Before finalizing the PR, I should update the Memory Bank with the implemented changes, new patterns discovered, and current project status. This ensures documentation continuity for future sessions. Proceed with Memory Bank update?</question>
<options>["Yes, update Memory Bank now", "Skip Memory Bank update", "Review changes first"]</options>
</ask_followup_question>
```

- **Update process**: Review and update `activeContext.md`, `progress.md`, and other relevant memory bank files to document:
  - New technical patterns discovered
  - Changes implemented and their impact
  - Current project phase and status
  - Key learnings and insights
  - Updated next steps and blockers
- **After Memory Bank update**: Proceed with PR merge confirmation below.

When ready to merge, always ask:

```xml
<ask_followup_question>
<question>Do you want me to merge the PR #{{pr_number}} now? (This will merge the branch into ${BASE_REF}.)</question>
<options>["Yes, merge now", "No, leave as draft/unmerged"]</options>
</ask_followup_question>
```

If merging is confirmed, run:

```bash
gh pr merge {{pr_number}} --merge --delete-branch
```

---

## Notes & safety

- **Every** destructive or repository-changing step is gated behind an explicit user confirmation. Nothing will be pushed, branched, or merged without user approval.
- The placeholders like `{{branch_name}}`, `{{pr_title}}`, `{{pr_body}}`, `{{short-summary}}`, and `{{pr_number}}` are filled from the AI summary step or subsequent user edits.
- If your repo uses a default branch name other than `main`, the workflow detects it automatically and uses that as the base when creating the branch.

---
