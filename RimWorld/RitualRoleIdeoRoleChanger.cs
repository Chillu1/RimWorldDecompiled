using System.Linq;
using Verse;

namespace RimWorld
{
	public class RitualRoleIdeoRoleChanger : RitualRole
	{
		public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
		{
			if (!AppliesIfChild(p, out reason, skipReason))
			{
				return false;
			}
			if (p.Ideo == null)
			{
				return false;
			}
			if (!p.IsFreeNonSlaveColonist)
			{
				if (!skipReason)
				{
					reason = "MessageRitualRoleMustBeFreeColonist".Translate(base.Label);
				}
				return false;
			}
			if (p.Ideo.GetRole(p) == null && !RitualUtility.AllRolesForPawn(p).Any((Precept_Role r) => r.RequirementsMet(p)))
			{
				reason = "MessageRitualNoRolesAvailable".Translate(p);
				return false;
			}
			if (!Faction.OfPlayer.ideos.Has(p.Ideo))
			{
				reason = "MessageRitualNotOfPlayerIdeo".Translate(p);
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
