---
name: openspec-general-rules
description: General rules that always apply when using any OpenSpec skill (propose, apply, explore, archive). Covers artifact maintenance in response to clarifying questions, keeping requirement artifacts in sync with answers given during implementation, and task.md update discipline when specs change mid-implementation.
---

# OpenSpec General Rules

Apply these rules throughout every OpenSpec skill session.

---

## Rule 1: Clarifying Questions Signal Artifact Gaps

When the user asks a clarifying question **after** any artifact has been written (proposal, design, specs, tasks, etc.):

1. Answer the question.
2. Evaluate whether the question reveals **ambiguity or incompleteness** in the existing artifacts.
3. If yes — update the affected artifact(s) so the answer is captured there permanently.

This applies at any phase: before implementation starts, during it, or after.

> If a question needed to be asked, the artifacts probably should have already answered it.

---

## Rule 2: Questions During Implementation Update Requirement Artifacts

When the user asks a question during the apply phase that touches **requirements or design** (scope, behavior, constraints, design decisions):

1. Answer the question.
2. Assess whether the answer changes or adds to any artifact (proposal.md, design.md, specs/, etc.).
3. If yes — update the relevant artifact(s) before continuing implementation.

> Answers to questions during implementation are decisions. Decisions belong in artifacts, not just in chat history.

---

## Rule 3: Spec Changes Mid-Apply — tasks.md Update Rules

When specifications are changed after implementation has started, and those changes affect what needs to be implemented:

| Task state | Allowed action |
|---|---|
| Completed (`- [x]`) | **Never delete.** Completed work is part of the change record. |
| Not yet started (`- [ ]`) | May be removed if no longer relevant. |
| Completed but now incorrect | **Add a new task** describing the required revision. |

When adding a correction task, format it as:

```
- [ ] Revise <component/area> per updated <artifact> — <one-line reason>
```

> tasks.md is an audit trail. Completed tasks must remain visible even if the spec changed after they were done.
