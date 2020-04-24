using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Merge : WorkGiver_Scanner
	{
		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.listerMergeables.ThingsPotentiallyNeedingMerging();
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return pawn.Map.listerMergeables.ThingsPotentiallyNeedingMerging().Count == 0;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.stackCount == t.def.stackLimit)
			{
				return null;
			}
			if (!HaulAIUtility.PawnCanAutomaticallyHaul(pawn, t, forced))
			{
				return null;
			}
			SlotGroup slotGroup = t.GetSlotGroup();
			if (slotGroup == null)
			{
				return null;
			}
			if (!pawn.CanReserve(t.Position, 1, -1, null, forced))
			{
				return null;
			}
			foreach (Thing heldThing in slotGroup.HeldThings)
			{
				if (heldThing != t && heldThing.CanStackWith(t) && (forced || heldThing.stackCount >= t.stackCount) && heldThing.stackCount < heldThing.def.stackLimit && pawn.CanReserve(heldThing.Position, 1, -1, null, forced) && pawn.CanReserve(heldThing) && heldThing.Position.IsValidStorageFor(heldThing.Map, t))
				{
					Job job = JobMaker.MakeJob(JobDefOf.HaulToCell, t, heldThing.Position);
					job.count = Mathf.Min(heldThing.def.stackLimit - heldThing.stackCount, t.stackCount);
					job.haulMode = HaulMode.ToCellStorage;
					return job;
				}
			}
			JobFailReason.Is("NoMergeTarget".Translate());
			return null;
		}
	}
}
