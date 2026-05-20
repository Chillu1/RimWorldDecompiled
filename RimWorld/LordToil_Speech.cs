using Verse;
using Verse.AI;

namespace RimWorld;

public class LordToil_Speech : LordToil_Ritual
{
	public new LordToilData_Speech Data => (LordToilData_Speech)data;

	public LordToil_Speech(IntVec3 spot, Precept_Ritual ritual, LordJob_Ritual lordJob, Pawn organizer)
		: base(spot, lordJob, null, organizer)
	{
		base.organizer = organizer;
		data = new LordToilData_Speech();
	}

	public override void Init()
	{
		base.Init();
		Data.spectateRect = CellRect.CenteredOn(spot, 0);
		Rot4 rotation = spot.GetFirstThing<Building_Throne>(organizer.MapHeld).Rotation;
		SpectateRectSide asSpectateSide = rotation.Opposite.AsSpectateSide;
		Data.spectateRectAllowedSides = SpectateRectSide.All & ~asSpectateSide;
		Data.spectateRectPreferredSide = rotation.AsSpectateSide;
	}

	public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
	{
		if (p == organizer)
		{
			return DutyDefOf.GiveSpeech.hook;
		}
		return DutyDefOf.Spectate.hook;
	}

	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn = lord.ownedPawns[i];
			if (pawn == organizer)
			{
				Building_Throne firstThing = spot.GetFirstThing<Building_Throne>(base.Map);
				pawn.mindState.duty = new PawnDuty(DutyDefOf.GiveSpeech, spot, firstThing);
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
			else
			{
				PawnDuty pawnDuty = new PawnDuty(DutyDefOf.Spectate);
				pawnDuty.spectateRect = Data.spectateRect;
				pawnDuty.spectateRectAllowedSides = Data.spectateRectAllowedSides;
				pawnDuty.spectateRectPreferredSide = Data.spectateRectPreferredSide;
				pawn.mindState.duty = pawnDuty;
			}
		}
	}
}
