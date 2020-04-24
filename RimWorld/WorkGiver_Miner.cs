using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Miner : WorkGiver_Scanner
	{
		private static string NoPathTrans;

		private const int MiningJobTicks = 20000;

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public static void ResetStaticData()
		{
			NoPathTrans = "NoPath".Translate();
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Mine))
			{
				bool flag = false;
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c = item.target.Cell + GenAdj.AdjacentCells[i];
					if (c.InBounds(pawn.Map) && c.Walkable(pawn.Map))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					Mineable firstMineable = item.target.Cell.GetFirstMineable(pawn.Map);
					if (firstMineable != null)
					{
						yield return firstMineable;
					}
				}
			}
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Mine);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!t.def.mineable)
			{
				return null;
			}
			if (pawn.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.Mine) == null)
			{
				return null;
			}
			if (!pawn.CanReserve(t, 1, -1, null, forced))
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
					if (!intVec2.InBounds(t.Map) || !ReachabilityImmediate.CanReachImmediate(intVec2, t, pawn.Map, PathEndMode.Touch, pawn) || !intVec2.Walkable(t.Map) || intVec2.Standable(t.Map))
					{
						continue;
					}
					Thing thing = null;
					List<Thing> thingList = intVec2.GetThingList(t.Map);
					for (int k = 0; k < thingList.Count; k++)
					{
						if (thingList[k].def.designateHaulable && thingList[k].def.passability == Traversability.PassThroughOnly)
						{
							thing = thingList[k];
							break;
						}
					}
					if (thing != null)
					{
						Job job = HaulAIUtility.HaulAsideJobFor(pawn, thing);
						if (job != null)
						{
							return job;
						}
						JobFailReason.Is(NoPathTrans);
						return null;
					}
				}
				JobFailReason.Is(NoPathTrans);
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.Mine, t, 20000, checkOverrideOnExpiry: true);
		}
	}
}
