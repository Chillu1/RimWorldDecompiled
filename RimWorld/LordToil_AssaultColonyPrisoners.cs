using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_AssaultColonyPrisoners : LordToil
	{
		public override bool ForceHighStoryDanger => true;

		public override bool AllowSatisfyLongNeeds => false;

		public override void Init()
		{
			base.Init();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.Drafting, OpportunityType.Critical);
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.PrisonerAssaultColony);
			}
		}
	}
}
