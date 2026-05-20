using Verse;

namespace RimWorld
{
	public class RitualRolePrisoner : RitualRole
	{
		public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
		{
			if (!AppliesIfChild(p, out reason, skipReason))
			{
				return false;
			}
			if (p.IsPrisonerOfColony)
			{
				return true;
			}
			if (!skipReason)
			{
				reason = "MessageRitualRoleMustBePrisoner".Translate(base.LabelCap);
			}
			return false;
		}

		public override bool AppliesToRole(Precept_Role role, out string reason, Precept_Ritual ritual = null, Pawn p = null, bool skipReason = false)
		{
			reason = null;
			return false;
		}
	}
}
