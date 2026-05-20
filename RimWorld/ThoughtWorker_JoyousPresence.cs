using Verse;

namespace RimWorld;

public class ThoughtWorker_JoyousPresence : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (p.MapHeld == null)
		{
			return false;
		}
		if (!p.IsPlayerControlled && !p.IsPrisonerOfColony)
		{
			return false;
		}
		foreach (Pawn item in p.MapHeld.mapPawns.FreeColonistsSpawned)
		{
			if (item != p && item.story.traits.HasTrait(TraitDefOf.Joyous) && !item.HostileTo(p))
			{
				return ThoughtState.ActiveWithReason(def.stages[0].description.Formatted(item.Named("PAWN")));
			}
		}
		return false;
	}
}
