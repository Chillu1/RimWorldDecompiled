using Verse;

namespace RimWorld
{
	public class RitualRolePrisonerOrSlave : RitualRole
	{
		public bool mustBeCapableToFight;

		public bool disallowWildManPrisoner = true;

		public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
		{
			if (!AppliesIfChild(p, out reason, skipReason))
			{
				return false;
			}
			if (disallowWildManPrisoner && p.IsWildMan() && p.IsPrisoner)
			{
				if (!skipReason)
				{
					reason = "MessageRitualRoleMustNotBeImprisonedWildMan".Translate(base.LabelCap);
				}
				return false;
			}
			if (!p.IsPrisonerOfColony && !p.IsSlaveOfColony)
			{
				if (!skipReason)
				{
					reason = "MessageRitualRoleMustBePrisonerOrSlave".Translate(base.LabelCap);
				}
				return false;
			}
			if (mustBeCapableToFight && (p.WorkTagIsDisabled(WorkTags.Violent) || !p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)))
			{
				if (!skipReason)
				{
					reason = "MessageRitualRoleMustBeCapableOfFighting".Translate(p);
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

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref mustBeCapableToFight, "mustBeCapableToFight", defaultValue: false);
			Scribe_Values.Look(ref disallowWildManPrisoner, "disallowWildManPrisoner", defaultValue: false);
		}
	}
}
