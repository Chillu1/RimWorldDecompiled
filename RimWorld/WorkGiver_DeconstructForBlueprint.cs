using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_DeconstructForBlueprint : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		foreach (Building item in pawn.Map.listerBuildings.allBuildingsColonist)
		{
			if (item.def != null && !pawn.Map.blueprintGrid[item.Position].NullOrEmpty() && item.DeconstructibleBy(pawn.Faction).Accepted)
			{
				yield return item;
			}
		}
		foreach (Building item2 in pawn.Map.listerBuildings.allBuildingsNonColonist)
		{
			if (item2.def != null && !pawn.Map.blueprintGrid[item2.Position].NullOrEmpty() && item2.DeconstructibleBy(pawn.Faction).Accepted)
			{
				yield return item2;
			}
		}
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		IEnumerable<Blueprint> enumerable = BuildableBlueprintsAt(pawn, t.Position);
		if (enumerable.EnumerableNullOrEmpty())
		{
			return false;
		}
		if (t.TryGetComp(out CompExplosive comp) && comp.wickStarted)
		{
			JobFailReason.Is("AboutToExplode".Translate());
			return false;
		}
		if (t is Building building)
		{
			AcceptanceReport acceptanceReport = building.DeconstructibleBy(pawn.Faction);
			if (!acceptanceReport.Accepted)
			{
				JobFailReason.Is(acceptanceReport.Reason);
				return false;
			}
			if (building.IsForbidden(pawn))
			{
				JobFailReason.Is("ForbiddenLower".Translate());
				return false;
			}
		}
		foreach (Blueprint item in enumerable)
		{
			if (item is Blueprint_Install blueprint_Install && blueprint_Install.ThingToInstall == t)
			{
				return false;
			}
			if (item.IsForbidden(pawn))
			{
				JobFailReason.Is("ForbiddenLower".Translate());
				return false;
			}
			if (GenConstruct.CanReplace(item.EntityToBuild(), t.def, item.Stuff, t.Stuff))
			{
				return true;
			}
		}
		return false;
	}

	private static IEnumerable<Blueprint> BuildableBlueprintsAt(Pawn pawn, IntVec3 cell)
	{
		List<Blueprint> list = pawn.Map.blueprintGrid[cell];
		if (list.NullOrEmpty())
		{
			yield break;
		}
		foreach (Blueprint item in list)
		{
			if (item.Faction == pawn.Faction)
			{
				yield return item;
			}
		}
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return JobMaker.MakeJob(JobDefOf.DeconstructForBlueprint, t);
	}
}
