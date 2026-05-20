using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class RitualRoleScarificationTarget : RitualRole
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
					reason = "MessageRitualRoleMustBeHumanlike".Translate(base.LabelCap);
				}
				return false;
			}
			if (p.Ideo == null || p.Ideo.RequiredScars <= p.health.hediffSet.GetHediffCount(HediffDefOf.Scarification))
			{
				if (!skipReason)
				{
					reason = "MessageRitualRoleMustRequireScarification".Translate(p);
				}
				return false;
			}
			if (!p.Faction.IsPlayerSafe())
			{
				if (!skipReason)
				{
					reason = "MessageRitualRoleMustBeColonist".Translate(base.Label);
				}
				return false;
			}
			return true;
		}

		public override bool AppliesToRole(Precept_Role role, out string reason, Precept_Ritual ritual = null, Pawn p = null, bool skipReason = false)
		{
			reason = null;
			return true;
		}

		public override string ExtraInfoForDialog(IEnumerable<Pawn> selected)
		{
			Pawn pawn = selected.FirstOrDefault();
			if (pawn != null)
			{
				if (pawn.Ideo == null || pawn.Ideo.RequiredScars <= 0)
				{
					return "RitualScarificationWarnNonScarificationIdeo".Translate(pawn.Named("PAWN"));
				}
				if (pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Wimp))
				{
					return "RitualScarificationWarnWimp".Translate(pawn.Named("PAWN"));
				}
			}
			return null;
		}
	}
}
