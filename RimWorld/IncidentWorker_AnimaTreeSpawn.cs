using Verse;

namespace RimWorld;

public class IncidentWorker_AnimaTreeSpawn : IncidentWorker_SpecialTreeSpawn
{
	protected override bool SendLetter => PawnsFinder.HomeMaps_FreeColonistsSpawned.Any((Pawn c) => c.HasPsylink && MeditationFocusDefOf.Natural.CanPawnUse(c));
}
