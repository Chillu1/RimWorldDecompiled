using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class MineAIUtility
{
	private static string NoPathTrans;

	private const int MiningJobTicks = 20000;

	private static List<Designation> tmpDesignations = new List<Designation>();

	public static void ResetStaticData()
	{
		NoPathTrans = "NoPath".Translate();
	}

	public static IEnumerable<Thing> PotentialMineables(Pawn pawn)
	{
		tmpDesignations.Clear();
		tmpDesignations.AddRange(pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Mine));
		tmpDesignations.AddRange(pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.MineVein));
		foreach (Designation tmpDesignation in tmpDesignations)
		{
			bool flag = false;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c = tmpDesignation.target.Cell + GenAdj.AdjacentCells[i];
				if (c.InBounds(pawn.Map) && c.Walkable(pawn.Map))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				Mineable firstMineable = tmpDesignation.target.Cell.GetFirstMineable(pawn.Map);
				if (firstMineable != null)
				{
					yield return firstMineable;
				}
			}
		}
	}

	public static Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!t.def.mineable)
		{
			return null;
		}
		if (pawn.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.Mine) == null && pawn.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.MineVein) == null)
		{
			return null;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return null;
		}
		if (!new HistoryEvent(HistoryEventDefOf.Mined, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			return null;
		}
		bool flag = false;
		for (int i = 0; i < 8; i++)
		{
			IntVec3 intVec = t.Position + GenAdj.AdjacentCells[i];
			if (intVec.InBounds(pawn.Map) && intVec.Standable(pawn.Map) && ReachabilityImmediate.CanReachImmediate(intVec, t, pawn.Map, PathEndMode.Touch, pawn))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			for (int j = 0; j < 8; j++)
			{
				IntVec3 intVec2 = t.Position + GenAdj.AdjacentCells[j];
				if (!intVec2.InBounds(t.Map) || !ReachabilityImmediate.CanReachImmediate(intVec2, t, pawn.Map, PathEndMode.Touch, pawn) || !intVec2.WalkableBy(t.Map, pawn) || intVec2.Standable(t.Map))
				{
					continue;
				}
				List<Thing> thingList = intVec2.GetThingList(t.Map);
				for (int k = 0; k < thingList.Count; k++)
				{
					if (thingList[k].def.designateHaulable && thingList[k].def.passability == Traversability.PassThroughOnly)
					{
						Job job = HaulAIUtility.HaulAsideJobFor(pawn, thingList[k]);
						if (job != null)
						{
							return job;
						}
					}
				}
				flag = true;
				break;
			}
			if (!flag)
			{
				JobFailReason.Is(NoPathTrans);
				return null;
			}
		}
		return JobMaker.MakeJob(JobDefOf.Mine, t, 20000, checkOverrideOnExpiry: true);
	}
}
