Before starting any work, read the following files in order:
1. CLAUDE.md — project overview, architecture rules, folder paths
2. .claude/docs/RS1_REQUIREMENTS.md — mandatory RS1 rules
3. .claude/docs/ENTITIES.md — domain entities and relationships
4. .claude/docs/DECISIONS.md — architecture decisions, do not contradict these
5. .claude/docs/POINTS_TRACKER.md — current points per member
6. .claude/docs/features_<member>.md — feature checklist for the active member

Always follow this workflow:
- /feat-plan "feature" → design before coding
- /feat-do "feature"   → implement from the plan
- /feat-check          → verify RS1 compliance
- /feat-update         → update docs and points

Commands are defined in .claude/commands/. Read them before executing.
Never write code that violates RS1 rules defined in RS1_REQUIREMENTS.md.
Never contradict decisions in DECISIONS.md.