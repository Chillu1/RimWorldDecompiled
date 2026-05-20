using Verse;

namespace RimWorld
{
	public class RitualRole_Mother : RitualRoleForced
	{
		public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
		{
			reason = null;
			if (base.AppliesToPawn(p, out reason, selectedTarget, ritual, assignments, precept, skipReason))
			{
				return true;
			}
			if (p.health.hediffSet.HasHediff(HediffDefOf.PregnancyLabor))
			{
				if (selectedTarget.IsValid && PregnancyUtility.IsUsableBedFor(p, p, (Building_Bed)selectedTarget.Thing))
				{
					return true;
				}
				reason = "CantUseThisBed".Translate();
			}
			else
			{
				reason = "NotInLabor".Translate();
			}
			return false;
		}
	}
}
