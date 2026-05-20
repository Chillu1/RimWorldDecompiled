using Verse;

namespace RimWorld
{
	public class RitualStage_InteractWithPrisoner : RitualStage
	{
		public override TargetInfo GetSecondFocus(LordJob_Ritual ritual)
		{
			return ritual.assignments.Participants.FirstOrDefault((Pawn p) => p.IsPrisonerOfColony);
		}
	}
}
