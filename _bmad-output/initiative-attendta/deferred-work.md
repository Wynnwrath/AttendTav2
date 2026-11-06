- source_plan: `_bmad-output/initiative-attendta/tickets/epic-platform-access/tickets/story-caller-owned-transaction-unit-of-work-and-append-only-activi-plan.md`
  summary: Remove venue image paths from cleanup-failure diagnostics.
  evidence: `VenueController.TryDeleteVenueImage` predates this story and logs the relative image path and exception, which may contain a filesystem path.
