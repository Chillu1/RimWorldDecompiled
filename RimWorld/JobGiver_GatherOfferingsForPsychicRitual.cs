using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_GatherOfferingsForPsychicRitual : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		Lord lord = pawn.GetLord();
		if (lord == null)
		{
			return null;
		}
		if (!(lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual))
		{
			return null;
		}
		PsychicRitualDef def = lordToil_PsychicRitual.RitualData.psychicRitual.def;
		PsychicRitualDef_InvocationCircle ritualDef = def as PsychicRitualDef_InvocationCircle;
		if (ritualDef == null)
		{
			return null;
		}
		if (ritualDef.RequiredOffering == null)
		{
			return null;
		}
		PsychicRitual psychicRitual = lordToil_PsychicRitual.RitualData.psychicRitual;
		PsychicRitualRoleDef psychicRitualRoleDef = psychicRitual.assignments.RoleForPawn(pawn);
		if (psychicRitualRoleDef == null)
		{
			return null;
		}
		float num = PsychicRitualToil_GatherOfferings.PawnsOfferingCount(psychicRitual.assignments.AssignedPawns(psychicRitualRoleDef), ritualDef.RequiredOffering);
		int needed = Mathf.CeilToInt(ritualDef.RequiredOffering.GetBaseCount() - num);
		if (needed == 0)
		{
			return null;
		}
		Thing thing = GenClosest.ClosestThingReachable(pawn.PositionHeld, pawn.MapHeld, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.Touch, TraverseParms.For(pawn), 9999f, delegate(Thing thing2)
		{
			if (!ritualDef.RequiredOffering.filter.Allows(thing2))
			{
				return false;
			}
			if (thing2.IsForbidden(pawn))
			{
				return false;
			}
			int stackCount = Mathf.Min(needed, thing2.stackCount);
			return pawn.CanReserve(thing2, 10, stackCount) ? true : false;
		});
		if (thing == null)
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.TakeCountToInventory, thing);
		job.count = Mathf.Min(needed, thing.stackCount);
		return job;
	}
}
