/** Where a match belongs: "Competition · Season · Round", or null when it has no round. */
export function roundLabel(match: {
  competitionName: string | null;
  seasonName: string | null;
  roundName: string | null;
}): string | null {
  if (!match.roundName) return null;
  return [match.competitionName, match.seasonName, match.roundName].filter(Boolean).join(' · ');
}
