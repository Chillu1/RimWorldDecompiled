using Verse;

namespace RimWorld
{
	public class RitualRoleConvertee : RitualRole
	{
		public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
		{
			if (!AppliesIfChild(p, out reason, skipReason))
			{
				return false;
			}
			if (!p.RaceProps.Humanlike)
			{
				if (!skipReason)
				{
					reason = "MessageRitualRoleMustBeHumanlike".Translate(base.Label);
				}
				return false;
			}
			if ((assignments == null || !assignments.Forced(p)) && p.Ideo == assignments?.Ritual.ideo)
			{
				if (!skipReason)
				{
					reason = "MessageRitualPawnIsAlreadyBelievingIdeo".Translate(p, Find.ActiveLanguageWorker.WithIndefiniteArticle(p.Ideo.memberName, p.gender));
				}
				return false;
			}
			if (p.DevelopmentalStage.Baby())
			{
				if (!skipReason)
				{
					reason = "MessageRitualRoleCannotBeABaby".Translate(base.Label);
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
