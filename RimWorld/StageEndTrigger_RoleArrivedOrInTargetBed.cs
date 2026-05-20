using Verse;

namespace RimWorld
{
	public class StageEndTrigger_RoleArrivedOrInTargetBed : StageEndTrigger_RolesArrived
	{
		protected override bool ArrivedCheck(string r, LordJob_Ritual ritual)
		{
			Pawn pawn = ritual.PawnWithRole(r);
			if (pawn != null && pawn.CurrentBed() == ritual.selectedTarget.Thing)
			{
				return true;
			}
			return base.ArrivedCheck(r, ritual);
		}
	}
}
