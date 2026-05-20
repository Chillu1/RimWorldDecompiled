using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Teach : WorkGiver_Scanner
{
	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn teacher)
	{
		return teacher.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (ModsConfig.BiotechActive)
		{
			return !SchoolUtility.CanTeachNow(pawn);
		}
		return true;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn pawn2) || pawn.skills == null || pawn2.skills == null)
		{
			return false;
		}
		if (SchoolUtility.NeedsTeacher(pawn2))
		{
			Thing thing = pawn2.CurJob.GetTarget(TargetIndex.A).Thing;
			if (thing != null && pawn.CanReserveSittableOrSpot(SchoolUtility.DeskSpotTeacher(thing)))
			{
				return true;
			}
		}
		return false;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn pawn2))
		{
			return null;
		}
		pawn2.CurJob.SetTarget(TargetIndex.B, pawn);
		return JobMaker.MakeJob(JobDefOf.Lessongiving, pawn2.CurJob.GetTarget(TargetIndex.A), pawn2);
	}
}
