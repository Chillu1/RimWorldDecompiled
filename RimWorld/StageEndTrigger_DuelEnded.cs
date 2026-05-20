using Verse;

namespace RimWorld
{
	public class StageEndTrigger_DuelEnded : StageEndTrigger_AnyPawnDead
	{
		protected override bool Trigger(LordJob_Ritual ritual)
		{
			if (base.Trigger(ritual))
			{
				return true;
			}
			foreach (string roleId in roleIds)
			{
				foreach (Pawn item in ritual.assignments.AssignedPawns(roleId))
				{
					if (item.Downed)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
