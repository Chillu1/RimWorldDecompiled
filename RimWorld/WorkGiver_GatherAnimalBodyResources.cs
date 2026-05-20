using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class WorkGiver_GatherAnimalBodyResources : WorkGiver_Scanner
{
	protected abstract JobDef JobDef { get; }

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	protected abstract CompHasGatherableBodyResource GetComp(Pawn animal);

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		List<Pawn> list = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsAnimal)
			{
				CompHasGatherableBodyResource comp = GetComp(list[i]);
				if (comp != null && comp.ActiveAndFull)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn { IsAnimal: not false } pawn2))
		{
			return false;
		}
		CompHasGatherableBodyResource comp = GetComp(pawn2);
		if (comp == null || !comp.ActiveAndFull || pawn2.Downed || (pawn2.roping != null && pawn2.roping.IsRopedByPawn) || !pawn2.CanCasuallyInteractNow() || !pawn.CanReserve(pawn2, 1, -1, null, forced))
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return JobMaker.MakeJob(JobDef, t);
	}
}
