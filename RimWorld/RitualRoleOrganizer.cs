using Verse;

namespace RimWorld
{
	public class RitualRoleOrganizer : RitualRole
	{
		public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
		{
			if (!AppliesIfChild(p, out reason, skipReason))
			{
				return false;
			}
			if (ritual == null)
			{
				if (assignments == null || !assignments.Required(p))
				{
					if (!skipReason)
					{
						reason = "MessageRitualRoleCannotReplaceRequiredPawn".Translate();
					}
					return false;
				}
				return true;
			}
			if (p != ritual.Organizer)
			{
				if (!skipReason)
				{
					reason = "MessageRitualRoleCannotReplaceRequiredPawn".Translate();
				}
				return false;
			}
			return true;
		}

		public override bool AppliesToRole(Precept_Role role, out string reason, Precept_Ritual ritual = null, Pawn p = null, bool skipReason = false)
		{
			reason = null;
			return false;
		}
	}
}
