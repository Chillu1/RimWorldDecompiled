using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_Joinable_Concert : LordJob_Joinable_Party
	{
		protected override ThoughtDef AttendeeThought => ThoughtDefOf.AttendedConcert;

		protected override TaleDef AttendeeTale => TaleDefOf.AttendedConcert;

		protected override ThoughtDef OrganizerThought => ThoughtDefOf.HeldConcert;

		protected override TaleDef OrganizerTale => TaleDefOf.HeldConcert;

		public LordJob_Joinable_Concert()
		{
		}

		public LordJob_Joinable_Concert(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
			: base(spot, organizer, gatheringDef)
		{
		}

		public override string GetReport(Pawn pawn)
		{
			if (pawn != organizer)
			{
				return "LordReportAttendingConcert".Translate();
			}
			return "LordReportHoldingConcert".Translate();
		}

		protected override LordToil CreateGatheringToil(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
		{
			return new LordToil_Concert(spot, organizer, gatheringDef);
		}

		protected override Trigger_TicksPassed GetTimeoutTrigger()
		{
			return new Trigger_TicksPassedAfterConditionMet(base.DurationTicks, () => GatheringsUtility.InGatheringArea(organizer.Position, spot, organizer.Map), 60);
		}
	}
}
