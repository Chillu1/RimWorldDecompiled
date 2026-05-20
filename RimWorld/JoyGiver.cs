using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class JoyGiver
{
	public JoyGiverDef def;

	public virtual float GetChance(Pawn pawn)
	{
		return def.baseChance;
	}

	protected virtual void GetSearchSet(Pawn pawn, List<Thing> outCandidates)
	{
		outCandidates.Clear();
		if (def.thingDefs == null)
		{
			return;
		}
		if (def.thingDefs.Count == 1)
		{
			outCandidates.AddRange(pawn.Map.listerThings.ThingsOfDef(def.thingDefs[0]));
		}
		else
		{
			for (int i = 0; i < def.thingDefs.Count; i++)
			{
				outCandidates.AddRange(pawn.Map.listerThings.ThingsOfDef(def.thingDefs[i]));
			}
		}
		if (pawn.ConcernedByVacuum)
		{
			outCandidates.RemoveWhere((Thing thing) => thing.Position.VacuumConcernTo(pawn));
		}
	}

	public abstract Job TryGiveJob(Pawn pawn);

	public virtual Job TryGiveJobWhileInBed(Pawn pawn)
	{
		return null;
	}

	public virtual Job TryGiveJobInGatheringArea(Pawn pawn, IntVec3 gatherSpot, float maxRadius = -1f)
	{
		return null;
	}

	public virtual bool CanBeGivenTo(Pawn pawn)
	{
		if (MissingRequiredCapacity(pawn) != null)
		{
			return false;
		}
		if (def.requiresEnjoyOutdoors && (!pawn.needs.PrefersOutdoors || (pawn.genes != null && !pawn.genes.EnjoysSunlight)))
		{
			return false;
		}
		return def.joyKind.PawnCanDo(pawn);
	}

	public PawnCapacityDef MissingRequiredCapacity(Pawn pawn)
	{
		for (int i = 0; i < def.requiredCapacities.Count; i++)
		{
			if (!pawn.health.capacities.CapableOf(def.requiredCapacities[i]))
			{
				return def.requiredCapacities[i];
			}
		}
		return null;
	}
}
