using Verse;

namespace RimWorld;

public class ThoughtWorker_BioferriteGenerator : ThoughtWorker
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
		if (!p.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
		{
			return false;
		}
		foreach (Thing item in p.MapHeld.listerThings.ThingsOfDef(ThingDefOf.BioferriteGenerator))
		{
			CompNoiseSource compNoiseSource = item.TryGetComp<CompNoiseSource>();
			if (compNoiseSource.Active && p.PositionHeld.InHorDistOf(item.Position, compNoiseSource.Props.radius))
			{
				return true;
			}
		}
		return false;
	}
}
