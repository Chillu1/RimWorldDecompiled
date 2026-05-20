using Verse;

namespace RimWorld
{
	public class StageFailTrigger_PawnAsleep : StageFailTrigger
	{
		[NoTranslate]
		public string pawnId;

		public override bool Failed(LordJob_Ritual ritual, TargetInfo spot, TargetInfo focus)
		{
			return ritual.PawnWithRole(pawnId).jobs.curDriver.asleep;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref pawnId, "pawnId");
		}
	}
}
