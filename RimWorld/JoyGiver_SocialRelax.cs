using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_SocialRelax : JoyGiver
	{
		private static List<CompGatherSpot> workingSpots = new List<CompGatherSpot>();

		private const float GatherRadius = 3.9f;

		private static readonly int NumRadiusCells = GenRadial.NumCellsInRadius(3.9f);

		private static readonly List<IntVec3> RadialPatternMiddleOutward = (from c in GenRadial.RadialPattern.Take(NumRadiusCells)
			orderby Mathf.Abs((c - IntVec3.Zero).LengthHorizontal - 1.95f)
			select c).ToList();

		private static List<ThingDef> nurseableDrugs = new List<ThingDef>();

		public override Job TryGiveJob(Pawn pawn)
		{
			return TryGiveJobInt(pawn, null);
		}

		public override Job TryGiveJobInGatheringArea(Pawn pawn, IntVec3 gatheringSpot)
		{
			return TryGiveJobInt(pawn, (CompGatherSpot x) => GatheringsUtility.InGatheringArea(x.parent.Position, gatheringSpot, pawn.Map));
		}

		private Job TryGiveJobInt(Pawn pawn, Predicate<CompGatherSpot> gatherSpotValidator)
		{
			if (pawn.Map.gatherSpotLister.activeSpots.Count == 0)
			{
				return null;
			}
			workingSpots.Clear();
			for (int i = 0; i < pawn.Map.gatherSpotLister.activeSpots.Count; i++)
			{
				workingSpots.Add(pawn.Map.gatherSpotLister.activeSpots[i]);
			}
			CompGatherSpot result;
			do
			{
				if (!workingSpots.TryRandomElement(out result))
				{
					return null;
				}
				workingSpots.Remove(result);
			}
			while (result.parent.IsForbidden(pawn) || !pawn.CanReach(result.parent, PathEndMode.Touch, Danger.None) || !result.parent.IsSociallyProper(pawn) || !result.parent.IsPoliticallyProper(pawn) || (gatherSpotValidator != null && !gatherSpotValidator(result)));
			Job job;
			Thing chair2;
			if (result.parent.def.surfaceType == SurfaceType.Eat)
			{
				if (!TryFindChairBesideTable(result.parent, pawn, out var chair))
				{
					return null;
				}
				job = JobMaker.MakeJob(def.jobDef, result.parent, chair);
			}
			else if (TryFindChairNear(result.parent.Position, pawn, out chair2))
			{
				job = JobMaker.MakeJob(def.jobDef, result.parent, chair2);
			}
			else
			{
				if (!TryFindSitSpotOnGroundNear(result.parent.Position, pawn, out var result2))
				{
					return null;
				}
				job = JobMaker.MakeJob(def.jobDef, result.parent, result2);
			}
			if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && TryFindIngestibleToNurse(result.parent.Position, pawn, out var ingestible))
			{
				job.targetC = ingestible;
				job.count = Mathf.Min(ingestible.stackCount, ingestible.def.ingestible.maxNumToIngestAtOnce);
			}
			return job;
		}

		private static bool TryFindIngestibleToNurse(IntVec3 center, Pawn ingester, out Thing ingestible)
		{
			if (ingester.IsTeetotaler())
			{
				ingestible = null;
				return false;
			}
			if (ingester.drugs == null)
			{
				ingestible = null;
				return false;
			}
			nurseableDrugs.Clear();
			DrugPolicy currentPolicy = ingester.drugs.CurrentPolicy;
			for (int i = 0; i < currentPolicy.Count; i++)
			{
				if (currentPolicy[i].allowedForJoy && currentPolicy[i].drug.ingestible.nurseable)
				{
					nurseableDrugs.Add(currentPolicy[i].drug);
				}
			}
			nurseableDrugs.Shuffle();
			for (int j = 0; j < nurseableDrugs.Count; j++)
			{
				List<Thing> list = ingester.Map.listerThings.ThingsOfDef(nurseableDrugs[j]);
				if (list.Count > 0)
				{
					Predicate<Thing> validator = (Thing t) => ingester.CanReserve(t) && !t.IsForbidden(ingester);
					ingestible = GenClosest.ClosestThing_Global_Reachable(center, ingester.Map, list, PathEndMode.OnCell, TraverseParms.For(ingester), 40f, validator);
					if (ingestible != null)
					{
						return true;
					}
				}
			}
			ingestible = null;
			return false;
		}

		private static bool TryFindChairBesideTable(Thing table, Pawn sitter, out Thing chair)
		{
			for (int i = 0; i < 30; i++)
			{
				Building edifice = table.RandomAdjacentCellCardinal().GetEdifice(table.Map);
				if (edifice != null && edifice.def.building.isSittable && sitter.CanReserve(edifice))
				{
					chair = edifice;
					return true;
				}
			}
			chair = null;
			return false;
		}

		private static bool TryFindChairNear(IntVec3 center, Pawn sitter, out Thing chair)
		{
			for (int i = 0; i < RadialPatternMiddleOutward.Count; i++)
			{
				Building edifice = (center + RadialPatternMiddleOutward[i]).GetEdifice(sitter.Map);
				if (edifice != null && edifice.def.building.isSittable && sitter.CanReserve(edifice) && !edifice.IsForbidden(sitter) && GenSight.LineOfSight(center, edifice.Position, sitter.Map, skipFirstCell: true))
				{
					chair = edifice;
					return true;
				}
			}
			chair = null;
			return false;
		}

		private static bool TryFindSitSpotOnGroundNear(IntVec3 center, Pawn sitter, out IntVec3 result)
		{
			for (int i = 0; i < 30; i++)
			{
				IntVec3 intVec = center + GenRadial.RadialPattern[Rand.Range(1, NumRadiusCells)];
				if (sitter.CanReserveAndReach(intVec, PathEndMode.OnCell, Danger.None) && intVec.GetEdifice(sitter.Map) == null && GenSight.LineOfSight(center, intVec, sitter.Map, skipFirstCell: true))
				{
					result = intVec;
					return true;
				}
			}
			result = IntVec3.Invalid;
			return false;
		}
	}
}
