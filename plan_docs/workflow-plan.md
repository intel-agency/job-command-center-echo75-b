# Workflow Execution Plan — project-setup

## Overview
- **Workflow name:** project-setup
- **Project name:** Job Command Center (job-command-center-echo75-b)
- **Total assignments:** 6
- **Pre-script event:** create-workflow-plan (this document)
- **Post-assignment event handlers:** validate-assignment-completion, report-progress

## Project Context Summary
- **Purpose:** Local-first LinkedIn job search automation with "God Mode" CDP architecture that attaches to an existing authenticated Chrome session.
- **Core architecture:** .NET Aspire orchestrator manages Postgres, Harvester worker, and Blazor Server UI.
- **Critical constraint:** Harvester must run as a **host process** (not containerized) to reach Chrome CDP port 9222.
- **Tech stack:** .NET 10, C# 12, Aspire, Blazor Server, PostgreSQL (EF Core), Playwright, MudBlazor.
- **Security posture:** No handling of LinkedIn credentials; session piggybacks on user-authenticated Chrome.
- **Primary risks:** LinkedIn DOM changes, bot detection behavior analysis, Chrome port availability, Docker networking constraints.
- **Repository owner:** intel-agency
- **Default branch:** main (not protected)

## Assignment Execution Plan

| Assignment | Goal | Key Acceptance Criteria | Project-Specific Notes | Prerequisites | Dependencies | Risks | Events |
|---|---|---|---|---|---|---|---|
| **init-existing-repository** | Initialize repo admin structure (project board, labels, rename files, branch/PR) | PR & branch created; GitHub Project board with columns; labels imported; filenames updated | Ensure workspace/devcontainer names align with `job-command-center-echo75-b` | GH auth with repo/project scopes | None | Missing GH permissions; repo name mismatch | **Workflow post-assignment:** validate-assignment-completion, report-progress |
| **create-app-plan** | Produce application plan issue and supporting plan docs | Plan follows template; phases/risks/tech stack documented; issue created with labels + milestone + project link | Must incorporate CDP "God Mode", host-process Harvester, Postgres, Blazor Server; use plan_docs as sources | plan_docs/ content available | init-existing-repository | Naming inconsistency (ProfileGenie vs JobCommandCenter) | **Assignment events:** pre-assignment-begin gather-context; on-failure recover-from-error; post-assignment report-progress |
| **create-project-structure** | Scaffold .NET solution, configs, CI/CD, docs | Solution structure created; config files; CI workflows; README/docs; initial commit | Must align with .NET 10 + Aspire; Harvester as host process; include MudBlazor UI; Postgres container | Approved plan | create-app-plan | Harvester containerization error; missing README | **Workflow post-assignment:** validate-assignment-completion, report-progress |
| **create-repository-summary** | Create `.ai-repository-summary.md` | Summary exists, formatted per `ai-instructions-format.md` | Requires thorough repo inventory and validated command sequences | Project structure exists | create-project-structure | Commands unavailable; incomplete validation | **Workflow post-assignment:** validate-assignment-completion, report-progress |
| **create-agents-md-file** | Create `AGENTS.md` with verified commands | AGENTS.md present, commands validated, structure documented | Should complement `.ai-repository-summary.md` and plan docs | Repo summary + build/test available | create-repository-summary | Commands fail or not yet implemented | **Workflow post-assignment:** validate-assignment-completion, report-progress |
| **debrief-and-document** | Document learnings + trace log | Report follows 12-section template; trace file created; committed | Must include execution trace file `debrief-and-document/trace.md` | All prior assignments done | create-agents-md-file | Trace completeness; large report scope | **Workflow post-assignment:** validate-assignment-completion, report-progress |

## Sequencing Diagram (Text)
```
pre-script-begin
  └─ create-workflow-plan (this doc)

init-existing-repository
  └─ post-assignment-complete: validate-assignment-completion → report-progress

create-app-plan
  ├─ pre-assignment-begin: gather-context
  ├─ on-assignment-failure: recover-from-error
  └─ post-assignment-complete: report-progress

create-project-structure
  └─ post-assignment-complete: validate-assignment-completion → report-progress

create-repository-summary
  └─ post-assignment-complete: validate-assignment-completion → report-progress

create-agents-md-file
  └─ post-assignment-complete: validate-assignment-completion → report-progress

debrief-and-document
  └─ post-assignment-complete: validate-assignment-completion → report-progress
```

## Open Questions (Resolved)
1. **Naming:** Use **JobCommandCenter** as the canonical solution/project name (ProfileGenie was internal codename).
2. **README:** README creation belongs to `create-project-structure` assignment.
3. **Owner:** Use **intel-agency** for downstream references (this is the actual GitHub owner).
4. **UI Stack:** Use **MudBlazor** as the primary UI component library (per App Implementation Spec).
5. **Template:** The application plan template is at `.github/ISSUE_TEMPLATE/application-plan.md` (confirmed present).

## Resolution Trace

All assignment files resolved from canonical remote repository:
- `https://raw.githubusercontent.com/nam20485/agent-instructions/main/ai_instruction_modules/ai-workflow-assignments/dynamic-workflows/project-setup.md`
- `https://raw.githubusercontent.com/nam20485/agent-instructions/main/ai_instruction_modules/ai-workflow-assignments/init-existing-repository.md`
- `https://raw.githubusercontent.com/nam20485/agent-instructions/main/ai_instruction_modules/ai-workflow-assignments/create-app-plan.md`
- `https://raw.githubusercontent.com/nam20485/agent-instructions/main/ai_instruction_modules/ai-workflow-assignments/create-project-structure.md`
- `https://raw.githubusercontent.com/nam20485/agent-instructions/main/ai_instruction_modules/ai-workflow-assignments/create-repository-summary.md`
- `https://raw.githubusercontent.com/nam20485/agent-instructions/main/ai_instruction_modules/ai-workflow-assignments/create-agents-md-file.md`
- `https://raw.githubusercontent.com/nam20485/agent-instructions/main/ai_instruction_modules/ai-workflow-assignments/debrief-and-document.md`
- `https://raw.githubusercontent.com/nam20485/agent-instructions/main/ai_instruction_modules/ai-workflow-assignments/validate-assignment-completion.md`
- `https://raw.githubusercontent.com/nam20485/agent-instructions/main/ai_instruction_modules/ai-workflow-assignments/report-progress.md`
