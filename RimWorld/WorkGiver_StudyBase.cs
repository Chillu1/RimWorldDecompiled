using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class WorkGiver_StudyBase : WorkGiver_Scanner
{
	public override bool Prioritized => true;

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return Find.StudyManager.GetStudiableThingsAndPlatforms(pawn.Map);
	}

	public override float GetPriority(Pawn pawn, TargetInfo t)
	{
		Thing thing = t.Thing;
		if (ModsConfig.AnomalyActive && thing is Building_HoldingPlatform building_HoldingPlatform)
		{
			thing = building_HoldingPlatform.HeldPawn;
		}
		if (thing == null)
		{
			return 0f;
		}
		CompStudiable compStudiable = thing.TryGetComp<CompStudiable>();
		return Find.TickManager.TicksGame - compStudiable.lastStudiedTick;
	}
}
